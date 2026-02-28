using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Displays a life bar (main fill) and a trailing red bar that lerps toward the current value—always shown when redBarImage is assigned.
/// Optional: color-by-health gradient, damage flash/pulse, eased motion, and numeric HP display.
/// </summary>
public class LifebarManager : MonoBehaviour
{
    [Header("Core References")]
    /// <summary>Image used as the main (current) life bar fill.</summary>
    [SerializeField] private Image lifeBarImage;

    /// <summary>Trailing red bar: lerps toward current value to show recent damage. Always used when assigned.</summary>
    [SerializeField] private Image redBarImage;

    [Header("Core Settings")]
    /// <summary>Maximum life value (denominator for fill ratio).</summary>
    [SerializeField] private float maxLifeValue = 100f;

    /// <summary>Current life value (numerator for fill ratio).</summary>
    [SerializeField] private float currentLifeValue = 100f;

    /// <summary>Width in units when the bar is at full fill.</summary>
    [SerializeField] private float barSpriteMaxWidth = 200f;

    /// <summary>Lerp speed for the red bar moving toward the current value (used when eased motion is off).</summary>
    [SerializeField] private float redBarLerpSpeed = 5f;

    [Header("Color by Health")]
    [SerializeField] private bool useColorByHealth = true;
    [Tooltip("Linear gradient: 0 = empty (e.g. red), 1 = full (e.g. green). Thresholds and stops are configured in the gradient.")]
    [SerializeField] private Gradient healthGradient;

    [Header("Damage Feedback")]
    [SerializeField] private bool useDamageFeedback = true;
    [SerializeField] private float damageFeedbackDuration = 0.2f;
    [Tooltip("Curve for scale pulse: 0 = start (pulse), 1 = end (normal). Y: 0 = scaled up, 1 = scale 1.")]
    [SerializeField] private AnimationCurve damageFeedbackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float damagePulseScaleMax = 1.08f;

    [Header("Eased Motion")]
    [SerializeField] private bool useEasedMotion = true;
    [Tooltip("Higher = faster catch-up. Used for both fill and trailing bar when eased motion is on.")]
    [SerializeField] private float easeSpeed = 8f;

    [Header("Numeric Display")]
    [SerializeField] private bool showNumeric = false;
    [SerializeField] private TMP_Text numericText;
    [Tooltip("Format: {0} = current, {1} = max. E.g. \"{0} / {1}\" or \"{0}\" only. Overridden by localization when available.")]
    [SerializeField] private string numericFormat = "{0} / {1}";

    private static readonly LocalizedString s_lifebarFormat = new LocalizedString("BladeParry_LocalizationTable", "UI_LifebarFormat");
    private string _cachedLifebarFormat;

    /// <summary>Maximum life (used as denominator for fill).</summary>
    public float MaxLifeValue { get => maxLifeValue; set { maxLifeValue = value; RefreshNumeric(); } }

    /// <summary>Current life (used for fill; red bar lerps toward this).</summary>
    public float CurrentLifeValue { get => currentLifeValue; set { currentLifeValue = value; RefreshNumeric(); } }

    /// <summary>Current width of the fill bar (smoothed when eased motion is on).</summary>
    private float _currentFillWidth;

    /// <summary>Current width of the red bar (lerped each frame).</summary>
    private float _currentRedBarWidth;

    private Coroutine _damageFeedbackCoroutine;

    void OnDestroy()
    {
        if (_damageFeedbackCoroutine != null)
            StopCoroutine(_damageFeedbackCoroutine);
    }

    /// <summary>
    /// Initializes bar widths from current life and red bar RectTransform.
    /// </summary>
    void Start()
    {
        if (maxLifeValue > 0)
        {
            float targetWidth = (Mathf.Clamp(currentLifeValue, 0f, maxLifeValue) / maxLifeValue) * barSpriteMaxWidth;
            _currentFillWidth = targetWidth;
        }
        if (redBarImage != null)
            _currentRedBarWidth = redBarImage.rectTransform.sizeDelta.x;
        StartCoroutine(PreloadFormat());
        RefreshNumeric();
    }

