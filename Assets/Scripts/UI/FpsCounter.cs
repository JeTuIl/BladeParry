using UnityEngine;
using TMPro;

public class FpsCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _fpsText;

    [Tooltip("Smoothing factor for FPS display (higher = more responsive, lower = smoother).")]
    [SerializeField, Range(1f, 20f)] float _smoothSpeed = 8f;

    float _smoothedFps = 60f;

    void Update()
    {
        float instantFps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        _smoothedFps = Mathf.Lerp(_smoothedFps, instantFps, _smoothSpeed * Time.unscaledDeltaTime);

        if (_fpsText != null)
            _fpsText.text = Mathf.RoundToInt(_smoothedFps).ToString();
    }
}
