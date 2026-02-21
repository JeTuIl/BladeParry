using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a life bar (green/fill) and a trailing red bar that lerps toward the current value for a damage feedback effect.
/// </summary>
public class LifebarManager : MonoBehaviour
{
    /// <summary>Image used as the main (current) life bar fill.</summary>
    [SerializeField] private Image lifeBarImage;

    /// <summary>Image used as the trailing red/damage bar.</summary>
    [SerializeField] private Image redBarImage;

    /// <summary>Maximum life value (denominator for fill ratio).</summary>
    [SerializeField] private int maxLifeValue = 100;

    /// <summary>Current life value (numerator for fill ratio).</summary>
    [SerializeField] private int currentLifeValue = 100;

    /// <summary>Width in units when the bar is at full fill.</summary>
    [SerializeField] private float barSpriteMaxWidth = 200f;

    /// <summary>Lerp speed for the red bar moving toward the current value.</summary>
    [SerializeField] private float redBarLerpSpeed = 5f;

    /// <summary>Maximum life (used as denominator for fill).</summary>
    public int MaxLifeValue { get => maxLifeValue; set => maxLifeValue = value; }

    /// <summary>Current life (used for fill; red bar lerps toward this).</summary>
    public int CurrentLifeValue { get => currentLifeValue; set => currentLifeValue = value; }

    /// <summary>Current width of the red bar (lerped each frame).</summary>
    private float _currentRedBarWidth;

    /// <summary>
    /// Initializes the red bar width from the RectTransform if assigned.
    /// </summary>
    void Start()
    {
        if (redBarImage != null)
            _currentRedBarWidth = redBarImage.rectTransform.sizeDelta.x;
    }

    /// <summary>
    /// Updates the life bar width from current life and lerps the red bar toward that width.
    /// </summary>
    void Update()
    {
        if (lifeBarImage == null || redBarImage == null)
            return;

        if (maxLifeValue <= 0)
            return;

        int clampedLife = Mathf.Clamp(currentLifeValue, 0, maxLifeValue);
        float targetWidth = (clampedLife / (float)maxLifeValue) * barSpriteMaxWidth;

        SetBarWidth(lifeBarImage.rectTransform, targetWidth);

        _currentRedBarWidth = Mathf.Lerp(_currentRedBarWidth, targetWidth, redBarLerpSpeed * Time.deltaTime);
        SetBarWidth(redBarImage.rectTransform, _currentRedBarWidth);
    }

    /// <summary>
    /// Sets the width of the given RectTransform's sizeDelta (x only).
    /// </summary>
    /// <param name="rectTransform">The RectTransform to update.</param>
    /// <param name="width">Desired width.</param>
    private static void SetBarWidth(RectTransform rectTransform, float width)
    {
        Vector2 size = rectTransform.sizeDelta;
        size.x = width;
        rectTransform.sizeDelta = size;
    }
}
