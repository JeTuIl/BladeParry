using UnityEngine;

/// <summary>
/// Multiplies the attached AudioSource's volume by the chosen option (Music or SFX)
/// from OptionManager. Add to a GameObject with an AudioSource and pick either
/// MusicVolume or SfxVolume in the inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioVolumeFromOptions : MonoBehaviour
{
    /// <summary>Which option to use for volume multiplier (Music or SFX).</summary>
    public enum VolumeType
    {
        /// <summary>Use music volume from options.</summary>
        Music,

        /// <summary>Use SFX volume from options.</summary>
        Sfx
    }

    /// <summary>AudioSource to control; auto-fetched if null.</summary>
    [SerializeField] private AudioSource audioSource;

    /// <summary>Base volume (0..1); final volume = baseVolume × option value.</summary>
    [Tooltip("Base volume (0..1); final volume = baseVolume × option value.")]
    [Range(0f, 1f)] [SerializeField] private float baseVolume = 1f;

    /// <summary>Whether to use music or SFX volume from OptionManager.</summary>
    [SerializeField] private VolumeType volumeType = VolumeType.Music;

    /// <summary>Caches AudioSource from component if not assigned.</summary>
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    /// <summary>Applies volume from options at start.</summary>
    private void Start()
    {
        ApplyVolume();
    }

    /// <summary>Applies volume from options when enabled (e.g. when object is re-enabled).</summary>
    private void OnEnable()
    {
        ApplyVolume();
    }

    /// <summary>Call to refresh volume from current options (e.g. after user changes volume in options menu).</summary>
    public void RefreshVolume()
    {
        ApplyVolume();
    }

    /// <summary>Sets the AudioSource volume to baseVolume × current option value.</summary>
    private void ApplyVolume()
    {
        if (audioSource == null)
            return;
        float optionVolume = volumeType == VolumeType.Music
            ? OptionManager.GetMusicVolumeFromPrefs()
            : OptionManager.GetSfxVolumeFromPrefs();
        audioSource.volume = baseVolume * optionVolume;
    }
}
