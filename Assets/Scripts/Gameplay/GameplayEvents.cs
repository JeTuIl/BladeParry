using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton MonoBehaviour that fires UnityEvents for key gameplay moments (parry, combo end, health).
/// Subscribe in the Inspector or from code for roguelite and other systems.
/// </summary>
public class GameplayEvents : MonoBehaviour
{
    private static GameplayEvents _instance;

    [Header("Parry")]
    [SerializeField] private UnityEvent onParry;
    [SerializeField] private UnityEvent onPerfectParry;
    [SerializeField] private UnityEvent onNonPerfectParry;
    [SerializeField] private UnityEvent onMissParry;

    [Header("Fight lifecycle")]
    [SerializeField] private UnityEvent onFightStarted;
    [SerializeField] private UnityEvent onEnemyComboEnd;

    [Header("Combo end")]
    [SerializeField] private UnityEvent onComboEndAllParried;
    [SerializeField] private UnityEvent onComboEndAllPerfectlyParried;
    [SerializeField] private UnityEvent onComboEndAtLeastOneNotParried;

    [Header("Health")]
    [SerializeField] private UnityEvent onPlayerLoseHealth;
    [SerializeField] private UnityEvent onEnemyLoseHealth;

    [Header("Game end")]
    [SerializeField] private UnityEvent<GameEndResult> onGameEnd;

    [SerializeField] private bool dontDestroyOnLoad = true;

    public static GameplayEvents Instance => _instance;

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

    public void InvokeFightStarted() => onFightStarted?.Invoke();
    public void InvokeEnemyComboEnd() => onEnemyComboEnd?.Invoke();
    public void InvokeParry() => onParry?.Invoke();
    public void InvokePerfectParry() => onPerfectParry?.Invoke();
    public void InvokeNonPerfectParry() => onNonPerfectParry?.Invoke();
    public void InvokeMissParry() => onMissParry?.Invoke();
    public void InvokeComboEndAllParried() => onComboEndAllParried?.Invoke();
    public void InvokeComboEndAllPerfectlyParried() => onComboEndAllPerfectlyParried?.Invoke();
    public void InvokeComboEndAtLeastOneNotParried() => onComboEndAtLeastOneNotParried?.Invoke();
    public void InvokePlayerLoseHealth() => onPlayerLoseHealth?.Invoke();
    public void InvokeEnemyLoseHealth() => onEnemyLoseHealth?.Invoke();
    public void InvokeGameEnd(GameEndResult result) => onGameEnd?.Invoke(result);

    /// <summary>Add/remove listeners from code (e.g. for FightSummaryRecorder).</summary>
    public void AddFightStartedListener(UnityAction listener) => onFightStarted?.AddListener(listener);
    public void RemoveFightStartedListener(UnityAction listener) => onFightStarted?.RemoveListener(listener);
    public void AddEnemyComboEndListener(UnityAction listener) => onEnemyComboEnd?.AddListener(listener);
    public void RemoveEnemyComboEndListener(UnityAction listener) => onEnemyComboEnd?.RemoveListener(listener);
    public void AddPerfectParryListener(UnityAction listener) => onPerfectParry?.AddListener(listener);
    public void RemovePerfectParryListener(UnityAction listener) => onPerfectParry?.RemoveListener(listener);
    public void AddNonPerfectParryListener(UnityAction listener) => onNonPerfectParry?.AddListener(listener);
    public void RemoveNonPerfectParryListener(UnityAction listener) => onNonPerfectParry?.RemoveListener(listener);
    public void AddMissParryListener(UnityAction listener) => onMissParry?.AddListener(listener);
    public void RemoveMissParryListener(UnityAction listener) => onMissParry?.RemoveListener(listener);
    public void AddGameEndListener(UnityAction<GameEndResult> listener) => onGameEnd?.AddListener(listener);
    public void RemoveGameEndListener(UnityAction<GameEndResult> listener) => onGameEnd?.RemoveListener(listener);
    public void AddPlayerLoseHealthListener(UnityAction listener) => onPlayerLoseHealth?.AddListener(listener);
    public void RemovePlayerLoseHealthListener(UnityAction listener) => onPlayerLoseHealth?.RemoveListener(listener);
}
