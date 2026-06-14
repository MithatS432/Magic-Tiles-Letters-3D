using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GraphicsSettingsManager : MonoBehaviour
{
    public static GraphicsSettingsManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyAutoSettings();
    }

    private void ApplyAutoSettings()
    {
        int ram = SystemInfo.systemMemorySize;
        int gpuMem = SystemInfo.graphicsMemorySize;
        int cores = SystemInfo.processorCount;

        if (ram < 3000 || gpuMem < 1500 || cores <= 4)
        {
            SetLow();
        }
        else if (ram < 6000 || gpuMem < 3000)
        {
            SetMedium();
        }
        else
        {
            SetHigh();
        }

        Application.targetFrameRate = 60;
    }

    public void SetLow()
    {
        QualitySettings.SetQualityLevel(0, true);
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowDistance = 10f;
        QualitySettings.particleRaycastBudget = 8;
    }

    public void SetMedium()
    {
        QualitySettings.SetQualityLevel(1, true);
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowDistance = 25f;
        QualitySettings.particleRaycastBudget = 16;
    }

    public void SetHigh()
    {
        QualitySettings.SetQualityLevel(2, true);
        QualitySettings.shadows = ShadowQuality.All;
        QualitySettings.shadowDistance = 50f;
        QualitySettings.particleRaycastBudget = 32;
    }
}