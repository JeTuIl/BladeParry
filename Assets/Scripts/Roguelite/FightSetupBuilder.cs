using UnityEngine;

/// <summary>
/// Builds three level options (runtime FightConfigs) for the map from run state and progression config.
/// </summary>
public static class FightSetupBuilder
{
    /// <summary>
    /// Number of level options offered on the map.
    /// </summary>
    public const int LevelOptionsCount = 3;

    /// <summary>
    /// Generates three fight setups (FightConfigs) for the current run progress.
    /// Each option has its own adjusted difficulty, random pool picks, and generated GameplayConfig.
    /// </summary>
    /// <param name="progressionConfig">Pools and numeric ranges. If null, returns array of nulls.</param>
    /// <param name="fightsCompleted">Number of fights already completed in this run (0 before first fight).</param>
    /// <returns>Array of length LevelOptionsCount (3). Elements may be null if config is null or generation fails.</returns>
    public static FightConfig[] BuildLevelOptions(RogueliteProgressionConfig progressionConfig, int fightsCompleted)
    {
        var result = new FightConfig[LevelOptionsCount];
        if (progressionConfig == null)
            return result;

        if (RogueliteRunState.IsRunActive && RogueliteRunState.RunSeed != 0)
            Random.InitState(RogueliteRunState.RunSeed + fightsCompleted * 31);

        int totalFights = progressionConfig.TotalFightsInRun;
        float variationRange = progressionConfig.DifficultyVariationRange;
        float baseDifficulty = DifficultyCalculator.GetBaseDifficulty(fightsCompleted, totalFights);
        GameplayConfig template = progressionConfig.TemplateGameplayConfig;
        var ranges = progressionConfig.GameplayRanges;
        if (ranges == null)
            return result;

        for (int i = 0; i < LevelOptionsCount; i++)
        {
            float adjustedDifficulty = DifficultyCalculator.GetAdjustedDifficulty(baseDifficulty, variationRange);

            GameplayConfig gameplayConfig = template != null
                ? template.CloneForRuntime()
                : ScriptableObject.CreateInstance<GameplayConfig>();
            NumericalGenerator.FillGameplayConfig(gameplayConfig, adjustedDifficulty, ranges);

            EnemyDefinition enemy = PoolSelector.Pick(progressionConfig.EnemyPool);
            MusicConfig music = PoolSelector.Pick(progressionConfig.MusicPool);
            EnvironmentConfig environment = PoolSelector.Pick(progressionConfig.EnvironmentPool);

            var fightConfig = ScriptableObject.CreateInstance<FightConfig>();
            fightConfig.SetGameplayConfig(gameplayConfig);
            fightConfig.SetOptionalEnemyDefinition(enemy);
            fightConfig.SetMusicConfig(music);
            fightConfig.SetEnvironmentConfig(environment);

            result[i] = fightConfig;
        }

        return result;
    }
}
