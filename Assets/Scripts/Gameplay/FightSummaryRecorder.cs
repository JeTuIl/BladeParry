using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Records fight stats by subscribing to GameplayEvents. When the game ends, the last summary
/// is available via GetLastSummary() for the endgame UI.
/// </summary>
public class FightSummaryRecorder : MonoBehaviour
{
    /// <summary>Summary data for one finished fight.</summary>
    public struct FightSummary
    {
        /// <summary>Total duration of the fight in seconds.</summary>
        public float DurationSeconds;

        /// <summary>Number of perfect parries (wind-down window) during the fight.</summary>
        public int PerfectParryCount;

        /// <summary>Number of non-perfect parries (wind-up window) during the fight.</summary>
        public int NonPerfectParryCount;

        /// <summary>Longest consecutive streak of perfect parries.</summary>
        public int MaxPerfectStreak;

        /// <summary>Number of enemy combos that completed.</summary>
        public int EnemyComboCount;

        /// <summary>Number of times the player lost health (missed parry).</summary>
        public int PlayerLoseHealthCount;
    }

    /// <summary>Time when the fight started (Time.time).</summary>
    private float _fightStartTime;

    /// <summary>True after the fight started event has fired.</summary>
    private bool _hasFightStarted;

    /// <summary>Running count of perfect parries this fight.</summary>
    private int _perfectParryCount;

    /// <summary>Running count of non-perfect parries this fight.</summary>
    private int _nonPerfectParryCount;

    /// <summary>Current consecutive perfect parry streak.</summary>
    private int _currentPerfectStreak;

    /// <summary>Maximum perfect parry streak reached this fight.</summary>
    private int _maxPerfectStreak;

    /// <summary>Running count of enemy combos completed.</summary>
    private int _enemyComboCount;

    /// <summary>Running count of times the player lost health.</summary>
    private int _playerLoseHealthCount;

    /// <summary>Last computed summary, populated when the game ends.</summary>
    private FightSummary _lastSummary;

    /// <summary>True when _lastSummary has been filled after game end.</summary>
    private bool _lastSummaryValid;

    /// <summary>Cached delegate for fight started listener.</summary>
    private UnityAction _onFightStartedHandler;

    /// <summary>Cached delegate for enemy combo end listener.</summary>
    private UnityAction _onEnemyComboEndHandler;

    /// <summary>Cached delegate for perfect parry listener.</summary>
    private UnityAction _onPerfectParryHandler;

    /// <summary>Cached delegate for non-perfect parry listener.</summary>
    private UnityAction _onNonPerfectParryHandler;

    /// <summary>Cached delegate for miss parry listener.</summary>
    private UnityAction _onMissParryHandler;

    /// <summary>Cached delegate for player lose health listener.</summary>
    private UnityAction _onPlayerLoseHealthHandler;

    /// <summary>Cached delegate for game end listener.</summary>
    private UnityAction<GameEndResult> _onGameEndHandler;

    /// <summary>Subscribes to GameplayEvents and caches handler delegates.</summary>
    private void Start()
    {
        if (GameplayEvents.Instance == null)
            return;

        _onFightStartedHandler = OnFightStarted;
        _onEnemyComboEndHandler = OnEnemyComboEnd;
        _onPerfectParryHandler = OnPerfectParry;
        _onNonPerfectParryHandler = OnNonPerfectParry;
        _onMissParryHandler = OnMissParry;
        _onPlayerLoseHealthHandler = OnPlayerLoseHealth;
        _onGameEndHandler = OnGameEnd;

        GameplayEvents.Instance.AddFightStartedListener(_onFightStartedHandler);
        GameplayEvents.Instance.AddEnemyComboEndListener(_onEnemyComboEndHandler);
        GameplayEvents.Instance.AddPerfectParryListener(_onPerfectParryHandler);
        GameplayEvents.Instance.AddNonPerfectParryListener(_onNonPerfectParryHandler);
        GameplayEvents.Instance.AddMissParryListener(_onMissParryHandler);
        GameplayEvents.Instance.AddPlayerLoseHealthListener(_onPlayerLoseHealthHandler);
        GameplayEvents.Instance.AddGameEndListener(_onGameEndHandler);
    }

    /// <summary>Unsubscribes from GameplayEvents on destroy.</summary>
    private void OnDestroy()
    {
        if (GameplayEvents.Instance == null)
            return;

        GameplayEvents.Instance.RemoveFightStartedListener(_onFightStartedHandler);
        GameplayEvents.Instance.RemoveEnemyComboEndListener(_onEnemyComboEndHandler);
        GameplayEvents.Instance.RemovePerfectParryListener(_onPerfectParryHandler);
        GameplayEvents.Instance.RemoveNonPerfectParryListener(_onNonPerfectParryHandler);
        GameplayEvents.Instance.RemoveMissParryListener(_onMissParryHandler);
        GameplayEvents.Instance.RemovePlayerLoseHealthListener(_onPlayerLoseHealthHandler);
        GameplayEvents.Instance.RemoveGameEndListener(_onGameEndHandler);
    }

    /// <summary>Returns the last computed fight summary (valid after onGameEnd has been invoked).</summary>
    /// <returns>The last fight summary, or default if no fight has ended yet.</returns>
    public FightSummary GetLastSummary()
    {
        return _lastSummaryValid ? _lastSummary : default;
    }

    /// <summary>True if GetLastSummary() contains data from a completed fight.</summary>
    public bool HasLastSummary => _lastSummaryValid;

    /// <summary>Handler for fight started: records start time.</summary>
    private void OnFightStarted()
    {
        _hasFightStarted = true;
        _fightStartTime = Time.time;
    }

    /// <summary>Handler for perfect parry: increments count and streak, updates max streak.</summary>
    private void OnPerfectParry()
    {
        _perfectParryCount++;
        _currentPerfectStreak++;
        if (_currentPerfectStreak > _maxPerfectStreak)
            _maxPerfectStreak = _currentPerfectStreak;
    }

    /// <summary>Handler for non-perfect parry: resets streak and increments non-perfect count.</summary>
    private void OnNonPerfectParry()
    {
        _currentPerfectStreak = 0;
        _nonPerfectParryCount++;
    }

    /// <summary>Handler for missed parry: resets perfect streak only.</summary>
    private void OnMissParry()
    {
        _currentPerfectStreak = 0;
    }

    /// <summary>Handler for enemy combo end: increments combo count.</summary>
    private void OnEnemyComboEnd()
    {
        _enemyComboCount++;
    }

    /// <summary>Handler for player lose health: increments lose-health count.</summary>
    private void OnPlayerLoseHealth()
    {
        _playerLoseHealthCount++;
    }

    /// <summary>Handler for game end: builds and stores the final fight summary.</summary>
    /// <param name="result">Game end result (player wins or enemy wins); not used for summary data.</param>
    private void OnGameEnd(GameEndResult result)
    {
        float duration = _hasFightStarted ? Time.time - _fightStartTime : 0f;
        _lastSummary = new FightSummary
        {
            DurationSeconds = duration,
            PerfectParryCount = _perfectParryCount,
            NonPerfectParryCount = _nonPerfectParryCount,
            MaxPerfectStreak = _maxPerfectStreak,
            EnemyComboCount = _enemyComboCount,
            PlayerLoseHealthCount = _playerLoseHealthCount
        };
        _lastSummaryValid = true;
    }
}
