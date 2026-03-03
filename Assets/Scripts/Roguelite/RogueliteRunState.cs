using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persists run state across Map and FightingScene loads. Tracks whether a run is active, fights completed, and owned enhancements.
/// </summary>
public class RogueliteRunState : MonoBehaviour
{
    /// <summary>Singleton instance.</summary>
    private static RogueliteRunState _instance;

    /// <summary>When true, this object persists across scene loads.</summary>
    [SerializeField] private bool dontDestroyOnLoad = true;

    /// <summary>True when a run is in progress (started and not yet ended).</summary>
    private bool _runActive;

    /// <summary>Number of fights completed in the current run.</summary>
    private int _fightsCompleted;

    /// <summary>Random seed for the current run (reproducible generation).</summary>
    private int _runSeed;
    /// <summary>Player life at end of last won fight; -1 means not set (e.g. first fight or after run end).</summary>
    private float _playerLifeAfterLastFight = -1f;
    /// <summary>Owned enhancements this run: enhancement id -> level (1..maxLevel).</summary>
    private readonly Dictionary<string, int> _ownedEnhancements = new Dictionary<string, int>();
    /// <summary>Max life bonus applied when MaxHealthBonus enhancement was selected or leveled up (not recalculated every fight).</summary>
    private float _maxLifeBonus;
    /// <summary>Pool of definitions for lookup (set from map when progression config is available).</summary>
    private List<RogueliteEnhancementDefinition> _enhancementPool;

    public static RogueliteRunState Instance => _instance;

    /// <summary>True when the player is in an active run (started from main menu, not yet won/lost).</summary>
    public static bool IsRunActive => _instance != null && _instance._runActive;

    /// <summary>Number of fights completed in the current run (0..TotalFightsInRun-1).</summary>
    public static int FightsCompleted => _instance != null ? _instance._fightsCompleted : 0;

    /// <summary>Seed for the current run (for reproducible generation). 0 if not set.</summary>
    public static int RunSeed => _instance != null ? _instance._runSeed : 0;

    /// <summary>Enforces singleton; optionally marks object DontDestroyOnLoad.</summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    /// <summary>Clears singleton reference when this instance is destroyed.</summary>
    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>Starts a new run: sets run active, fights completed to 0, and clears enhancements. Optional seed for reproducible generation (0 = random).</summary>
    /// <param name="seed">Random seed for the run; 0 to generate a random seed.</param>
    public void StartRun(int seed = 0)
    {
        _runActive = true;
        _fightsCompleted = 0;
        _runSeed = seed != 0 ? seed : Random.Range(1, int.MaxValue);
        _playerLifeAfterLastFight = -1f;
        _ownedEnhancements.Clear();
        _maxLifeBonus = 0f;
    }

    /// <summary>Records the player's current life at the end of a won fight. Call when the player wins so the next fight can start with this life.</summary>
    /// <param name="life">Player life value to store for the next fight.</param>
    public void RecordPlayerLifeAfterFight(float life)
    {
        _playerLifeAfterLastFight = life;
    }

    /// <summary>Gets the stored player life for the next fight when there was a previous fight in this run. Returns true and the value when FightsCompleted >= 1 and a value was recorded; otherwise false (e.g. first fight uses config full life).</summary>
    /// <param name="life">Output: the stored life value when the method returns true.</param>
    /// <returns>True if a stored life value is available; otherwise false.</returns>
    public bool TryGetPlayerLifeForNextFight(out float life)
    {
        life = 0f;
        if (_fightsCompleted < 1 || _playerLifeAfterLastFight < 0f)
            return false;
        life = _playerLifeAfterLastFight;
        return true;
    }

    /// <summary>Marks one more fight as completed. Call after player wins a fight.</summary>
    public void CompleteFight()
    {
        _fightsCompleted++;
    }

    /// <summary>Loads run state from a save for resume. Sets run active and restores seed, fights completed, player life, and optionally extensions (enhancements).</summary>
    /// <param name="seed">Run seed to restore.</param>
    /// <param name="fightsCompleted">Number of fights completed to restore.</param>
    /// <param name="playerLifeAfterLastFight">Player life after last fight (-1 if not set).</param>
    /// <param name="extensionsJson">Optional JSON for extensions (enhancements); null or empty to skip.</param>
    public void LoadState(int seed, int fightsCompleted, float playerLifeAfterLastFight, string extensionsJson = null)
    {
        _runActive = true;
        _runSeed = seed;
        _fightsCompleted = fightsCompleted;
        _playerLifeAfterLastFight = playerLifeAfterLastFight;
        _ownedEnhancements.Clear();
        if (!string.IsNullOrEmpty(extensionsJson))
        {
            try
            {
                var ext = JsonUtility.FromJson<RogueliteRunExtensions>(extensionsJson);
                if (ext?.enhancements != null)
                {
                    foreach (var e in ext.enhancements)
                    {
                        if (!string.IsNullOrEmpty(e.id) && e.level > 0)
                            _ownedEnhancements[e.id] = e.level;
                    }
                }
                _maxLifeBonus = ext.maxLifeBonus;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("RogueliteRunState: Failed to parse extensionsJson: " + ex.Message);
            }
        }
    }

    /// <summary>Ends the run (e.g. on player loss or run complete). Clears run state, enhancements, and the run save file.</summary>
    public void EndRun()
    {
        _runActive = false;
        _fightsCompleted = 0;
        _playerLifeAfterLastFight = -1f;
        _ownedEnhancements.Clear();
        _maxLifeBonus = 0f;
        RunSaveService.ClearSave();
    }

