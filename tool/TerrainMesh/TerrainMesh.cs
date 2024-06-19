using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class TerrainCollector : EditorWindow
{
    private List<Terrain> terrains = new List<Terrain>();
    private string savePath = "";

    private const string SavePathKey = "TerrainCollector_SavePath";

    [MenuItem("Custom/터레인을 메쉬로 변경")]
    public static void ShowWindow()
    {
        GetWindow<TerrainCollector>("터레인을 메쉬로 변경");
    }

    private void OnEnable()
    {
        // Load the saved path on enable
        savePath = EditorPrefs.GetString(SavePathKey, "");
    }

    private void OnDisable()
    {
        // Save the path when the window is disabled
        EditorPrefs.SetString(SavePathKey, savePath);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Terrains", EditorStyles.boldLabel);

        for (int i = 0; i < terrains.Count; i++)
        {
            terrains[i] = (Terrain)EditorGUILayout.ObjectField(terrains[i], typeof(Terrain), true);
        }

        if (GUILayout.Button("Add Selected Terrains"))
        {
            AddSelectedTerrains();
        }

        if (GUILayout.Button("Clear Terrains"))
        {
            terrains.Clear();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("OBJ 파일 저장 경로", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField("경로", savePath);
        if (GUILayout.Button("경로 선택"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Choose Save Location", "", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                savePath = selectedPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Process Terrains"))
        {
            ProcessTerrains();
        }

        // Handle drag and drop
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "여기에 터레인을 넣어주세요");

        if (evt.type == EventType.DragUpdated && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform && dropArea.Contains(evt.mousePosition))
        {
            DragAndDrop.AcceptDrag();
            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is GameObject go)
                {
                    AddTerrainsFromGameObject(go);
                }
            }
            evt.Use();
        }
    }

    private void AddSelectedTerrains()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            AddTerrainsFromGameObject(obj);
        }
    }

    private void AddTerrainsFromGameObject(GameObject go)
    {
        Terrain terrain = go.GetComponent<Terrain>();
        if (terrain != null && !terrains.Contains(terrain))
        {
            terrains.Add(terrain);
        }

        foreach (Transform child in go.transform)
        {
            AddTerrainsFromGameObject(child.gameObject);
        }
    }

    private void ProcessTerrains()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            Debug.LogError("저장 경로를 선택하세요.");
            return;
        }

        foreach (Terrain terrain in terrains)
        {
            if (terrain != null)
            {
                SaveTerrainAsOBJ(terrain, savePath);
            }
        }

        AssetDatabase.Refresh();
    }

    private void SaveTerrainAsOBJ(Terrain terrain, string savePath)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int w = terrainData.heightmapResolution;
        int h = terrainData.heightmapResolution;
        Vector3 meshScale = terrainData.size;
        int tRes = (w - 1) * (h - 1) * 6;
        int vRes = w * h;

        Vector3[] vertices = new Vector3[vRes];
        Vector3[] normals = new Vector3[vRes];
        Vector2[] uvs = new Vector2[vRes];
        int[] triangles = new int[tRes];

        float[,] heights = terrainData.GetHeights(0, 0, w, h);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Transform vertices to apply a 90-degree rotation around the Y-axis
                float originalX = x / (float)(w - 1) * meshScale.x;
                float originalY = heights[x, y] * meshScale.y;
                float originalZ = y / (float)(h - 1) * meshScale.z;

                // Apply Y-axis 90-degree rotation
                float rotatedX = -originalZ;
                float rotatedZ = originalX;

                vertices[y * w + x] = new Vector3(rotatedX, originalY, rotatedZ) + new Vector3(-terrainPos.z, terrainPos.y, terrainPos.x);

                // UVs: No rotation needed, just apply as is
                uvs[y * w + x] = new Vector2(x / (float)(w - 1), y / (float)(h - 1));
            }
        }

        int index = 0;
        for (int y = 0; y < h - 1; y++)
        {
            for (int x = 0; x < w - 1; x++)
            {
                triangles[index++] = (y * w) + x;
                triangles[index++] = ((y + 1) * w) + x;
                triangles[index++] = (y * w) + x + 1;

                triangles[index++] = ((y + 1) * w) + x;
                triangles[index++] = ((y + 1) * w) + x + 1;
                triangles[index++] = (y * w) + x + 1;
            }
        }

        // Normal calculation
        CalculateNormals(vertices, triangles, normals);

        string fileName = $"{savePath}/{terrain.name}.obj";
        using (StreamWriter sw = new StreamWriter(fileName))
        {
            sw.WriteLine($"# Unity terrain OBJ file\n# File created at {System.DateTime.Now}");
            foreach (Vector3 v in vertices)
            {
                // Write OBJ vertices
                sw.WriteLine($"v {v.x} {v.y} {v.z}");
            }
            foreach (Vector3 n in normals)
            {
                // Write OBJ normals
                sw.WriteLine($"vn {n.x} {n.y} {n.z}");
            }
            foreach (Vector2 uv in uvs)
            {
                sw.WriteLine($"vt {uv.x} {uv.y}");
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sw.WriteLine($"f {triangles[i] + 1}/{triangles[i] + 1}/{triangles[i] + 1} {triangles[i + 1] + 1}/{triangles[i + 1] + 1}/{triangles[i + 1] + 1} {triangles[i + 2] + 1}/{triangles[i + 2] + 1}/{triangles[i + 2] + 1}");
            }
        }

        Debug.Log($"Saved {terrain.name} as OBJ to {fileName}");
    }


    private void CalculateNormals(Vector3[] vertices, int[] triangles, Vector3[] normals)
    {
        int[] triCount = new int[vertices.Length];
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];

            Vector3 side1 = vertices[v1] - vertices[v0];
            Vector3 side2 = vertices[v2] - vertices[v0];
            Vector3 normal = Vector3.Cross(side1, side2).normalized;

            // Rotate normal by 90 degrees around Y axis
            float rotatedX = -normal.z;
            float rotatedZ = -normal.x;

            normal = new Vector3(rotatedX, normal.y, rotatedZ);

            normals[v0] += normal;
            normals[v1] += normal;
            normals[v2] += normal;

            triCount[v0]++;
            triCount[v1]++;
            triCount[v2]++;
        }

        for (int i = 0; i < normals.Length; i++)
        {
            if (triCount[i] > 0)
            {
                normals[i] = (normals[i] / triCount[i]).normalized;
            }
        }
    }

}
