using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Drives the attached AudioSource pitch and an AudioMixer "PitchShifter" float parameter from a single pitch value.
/// </summary>
public class MusicPitchManager : MonoBehaviour
{
    /// <summary>AudioSource whose pitch is controlled.</summary>
    [SerializeField] private AudioSource audioSource;

    /// <summary>Cached output mixer group from the AudioSource.</summary>
    private AudioMixerGroup mixerGroup;

    /// <summary>Current pitch; applied to AudioSource and to mixer "PitchShifter" as 1/pitch.</summary>
    public float pitch = 1.0f;

    /// <summary>Last applied pitch value to detect changes.</summary>
    private float previousPitch = 0;

    /// <summary>
    /// Caches the AudioSource's output AudioMixerGroup for Update.
    /// </summary>
    void Start()
    {
        mixerGroup = audioSource.outputAudioMixerGroup;
    }

    /// <summary>
    /// When pitch changes, applies it to the AudioSource and sets the mixer "PitchShifter" parameter to 1/pitch.
    /// </summary>
    void Update()
    {
        if(previousPitch != pitch)
        {
            previousPitch = pitch;
            audioSource.pitch = pitch;
            mixerGroup.audioMixer.SetFloat("PitchShifter", 1f/pitch);
        }
    }
}
