using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configurable min/max and lerp adjustment range for a single float field used in roguelite generation.
/// </summary>
[Serializable]
public class RogueliteFloatRange
{
    [Tooltip("Value at difficulty 0 (easy).")]
    public float valueMin;

    [Tooltip("Value at difficulty 1 (hard).")]
    public float valueMax;

    [Tooltip("Min random added to adjusted difficulty for lerp progress.")]
    public float adjustmentMin = -0.1f;

    [Tooltip("Max random added to adjusted difficulty for lerp progress.")]
    public float adjustmentMax = 0.1f;
}

/// <summary>
/// Configurable min/max and lerp adjustment range for a single int field.
/// </summary>
[Serializable]
public class RogueliteIntRange
{
    [Tooltip("Value at difficulty 0 (easy).")]
    public int valueMin;

    [Tooltip("Value at difficulty 1 (hard).")]
    public int valueMax;

    [Tooltip("Min random added to adjusted difficulty for lerp progress.")]
    public float adjustmentMin = -0.1f;

    [Tooltip("Max random added to adjusted difficulty for lerp progress.")]
    public float adjustmentMax = 0.1f;
}

/// <summary>
/// Numeric ranges for GameplayConfig fields that are randomized by difficulty.
/// PlayerStartLife and parry damage (damageOnParry, damagePerfectRatio, damageOnComboParry) are not randomized; they come from the template only.
/// </summary>
[Serializable]
public class RogueliteGameplayRanges
{
    [Header("Life (enemy only; player life from template)")]
    public RogueliteFloatRange enemyStartLife = new RogueliteFloatRange { valueMin = 2f, valueMax = 5f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };

    [Header("Full Life Combo")]
    public RogueliteIntRange fullLifeComboNumberOfAttaques = new RogueliteIntRange { valueMin = 2, valueMax = 4, adjustmentMin = -0.1f, adjustmentMax = 0.1f };
    public RogueliteFloatRange fullLifeDurationBetweenAttaque = new RogueliteFloatRange { valueMin = 0.25f, valueMax = 0.15f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };
    public RogueliteFloatRange fullLifeWindUpDuration = new RogueliteFloatRange { valueMin = 0.35f, valueMax = 0.25f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };
    public RogueliteFloatRange fullLifeWindDownDuration = new RogueliteFloatRange { valueMin = 0.25f, valueMax = 0.15f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };

    [Header("Empty Life Combo")]
    public RogueliteIntRange emptyLifeComboNumberOfAttaques = new RogueliteIntRange { valueMin = 2, valueMax = 3, adjustmentMin = -0.1f, adjustmentMax = 0.1f };
    public RogueliteFloatRange emptyLifeDurationBetweenAttaque = new RogueliteFloatRange { valueMin = 0.3f, valueMax = 0.2f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };
    public RogueliteFloatRange emptyLifeWindUpDuration = new RogueliteFloatRange { valueMin = 0.4f, valueMax = 0.3f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };
    public RogueliteFloatRange emptyLifeWindDownDuration = new RogueliteFloatRange { valueMin = 0.3f, valueMax = 0.2f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };

    [Header("Between Combos")]
    public RogueliteFloatRange pauseBetweenComboDuration = new RogueliteFloatRange { valueMin = 3.5f, valueMax = 2f, adjustmentMin = -0.1f, adjustmentMax = 0.1f };
}

/// <summary>
/// ScriptableObject defining pools and numeric ranges for roguelite level generation.
/// </summary>
[CreateAssetMenu(fileName = "RogueliteProgressionConfig", menuName = "BladeParry/Roguelite Progression Config")]
public class RogueliteProgressionConfig : ScriptableObject
{
    [Header("Run")]
    [Tooltip("Number of fights in a full run.")]
    [SerializeField] private int totalFightsInRun = 10;

    [Tooltip("Per-level difficulty variation: added to base difficulty, then clamped to [0,1]. E.g. 0.15 gives ±0.15.")]
    [SerializeField] [Range(0f, 0.5f)] private float difficultyVariationRange = 0.15f;

    [Header("Template & Ranges")]
    [Tooltip("Base config to clone for runtime; numeric fields are overwritten by lerped ranges below.")]
    [SerializeField] private GameplayConfig templateGameplayConfig;

    [SerializeField] private RogueliteGameplayRanges gameplayRanges = new RogueliteGameplayRanges();

    [Header("Pools (artistic)")]
    [Tooltip("Enemy definitions to pick from at random per level option.")]
    [SerializeField] private List<EnemyDefinition> enemyPool = new List<EnemyDefinition>();

    [Tooltip("Music configs to pick from at random per level option.")]
    [SerializeField] private List<MusicConfig> musicPool = new List<MusicConfig>();

    [Tooltip("Environment configs to pick from at random per level option.")]
    [SerializeField] private List<EnvironmentConfig> environmentPool = new List<EnvironmentConfig>();

    public int TotalFightsInRun => totalFightsInRun;
    public float DifficultyVariationRange => difficultyVariationRange;
    public GameplayConfig TemplateGameplayConfig => templateGameplayConfig;
    public RogueliteGameplayRanges GameplayRanges => gameplayRanges;
    public IReadOnlyList<EnemyDefinition> EnemyPool => enemyPool;
    public IReadOnlyList<MusicConfig> MusicPool => musicPool;
    public IReadOnlyList<EnvironmentConfig> EnvironmentPool => environmentPool;
}
