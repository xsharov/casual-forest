using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class QualitySettingsManager
{
    public static event Action<int> OnQualityLevelChanged;

    private static int _currentQualityLevel = QualitySettings.GetQualityLevel();

    public static void SetQualityLevel(int newQualityLevel, bool applyExpensiveChanges = true)
    {
        if (newQualityLevel != _currentQualityLevel)
        {
            QualitySettings.SetQualityLevel(newQualityLevel, applyExpensiveChanges);
            _currentQualityLevel = newQualityLevel;
            OnQualityLevelChanged?.Invoke(_currentQualityLevel);
        }
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Quality/Change to Low")]
    public static void ChangeToLow()
    {
        SetQualityLevel(0);
        Debug.Log("Quality set to Low");
    }

    [MenuItem("Tools/Quality/Change to Mid")]
    public static void ChangeToMid()
    {
        SetQualityLevel(1);
        Debug.Log("Quality set to Medium");
    }

    [MenuItem("Tools/Quality/Change to High")]
    public static void ChangeToHigh()
    {
        SetQualityLevel(2);
        Debug.Log("Quality set to High");
    }
#endif
}