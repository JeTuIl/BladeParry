using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

public class OptionUiManager : MonoBehaviour
{
    [SerializeField] private Image imageEnglish;
    [SerializeField] private Image imageFrench;
    [SerializeField] private Image imageSpanish;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle hapticToggle;
    [SerializeField] private Toggle screenEffectsToggle;

    private void Start()
    {
        UpdateLanguageIndicatorAlphas();
        float musicVolume = OptionManager.GetMusicVolumeFromPrefs();
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;
        if (musicAudioSource != null)
            musicAudioSource.volume = musicVolume;
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = OptionManager.GetSfxVolumeFromPrefs();
        if (hapticToggle != null)
            hapticToggle.isOn = OptionManager.GetHapticEnabledFromPrefs();
        if (screenEffectsToggle != null)
            screenEffectsToggle.isOn = OptionManager.GetScreenEffectsEnabledFromPrefs();
    }

    /// <summary>Call from the music volume Slider's On Value Changed. Applies the slider value to OptionManager and the music AudioSource.</summary>
    public void OnMusicVolumeSliderChanged()
    {
        if (musicVolumeSlider == null || OptionManager.Instance == null)
            return;
        float value = musicVolumeSlider.value;
        OptionManager.Instance.SetMusicVolume(value);
        if (musicAudioSource != null)
            musicAudioSource.volume = value;
    }

    /// <summary>Call from the SFX volume Slider's On Value Changed. Applies the slider value to OptionManager.</summary>
    public void OnSfxVolumeSliderChanged()
    {
        if (sfxVolumeSlider == null || OptionManager.Instance == null)
            return;
        OptionManager.Instance.SetSfxVolume(sfxVolumeSlider.value);
    }

    /// <summary>Call from the haptic Toggle's On Value Changed. Applies the toggle value to OptionManager.</summary>
    public void OnHapticToggleChanged()
    {
        if (hapticToggle == null || OptionManager.Instance == null)
            return;
        OptionManager.Instance.SetHapticEnabled(hapticToggle.isOn);
    }

    /// <summary>Call from the screen effects Toggle's On Value Changed. Applies the toggle value to OptionManager.</summary>
    public void OnScreenEffectsToggleChanged()
    {
        if (screenEffectsToggle == null || OptionManager.Instance == null)
            return;
        OptionManager.Instance.SetScreenEffectsEnabled(screenEffectsToggle.isOn);
    }

    private void OnEnable()
    {
        UpdateLanguageIndicatorAlphas();
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    private void OnSelectedLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateLanguageIndicatorAlphas();
    }

    private void UpdateLanguageIndicatorAlphas()
    {
        string currentCode = OptionManager.GetLanguageCodeFromPrefs();

        SetImageAlphaForLanguage(imageEnglish, "en", currentCode);
        SetImageAlphaForLanguage(imageFrench, "fr", currentCode);
        SetImageAlphaForLanguage(imageSpanish, "es", currentCode);
    }

    private static void SetImageAlphaForLanguage(Image image, string languageCode, string currentCode)
    {
        if (image == null)
            return;
        float alpha = languageCode == currentCode ? 1f : 0.5f;
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}
