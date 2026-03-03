using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Singleton MonoBehaviour that fires UnityEvents for key gameplay moments (parry, combo end, health).
/// Subscribe in the Inspector or from code for roguelite and other systems.
/// </summary>
public class GameplayEvents : MonoBehaviour
{
    /// <summary>Singleton instance.</summary>
    private static GameplayEvents _instance;

    [Header("Parry")]
    /// <summary>Fired when any parry occurs.</summary>
    [SerializeField] private UnityEvent onParry;

    /// <summary>Fired when a perfect parry (wind-down window) occurs.</summary>
    [SerializeField] private UnityEvent onPerfectParry;

    /// <summary>Fired when a non-perfect parry (wind-up window) occurs.</summary>
    [SerializeField] private UnityEvent onNonPerfectParry;

    /// <summary>Fired when the player fails to parry (miss).</summary>
    [SerializeField] private UnityEvent onMissParry;

    [Header("Fight lifecycle")]
    /// <summary>Fired when the fight starts (after countdown).</summary>
    [SerializeField] private UnityEvent onFightStarted;

    /// <summary>Fired when an enemy combo finishes.</summary>
    [SerializeField] private UnityEvent onEnemyComboEnd;

    [Header("Combo end")]
    /// <summary>Fired when a combo ends with all attacks parried.</summary>
    [SerializeField] private UnityEvent onComboEndAllParried;

    /// <summary>Fired when a combo ends with all attacks perfectly parried.</summary>
    [SerializeField] private UnityEvent onComboEndAllPerfectlyParried;

    /// <summary>Fired when a combo ends with at least one attack not parried.</summary>
    [SerializeField] private UnityEvent onComboEndAtLeastOneNotParried;

    [Header("Health")]
    /// <summary>Fired when the player loses health (missed parry).</summary>
    [SerializeField] private UnityEvent onPlayerLoseHealth;

    /// <summary>Fired when the enemy loses health.</summary>
    [SerializeField] private UnityEvent onEnemyLoseHealth;

    [Header("Game end")]
    /// <summary>Fired when the game ends; payload is win/loss result.</summary>
    [SerializeField] private UnityEvent<GameEndResult> onGameEnd;

    /// <summary>When true, the singleton persists across scene loads.</summary>
    [SerializeField] private bool dontDestroyOnLoad = true;

    /// <summary>Singleton instance. Null until Awake runs.</summary>
    public static GameplayEvents Instance => _instance;

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

    /// <summary>Invokes the fight started event.</summary>
    public void InvokeFightStarted() => onFightStarted?.Invoke();

    /// <summary>Invokes the enemy combo end event.</summary>
    public void InvokeEnemyComboEnd() => onEnemyComboEnd?.Invoke();

    /// <summary>Invokes the parry event.</summary>
    public void InvokeParry() => onParry?.Invoke();

    /// <summary>Invokes the perfect parry event.</summary>
    public void InvokePerfectParry() => onPerfectParry?.Invoke();

    /// <summary>Invokes the non-perfect parry event.</summary>
    public void InvokeNonPerfectParry() => onNonPerfectParry?.Invoke();

    /// <summary>Invokes the miss parry event.</summary>
    public void InvokeMissParry() => onMissParry?.Invoke();

    /// <summary>Invokes the combo end all parried event.</summary>
    public void InvokeComboEndAllParried() => onComboEndAllParried?.Invoke();

    /// <summary>Invokes the combo end all perfectly parried event.</summary>
    public void InvokeComboEndAllPerfectlyParried() => onComboEndAllPerfectlyParried?.Invoke();

    /// <summary>Invokes the combo end at least one not parried event.</summary>
    public void InvokeComboEndAtLeastOneNotParried() => onComboEndAtLeastOneNotParried?.Invoke();

    /// <summary>Invokes the player lose health event.</summary>
    public void InvokePlayerLoseHealth() => onPlayerLoseHealth?.Invoke();

    /// <summary>Invokes the enemy lose health event.</summary>
    public void InvokeEnemyLoseHealth() => onEnemyLoseHealth?.Invoke();

    /// <summary>Invokes the game end event with the given result.</summary>
    /// <param name="result">The game end result (player wins or enemy wins).</param>
    public void InvokeGameEnd(GameEndResult result) => onGameEnd?.Invoke(result);

    /// <summary>Adds a listener for fight started. Used by FightSummaryRecorder and others.</summary>
    /// <param name="listener">Callback to invoke when the event fires.</param>
    public void AddFightStartedListener(UnityAction listener) => onFightStarted?.AddListener(listener);
    /// <summary>Removes a fight started listener.</summary>
    /// <param name="listener">Callback to remove.</param>
    public void RemoveFightStartedListener(UnityAction listener) => onFightStarted?.RemoveListener(listener);

    /// <summary>Adds a listener for enemy combo end.</summary>
    /// <param name="listener">Callback to invoke.</param>
    public void AddEnemyComboEndListener(UnityAction listener) => onEnemyComboEnd?.AddListener(listener);

    /// <summary>Removes an enemy combo end listener.</summary>
    /// <param name="listener">Callback to remove.</param>
    public void RemoveEnemyComboEndListener(UnityAction listener) => onEnemyComboEnd?.RemoveListener(listener);

    /// <summary>Adds a listener for perfect parry.</summary>
    /// <param name="listener">Callback to invoke.</param>
    public void AddPerfectParryListener(UnityAction listener) => onPerfectParry?.AddListener(listener);

    /// <summary>Removes a perfect parry listener.</summary>
    /// <param name="listener">Callback to remove.</param>
    public void RemovePerfectParryListener(UnityAction listener) => onPerfectParry?.RemoveListener(listener);

    /// <summary>Adds a listener for non-perfect parry.</summary>
    /// <param name="listener">Callback to invoke.</param>
    public void AddNonPerfectParryListener(UnityAction listener) => onNonPerfectParry?.AddListener(listener);

    /// <summary>Removes a non-perfect parry listener.</summary>
    /// <param name="listener">Callback to remove.</param>
    public void RemoveNonPerfectParryListener(UnityAction listener) => onNonPerfectParry?.RemoveListener(listener);

    /// <summary>Adds a listener for miss parry.</summary>
    /// <param name="listener">Callback to invoke.</param>
    public void AddMissParryListener(UnityAction listener) => onMissParry?.AddListener(listener);

    /// <summary>Removes a miss parry listener.</summary>
    /// <param name="listener">Callback to remove.</param>
    public void RemoveMissParryListener(UnityAction listener) => onMissParry?.RemoveListener(listener);

    /// <summary>Adds a listener for game end.</summary>
    /// <param name="listener">Callback that receives the game end result.</param>
    public void AddGameEndListener(UnityAction<GameEndResult> listener) => onGameEnd?.AddListener(listener);

    /// <summary>Removes a game end listener.</summary>
    /// <param name="listener">Callback to remove.</param>
    public void RemoveGameEndListener(UnityAction<GameEndResult> listener) => onGameEnd?.RemoveListener(listener);

    /// <summary>Adds a listener for player lose health.</summary>
    /// <param name="listener">Callback to invoke.</param>
    public void AddPlayerLoseHealthListener(UnityAction listener) => onPlayerLoseHealth?.AddListener(listener);

    /// <summary>Removes a player lose health listener.</summary>
    /// <param name="listener">Callback to remove.</param>
    public void RemovePlayerLoseHealthListener(UnityAction listener) => onPlayerLoseHealth?.RemoveListener(listener);
}
