using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;

/// <summary>
/// Options panel UI controller: binds sliders and toggles to OptionManager (music, SFX, haptic, screen effects)
/// and updates language indicator images when the locale changes.
/// </summary>
public class OptionUiManager : MonoBehaviour
{
    /// <summary>Image used as the English language indicator (alpha 1 when selected, 0.5 otherwise).</summary>
    [SerializeField] private Image imageEnglish;

    /// <summary>Image used as the French language indicator (alpha 1 when selected, 0.5 otherwise).</summary>
    [SerializeField] private Image imageFrench;

    /// <summary>Image used as the Spanish language indicator (alpha 1 when selected, 0.5 otherwise).</summary>
    [SerializeField] private Image imageSpanish;

    /// <summary>Slider for music volume (0..1). Value is synced from OptionManager at Start and written back on change.</summary>
    [SerializeField] private Slider musicVolumeSlider;

    /// <summary>AudioSource for menu music; volume is set from the music slider.</summary>
    [SerializeField] private AudioSource musicAudioSource;

    /// <summary>Slider for SFX volume (0..1). Value is synced from OptionManager at Start and written back on change.</summary>
    [SerializeField] private Slider sfxVolumeSlider;

    /// <summary>Toggle for haptic feedback. State is synced from OptionManager at Start and written back on change.</summary>
    [SerializeField] private Toggle hapticToggle;

    /// <summary>Toggle for screen effects (e.g. camera shake). State is synced from OptionManager at Start and written back on change.</summary>
    [SerializeField] private Toggle screenEffectsToggle;

    /// <summary>Loads current options from OptionManager into sliders/toggles and updates language indicator alphas.</summary>
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
    /// <remarks>Wire to the Slider's onValueChanged in the Inspector.</remarks>
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
    /// <remarks>Wire to the Slider's onValueChanged in the Inspector.</remarks>
    public void OnSfxVolumeSliderChanged()
    {
        if (sfxVolumeSlider == null || OptionManager.Instance == null)
            return;
        OptionManager.Instance.SetSfxVolume(sfxVolumeSlider.value);
    }

    /// <summary>Call from the haptic Toggle's On Value Changed. Applies the toggle value to OptionManager.</summary>
    /// <remarks>Wire to the Toggle's onValueChanged in the Inspector.</remarks>
    public void OnHapticToggleChanged()
    {
        if (hapticToggle == null || OptionManager.Instance == null)
            return;
        OptionManager.Instance.SetHapticEnabled(hapticToggle.isOn);
    }

    /// <summary>Call from the screen effects Toggle's On Value Changed. Applies the toggle value to OptionManager.</summary>
    /// <remarks>Wire to the Toggle's onValueChanged in the Inspector.</remarks>
    public void OnScreenEffectsToggleChanged()
    {
        if (screenEffectsToggle == null || OptionManager.Instance == null)
            return;
        OptionManager.Instance.SetScreenEffectsEnabled(screenEffectsToggle.isOn);
    }

    /// <summary>Updates language indicators and subscribes to SelectedLocaleChanged.</summary>
    private void OnEnable()
    {
        UpdateLanguageIndicatorAlphas();
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    /// <summary>Unsubscribes from SelectedLocaleChanged.</summary>
    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    /// <summary>Handler for locale change: refreshes language indicator alphas.</summary>
    /// <param name="locale">The newly selected locale (not used; we read current from OptionManager).</param>
    private void OnSelectedLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        UpdateLanguageIndicatorAlphas();
    }

    /// <summary>Sets each language flag image alpha to 1 for the current language and 0.5 for others.</summary>
    private void UpdateLanguageIndicatorAlphas()
    {
        string currentCode = OptionManager.GetLanguageCodeFromPrefs();

        SetImageAlphaForLanguage(imageEnglish, "en", currentCode);
        SetImageAlphaForLanguage(imageFrench, "fr", currentCode);
        SetImageAlphaForLanguage(imageSpanish, "es", currentCode);
    }

    /// <summary>Sets the image alpha to 1 if languageCode matches currentCode, otherwise 0.5.</summary>
    /// <param name="image">Image to update; no-op if null.</param>
    /// <param name="languageCode">Language code this image represents (e.g. "en").</param>
    /// <param name="currentCode">Currently selected language code from options.</param>
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
