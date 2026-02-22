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
        public float DurationSeconds;
        public int PerfectParryCount;
        public int NonPerfectParryCount;
        public int MaxPerfectStreak;
        public int EnemyComboCount;
        public int PlayerLoseHealthCount;
    }

    private float _fightStartTime;
    private bool _hasFightStarted;
    private int _perfectParryCount;
    private int _nonPerfectParryCount;
    private int _currentPerfectStreak;
    private int _maxPerfectStreak;
    private int _enemyComboCount;
    private int _playerLoseHealthCount;
    private FightSummary _lastSummary;
    private bool _lastSummaryValid;

    private UnityAction _onFightStartedHandler;
    private UnityAction _onEnemyComboEndHandler;
    private UnityAction _onPerfectParryHandler;
    private UnityAction _onNonPerfectParryHandler;
    private UnityAction _onMissParryHandler;
    private UnityAction _onPlayerLoseHealthHandler;
    private UnityAction<GameEndResult> _onGameEndHandler;

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
    public FightSummary GetLastSummary()
    {
        return _lastSummaryValid ? _lastSummary : default;
    }

    /// <summary>True if GetLastSummary() contains data from a completed fight.</summary>
    public bool HasLastSummary => _lastSummaryValid;

    private void OnFightStarted()
    {
        _hasFightStarted = true;
        _fightStartTime = Time.time;
    }

    private void OnPerfectParry()
    {
        _perfectParryCount++;
        _currentPerfectStreak++;
        if (_currentPerfectStreak > _maxPerfectStreak)
            _maxPerfectStreak = _currentPerfectStreak;
    }

    private void OnNonPerfectParry()
    {
        _currentPerfectStreak = 0;
        _nonPerfectParryCount++;
    }

    private void OnMissParry()
    {
        _currentPerfectStreak = 0;
    }

    private void OnEnemyComboEnd()
    {
        _enemyComboCount++;
    }

    private void OnPlayerLoseHealth()
    {
        _playerLoseHealthCount++;
    }

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
