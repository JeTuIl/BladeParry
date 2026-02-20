using UnityEngine;
using UnityEngine.Audio;

public class MusicPitchManager : MonoBehaviour
{


    [SerializeField] private AudioSource audioSource;
     
    private AudioMixerGroup mixerGroup;

    public float pitch = 1.0f;
    private float previousPitch = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mixerGroup = audioSource.outputAudioMixerGroup;
    }

    // Update is called once per frame
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
