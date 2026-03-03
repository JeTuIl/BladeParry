using UnityEngine;

/// <summary>
/// Single ScriptableObject entry point for a fight: references GameplayConfig and optional EnemyDefinition.
/// Assign to GameplayLoopController or set on FightConfigProvider before loading the fight scene.
/// </summary>
[CreateAssetMenu(fileName = "FightConfig", menuName = "BladeParry/Fight Config")]
public class FightConfig : ScriptableObject
{
    /// <summary>Gameplay tuning (life, combo timings, pause) for this fight.</summary>
    [SerializeField] private GameplayConfig gameplayConfig;

    /// <summary>Optional enemy definition (e.g. for roguelite encounter selection).</summary>
    [Tooltip("Optional: which enemy to fight (for roguelite encounter selection).")]
    [SerializeField] private EnemyDefinition optionalEnemyDefinition;

    /// <summary>Optional music and pitch tuning for this fight.</summary>
    [Tooltip("Optional: music and pitch tuning for this fight.")]
    [SerializeField] private MusicConfig musicConfig;

    /// <summary>Optional environment (background) for this fight.</summary>
    [Tooltip("Optional: environment (background) for this fight.")]
    [SerializeField] private EnvironmentConfig environmentConfig;

    /// <summary>Gameplay tuning (life, combo timings, pause). Required.</summary>
    public GameplayConfig GetGameplayConfig() => gameplayConfig;

    /// <summary>Optional enemy definition (e.g. for swapping enemy visuals later).</summary>
    public EnemyDefinition OptionalEnemyDefinition => optionalEnemyDefinition;

    /// <summary>Optional music config (clip and pitch speeds).</summary>
    public MusicConfig GetMusicConfig() => musicConfig;

    /// <summary>Optional environment config (background).</summary>
    public EnvironmentConfig GetEnvironmentConfig() => environmentConfig;

    /// <summary>Sets the gameplay config (for runtime-created FightConfig).</summary>
    /// <param name="value">The gameplay config to use.</param>
    public void SetGameplayConfig(GameplayConfig value) => gameplayConfig = value;

    /// <summary>Sets the optional enemy definition (for runtime-created FightConfig).</summary>
    /// <param name="value">The enemy definition to use; can be null.</param>
    public void SetOptionalEnemyDefinition(EnemyDefinition value) => optionalEnemyDefinition = value;

    /// <summary>Sets the optional music config (for runtime-created FightConfig).</summary>
    /// <param name="value">The music config to use; can be null.</param>
    public void SetMusicConfig(MusicConfig value) => musicConfig = value;

    /// <summary>Sets the optional environment config (for runtime-created FightConfig).</summary>
    /// <param name="value">The environment config to use; can be null.</param>
    public void SetEnvironmentConfig(EnvironmentConfig value) => environmentConfig = value;
}
