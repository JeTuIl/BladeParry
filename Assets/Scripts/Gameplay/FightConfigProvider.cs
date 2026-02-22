using UnityEngine;

/// <summary>
/// Singleton holder for the current fight config. Set before loading the fight scene so
/// GameplayLoopController uses it instead of the serialized config (roguelite / runtime encounters).
/// </summary>
public class FightConfigProvider : MonoBehaviour
{
    private static FightConfigProvider _instance;

    [SerializeField] private bool dontDestroyOnLoad = true;

    /// <summary>Config to use for the next fight; takes precedence over scene-assigned config.</summary>
    private FightConfig _currentFightConfig;

    /// <summary>Optional: set a GameplayConfig directly instead of a FightConfig (used as fallback).</summary>
    private GameplayConfig _currentGameplayConfigOverride;

    /// <summary>Singleton instance. Create in scene or via code if needed.</summary>
    public static FightConfigProvider Instance => _instance;

    /// <summary>Current fight config if set; otherwise null.</summary>
    public static FightConfig CurrentFightConfig => _instance != null ? _instance._currentFightConfig : null;

    /// <summary>Current gameplay config override if set (when no FightConfig is used).</summary>
    public static GameplayConfig CurrentGameplayConfigOverride => _instance != null ? _instance._currentGameplayConfigOverride : null;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>Sets the config for the next fight. Clear after the fight loads if you want scene config for the next time.</summary>
    public void SetFightConfig(FightConfig fightConfig)
    {
        _currentFightConfig = fightConfig;
        _currentGameplayConfigOverride = null;
    }

    /// <summary>Sets a gameplay config directly for the next fight (no FightConfig).</summary>
    public void SetGameplayConfig(GameplayConfig gameplayConfig)
    {
        _currentGameplayConfigOverride = gameplayConfig;
        _currentFightConfig = null;
    }

    /// <summary>Clears runtime config so the next fight uses scene-assigned config.</summary>
    public void Clear()
    {
        _currentFightConfig = null;
        _currentGameplayConfigOverride = null;
    }

    /// <summary>Resolves the GameplayConfig to use: provider FightConfig > provider GameplayConfig override > null (caller uses scene fallback).</summary>
    public static GameplayConfig GetConfigForFight(GameplayConfig sceneFallback)
    {
        if (_instance == null)
            return sceneFallback;
        if (_instance._currentFightConfig != null)
        {
            var gc = _instance._currentFightConfig.GetGameplayConfig();
            return gc != null ? gc : sceneFallback;
        }
        if (_instance._currentGameplayConfigOverride != null)
            return _instance._currentGameplayConfigOverride;
        return sceneFallback;
    }
}
