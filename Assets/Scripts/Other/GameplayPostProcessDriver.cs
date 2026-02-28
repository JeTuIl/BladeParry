using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Drives post-processing (vignette, chromatic aberration) from gameplay: low-life intensity, a brief pulse on missed parry,
/// and chromatic aberration that lerps between max-life and zero-life values.
/// </summary>
public class GameplayPostProcessDriver : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [Tooltip("Vignette intensity when player life ratio is below threshold.")]
    [SerializeField] private float vignetteIntensityOnLowLife = 0.4f;
    [Tooltip("Player life ratio below which vignette kicks in (e.g. 0.35).")]
    [SerializeField] [Range(0f, 1f)] private float lowLifeThreshold = 0.35f;
    [Tooltip("Brief vignette intensity when the player takes a hit.")]
    [SerializeField] private float vignettePulseOnMiss = 0.5f;
    [Tooltip("How fast vignette recovers toward target (low-life or 0).")]
    [SerializeField] private float vignetteRecoverySpeed = 3f;

    [Header("Chromatic Aberration (by life)")]
    [Tooltip("Chromatic aberration intensity when player is at full life.")]
    [SerializeField] [Range(0f, 1f)] private float chromaticAberrationAtMaxLife = 0f;
    [Tooltip("Chromatic aberration intensity when player is at zero life.")]
    [SerializeField] [Range(0f, 1f)] private float chromaticAberrationAtZeroLife = 0.5f;

    /// <summary>Optional: source of player current/max life for low-life vignette. If null, only miss pulse is used.</summary>
    [SerializeField] private LifebarManager playerLifebarManager;

    private float _currentVignetteIntensity;
    private float _pulseRemaining;

    /// <summary>
    /// Call when the player takes damage (missed parry) to trigger a brief vignette pulse.
    /// </summary>
    public void NotifyPlayerDamaged()
    {
        _pulseRemaining = 0.4f;
        _currentVignetteIntensity = Mathf.Max(_currentVignetteIntensity, vignettePulseOnMiss);
    }

    private void Update()
    {
        if (volume == null || volume.profile == null)
            return;

        if (!OptionManager.GetScreenEffectsEnabledFromPrefs())
        {
            ApplyScreenEffectsDisabled();
            return;
        }

        if (!volume.profile.TryGet<Vignette>(out Vignette vignette))
            return;

        float targetFromLife = 0f;
        if (playerLifebarManager != null && playerLifebarManager.MaxLifeValue > 0)
        {
            float ratio = playerLifebarManager.CurrentLifeValue / playerLifebarManager.MaxLifeValue;
            if (ratio < lowLifeThreshold)
                targetFromLife = Mathf.Lerp(vignetteIntensityOnLowLife, 0f, ratio / lowLifeThreshold);
        }

        if (_pulseRemaining > 0f)
        {
            _pulseRemaining -= Time.deltaTime;
            float target = Mathf.Max(targetFromLife, vignettePulseOnMiss);
            _currentVignetteIntensity = Mathf.MoveTowards(_currentVignetteIntensity, target, Time.deltaTime * vignetteRecoverySpeed * 2f);
        }
        else
        {
            float target = targetFromLife;
            _currentVignetteIntensity = Mathf.MoveTowards(_currentVignetteIntensity, target, Time.deltaTime * vignetteRecoverySpeed);
        }

        vignette.active = true;
        vignette.intensity.Override(_currentVignetteIntensity);

        // Chromatic aberration: lerp between max-life and zero-life intensity by current life ratio
        if (volume.profile.TryGet<ChromaticAberration>(out ChromaticAberration chromaticAberration))
        {
            float lifeRatio = 1f;
            if (playerLifebarManager != null && playerLifebarManager.MaxLifeValue > 0)
                lifeRatio = playerLifebarManager.CurrentLifeValue / playerLifebarManager.MaxLifeValue;
            float caIntensity = Mathf.Lerp(chromaticAberrationAtZeroLife, chromaticAberrationAtMaxLife, lifeRatio);
            chromaticAberration.active = true;
            chromaticAberration.intensity.Override(caIntensity);
        }
    }

    private void ApplyScreenEffectsDisabled()
    {
        if (volume.profile.TryGet<Vignette>(out Vignette vignette))
        {
            vignette.active = true;
            vignette.intensity.Override(0f);
        }
        if (volume.profile.TryGet<ChromaticAberration>(out ChromaticAberration chromaticAberration))
        {
            chromaticAberration.active = true;
            chromaticAberration.intensity.Override(0f);
        }
        if (volume.profile.TryGet<LensDistortion>(out LensDistortion lensDistortion))
        {
            lensDistortion.active = true;
            lensDistortion.intensity.Override(0f);
        }
    }
}
