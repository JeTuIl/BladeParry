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
    }

    /// <summary>Marks one more fight as completed. Call after player wins a fight.</summary>
    public void CompleteFight()
    {
        _fightsCompleted++;
    }

    /// <summary>Ends the run (e.g. on player loss or run complete). Clears run state.</summary>
    public void EndRun()
    {
        _runActive = false;
        _fightsCompleted = 0;
    }

    /// <summary>Returns the current fights-completed count (instance method for use by builders).</summary>
    public int GetFightsCompleted() => _fightsCompleted;

    /// <summary>Returns the current run seed (for reproducible level generation).</summary>
    public int GetRunSeed() => _runSeed;
}
