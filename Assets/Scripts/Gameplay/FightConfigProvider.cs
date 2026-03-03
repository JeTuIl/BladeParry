using UnityEngine;

/// <summary>
/// Singleton holder for the current fight config. Set before loading the fight scene so
/// GameplayLoopController uses it instead of the serialized config (roguelite / runtime encounters).
/// </summary>
public class FightConfigProvider : MonoBehaviour
{
    /// <summary>Singleton instance.</summary>
    private static FightConfigProvider _instance;

    /// <summary>When true, the provider persists across scene loads.</summary>
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

    /// <summary>Enforces singleton; optionally marks object DontDestroyOnLoad.</summary>
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

    /// <summary>Clears singleton reference when this instance is destroyed.</summary>
    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>Sets the config for the next fight. Clear after the fight loads if you want scene config for the next time.</summary>
    /// <param name="fightConfig">The fight config to use; can be null to clear.</param>
    public void SetFightConfig(FightConfig fightConfig)
    {
        _currentFightConfig = fightConfig;
        _currentGameplayConfigOverride = null;
    }

    /// <summary>Sets a gameplay config directly for the next fight (no FightConfig).</summary>
    /// <param name="gameplayConfig">The gameplay config to use; can be null to clear.</param>
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

    /// <summary>Resolves the GameplayConfig to use: provider FightConfig > provider GameplayConfig override > scene fallback.</summary>
    /// <param name="sceneFallback">Config to return when provider has no override set.</param>
    /// <returns>The config to use for the fight, or sceneFallback if none set.</returns>
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
