using UnityEngine;

/// <summary>
/// Persists run state across Map and FightingScene loads. Tracks whether a run is active and how many fights have been completed.
/// </summary>
public class RogueliteRunState : MonoBehaviour
{
    private static RogueliteRunState _instance;

    [SerializeField] private bool dontDestroyOnLoad = true;

    private bool _runActive;
    private int _fightsCompleted;
    private int _runSeed;
    /// <summary>Player life at end of last won fight; -1 means not set (e.g. first fight or after run end).</summary>
    private float _playerLifeAfterLastFight = -1f;

    public static RogueliteRunState Instance => _instance;

    /// <summary>True when the player is in an active run (started from main menu, not yet won/lost).</summary>
    public static bool IsRunActive => _instance != null && _instance._runActive;

    /// <summary>Number of fights completed in the current run (0..TotalFightsInRun-1).</summary>
    public static int FightsCompleted => _instance != null ? _instance._fightsCompleted : 0;

    /// <summary>Seed for the current run (for reproducible generation). 0 if not set.</summary>
    public static int RunSeed => _instance != null ? _instance._runSeed : 0;

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

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    /// <summary>Starts a new run: sets run active and fights completed to 0. Optional seed for reproducible generation (0 = random).</summary>
    public void StartRun(int seed = 0)
    {
        _runActive = true;
        _fightsCompleted = 0;
        _runSeed = seed != 0 ? seed : Random.Range(1, int.MaxValue);
        _playerLifeAfterLastFight = -1f;
    }

    /// <summary>Records the player's current life at the end of a won fight. Call when the player wins so the next fight can start with this life.</summary>
    public void RecordPlayerLifeAfterFight(float life)
    {
        _playerLifeAfterLastFight = life;
    }

    /// <summary>Gets the stored player life for the next fight when there was a previous fight in this run. Returns true and the value when FightsCompleted >= 1 and a value was recorded; otherwise false (e.g. first fight uses config full life).</summary>
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

    /// <summary>Loads run state from a save for resume. Sets run active and restores seed, fights completed, and player life.</summary>
    public void LoadState(int seed, int fightsCompleted, float playerLifeAfterLastFight)
    {
        _runActive = true;
        _runSeed = seed;
        _fightsCompleted = fightsCompleted;
        _playerLifeAfterLastFight = playerLifeAfterLastFight;
    }

    /// <summary>Ends the run (e.g. on player loss or run complete). Clears run state and the run save file.</summary>
    public void EndRun()
    {
        _runActive = false;
        _fightsCompleted = 0;
        _playerLifeAfterLastFight = -1f;
        RunSaveService.ClearSave();
    }

    /// <summary>Returns the current fights-completed count (instance method for use by builders).</summary>
    public int GetFightsCompleted() => _fightsCompleted;

    /// <summary>Returns the current run seed (for reproducible level generation).</summary>
    public int GetRunSeed() => _runSeed;
}