    /// <summary>Adds a new enhancement or upgrades an existing one (level must be 1..definition.MaxLevel).</summary>
    /// <param name="enhancementId">Stable ID of the enhancement (e.g. Gambeson).</param>
    /// <param name="level">Level to set (1 to definition.MaxLevel).</param>
    public void AddOrUpgradeEnhancement(string enhancementId, int level)
    {
        if (string.IsNullOrEmpty(enhancementId) || level < 1) return;
        _ownedEnhancements[enhancementId] = level;
    }

    /// <summary>Gets the current level for an enhancement (0 if not owned).</summary>
    /// <param name="enhancementId">Stable ID of the enhancement.</param>
    /// <returns>Level 1..MaxLevel if owned; 0 otherwise.</returns>
    public int GetEnhancementLevel(string enhancementId)
    {
        if (string.IsNullOrEmpty(enhancementId)) return 0;
        return _ownedEnhancements.TryGetValue(enhancementId, out int l) ? l : 0;
    }

    /// <summary>Returns all owned enhancement ids and their levels.</summary>
    public IReadOnlyDictionary<string, int> GetOwnedEnhancements() => _ownedEnhancements;

    /// <summary>Max life bonus from MaxHealthBonus enhancements, applied only when selected or leveled up. Restored from owned enhancements on load.</summary>
    public float GetMaxLifeBonus() => _maxLifeBonus;

    /// <summary>Call when the player selects or levels up an enhancement. If it is MaxHealthBonus, adds the delta to the run's max life bonus and to current life so the missing amount stays the same.</summary>
    /// <param name="def">The enhancement definition (checked for MaxHealthBonus effect type).</param>
    /// <param name="oldLevel">Previous level (0 if newly added).</param>
    /// <param name="newLevel">New level after selection/upgrade.</param>
    public void ApplyMaxHealthBonusForEnhancement(RogueliteEnhancementDefinition def, int oldLevel, int newLevel)
    {
        if (def == null || def.EffectType != RogueliteEnhancementEffectType.MaxHealthBonus) return;
        float delta = def.GetValueAtLevel(newLevel) - def.GetValueAtLevel(oldLevel);
        _maxLifeBonus += delta;
        if (delta > 0f && _playerLifeAfterLastFight >= 0f)
            _playerLifeAfterLastFight += delta;
    }

    /// <summary>Builds the extensions JSON for saving (enhancements list).</summary>
    /// <returns>JSON string containing enhancements and maxLifeBonus.</returns>
    public string BuildExtensionsJson()
    {
        var list = new List<RogueliteRunEnhancementEntry>();
        foreach (var kv in _ownedEnhancements)
        {
            if (!string.IsNullOrEmpty(kv.Key) && kv.Value > 0)
                list.Add(new RogueliteRunEnhancementEntry { id = kv.Key, level = kv.Value });
        }
        var ext = new RogueliteRunExtensions { enhancements = list.ToArray(), maxLifeBonus = _maxLifeBonus };
        return JsonUtility.ToJson(ext);
    }

    /// <summary>Returns the current fights-completed count (instance method for use by builders).</summary>
    /// <returns>Number of fights completed in the current run.</returns>
    public int GetFightsCompleted() => _fightsCompleted;

    /// <summary>Returns the current run seed (for reproducible level generation).</summary>
    /// <returns>Run seed; 0 if not set.</returns>
    public int GetRunSeed() => _runSeed;

    /// <summary>Sets the enhancement definition pool (call from map controller when progression config is available).</summary>
    /// <param name="pool">List of enhancement definitions; null to clear.</param>
    public void SetEnhancementPool(IReadOnlyList<RogueliteEnhancementDefinition> pool)
    {
        _enhancementPool = pool != null ? new List<RogueliteEnhancementDefinition>(pool) : null;
    }

    /// <summary>Gets the definition for an enhancement id, or null if not in pool.</summary>
    /// <param name="enhancementId">Stable ID of the enhancement.</param>
    /// <returns>The definition if found in pool; otherwise null.</returns>
    public RogueliteEnhancementDefinition GetEnhancementDefinition(string enhancementId)
    {
        if (string.IsNullOrEmpty(enhancementId) || _enhancementPool == null) return null;
        foreach (var def in _enhancementPool)
        {
            if (def != null && def.Id == enhancementId) return def;
        }
        return null;
    }

    /// <summary>Sum of (definition.BaseValue * level) for all owned enhancements of the given effect type.</summary>
    /// <param name="effectType">Effect type to sum (e.g. DamageBonus).</param>
    /// <returns>Total value across all owned enhancements of that type.</returns>
    public float GetTotalValueForEffect(RogueliteEnhancementEffectType effectType)
    {
        if (_enhancementPool == null) return 0f;
        float sum = 0f;
        foreach (var kv in _ownedEnhancements)
        {
            var def = GetEnhancementDefinition(kv.Key);
            if (def != null && def.EffectType == effectType && kv.Value > 0)
                sum += def.GetValueAtLevel(kv.Value);
        }
        return sum;
    }

    /// <summary>True with probability equal to the total value for the given (chance) effect type (0..1).</summary>
    /// <param name="effectType">Chance-based effect type (e.g. ChanceIgnoreEachHit).</param>
    /// <returns>True if random roll is below the total chance value.</returns>
    public bool RollChanceForEffect(RogueliteEnhancementEffectType effectType)
    {
        float chance = GetTotalValueForEffect(effectType);
        return chance > 0f && Random.value < chance;
    }
}
