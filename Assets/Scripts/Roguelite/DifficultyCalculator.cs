using UnityEngine;

/// <summary>
/// Computes base and adjusted difficulty for roguelite level generation.
/// </summary>
public static class DifficultyCalculator
{
    /// <summary>
    /// Base difficulty in [0, 1] from run progress.
    /// difficulty = fightsCompleted / (totalFightsInRun - 1). First fight = 0, last = 1.
    /// </summary>
    /// <param name="fightsCompleted">Number of fights completed in the run.</param>
    /// <param name="totalFightsInRun">Total number of fights in the run.</param>
    /// <returns>Difficulty in [0, 1]; 0 if totalFightsInRun <= 1.</returns>
    public static float GetBaseDifficulty(int fightsCompleted, int totalFightsInRun)
    {
        if (totalFightsInRun <= 1)
            return 0f;
        float t = (float)fightsCompleted / (totalFightsInRun - 1);
        return Mathf.Clamp01(t);
    }

    /// <summary>
    /// Adjusted difficulty for one level option: base + random variation in [-variationRange, +variationRange], clamped to [0, 1].
    /// </summary>
    /// <param name="baseDifficulty">Base difficulty in [0, 1].</param>
    /// <param name="variationRange">Half-range of random variation (e.g. 0.15 for ±0.15).</param>
    /// <returns>Adjusted difficulty clamped to [0, 1].</returns>
    public static float GetAdjustedDifficulty(float baseDifficulty, float variationRange)
    {
        float variation = Random.Range(-variationRange, variationRange);
        return Mathf.Clamp01(baseDifficulty + variation);
    }
}
