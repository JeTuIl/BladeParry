using System.Collections;
using UnityEngine;

/// <summary>
/// Switches the current music clip with a fade-out transition, then plays the new clip.
/// Optionally uses MusicPitchManager to reset pitch when switching.
/// </summary>
public class MusciSwitchManager : MonoBehaviour
{
    /// <summary>AudioSource that plays the music.</summary>
    [SerializeField] private AudioSource audioSource;

    /// <summary>Optional pitch manager; if set, pitch is reset to 1 when switching.</summary>
    [SerializeField] private MusicPitchManager musicPitchManager;

    /// <summary>Active switch coroutine; null when no transition is running.</summary>
    private Coroutine _switchCoroutine;

    /// <summary>
    /// Tries to find MusicPitchManager on this GameObject if not assigned.
    /// </summary>
    private void Awake()
    {
        if (musicPitchManager == null)
            musicPitchManager = GetComponent<MusicPitchManager>();
    }

    /// <summary>
    /// Starts a transition: fades out current clip over transitionDuration, then plays the new clip.
    /// If no clip is playing, applies the new clip immediately.
    /// </summary>
    /// <param name="newClip">Clip to play after the transition.</param>
    /// <param name="transitionDuration">Fade-out duration in seconds.</param>
    /// <param name="shouldLoop">Whether the new clip should loop. Default is true.</param>
    public void SwitchMusic(AudioClip newClip, float transitionDuration, bool shouldLoop = true)
    {
        if (_switchCoroutine != null)
            StopCoroutine(_switchCoroutine);

        _switchCoroutine = StartCoroutine(SwitchMusicCoroutine(newClip, transitionDuration, shouldLoop));
    }

    /// <summary>
    /// Fades out the current clip, then applies and plays the new clip and restores volume.
    /// </summary>
    /// <param name="newClip">Clip to play after fade-out.</param>
    /// <param name="transitionDuration">Fade-out duration in seconds.</param>
    /// <param name="shouldLoop">Whether the new clip should loop.</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator SwitchMusicCoroutine(AudioClip newClip, float transitionDuration, bool shouldLoop)
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            ApplyNewClipAndPlay(newClip, shouldLoop);
            _switchCoroutine = null;
            yield break;
        }

        float startVolume = GetMusicVolumeFromOptions();
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
        audioSource.volume = GetMusicVolumeFromOptions();

        _switchCoroutine = null;
    }

    private static float GetMusicVolumeFromOptions()
    {
        return OptionManager.Instance != null ? OptionManager.Instance.GetMusicVolume() : 1f;
    }

    /// <summary>
    /// Assigns the new clip to the AudioSource, sets loop, resets pitch via MusicPitchManager if present, and plays.
    /// </summary>
    /// <param name="newClip">Clip to assign and play.</param>
    /// <param name="shouldLoop">Whether to set loop on the AudioSource.</param>
    private void ApplyNewClipAndPlay(AudioClip newClip, bool shouldLoop)
    {
        audioSource.clip = newClip;
        audioSource.loop = shouldLoop;
        if (musicPitchManager != null)
            musicPitchManager.pitch = 1.0f;
        else
            audioSource.pitch = 1.0f;
        audioSource.volume = GetMusicVolumeFromOptions();
        audioSource.Play();
    }
}
