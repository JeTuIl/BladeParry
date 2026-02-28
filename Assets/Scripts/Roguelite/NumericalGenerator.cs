using UnityEngine;

/// <summary>
/// Generates numeric values for GameplayConfig from adjusted difficulty and configurable ranges.
/// </summary>
public static class NumericalGenerator
{
    /// <summary>
    /// Lerp progress = Clamp01(adjustedDifficulty + Random.Range(range.adjustmentMin, range.adjustmentMax)).
    /// Value = Lerp(valueMin, valueMax, lerpProgress).
    /// </summary>
    public static float FloatFromDifficulty(float adjustedDifficulty, RogueliteFloatRange range)
    {
        float adjustment = Random.Range(range.adjustmentMin, range.adjustmentMax);
        float progress = Mathf.Clamp01(adjustedDifficulty + adjustment);
        float value = Mathf.Lerp(range.valueMin, range.valueMax, progress);
        return Mathf.Clamp(value, Mathf.Min(range.valueMin, range.valueMax), Mathf.Max(range.valueMin, range.valueMax));
    }

    /// <summary>
    /// Same as float but result rounded to int and clamped to [valueMin, valueMax].
    /// </summary>
    public static int IntFromDifficulty(float adjustedDifficulty, RogueliteIntRange range)
    {
        float adjustment = Random.Range(range.adjustmentMin, range.adjustmentMax);
        float progress = Mathf.Clamp01(adjustedDifficulty + adjustment);
        float value = Mathf.Lerp(range.valueMin, range.valueMax, progress);
        return Mathf.Clamp(Mathf.RoundToInt(value), range.valueMin, range.valueMax);
    }

    /// <summary>
    /// Overwrites the given GameplayConfig with values from ranges and adjusted difficulty.
    /// Caller should pass a clone of the template so base values are set; player and parry damage are left from the template (not randomized).
    /// </summary>
    public static void FillGameplayConfig(
        GameplayConfig target,
        float adjustedDifficulty,
        RogueliteGameplayRanges ranges)
    {
        // Player and parry damage: not randomized; keep template values from the clone.
        // target.SetPlayerStartLife / SetDamageOnParry / SetDamagePerfectRatio / SetDamageOnComboParry — unchanged.

        target.SetEnemyStartLife(FloatFromDifficulty(adjustedDifficulty, ranges.enemyStartLife));
        target.SetFullLifeComboNumberOfAttaques(IntFromDifficulty(adjustedDifficulty, ranges.fullLifeComboNumberOfAttaques));
        target.SetFullLifeDurationBetweenAttaque(FloatFromDifficulty(adjustedDifficulty, ranges.fullLifeDurationBetweenAttaque));
        target.SetFullLifeWindUpDuration(FloatFromDifficulty(adjustedDifficulty, ranges.fullLifeWindUpDuration));
        target.SetFullLifeWindDownDuration(FloatFromDifficulty(adjustedDifficulty, ranges.fullLifeWindDownDuration));
        target.SetEmptyLifeComboNumberOfAttaques(IntFromDifficulty(adjustedDifficulty, ranges.emptyLifeComboNumberOfAttaques));
        target.SetEmptyLifeDurationBetweenAttaque(FloatFromDifficulty(adjustedDifficulty, ranges.emptyLifeDurationBetweenAttaque));
        target.SetEmptyLifeWindUpDuration(FloatFromDifficulty(adjustedDifficulty, ranges.emptyLifeWindUpDuration));
        target.SetEmptyLifeWindDownDuration(FloatFromDifficulty(adjustedDifficulty, ranges.emptyLifeWindDownDuration));
        target.SetPauseBetweenComboDuration(FloatFromDifficulty(adjustedDifficulty, ranges.pauseBetweenComboDuration));
    }
}
