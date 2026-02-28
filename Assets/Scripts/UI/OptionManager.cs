using System.Reflection;
using UnityEngine;
using UnityEngine.Localization.Settings;

/// <summary>
/// Singleton that manages game options: music volume, SFX volume, haptic feedback,
/// language (en/fr/es), and screen effects. Persists via PlayerPrefs and applies
/// effects at load and when options change.
/// </summary>
public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance { get; private set; }

    private const string PrefsPrefix = "BladeParry_";
    private const string KeyMusicVolume = PrefsPrefix + "MusicVolume";
    private const string KeySfxVolume = PrefsPrefix + "SfxVolume";
    private const string KeyHapticEnabled = PrefsPrefix + "HapticEnabled";
    private const string KeyLanguageCode = PrefsPrefix + "LanguageCode";
    private const string KeyScreenEffectsEnabled = PrefsPrefix + "ScreenEffectsEnabled";

    private const float DefaultMusicVolume = 1f;
    private const float DefaultSfxVolume = 1f;
    private const bool DefaultHapticEnabled = true;
    private const string DefaultLanguageCode = "en";
    private const bool DefaultScreenEffectsEnabled = true;

    private float _musicVolume;
    private float _sfxVolume;
    private bool _hapticEnabled;
    private string _languageCode;
    private bool _screenEffectsEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadAllOptions();
        ApplyGlobalOptions();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyOptionsAtStartup()
    {
        ApplyGlobalOptionsFromPrefs();
    }

    /// <summary>Reads options from PlayerPrefs and applies locale and screen effects. Call at startup or when OptionManager is not in the scene.</summary>
    public static void ApplyGlobalOptionsFromPrefs()
    {
        string languageCode = PlayerPrefs.GetString(KeyLanguageCode, DefaultLanguageCode);
        bool screenEffectsEnabled = PlayerPrefs.GetInt(KeyScreenEffectsEnabled, DefaultScreenEffectsEnabled ? 1 : 0) != 0;
        ApplyLocale(languageCode);
        ApplyScreenEffectsGlobal(screenEffectsEnabled);
    }

    /// <summary>Returns music volume from PlayerPrefs (0..1).</summary>
    public static float GetMusicVolumeFromPrefs() => PlayerPrefs.GetFloat(KeyMusicVolume, DefaultMusicVolume);
    /// <summary>Returns SFX volume from PlayerPrefs (0..1).</summary>
    public static float GetSfxVolumeFromPrefs() => PlayerPrefs.GetFloat(KeySfxVolume, DefaultSfxVolume);
    /// <summary>Returns haptic enabled from PlayerPrefs.</summary>
    public static bool GetHapticEnabledFromPrefs() => PlayerPrefs.GetInt(KeyHapticEnabled, DefaultHapticEnabled ? 1 : 0) != 0;
    /// <summary>Returns language code from PlayerPrefs (e.g. "en", "fr").</summary>
    public static string GetLanguageCodeFromPrefs() => PlayerPrefs.GetString(KeyLanguageCode, DefaultLanguageCode);
    /// <summary>Returns screen effects enabled from PlayerPrefs.</summary>
    public static bool GetScreenEffectsEnabledFromPrefs() => PlayerPrefs.GetInt(KeyScreenEffectsEnabled, DefaultScreenEffectsEnabled ? 1 : 0) != 0;

    /// <summary>Loads all options from PlayerPrefs with defaults for missing keys.</summary>
    private void LoadAllOptions()
    {
        _musicVolume = PlayerPrefs.GetFloat(KeyMusicVolume, DefaultMusicVolume);
        _sfxVolume = PlayerPrefs.GetFloat(KeySfxVolume, DefaultSfxVolume);
        _hapticEnabled = PlayerPrefs.GetInt(KeyHapticEnabled, DefaultHapticEnabled ? 1 : 0) != 0;
        _languageCode = PlayerPrefs.GetString(KeyLanguageCode, DefaultLanguageCode);
        _screenEffectsEnabled = PlayerPrefs.GetInt(KeyScreenEffectsEnabled, DefaultScreenEffectsEnabled ? 1 : 0) != 0;
    }

    /// <summary>Applies options that have global side effects (locale, CFXR camera shake).</summary>
    private void ApplyGlobalOptions()
    {
        ApplyLocale(_languageCode);
        ApplyScreenEffectsGlobal(_screenEffectsEnabled);
    }

    private static void ApplyLocale(string code)
    {
        if (LocalizationSettings.AvailableLocales == null)
            return;
        var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
        if (locale != null)
            LocalizationSettings.SelectedLocale = locale;
        else if (code != DefaultLanguageCode)
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale(DefaultLanguageCode);
    }

    private static void ApplyScreenEffectsGlobal(bool enabled)
    {
        SetCFXRGlobalDisableCameraShake(!enabled);
    }

    private static void SetCFXRGlobalDisableCameraShake(bool disable)
    {
        var type = System.Type.GetType("CFXR_Effect, CFXRRuntime");
        if (type == null)
            return;
        var field = type.GetField("GlobalDisableCameraShake", BindingFlags.Public | BindingFlags.Static);
        if (field != null && field.FieldType == typeof(bool))
            field.SetValue(null, disable);
    }

    // ----- Music volume -----
    public float GetMusicVolume() => _musicVolume;
    public void SetMusicVolume(float value)
    {
        _musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeyMusicVolume, _musicVolume);
        PlayerPrefs.Save();
    }

    // ----- SFX volume -----
    public float GetSfxVolume() => _sfxVolume;
    public void SetSfxVolume(float value)
    {
        _sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeySfxVolume, _sfxVolume);
        PlayerPrefs.Save();
    }

    // ----- Haptic feedback -----
    public bool GetHapticEnabled() => _hapticEnabled;
    public void SetHapticEnabled(bool value)
    {
        _hapticEnabled = value;
        PlayerPrefs.SetInt(KeyHapticEnabled, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ----- Language -----
    public string GetLanguageCode() => _languageCode;
    public void SetLanguageByCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            code = DefaultLanguageCode;
        _languageCode = code;
        PlayerPrefs.SetString(KeyLanguageCode, _languageCode);
        PlayerPrefs.Save();
        ApplyLocale(_languageCode);
    }

    // ----- Screen effects -----
    public bool GetScreenEffectsEnabled() => _screenEffectsEnabled;
    public void SetScreenEffectsEnabled(bool value)
    {
        _screenEffectsEnabled = value;
        PlayerPrefs.SetInt(KeyScreenEffectsEnabled, value ? 1 : 0);
        PlayerPrefs.Save();
        ApplyScreenEffectsGlobal(_screenEffectsEnabled);
    }
}
