using System.Collections;
using UnityEngine;

/// <summary>
/// Intensity level for screen shake effects (distance and duration).
/// </summary>
public enum ScreenShakeStrength
{
    /// <summary>Light shake; short distance and duration.</summary>
    Low,

    /// <summary>Moderate shake.</summary>
    Medium,

    /// <summary>Strong shake; large distance and longer duration.</summary>
    High
}

/// <summary>
/// Singleton that applies screen shake by offsetting a RectTransform and smoothly returning it.
/// </summary>
public class ScreenshackManager : MonoBehaviour
{
    /// <summary>Global singleton instance of the screen shake manager.</summary>
    public static ScreenshackManager Instance { get; private set; }

    /// <summary>RectTransform whose anchored position is offset for the shake effect.</summary>
    [SerializeField] private RectTransform targetRectTransform;

    /// <summary>Offset distance (units) for low strength shake.</summary>
    [Header("Shake distance (units)")]
    [SerializeField] private float shakeDistanceLow = 5f;

    /// <summary>Offset distance (units) for medium strength shake.</summary>
    [SerializeField] private float shakeDistanceMedium = 15f;

    /// <summary>Offset distance (units) for high strength shake.</summary>
    [SerializeField] private float shakeDistanceHigh = 30f;

    /// <summary>Duration in seconds for low strength shake return.</summary>
    [Header("Shake duration (seconds)")]
    [SerializeField] private float shakeDurationLow = 0.1f;

    /// <summary>Duration in seconds for medium strength shake return.</summary>
    [SerializeField] private float shakeDurationMedium = 0.2f;

    /// <summary>Duration in seconds for high strength shake return.</summary>
    [SerializeField] private float shakeDurationHigh = 0.35f;

    /// <summary>Coroutine running the current shake return; null when idle.</summary>
    private Coroutine _activeShakeCoroutine;

    /// <summary>Cached rest position of the target RectTransform before shake.</summary>
    private Vector2 _originalTargetPosition;

    /// <summary>
    /// Registers this instance as the singleton and caches the target's initial position.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (targetRectTransform != null)
            _originalTargetPosition = targetRectTransform.anchoredPosition;
    }

    /// <summary>Seed for Perlin noise (keeps shake reproducible per trigger).</summary>
    private static float _perlinSeed;

    /// <summary>
    /// Triggers a screen shake at the given strength using multi-frame Perlin noise.
    /// </summary>
    /// <param name="strength">Strength level determining shake distance and duration.</param>
    public void TriggerScreenShake(ScreenShakeStrength strength)
    {
        if (OptionManager.Instance != null && !OptionManager.Instance.GetScreenEffectsEnabled())
            return;
        if (targetRectTransform == null)
        {
            Debug.LogWarning("[ScreenshackManager] targetRectTransform is not assigned.");
            return;
        }

        if (_activeShakeCoroutine != null)
            StopCoroutine(_activeShakeCoroutine);

        GetStrengthParams(strength, out float distance, out float duration);
        _perlinSeed = UnityEngine.Random.Range(0f, 1000f);
        _activeShakeCoroutine = StartCoroutine(PerlinShakeRoutine(distance, duration));
    }

    /// <summary>
    /// Gets the distance and duration parameters for a given shake strength.
    /// </summary>
    /// <param name="strength">The shake strength.</param>
    /// <param name="distance">Output: offset distance in units.</param>
    /// <param name="duration">Output: duration in seconds.</param>
    private void GetStrengthParams(ScreenShakeStrength strength, out float distance, out float duration)
    {
        switch (strength)
        {
            case ScreenShakeStrength.Low:
                distance = shakeDistanceLow;
                duration = shakeDurationLow;
                break;
            case ScreenShakeStrength.Medium:
                distance = shakeDistanceMedium;
                duration = shakeDurationMedium;
                break;
            case ScreenShakeStrength.High:
                distance = shakeDistanceHigh;
                duration = shakeDurationHigh;
                break;
            default:
                distance = shakeDistanceMedium;
                duration = shakeDurationMedium;
                break;
        }
    }

    /// <summary>
    /// Multi-frame Perlin shake: applies noise-based offset that decays over duration.
    /// </summary>
    /// <param name="distance">Max offset distance in units.</param>
    /// <param name="duration">Shake duration in seconds.</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator PerlinShakeRoutine(float distance, float duration)
    {
        float elapsed = 0f;
        Vector2 original = _originalTargetPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float decay = 1f - (t * t);
            float time = _perlinSeed + elapsed * 25f;
            float nx = (Mathf.PerlinNoise(time, _perlinSeed) - 0.5f) * 2f;
            float ny = (Mathf.PerlinNoise(_perlinSeed + 100f, time) - 0.5f) * 2f;
            Vector2 offset = new Vector2(nx, ny).normalized * distance * decay;
            if (offset.sqrMagnitude < 0.01f)
                offset = Vector2.zero;
            targetRectTransform.anchoredPosition = original + offset;
            yield return null;
        }

        targetRectTransform.anchoredPosition = original;
        _activeShakeCoroutine = null;
    }
}
