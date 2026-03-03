using UnityEngine;
using TMPro;

/// <summary>
/// Displays the current number of perfect parries in a row. Text color follows a gradient by count;
/// text position trembles with strength and speed scaled by count (same max as gradient).
/// </summary>
public class PerfectParryComboDisplay : MonoBehaviour
{
    /// <summary>Text that displays the perfect parry count.</summary>
    [SerializeField] private TMP_Text text;

    [Header("Gradient")]
    /// <summary>Max count at which gradient and tremble are at full. Same value used for both.</summary>
    [Tooltip("Max count at which gradient and tremble are at full. Same value used for both.")]
    [SerializeField] private int maxComboForGradient = 5;

    /// <summary>Color at 0 = count 0, 1 = count maxComboForGradient.</summary>
    [Tooltip("Color at 0 = count 0, 1 = count maxComboForGradient.")]
    [SerializeField] private Gradient colorGradient;

    [Header("Tremble")]
    /// <summary>Position offset magnitude in pixels when count = max.</summary>
    [Tooltip("Position offset magnitude in pixels when count = max.")]
    [SerializeField] private float trembleStrengthAtMax = 4f;

    /// <summary>Tremble speed (rad/s) when count = max.</summary>
    [Tooltip("Tremble speed (rad/s) when count = max.")]
    [SerializeField] private float trembleSpeedAtMax = 30f;

    /// <summary>Current perfect parry count displayed.</summary>
    private int _count;

    /// <summary>Cached RectTransform of the text for tremble offset.</summary>
    private RectTransform _rect;

    /// <summary>Base anchored position when enabled (tremble applied on top).</summary>
    private Vector2 _baseAnchoredPosition;

    /// <summary>Current tremble phase in radians.</summary>
    private float _tremblePhase;

    /// <summary>Caches text RectTransform and ensures default gradient if null.</summary>
    private void Awake()
    {
        if (text != null)
            _rect = text.rectTransform;
        if (colorGradient == null)
        {
            colorGradient = new Gradient();
            colorGradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.9f, 0.2f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        }
    }

    /// <summary>Stores base anchored position and resets tremble phase.</summary>
    private void OnEnable()
    {
        if (_rect != null)
            _baseAnchoredPosition = _rect.anchoredPosition;
        _tremblePhase = 0f;
    }

    /// <summary>
    /// Sets the current perfect parry count and updates the displayed number. Gradient and tremble use this with maxComboForGradient.
    /// </summary>
    /// <param name="count">Perfect parries in a row (0 or greater).</param>
    public void SetPerfectParryCount(int count)
    {
        _count = Mathf.Max(0, count);
        if (text != null)
            text.text = _count.ToString();
    }

    /// <summary>Updates text color from gradient and applies tremble offset to position.</summary>
    private void Update()
    {
        if (text == null || _rect == null)
            return;

        int max = Mathf.Max(1, maxComboForGradient);
        float t = Mathf.Clamp01((float)_count / max);

        if (colorGradient != null)
            text.color = colorGradient.Evaluate(t);

        float strength = t * trembleStrengthAtMax;
        float speed = t * trembleSpeedAtMax;
        _tremblePhase += speed * Time.deltaTime;

        float ox = Mathf.Sin(_tremblePhase) * strength;
        float oy = Mathf.Cos(_tremblePhase * 1.3f) * strength;
        _rect.anchoredPosition = _baseAnchoredPosition + new Vector2(ox, oy);
    }
}