    private IEnumerator PreloadFormat()
    {
        var op = s_lifebarFormat.GetLocalizedStringAsync();
        if (!op.IsDone)
            yield return op;
        if (op.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(op.Result))
            _cachedLifebarFormat = op.Result;
        RefreshNumeric();
    }

    /// <summary>
    /// Call when this entity took damage; triggers a one-shot damage feedback (flash/pulse) if enabled.
    /// </summary>
    public void NotifyDamage()
    {
        if (!useDamageFeedback || lifeBarImage == null)
            return;
        if (_damageFeedbackCoroutine != null)
            StopCoroutine(_damageFeedbackCoroutine);
        _damageFeedbackCoroutine = StartCoroutine(DamageFeedbackCoroutine());
    }

    private IEnumerator DamageFeedbackCoroutine()
    {
        RectTransform pulseTarget = lifeBarImage.rectTransform;
        Vector3 baseScale = pulseTarget.localScale;
        float elapsed = 0f;

        while (elapsed < damageFeedbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / damageFeedbackDuration);
            float curveValue = damageFeedbackCurve.Evaluate(t);
            float scale = Mathf.Lerp(damagePulseScaleMax, 1f, curveValue);
            pulseTarget.localScale = new Vector3(baseScale.x * scale, baseScale.y * scale, baseScale.z);
            yield return null;
        }

        pulseTarget.localScale = baseScale;
        _damageFeedbackCoroutine = null;
    }

    /// <summary>
    /// Updates the life bar width from current life, applies color-by-health, and lerps the red bar toward that width.
    /// </summary>
    void Update()
    {
        if (lifeBarImage == null || redBarImage == null)
            return;

        if (maxLifeValue <= 0)
            return;

        float clampedLife = Mathf.Clamp(currentLifeValue, 0f, maxLifeValue);
        float targetWidth = (clampedLife / maxLifeValue) * barSpriteMaxWidth;

        if (useEasedMotion)
        {
            float smooth = 1f - Mathf.Exp(-easeSpeed * Time.deltaTime);
            _currentFillWidth = Mathf.Lerp(_currentFillWidth, targetWidth, smooth);
            SetBarWidth(lifeBarImage.rectTransform, _currentFillWidth);

            _currentRedBarWidth = Mathf.Lerp(_currentRedBarWidth, targetWidth, smooth);
            SetBarWidth(redBarImage.rectTransform, _currentRedBarWidth);
        }
        else
        {
            SetBarWidth(lifeBarImage.rectTransform, targetWidth);
            _currentRedBarWidth = Mathf.Lerp(_currentRedBarWidth, targetWidth, redBarLerpSpeed * Time.deltaTime);
            SetBarWidth(redBarImage.rectTransform, _currentRedBarWidth);
        }

        if (useColorByHealth && healthGradient != null)
        {
            float ratio = clampedLife / maxLifeValue;
            lifeBarImage.color = healthGradient.Evaluate(ratio);
        }
    }

    private void Reset()
    {
        if (healthGradient == null)
        {
            healthGradient = new Gradient();
            healthGradient.SetKeys(
                new[] { new GradientColorKey(new Color(0.95f, 0.2f, 0.2f), 0f), new GradientColorKey(new Color(0.95f, 0.9f, 0.2f), 0.5f), new GradientColorKey(new Color(0.2f, 0.9f, 0.2f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
            );
        }
    }

    private void RefreshNumeric()
    {
        if (!showNumeric || numericText == null)
            return;
        string format = !string.IsNullOrEmpty(_cachedLifebarFormat) ? _cachedLifebarFormat : numericFormat;
        try
        {
            numericText.text = string.Format(format, currentLifeValue, maxLifeValue);
        }
        catch (System.Exception)
        {
            numericText.text = $"{currentLifeValue} / {maxLifeValue}";
        }
    }

    /// <summary>
    /// Sets the width of the given RectTransform's sizeDelta (x only).
    /// </summary>
    private static void SetBarWidth(RectTransform rectTransform, float width)
    {
        Vector2 size = rectTransform.sizeDelta;
        size.x = width;
        rectTransform.sizeDelta = size;
    }
}
