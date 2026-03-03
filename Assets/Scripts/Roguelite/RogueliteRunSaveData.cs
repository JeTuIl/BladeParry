using System;

/// <summary>
/// Single enhancement entry for extensions JSON (id + level).
/// </summary>
[Serializable]
public class RogueliteRunEnhancementEntry
{
    /// <summary>Enhancement definition ID (e.g. Gambeson).</summary>
    public string id;

    /// <summary>Owned level (1..definition.MaxLevel).</summary>
    public int level;
}

/// <summary>
/// Extensible run data serialized in extensionsJson (e.g. enhancements).
/// </summary>
[Serializable]
public class RogueliteRunExtensions
{
    /// <summary>Owned enhancements for this run (id and level per entry).</summary>
    public RogueliteRunEnhancementEntry[] enhancements = Array.Empty<RogueliteRunEnhancementEntry>();
    /// <summary>Max life bonus from MaxHealthBonus enhancements (applied when selected/leveled up). Restored on continue.</summary>
    public float maxLifeBonus;
}

/// <summary>
/// Serializable DTO for the roguelite run save file.
/// Versioned for future migrations; includes optional fields for level options and extensions.
/// </summary>
[Serializable]
public class RogueliteRunSaveData
{
    public const int CurrentVersion = 1;

    /// <summary>Save format version for migrations.</summary>
    public int version = CurrentVersion;

    /// <summary>Seed for the run (reproducible generation).</summary>
    public int runSeed;

    /// <summary>Number of fights completed in the run.</summary>
    public int fightsCompleted;

    /// <summary>Player life at end of last won fight; -1 if not set.</summary>
    public float playerLifeAfterLastFight = -1f;

    /// <summary>Total fights in the run (from progression config).</summary>
    public int totalFightsInRun;

    /// <summary>Scene to load on resume (e.g. WorldMap).</summary>
    public string targetSceneName;

    /// <summary>Optional: serialized level options for the current map. Null or empty for Option A (deterministic).</summary>
    public string[] savedLevelOptionsJson;

    /// <summary>Optional: JSON string for extensible data from other systems.</summary>
    public string extensionsJson;
}
