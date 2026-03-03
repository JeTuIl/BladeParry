using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays an audio clip when the attached Button is clicked. Uses OptionManager for SFX volume.
/// Add to any GameObject with a Button component.
/// </summary>
public class ButtonAudio : MonoBehaviour
{
    /// <summary>Clip to play on click.</summary>
    [SerializeField] private AudioClip audioClip;

    /// <summary>Cached or added AudioSource for PlayOneShot.</summary>
    private AudioSource _audioSource;

    /// <summary>Subscribes to the Button's onClick and caches or adds AudioSource (2D).</summary>
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 0f; // Full 2D, no spatialization
    }

    /// <summary>Plays the configured clip at current SFX volume when the button is clicked.</summary>
    private void OnButtonClicked()
    {
        if (audioClip == null)
            return;

        float volume = OptionManager.GetSfxVolumeFromPrefs();
        _audioSource.PlayOneShot(audioClip, volume);
    }
}
