using UnityEngine;
using UnityEngine.UI;

public class SpriteWinker : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField] float onPhaseDuration = 0.1f;
    [SerializeField] float offPhaseDuration = 0.1f;
    [SerializeField] float totalDuration = 1.0f;

    Coroutine _winkCoroutine;

    public void TriggerWink()
    {
        if (_winkCoroutine != null)
            StopCoroutine(_winkCoroutine);

        if (image != null)
            _winkCoroutine = StartCoroutine(WinkRoutine());
    }

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
