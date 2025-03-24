using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;

public class TerrainToFbxExporter : EditorWindow
{
    private Terrain selectedTerrain;
    private string exportPath = "Assets/TerrainExport.fbx";

    [MenuItem("Tools/Terrain To FBX Exporter")]
    public static void ShowWindow()
    {
        GetWindow<TerrainToFbxExporter>("Terrain To FBX Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Экспорт Terrain в FBX (Binary)", EditorStyles.boldLabel);
        selectedTerrain = EditorGUILayout.ObjectField("Terrain", selectedTerrain, typeof(Terrain), true) as Terrain;
        exportPath = EditorGUILayout.TextField("Путь экспорта", exportPath);

        if (GUILayout.Button("Экспортировать"))
        {
            if (selectedTerrain == null)
            {
                EditorUtility.DisplayDialog("Ошибка", "Пожалуйста, выберите объект Terrain!", "OK");
                return;
            }
            ExportTerrain();
        }
    }

    void ExportTerrain()
    {
        Mesh terrainMesh = ConvertTerrainToMesh(selectedTerrain);
        if (terrainMesh == null)
        {
            Debug.LogError("Не удалось создать меш из Terrain.");
            return;
        }

        GameObject tempGO = new GameObject("TempTerrainMesh");
        MeshFilter mf = tempGO.AddComponent<MeshFilter>();
        mf.sharedMesh = terrainMesh;
        tempGO.AddComponent<MeshRenderer>();

        // Устанавливаем формат экспорта FBX в бинарный
        
        ExportBinaryFBX(exportPath, tempGO);

        DestroyImmediate(tempGO);

        EditorUtility.DisplayDialog("Успех", "Terrain успешно экспортирован в бинарном формате по пути: " + exportPath, "OK");
    }
    
    private static void ExportBinaryFBX (string filePath, UnityEngine.Object singleObject)
    {
        // Find relevant internal types in Unity.Formats.Fbx.Editor assembly
        Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
        Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
        Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");

        // Instantiate a settings object instance
        MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
        object optionsInstance = optionsProperty.Invoke(null, null);

        // Change the export setting from ASCII to binary
        FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
        exportFormatField.SetValue(optionsInstance, 1);

        // Invoke the ExportObject method with the settings param
        MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
        exportObjectMethod.Invoke(null, new object[] { filePath, singleObject, optionsInstance });
    }

    Mesh ConvertTerrainToMesh(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int heightmapResolution = terrainData.heightmapResolution;
        Vector3 terrainSize = terrainData.size;

        int sampleFactor = 4; 
        int meshWidth = (heightmapResolution - 1) / sampleFactor + 1;
        int meshHeight = (heightmapResolution - 1) / sampleFactor + 1;

        Vector3[] vertices = new Vector3[meshWidth * meshHeight];
        Vector2[] uvs = new Vector2[meshWidth * meshHeight];

        for (int y = 0; y < meshHeight; y++)
        {
            for (int x = 0; x < meshWidth; x++)
            {
                int terrainX = x * sampleFactor;
                int terrainY = y * sampleFactor;
                float heightValue = terrainData.GetHeight(terrainX, terrainY);
                float xPos = (float)terrainX / (heightmapResolution - 1) * terrainSize.x;
                float zPos = (float)terrainY / (heightmapResolution - 1) * terrainSize.z;
                float yPos = heightValue;
                vertices[y * meshWidth + x] = new Vector3(xPos, yPos, zPos);
                uvs[y * meshWidth + x] = new Vector2((float)x / (meshWidth - 1), (float)y / (meshHeight - 1));
            }
        }

        int[] triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        int t = 0;
        for (int y = 0; y < meshHeight - 1; y++)
        {
            for (int x = 0; x < meshWidth - 1; x++)
            {
                int i = y * meshWidth + x;
                // Первый треугольник
                triangles[t++] = i;
                triangles[t++] = i + meshWidth;
                triangles[t++] = i + meshWidth + 1;
                // Второй треугольник
                triangles[t++] = i;
                triangles[t++] = i + meshWidth + 1;
                triangles[t++] = i + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "TerrainMesh";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
