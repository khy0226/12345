using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ReflectionProbeSpecular : EditorWindow
{
    private GameObject[] tempSpheres;
    private Material emissiveMaterial;
    private float sphereScale = 0.5f;
    private float emissionIntensity = 1.0f;

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

    void UpdateSphereProperties()
    {
        if (tempSpheres != null)
        {
            foreach (GameObject sphere in tempSpheres)
            {
                if (sphere != null)
                {
                    sphere.transform.localScale = Vector3.one * sphereScale;
                    Renderer renderer = sphere.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Safely check for properties to prevent material instantiation
                        if (renderer.sharedMaterial.HasProperty("_EmissionColor"))
                        {
                            renderer.sharedMaterial.SetColor("_EmissionColor", renderer.sharedMaterial.color * emissionIntensity);
                        }
                    }
                }
            }
        }
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
        float newSphereScale = EditorGUILayout.Slider("Scale", sphereScale, 0.1f, 2.0f);
        float newEmissionIntensity = EditorGUILayout.Slider("Intensity", emissionIntensity, 0.0f, 10.0f);
        if (EditorGUI.EndChangeCheck())
        {
            sphereScale = newSphereScale;
            emissionIntensity = newEmissionIntensity;
        }


    }

    private void CreateSpheres()
    {
        RemoveSpheres(); // Ensure clean start
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
                sphereMaterial.SetColor("_BaseColor", light.color);
                sphereMaterial.SetColor("_EmissionColor", light.color * emissionIntensity);
                sphereMaterial.EnableKeyword("_EMISSION");
                sphereMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;


                renderer.material = sphereMaterial;

                // 재질의 변경 사항을 에디터에 반영
                EditorUtility.SetDirty(renderer);
                EditorUtility.SetDirty(sphereMaterial);

                tempSpheres[index++] = sphere;
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
