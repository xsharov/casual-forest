using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class BillboardGenerator : MonoBehaviour
{
    private const int Resolution = 512;
    private const string OutputFolder = "Assets/Billboards";

    [MenuItem("Tools/Generate Billboards")]
    public static void GenerateBillboardsMenu()
    {
        GenerateBillboards();
    }

    private static void GenerateBillboards()
    {
        var previousScene = EditorSceneManager.GetActiveScene();

        var sceneGuids = AssetDatabase.FindAssets("BillboardGenerator t:Scene");
        if (sceneGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "BillboardGenerator not founded!", "OK");
            return;
        }
        var targetScenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[0]);

        EditorSceneManager.OpenScene(targetScenePath);

        var selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            return;
        }

        var cam = GameObject.Find("Camera").GetComponent<Camera>();
        var root = GameObject.Find("Root");

        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Billboards");
        }

        var normalShader = Shader.Find("Hidden/NormalShader");
        if (normalShader == null)
        {
            EditorUtility.DisplayDialog("Error", "Shader 'Hidden/NormalShader' not founded.", "OK");
            return;
        }

        foreach (var obj in selectedObjects)
        {
            var assetPath = AssetDatabase.GetAssetPath(obj);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
                continue;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
            instance.transform.position = Vector3.zero;

            // Сохраняем оригинальные шейдеры
            var originalShaders = SaveOriginalShaders(instance);

            // Делаем снимок с обычными материалами
            var albedoTex = CaptureCamera(cam, Resolution, null);

            // Меняем шейдеры на NormalShader
            ChangeShaders(instance, normalShader);
            var normalTex = CaptureCamera(cam, Resolution, null);

            // Восстанавливаем оригинальные шейдеры
            RestoreOriginalShaders(instance, originalShaders);

            var albedoBytes = albedoTex.EncodeToPNG();
            var normalBytes = normalTex.EncodeToPNG();

            var baseName = prefab.name;
            var albedoPath = Path.Combine(OutputFolder, baseName + "_Albedo.png");
            var normalPath = Path.Combine(OutputFolder, baseName + "_Normal.png");

            File.WriteAllBytes(albedoPath, albedoBytes);
            File.WriteAllBytes(normalPath, normalBytes);

            AssetDatabase.Refresh();

            var normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (normalImporter != null)
            {
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.SaveAndReimport();
            }
            else
            {
                Debug.LogError($"Failed to get TextureImporter for {normalPath}");
            }

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(albedoTex);
            Object.DestroyImmediate(normalTex);
        }

        if (!string.IsNullOrEmpty(previousScene.path))
        {
            EditorSceneManager.OpenScene(previousScene.path);
        }
    }

    private static Dictionary<Material, Shader> SaveOriginalShaders(GameObject obj)
    {
        var originalShaders = new Dictionary<Material, Shader>();
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null)
                {
                    originalShaders[material] = material.shader;
                }
            }
        }
        
        return originalShaders;
    }

    private static void ChangeShaders(GameObject obj, Shader newShader)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.sharedMaterials)
            {
                if (material != null)
                {
                    material.shader = newShader;
                }
            }
        }
    }

    private static void RestoreOriginalShaders(GameObject obj, Dictionary<Material, Shader> originalShaders)
    {
        foreach (var kvp in originalShaders)
        {
            if (kvp.Key != null)
            {
                kvp.Key.shader = kvp.Value;
            }
        }
    }

    private static Texture2D CaptureCamera(Camera cam, int res, Shader replacementShader)
    {
        var rt = new RenderTexture(res, res, 24);
        cam.targetTexture = rt;
        
        if (replacementShader != null)
        {
            cam.SetReplacementShader(replacementShader, "");
        }

        cam.Render();


        RenderTexture.active = rt;
        var image = new Texture2D(res, res, TextureFormat.ARGB32, false);
        image.ReadPixels(new Rect(0, 0, res, res), 0, 0);
        image.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
        return image;
    }
}
