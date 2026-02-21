using System.Collections;
using UnityEngine;

public class MusciSwitchManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private MusicPitchManager musicPitchManager;

    private Coroutine _switchCoroutine;

    private void Awake()
    {
        if (musicPitchManager == null)
            musicPitchManager = GetComponent<MusicPitchManager>();
    }

    public void SwitchMusic(AudioClip newClip, float transitionDuration, bool shouldLoop = true)
    {
        if (_switchCoroutine != null)
            StopCoroutine(_switchCoroutine);

        _switchCoroutine = StartCoroutine(SwitchMusicCoroutine(newClip, transitionDuration, shouldLoop));
    }

    private IEnumerator SwitchMusicCoroutine(AudioClip newClip, float transitionDuration, bool shouldLoop)
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            ApplyNewClipAndPlay(newClip, shouldLoop);
            _switchCoroutine = null;
            yield break;
        }

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        // Fade-out phase
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / transitionDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        ApplyNewClipAndPlay(newClip, shouldLoop);
        audioSource.volume = startVolume;

        _switchCoroutine = null;
    }

    private void ApplyNewClipAndPlay(AudioClip newClip, bool shouldLoop)
    {
        audioSource.clip = newClip;
        audioSource.loop = shouldLoop;
        if (musicPitchManager != null)
            musicPitchManager.pitch = 1.0f;
        else
            audioSource.pitch = 1.0f;
        audioSource.Play();
    }
}
