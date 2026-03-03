using UnityEngine;

/// <summary>
/// ScriptableObject holding gameplay tuning: starting life, combo counts and timings (full life vs empty life), and pause between combos.
/// </summary>
[CreateAssetMenu(fileName = "GameplayConfig", menuName = "BladeParry/Gameplay Config")]
public class GameplayConfig : ScriptableObject
{
    /// <summary>Player's starting life count.</summary>
    [Header("Life")]
    [SerializeField] private float playerStartLife = 3f;

    /// <summary>Enemy's starting life count.</summary>
    [SerializeField] private float enemyStartLife = 3f;

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

    /// <summary>Life removed from enemy per parried attack.</summary>
    [Header("Parry Damage")]
    [SerializeField] private float damageOnParry = 0.5f;

    /// <summary>Multiplier when parry is perfect: effective damage = damageOnParry * damagePerfectRatio.</summary>
    [SerializeField] private float damagePerfectRatio = 1.5f;

    /// <summary>Life removed from enemy when all attacks in a combo are parried.</summary>
    [SerializeField] private float damageOnComboParry = 1f;

    /// <summary>Gets the player's starting life.</summary>
    public float PlayerStartLife => playerStartLife;

    /// <summary>Gets the enemy's starting life.</summary>
    public float EnemyStartLife => enemyStartLife;

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

    /// <summary>Gets the life removed from enemy per parried attack.</summary>
    public float DamageOnParry => damageOnParry;

    /// <summary>Gets the multiplier for perfect parry damage.</summary>
    public float DamagePerfectRatio => damagePerfectRatio;

    /// <summary>Gets the life removed when all attacks in a combo are parried.</summary>
    public float DamageOnComboParry => damageOnComboParry;

    // --- Runtime API (for runtime-created configs, e.g. roguelite) ---

    /// <summary>Sets the player's starting life count.</summary>
    /// <param name="value">Starting life value.</param>
    public void SetPlayerStartLife(float value) => playerStartLife = value;

    /// <summary>Sets the enemy's starting life count.</summary>
    /// <param name="value">Starting life value.</param>
    public void SetEnemyStartLife(float value) => enemyStartLife = value;

    /// <summary>Sets the number of attacks in a combo when enemy is at full life.</summary>
    /// <param name="value">Number of attacks.</param>
    public void SetFullLifeComboNumberOfAttaques(int value) => fullLifeComboNumberOfAttaques = value;

    /// <summary>Sets the seconds between attacks in a full-life combo.</summary>
    /// <param name="value">Duration in seconds.</param>
    public void SetFullLifeDurationBetweenAttaque(float value) => fullLifeDurationBetweenAttaque = value;

    /// <summary>Sets the wind-up duration per attack at full life.</summary>
    /// <param name="value">Duration in seconds.</param>
    public void SetFullLifeWindUpDuration(float value) => fullLifeWindUpDuration = value;

    /// <summary>Sets the wind-down duration per attack at full life.</summary>
    /// <param name="value">Duration in seconds.</param>
    public void SetFullLifeWindDownDuration(float value) => fullLifeWindDownDuration = value;

    /// <summary>Sets the number of attacks in a combo when enemy is at empty life.</summary>
    /// <param name="value">Number of attacks.</param>
    public void SetEmptyLifeComboNumberOfAttaques(int value) => emptyLifeComboNumberOfAttaques = value;

    /// <summary>Sets the seconds between attacks in an empty-life combo.</summary>
    /// <param name="value">Duration in seconds.</param>
    public void SetEmptyLifeDurationBetweenAttaque(float value) => emptyLifeDurationBetweenAttaque = value;

    /// <summary>Sets the wind-up duration per attack at empty life.</summary>
    /// <param name="value">Duration in seconds.</param>
    public void SetEmptyLifeWindUpDuration(float value) => emptyLifeWindUpDuration = value;

    /// <summary>Sets the wind-down duration per attack at empty life.</summary>
    /// <param name="value">Duration in seconds.</param>
    public void SetEmptyLifeWindDownDuration(float value) => emptyLifeWindDownDuration = value;

    /// <summary>Sets the seconds to wait between combos.</summary>
    /// <param name="value">Duration in seconds.</param>
    public void SetPauseBetweenComboDuration(float value) => pauseBetweenComboDuration = value;

    /// <summary>Sets the life removed from enemy per parried attack.</summary>
    /// <param name="value">Damage value.</param>
    public void SetDamageOnParry(float value) => damageOnParry = value;

    /// <summary>Sets the multiplier for perfect parry damage.</summary>
    /// <param name="value">Ratio value (e.g. 1.5 for 50% bonus).</param>
    public void SetDamagePerfectRatio(float value) => damagePerfectRatio = value;

    /// <summary>Sets the life removed when all attacks in a combo are parried.</summary>
    /// <param name="value">Damage value.</param>
    public void SetDamageOnComboParry(float value) => damageOnComboParry = value;

    /// <summary>Creates a runtime copy with the same values. Call setters to customize.</summary>
    /// <returns>A new GameplayConfig instance with copied values.</returns>
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
        copy.damageOnParry = damageOnParry;
        copy.damagePerfectRatio = damagePerfectRatio;
        copy.damageOnComboParry = damageOnComboParry;
        return copy;
    }
}
