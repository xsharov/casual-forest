using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraGraphicSettingsController : MonoBehaviour
{
    [SerializeField] public CameraQualityConfig qualityConfig;
    [SerializeField] private Camera mainCamera;

    private void Awake()
    {
        if (mainCamera == null) return;
        
        QualitySettingsManager.OnQualityLevelChanged += ApplySettingsForQuality;
        ApplySettingsForQuality(QualitySettings.GetQualityLevel());
    }

    private void OnDestroy()
    {
        QualitySettingsManager.OnQualityLevelChanged -= ApplySettingsForQuality;
    }

    private void ApplySettingsForQuality(int qualityLevel)
    {
        if (qualityConfig == null)
        {
            return;
        }

        var settingsToApply = qualityLevel switch
        {
            0 => qualityConfig.low,
            1 => qualityConfig.medium,
            2 => qualityConfig.high,
            _ => qualityConfig.high
        };
        
        var urpData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
        if(urpData != null)
        {
            switch (settingsToApply.antiAliasing)
            {
                case CameraSettings.AntiAliasingMode.SMAA:
                    urpData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    urpData.antialiasingQuality = (AntialiasingQuality)settingsToApply.smaaQuality;
                    break;
                case CameraSettings.AntiAliasingMode.FXAA:
                    urpData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    break;
                case CameraSettings.AntiAliasingMode.None:
                default:
                    urpData.antialiasing = AntialiasingMode.None;
                    break;
            }
        }
        else
        {
            Debug.LogWarning("UniversalAdditionalCameraData not founded");
        }

        mainCamera.fieldOfView = settingsToApply.fieldOfView;
        mainCamera.allowHDR = settingsToApply.allowHDR;
        mainCamera.allowMSAA = settingsToApply.allowMSAA;
    }
}
