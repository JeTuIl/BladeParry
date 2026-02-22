using UnityEngine;

/// <summary>
/// Sets the application target frame rate at startup. Fixes Android (and other platforms) being capped at 30 FPS
/// when Unity uses default or display-based limits. Runs before the first scene loads.
/// </summary>
public static class FrameRateSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ConfigureFrameRate()
    {
        // 60 FPS cap; use -1 for uncapped (may increase battery use on mobile).
        Application.targetFrameRate = 60;

        // Disable VSync so targetFrameRate is respected (avoids 30 FPS when device uses half-refresh VSync on mobile).
        QualitySettings.vSyncCount = 0;
    }
}
