using System.Collections;
using UnityEngine;

/// <summary>
/// Plays squash and stretch scale animations on a target transform for parry success or hit feedback.
/// </summary>
public class SquashStretchAnimator : MonoBehaviour
{
    /// <summary>Transform to scale (e.g. character Image or a parent of it).</summary>
    [SerializeField] private Transform scaleTarget;

    [Header("Parry success")]
    /// <summary>Duration in seconds of the parry success scale animation.</summary>
    [SerializeField] private float parryDuration = 0.12f;

    /// <summary>Easing curve for the parry success animation (time 0-1).</summary>
    [SerializeField] private AnimationCurve parryCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    /// <summary>Scale sequence: start, squash, overshoot, end (e.g. 1, 0.85, 1.1, 1).</summary>
    [Tooltip("Scale sequence: start, squash, overshoot, end (e.g. 1, 0.85, 1.1, 1).")]
    [SerializeField] private float[] parryScaleKeys = { 1f, 0.85f, 1.1f, 1f };

    [Header("Hit")]
    /// <summary>Duration in seconds of the hit scale animation.</summary>
    [SerializeField] private float hitDuration = 0.15f;

    /// <summary>Easing curve for the hit animation (time 0-1).</summary>
    [SerializeField] private AnimationCurve hitCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    /// <summary>Scale sequence: start, squash, end (e.g. 1, 0.9, 1).</summary>
    [Tooltip("Scale sequence: start, squash, end (e.g. 1, 0.9, 1).")]
    [SerializeField] private float[] hitScaleKeys = { 1f, 0.9f, 1f };

    /// <summary>Currently running scale animation coroutine; null when idle.</summary>
    private Coroutine _activeCoroutine;

    /// <summary>
    /// Plays the parry success animation (scale down then bounce back).
    /// </summary>
    public void PlayParrySuccess()
    {
        if (scaleTarget == null) return;
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(AnimateCoroutine(parryDuration, parryCurve, parryScaleKeys));
    }

    /// <summary>
    /// Plays the hit animation (squash then recover).
    /// </summary>
    public void PlayHit()
    {
        if (scaleTarget == null) return;
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(AnimateCoroutine(hitDuration, hitCurve, hitScaleKeys));
    }

    /// <summary>
    /// Interpolates through the scale keyframes over duration using the curve, then resets scale to 1.
    /// </summary>
    /// <param name="duration">Total animation duration in seconds.</param>
    /// <param name="curve">Easing curve (0-1); null uses linear.</param>
    /// <param name="scaleKeys">Scale values to interpolate between (at least 2).</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator AnimateCoroutine(float duration, AnimationCurve curve, float[] scaleKeys)
    {
        if (scaleKeys == null || scaleKeys.Length < 2)
        {
            _activeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        int keyCount = scaleKeys.Length;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = curve != null ? curve.Evaluate(t) : t;
            // Map eased time to segment index and local t for lerp between consecutive keys
            int segment = Mathf.Min((int)(eased * (keyCount - 1)), keyCount - 2);
            float localT = (eased * (keyCount - 1)) - segment;
            float scale = Mathf.Lerp(scaleKeys[segment], scaleKeys[segment + 1], localT);
            scaleTarget.localScale = Vector3.one * scale;
            yield return null;
        }

        scaleTarget.localScale = Vector3.one;
        _activeCoroutine = null;
    }
}
