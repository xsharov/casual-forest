using UnityEngine;

[CreateAssetMenu(fileName = "CameraQualityConfig", menuName = "Config/CameraQualityConfig", order = 1)]
public class CameraQualityConfig : ScriptableObject
{
    public CameraSettings low;
    public CameraSettings medium;
    public CameraSettings high;
}

[System.Serializable]
public class CameraSettings
{
    public enum AntiAliasingMode {
        None,
        FXAA,
        SMAA
    }

    public enum SMAAQualityLevel {
        Low,
        Medium,
        High
    }
    [Tooltip("FOV")]
    public float fieldOfView = 60f;
    [Tooltip("AntiAliasingMode")]
    public AntiAliasingMode antiAliasing = AntiAliasingMode.None;
    [Tooltip("AntiAliasing Quality(for smaa)")]
    public SMAAQualityLevel smaaQuality = SMAAQualityLevel.Medium;
    [Tooltip("HDR")]
    public bool allowHDR = true;
    [Tooltip("MSAA")]
    public bool allowMSAA = true;
}


