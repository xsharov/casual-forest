using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LightProbePlacementTool : EditorWindow
{
    private Terrain _targetTerrain;
    private float _probeSpacing = 5f;
    private int _layerCount = 3;
    private float _terrainOffset = 0.5f;

    [MenuItem("Tools/Light Probe Placement Tool")]
    public static void ShowWindow()
    {
        GetWindow<LightProbePlacementTool>("Light Probe Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("LightProbes settings", EditorStyles.boldLabel);

        _targetTerrain = EditorGUILayout.ObjectField("Terrain", _targetTerrain, typeof(Terrain), true) as Terrain;
        _probeSpacing = EditorGUILayout.FloatField("Space between probes", _probeSpacing);
        _layerCount = EditorGUILayout.IntField("Layers Count", _layerCount);
        _terrainOffset = EditorGUILayout.FloatField("Terrain offset", _terrainOffset);

        if (!GUILayout.Button("Place")) return;
        if (_targetTerrain == null)
        {
            EditorUtility.DisplayDialog("Error", "Terrain doesn't selected", "OK");
            return;
        }
        PlaceLightProbes();
    }

    void PlaceLightProbes()
    {
        var terrainData = _targetTerrain.terrainData;
        var terrainPos = _targetTerrain.transform.position;
        var terrainWidth = terrainData.size.x;
        var terrainLength = terrainData.size.z;

        var probePositions = new List<Vector3>();

        var xSteps = Mathf.CeilToInt(terrainWidth / _probeSpacing);
        var zSteps = Mathf.CeilToInt(terrainLength / _probeSpacing);

        var rayHeight = terrainPos.y + terrainData.size.y + 100f;

        for (var x = 0; x <= xSteps; x++)
        {
            for (var z = 0; z <= zSteps; z++)
            {
                var posX = terrainPos.x + x * _probeSpacing;
                var posZ = terrainPos.z + z * _probeSpacing;
                var rayOrigin = new Vector3(posX, rayHeight, posZ);
                var ray = new Ray(rayOrigin, Vector3.down);

                if (!Physics.Raycast(ray, out RaycastHit hit, rayHeight - terrainPos.y + 10f)) continue;
                if (hit.collider.gameObject != _targetTerrain.gameObject) continue;
                var basePosition = hit.point + new Vector3(0, _terrainOffset, 0);

                for (var layer = 0; layer < _layerCount; layer++)
                {
                    var probePos = basePosition + new Vector3(0, layer * _probeSpacing, 0);
                    probePositions.Add(probePos);
                }
            }
        }

        var probeGroupObj = new GameObject("Light Probe Group");
        var probeGroup = probeGroupObj.AddComponent<LightProbeGroup>();
        probeGroup.probePositions = probePositions.ToArray();

        Selection.activeGameObject = probeGroupObj;
    }
}
