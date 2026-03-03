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
    /// <param name="adjustedDifficulty">Base difficulty in [0, 1].</param>
    /// <param name="range">Min/max values and adjustment range for lerp.</param>
    /// <returns>Interpolated value within range, with random variation.</returns>
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
    /// <param name="adjustedDifficulty">Base difficulty in [0, 1].</param>
    /// <param name="range">Min/max values and adjustment range for lerp.</param>
    /// <returns>Rounded integer within range.</returns>
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
    /// <param name="target">Config to fill (typically a clone of the template).</param>
    /// <param name="adjustedDifficulty">Difficulty in [0, 1] for lerp.</param>
    /// <param name="ranges">Numeric ranges for each field.</param>
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
