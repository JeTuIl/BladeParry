using UnityEngine;

public class RandomAudio : MonoBehaviour
{
    [SerializeField] AudioClip[] _clips;
    [SerializeField] bool _playOnStart;
    [SerializeField] AudioSource _audioSource;

    void Start()
    {
        if (_playOnStart)
            PlayRandom();
    }

    public void PlayRandom()
    {
        if (_clips == null || _clips.Length == 0 || _audioSource == null)
            return;

        int index = Random.Range(0, _clips.Length);
        AudioClip clip = _clips[index];
        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }
}
