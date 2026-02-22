using UnityEngine;

/// <summary>
/// ScriptableObject describing which enemy to fight (name, optional prefab/sprite reference).
/// Used by FightConfig; can be expanded later for roguelite enemy variants.
/// </summary>
[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "BladeParry/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    [SerializeField] private string displayName = "Enemy";
    [Tooltip("Optional prefab to spawn for this enemy (e.g. different character).")]
    [SerializeField] private GameObject optionalPrefab;

    [Tooltip("Optional sprite set for this enemy (direction and state sprites + scales). When set, applied to the enemy's CharacterSpriteDirection at fight start.")]
    [SerializeField] private CharacterSpriteSet spriteSet;

    [Header("Fight FX (FxManager prefab indices)")]
    [Tooltip("Index of the effect spawned at the start of wind-down during each attack (position/orientation from CharacterAttaqueSequence).")]
    [SerializeField] private int windDownFxIndex = 0;

    [Tooltip("Index of the effect spawned when all attacks in a combo are parried (position from GameplayLoopController; orientation not used).")]
    [SerializeField] private int allParriedComboFxIndex = 3;

    /// <summary>Display name for UI or debug.</summary>
    public string DisplayName => displayName;

    /// <summary>Optional prefab for this enemy; null means use scene enemy.</summary>
    public GameObject OptionalPrefab => optionalPrefab;

    /// <summary>Optional sprite set; null means use prefab/scene defaults.</summary>
    public CharacterSpriteSet SpriteSet => spriteSet;

    /// <summary>True when this definition has a sprite set to apply.</summary>
    public bool HasSpriteConfig => spriteSet != null;

    /// <summary>FxManager prefab index for the effect at start of wind-down.</summary>
    public int WindDownFxIndex => windDownFxIndex;

    /// <summary>FxManager prefab index for the effect when all combo attacks are parried.</summary>
    public int AllParriedComboFxIndex => allParriedComboFxIndex;
}
