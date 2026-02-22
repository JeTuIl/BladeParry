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

    /// <summary>Display name for UI or debug.</summary>
    public string DisplayName => displayName;

    /// <summary>Optional prefab for this enemy; null means use scene enemy.</summary>
    public GameObject OptionalPrefab => optionalPrefab;
}
