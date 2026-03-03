using UnityEngine;
using TMPro;

/// <summary>
/// Displays smoothed frames per second in a TextMeshProUGUI. Updates every frame using unscaled delta time.
/// </summary>
public class FpsCounter : MonoBehaviour
{
    /// <summary>Text component to display the FPS value.</summary>
    [SerializeField] private TextMeshProUGUI _fpsText;

    /// <summary>Smoothing factor for FPS display (higher = more responsive, lower = smoother).</summary>
    [Tooltip("Smoothing factor for FPS display (higher = more responsive, lower = smoother).")]
    [SerializeField, Range(1f, 20f)] private float _smoothSpeed = 8f;

    /// <summary>Smoothed FPS value used for display.</summary>
    private float _smoothedFps = 60f;

    /// <summary>Computes instant FPS, lerps smoothed value, and updates the text.</summary>
    private void Update()
    {
        float instantFps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        _smoothedFps = Mathf.Lerp(_smoothedFps, instantFps, _smoothSpeed * Time.unscaledDeltaTime);

        if (_fpsText != null)
            _fpsText.text = Mathf.RoundToInt(_smoothedFps).ToString();
    }
}
