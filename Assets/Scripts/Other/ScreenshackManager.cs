using System.Collections;
using UnityEngine;

public enum ScreenShakeStrength
{
    Low,
    Medium,
    High
}

public class ScreenshackManager : MonoBehaviour
{
    public static ScreenshackManager Instance { get; private set; }

    [SerializeField] private RectTransform targetRectTransform;

    [Header("Shake distance (units)")]
    [SerializeField] private float shakeDistanceLow = 5f;
    [SerializeField] private float shakeDistanceMedium = 15f;
    [SerializeField] private float shakeDistanceHigh = 30f;

    [Header("Shake duration (seconds)")]
    [SerializeField] private float shakeDurationLow = 0.1f;
    [SerializeField] private float shakeDurationMedium = 0.2f;
    [SerializeField] private float shakeDurationHigh = 0.35f;

    private Coroutine _activeShakeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void TriggerScreenShake(ScreenShakeStrength strength)
    {
        if (targetRectTransform == null)
        {
            Debug.LogWarning("[ScreenshackManager] targetRectTransform is not assigned.");
            return;
        }

        if (_activeShakeCoroutine != null)
            StopCoroutine(_activeShakeCoroutine);

        GetStrengthParams(strength, out float distance, out float duration);
        Vector2 original = targetRectTransform.anchoredPosition;

        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir.sqrMagnitude < 0.01f)
            dir = Vector2.up;

        targetRectTransform.anchoredPosition = original + dir * distance;

        _activeShakeCoroutine = StartCoroutine(ShakeReturnRoutine(original, duration));
    }

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

    private IEnumerator ShakeReturnRoutine(Vector2 originalPosition, float duration)
    {
        Vector2 startPosition = targetRectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            targetRectTransform.anchoredPosition = Vector2.Lerp(startPosition, originalPosition, smoothT);
            yield return null;
        }

        targetRectTransform.anchoredPosition = originalPosition;
        _activeShakeCoroutine = null;
    }
}
