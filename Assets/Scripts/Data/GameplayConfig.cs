using UnityEngine;

/// <summary>
/// ScriptableObject holding gameplay tuning: starting life, combo counts and timings (full life vs empty life), and pause between combos.
/// </summary>
[CreateAssetMenu(fileName = "GameplayConfig", menuName = "BladeParry/Gameplay Config")]
public class GameplayConfig : ScriptableObject
{
    /// <summary>Player's starting life count.</summary>
    [Header("Life")]
    [SerializeField] private int playerStartLife = 3;

    /// <summary>Enemy's starting life count.</summary>
    [SerializeField] private int enemyStartLife = 3;

    /// <summary>Number of attacks in a combo when enemy is at full life.</summary>
    [Header("Full Life Combo")]
    [SerializeField] private int fullLifeComboNumberOfAttaques = 3;

    /// <summary>Seconds between attacks in a full-life combo.</summary>
    [SerializeField] private float fullLifeDurationBetweenAttaque = 0.2f;

    /// <summary>Wind-up duration per attack at full life.</summary>
    [SerializeField] private float fullLifeWindUpDuration = 0.3f;

    /// <summary>Wind-down duration per attack at full life.</summary>
    [SerializeField] private float fullLifeWindDownDuration = 0.2f;

    /// <summary>Number of attacks in a combo when enemy is at empty life.</summary>
    [Header("Empty Life Combo")]
    [SerializeField] private int emptyLifeComboNumberOfAttaques = 2;

    /// <summary>Seconds between attacks in an empty-life combo.</summary>
    [SerializeField] private float emptyLifeDurationBetweenAttaque = 0.25f;

    /// <summary>Wind-up duration per attack at empty life.</summary>
    [SerializeField] private float emptyLifeWindUpDuration = 0.35f;

    /// <summary>Wind-down duration per attack at empty life.</summary>
    [SerializeField] private float emptyLifeWindDownDuration = 0.25f;

    /// <summary>Seconds to wait between combos.</summary>
    [Header("Between Combos")]
    [SerializeField] private float pauseBetweenComboDuration = 3.0f;

    /// <summary>Gets the player's starting life.</summary>
    public int PlayerStartLife => playerStartLife;

    /// <summary>Gets the enemy's starting life.</summary>
    public int EnemyStartLife => enemyStartLife;

    /// <summary>Gets the number of attacks in a combo at full enemy life.</summary>
    public int FullLifeComboNumberOfAttaques => fullLifeComboNumberOfAttaques;

    /// <summary>Gets the duration between attacks at full enemy life.</summary>
    public float FullLifeDurationBetweenAttaque => fullLifeDurationBetweenAttaque;

    /// <summary>Gets the wind-up duration at full enemy life.</summary>
    public float FullLifeWindUpDuration => fullLifeWindUpDuration;

    /// <summary>Gets the wind-down duration at full enemy life.</summary>
    public float FullLifeWindDownDuration => fullLifeWindDownDuration;

    /// <summary>Gets the number of attacks in a combo at empty enemy life.</summary>
    public int EmptyLifeComboNumberOfAttaques => emptyLifeComboNumberOfAttaques;

    /// <summary>Gets the duration between attacks at empty enemy life.</summary>
    public float EmptyLifeDurationBetweenAttaque => emptyLifeDurationBetweenAttaque;

    /// <summary>Gets the wind-up duration at empty enemy life.</summary>
    public float EmptyLifeWindUpDuration => emptyLifeWindUpDuration;

    /// <summary>Gets the wind-down duration at empty enemy life.</summary>
    public float EmptyLifeWindDownDuration => emptyLifeWindDownDuration;

    /// <summary>Gets the pause duration between combos in seconds.</summary>
    public float PauseBetweenComboDuration => pauseBetweenComboDuration;

    // --- Runtime API (for runtime-created configs, e.g. roguelite) ---

    public void SetPlayerStartLife(int value) => playerStartLife = value;
    public void SetEnemyStartLife(int value) => enemyStartLife = value;
    public void SetFullLifeComboNumberOfAttaques(int value) => fullLifeComboNumberOfAttaques = value;
    public void SetFullLifeDurationBetweenAttaque(float value) => fullLifeDurationBetweenAttaque = value;
    public void SetFullLifeWindUpDuration(float value) => fullLifeWindUpDuration = value;
    public void SetFullLifeWindDownDuration(float value) => fullLifeWindDownDuration = value;
    public void SetEmptyLifeComboNumberOfAttaques(int value) => emptyLifeComboNumberOfAttaques = value;
    public void SetEmptyLifeDurationBetweenAttaque(float value) => emptyLifeDurationBetweenAttaque = value;
    public void SetEmptyLifeWindUpDuration(float value) => emptyLifeWindUpDuration = value;
    public void SetEmptyLifeWindDownDuration(float value) => emptyLifeWindDownDuration = value;
    public void SetPauseBetweenComboDuration(float value) => pauseBetweenComboDuration = value;

    /// <summary>Creates a runtime copy with the same values. Call setters to customize.</summary>
    public GameplayConfig CloneForRuntime()
    {
        var copy = CreateInstance<GameplayConfig>();
        copy.playerStartLife = playerStartLife;
        copy.enemyStartLife = enemyStartLife;
        copy.fullLifeComboNumberOfAttaques = fullLifeComboNumberOfAttaques;
        copy.fullLifeDurationBetweenAttaque = fullLifeDurationBetweenAttaque;
        copy.fullLifeWindUpDuration = fullLifeWindUpDuration;
        copy.fullLifeWindDownDuration = fullLifeWindDownDuration;
        copy.emptyLifeComboNumberOfAttaques = emptyLifeComboNumberOfAttaques;
        copy.emptyLifeDurationBetweenAttaque = emptyLifeDurationBetweenAttaque;
        copy.emptyLifeWindUpDuration = emptyLifeWindUpDuration;
        copy.emptyLifeWindDownDuration = emptyLifeWindDownDuration;
        copy.pauseBetweenComboDuration = pauseBetweenComboDuration;
        return copy;
    }
}
