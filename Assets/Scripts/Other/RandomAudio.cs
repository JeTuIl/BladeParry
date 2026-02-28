using UnityEngine;

/// <summary>
/// Plays a random clip from a list via an AudioSource. Optionally plays once on Start.
/// </summary>
public class RandomAudio : MonoBehaviour
{
    /// <summary>Clips to choose from when playing random.</summary>
    [SerializeField] AudioClip[] _clips;

    /// <summary>If true, PlayRandom is called once in Start.</summary>
    [SerializeField] bool _playOnStart;

    /// <summary>AudioSource used to play the clips (PlayOneShot).</summary>
    [SerializeField] AudioSource _audioSource;

    /// <summary>
    /// If _playOnStart is true, plays a random clip once.
    /// </summary>
    void Start()
    {
        if (_playOnStart)
            PlayRandom();
    }

    /// <summary>
    /// Picks a random clip from _clips and plays it with PlayOneShot. No-op if clips or AudioSource is missing.
    /// </summary>
    public void PlayRandom()
    {
        if (_clips == null || _clips.Length == 0 || _audioSource == null)
            return;

        int index = Random.Range(0, _clips.Length);
        AudioClip clip = _clips[index];
        if (clip != null)
        {
            float volume = OptionManager.GetSfxVolumeFromPrefs();
            _audioSource.PlayOneShot(clip, volume);
        }
    }
}
