using UnityEngine;

/// <summary>
/// Single ScriptableObject entry point for a fight: references GameplayConfig and optional EnemyDefinition.
/// Assign to GameplayLoopController or set on FightConfigProvider before loading the fight scene.
/// </summary>
[CreateAssetMenu(fileName = "FightConfig", menuName = "BladeParry/Fight Config")]
public class FightConfig : ScriptableObject
{
    [SerializeField] private GameplayConfig gameplayConfig;
    [Tooltip("Optional: which enemy to fight (for roguelite encounter selection).")]
    [SerializeField] private EnemyDefinition optionalEnemyDefinition;

    /// <summary>Gameplay tuning (life, combo timings, pause). Required.</summary>
    public GameplayConfig GetGameplayConfig() => gameplayConfig;

    /// <summary>Optional enemy definition (e.g. for swapping enemy visuals later).</summary>
    public EnemyDefinition OptionalEnemyDefinition => optionalEnemyDefinition;

    /// <summary>Sets the gameplay config (for runtime-created FightConfig).</summary>
    public void SetGameplayConfig(GameplayConfig value) => gameplayConfig = value;

    /// <summary>Sets the optional enemy definition (for runtime-created FightConfig).</summary>
    public void SetOptionalEnemyDefinition(EnemyDefinition value) => optionalEnemyDefinition = value;
}
