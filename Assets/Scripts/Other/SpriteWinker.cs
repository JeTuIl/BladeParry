using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Blinks an Image on and off for a set total duration when TriggerWink is called.
/// </summary>
public class SpriteWinker : MonoBehaviour
{
    /// <summary>Image to enable/disable during the wink.</summary>
    [SerializeField] Image image;

    /// <summary>Duration in seconds the image is visible per on-phase.</summary>
    [SerializeField] float onPhaseDuration = 0.1f;

    /// <summary>Duration in seconds the image is hidden per off-phase.</summary>
    [SerializeField] float offPhaseDuration = 0.1f;

    /// <summary>Total duration of the wink in seconds (on/off cycles until this is reached).</summary>
    [SerializeField] float totalDuration = 1.0f;

    /// <summary>Active wink coroutine; null when not winking.</summary>
    Coroutine _winkCoroutine;

    /// <summary>
    /// Starts the wink routine: alternates image enabled/disabled for totalDuration, then leaves it enabled.
    /// Stops any running wink first.
    /// </summary>
    public void TriggerWink()
    {
        if (_winkCoroutine != null)
            StopCoroutine(_winkCoroutine);

        if (image != null)
            _winkCoroutine = StartCoroutine(WinkRoutine());
    }

    /// <summary>
    /// Alternates image.enabled on/off using onPhaseDuration and offPhaseDuration until totalDuration is reached.
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
    System.Collections.IEnumerator WinkRoutine()
    {
        float elapsed = 0f;
        bool isOn = true;
        image.enabled = true;

        while (elapsed < totalDuration)
        {
            if (isOn)
            {
                yield return new WaitForSeconds(onPhaseDuration);
                elapsed += onPhaseDuration;
                if (elapsed >= totalDuration) break;
                image.enabled = false;
                isOn = false;
            }
            else
            {
                yield return new WaitForSeconds(offPhaseDuration);
                elapsed += offPhaseDuration;
                if (elapsed >= totalDuration) break;
                image.enabled = true;
                isOn = true;
            }
        }

        image.enabled = true;
        _winkCoroutine = null;
    }
}
