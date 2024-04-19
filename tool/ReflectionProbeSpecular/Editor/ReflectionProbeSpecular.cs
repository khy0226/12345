using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ReflectionProbeSpecular : EditorWindow
{
    private GameObject[] tempSpheres;
    private Material emissiveMaterial;
    private float sphereScale = 0.5f;
    private float emissionIntensity = 1.0f;
    private float saturation = 1.0f;

    public class ColorData : MonoBehaviour
    {
        public Color originalBaseColor;
        public Color originalEmissionColor;
    }

    [MenuItem("Tools/Reflection Probe Specular")]
    public static void ShowWindow()
    {
        var window = GetWindow<ReflectionProbeSpecular>("Reflection Probe Specular");
        window.minSize = new Vector2(200, 200);
    }

    void OnEnable()
    {
        emissiveMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        EditorApplication.update += UpdateSphereProperties;
    }

    void OnDisable()
    {
        DestroyImmediate(emissiveMaterial);
        EditorApplication.update -= UpdateSphereProperties;
    }




    void OnGUI()
    {
        GUILayout.Space(20);
        if (GUILayout.Button("Create Sphere"))
        {
            CreateSpheres();
        }

        if (GUILayout.Button("Remove Sphere"))
        {
            RemoveSpheres();
        }

        if (GUILayout.Button("Bake Reflection Probes"))
        {
            BakeReflectionProbes();
        }

        GUILayout.Space(20);

        GUILayout.Label("Sphere Control", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        float newSphereScale = EditorGUILayout.Slider("Scale", sphereScale, 0.1f, 4.0f);
        float newEmissionIntensity = EditorGUILayout.Slider("Intensity", emissionIntensity, 0.0f, 10.0f);
        float newSaturation = EditorGUILayout.Slider("Saturation", saturation, 0.0f, 2.0f);
        if (EditorGUI.EndChangeCheck())
        {
            sphereScale = newSphereScale;
            emissionIntensity = newEmissionIntensity;
            saturation = newSaturation;
        }


    }

    private Color AdjustSaturation(Color color, float saturationMultiplier)
    {
        Color.RGBToHSV(color, out float H, out float S, out float V);
        S = Mathf.Clamp(S * saturationMultiplier, 0, 1);  // 채도를 조절하되 0과 1의 범위를 벗어나지 않도록
        return Color.HSVToRGB(H, S, V, true);  // true는 HDR 색상 처리를 활성화
    }

    private void CreateSpheres()
    {
        RemoveSpheres();
        GameObject parentObject = new GameObject("SpheresParent");
        Light[] pointLights = FindObjectsOfType<Light>();
        tempSpheres = new GameObject[pointLights.Length];
        int index = 0;

        foreach (Light light in pointLights)
        {
            if (light.type == LightType.Point)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(parentObject.transform);
                sphere.transform.position = light.transform.position;
                sphere.transform.localScale = Vector3.one * sphereScale;
                Renderer renderer = sphere.GetComponent<Renderer>();
                Material sphereMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                Color baseColor = light.color; // 원래 색상
                Color emissionColor = light.color * emissionIntensity; // 원래 발광 색상

                sphereMaterial.SetColor("_BaseColor", baseColor);
                sphereMaterial.SetColor("_EmissionColor", emissionColor);
                sphereMaterial.EnableKeyword("_EMISSION");
                sphereMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

                renderer.material = sphereMaterial;

                ColorData colorData = sphere.AddComponent<ColorData>();
                colorData.originalBaseColor = baseColor;
                colorData.originalEmissionColor = emissionColor;

                tempSpheres[index++] = sphere;
            }
        }
    }

    void UpdateSphereProperties()
    {
        if (tempSpheres != null)
        {
            foreach (GameObject sphere in tempSpheres)
            {
                if (sphere != null)
                {
                    sphere.transform.localScale = Vector3.one * sphereScale;  // 크기 조절 적용

                    ColorData colorData = sphere.GetComponent<ColorData>();
                    if (colorData != null)
                    {
                        Renderer renderer = sphere.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // 원래 색상을 기반으로 채도 조절 적용
                            Color adjustedBaseColor = AdjustSaturation(colorData.originalBaseColor, saturation);
                            Color adjustedEmissionColor = AdjustSaturation(colorData.originalEmissionColor * emissionIntensity, saturation);  // 인텐시티 조절 적용

                            // 조정된 색상을 재적용
                            renderer.sharedMaterial.SetColor("_BaseColor", adjustedBaseColor);
                            renderer.sharedMaterial.SetColor("_EmissionColor", adjustedEmissionColor);
                        }
                    }
                }
            }
        }
    }





    private void RemoveSpheres()
    {
        GameObject parentObject = GameObject.Find("SpheresParent");
        if (parentObject != null)
        {
            DestroyImmediate(parentObject);
        }
        tempSpheres = null;
    }

    private void BakeReflectionProbes()
    {
        ReflectionProbe[] probes = FindObjectsOfType<ReflectionProbe>();
        UnityEngine.SceneManagement.Scene activeScene = EditorSceneManager.GetActiveScene();
        string sceneFilePath = activeScene.path;
        string sceneFolderPath = sceneFilePath.Remove(sceneFilePath.Length - ".unity".Length);

        foreach (ReflectionProbe probe in probes)
        {

            // Construct the path for each reflection probe
            string reflectionProbePath = sceneFolderPath + "/" + probe.name + ".exr";

            // Bake each reflection probe individually
            Lightmapping.BakeReflectionProbe(probe, reflectionProbePath);
        }
    }

}
