using UnityEngine;
using UnityEngine.UI;

public class ButtonAudio : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;

    private AudioSource _audioSource;

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

    private void OnButtonClicked()
    {
        if (audioClip == null)
            return;

        float volume = OptionManager.GetSfxVolumeFromPrefs();
        _audioSource.PlayOneShot(audioClip, volume);
    }
}
