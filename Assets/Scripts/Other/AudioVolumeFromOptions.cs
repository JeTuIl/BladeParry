using UnityEngine;

/// <summary>
/// Multiplies the attached AudioSource's volume by the chosen option (Music or SFX)
/// from OptionManager. Add to a GameObject with an AudioSource and pick either
/// MusicVolume or SfxVolume in the inspector.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioVolumeFromOptions : MonoBehaviour
{
    public enum VolumeType
    {
        Music,
        Sfx
    }

    [SerializeField] private AudioSource audioSource;
    [Tooltip("Base volume (0..1); final volume = baseVolume × option value.")]
    [Range(0f, 1f)] [SerializeField] private float baseVolume = 1f;
    [SerializeField] private VolumeType volumeType = VolumeType.Music;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        ApplyVolume();
    }

    private void OnEnable()
    {
        ApplyVolume();
    }

    /// <summary>Call to refresh volume from current options (e.g. after user changes volume in options menu).</summary>
    public void RefreshVolume()
    {
        ApplyVolume();
    }

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
