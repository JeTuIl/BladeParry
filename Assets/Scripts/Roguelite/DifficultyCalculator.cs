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
    public static float GetAdjustedDifficulty(float baseDifficulty, float variationRange)
    {
        float variation = Random.Range(-variationRange, variationRange);
        return Mathf.Clamp01(baseDifficulty + variation);
    }
}
