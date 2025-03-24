using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class SelectMeshRenderersTool
{
    [MenuItem("Tools/Select MeshRenderers %#&r")]
    public static void SelectMeshRenderers()
    {
        var selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            return;
        }
        
        var objectsWithMeshRenderer = new List<GameObject>();
        
        foreach (var go in selectedObjects)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null && !objectsWithMeshRenderer.Contains(go))
                objectsWithMeshRenderer.Add(go);
            
            var childMeshRenderers = go.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var childMr in childMeshRenderers)
            {
                if (!objectsWithMeshRenderer.Contains(childMr.gameObject))
                    objectsWithMeshRenderer.Add(childMr.gameObject);
            }
        }
        
        Selection.objects = objectsWithMeshRenderer.ToArray();
    }
}
