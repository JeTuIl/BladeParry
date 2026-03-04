using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Result of a finished game: either the player or the enemy wins.
/// </summary>
public enum GameEndResult
{
    /// <summary>The player won (enemy life reached zero).</summary>
    PlayerWins,

    /// <summary>The enemy won (player life reached zero).</summary>
    EnemyWins
}

/// <summary>
/// Outcome for a single attack when simulating test flows (editor tool).
/// </summary>
public enum ParryOutcome
{
    /// <summary>Player did not parry; take damage.</summary>
    Miss,

    /// <summary>Player parried in wind-up (normal parry).</summary>
    NormalParry,

    /// <summary>Player parried in wind-down (perfect parry).</summary>
    PerfectParry
}

/// <summary>
/// Runs the main gameplay loop: countdown, combos, parry windows, life tracking, music, and game end.
/// Subscribes to combo and input events and applies damage/FX based on parry success or failure.
/// </summary>
public class GameplayLoopController : MonoBehaviour
{
    /// <summary>Gameplay configuration (life, combo counts, timings, pause). Fallback when no FightConfig and no provider override.</summary>
    [SerializeField] private GameplayConfig config;

    /// <summary>Optional single fight config; if set, its GameplayConfig is used (overridden by FightConfigProvider at runtime).</summary>
    [SerializeField] private FightConfig fightConfig;

    /// <summary>Drives the sequence of enemy attacks in a combo.</summary>
    [SerializeField] private CharacterComboSequence characterComboSequence;

    /// <summary>Drives a single attack and parry window.</summary>
    [SerializeField] private CharacterAttaqueSequence characterAttaqueSequence;

    /// <summary>Detects player swipe input for parry direction.</summary>
    [SerializeField] private SlideDetection slideDetection;

    /// <summary>UI life bar for the player.</summary>
    [SerializeField] private LifebarManager playerLifebarManager;

    /// <summary>UI life bar for the enemy.</summary>
    [SerializeField] private LifebarManager enemyLifebarManager;

    /// <summary>Visual feedback when the player misses a parry.</summary>
    [SerializeField] private SpriteWinker playerSpriteWinker;

    /// <summary>Player sprite and hurt/dead state.</summary>
    [SerializeField] private PlayerSpriteManager playerSpriteManager;

    /// <summary>Visual feedback when the enemy loses life (all parried).</summary>
    [SerializeField] private SpriteWinker enemySpriteWinker;

    /// <summary>Enemy sprite direction and hurt/down state.</summary>
    [SerializeField] private CharacterSpriteDirection enemySpriteDirection;

    /// <summary>World position to spawn parry success FX.</summary>
    [SerializeField] private Vector3 parryFxPosition;

    /// <summary>World position to spawn missed parry FX.</summary>
    [SerializeField] private Vector3 missedParryFxPosition;

    /// <summary>World position to spawn all-parried combo FX.</summary>
    [SerializeField] private Vector3 allParriedFxPosition;

    /// <summary>Controls music pitch based on enemy life.</summary>
    [SerializeField] private MusicPitchManager musicPitchManager;

    /// <summary>Switches music at game end (win/loss).</summary>
    [SerializeField] private MusciSwitchManager musicSwitchManager;

    /// <summary>Music clip to play when the player wins.</summary>
    [SerializeField] private AudioClip musicOnPlayerWins;

    /// <summary>Music clip to play when the enemy wins.</summary>
    [SerializeField] private AudioClip musicOnEnemyWins;

    /// <summary>Duration of the crossfade when switching to game-end music.</summary>
    [SerializeField] private float gameEndMusicTransitionDuration = 2f;

    /// <summary>Music pitch when enemy is at full life.</summary>
    [SerializeField] private float fullLifeMusicSpeed = 1f;

    /// <summary>Music pitch when enemy is at empty life.</summary>
    [SerializeField] private float emptyLifeMusicSpeed = 1f;

    /// <summary>Text used for the pre-fight countdown (5, 4, 3, 2, 1, Fight!).</summary>
    [SerializeField] private TMP_Text countdownText;

    private static readonly LocalizedString s_fightLabel = new LocalizedString("BladeParry_LocalizationTable", "Gameplay_Fight");

    /// <summary>Duration of each countdown number animation in seconds.</summary>
    [SerializeField] private float countdownStepDuration = 1f;

    /// <summary>Scale of the countdown text at the start of each step.</summary>
    [SerializeField] private Vector3 countdownScaleStart = new Vector3(1.5f, 1.5f, 1f);

    /// <summary>Scale of the countdown text at the end of each step.</summary>
    [SerializeField] private Vector3 countdownScaleEnd = Vector3.one;

    [Header("Hit stop")]
    /// <summary>Time scale applied during hit stop (e.g. 0.92 for slight slow).</summary>
    [SerializeField] private float hitStopTimeScale = 0.92f;

    /// <summary>Duration in seconds of the hit stop effect.</summary>
    [SerializeField] private float hitStopDuration = 0.05f;

    /// <summary>Squash and stretch for the player (parry success / hit).</summary>
    [SerializeField] private SquashStretchAnimator playerSquashStretch;

    /// <summary>Squash and stretch for the enemy (combo success / hurt).</summary>
    [SerializeField] private SquashStretchAnimator enemySquashStretch;

    /// <summary>Floating text for parry / miss / combo feedback.</summary>
    [SerializeField] private FloatingFeedbackText floatingFeedback;

    /// <summary>FX prefab index in FxManager for perfect parry (e.g. 5). Normal parry uses index 1.</summary>
    [SerializeField] private int perfectParryFxIndex = 5;

    /// <summary>Displays current perfect parries in a row (count, gradient, tremble).</summary>
    [SerializeField] private PerfectParryComboDisplay perfectParryComboDisplay;

    /// <summary>Drives vignette from low life and missed parry. Notify via NotifyPlayerDamaged.</summary>
    [SerializeField] private GameplayPostProcessDriver gameplayPostProcessDriver;

    /// <summary>Optional fight background; when EnvironmentConfig is set, its sprite/position/scale are applied here at fight start.</summary>
    [SerializeField] private Image fightBackgroundImage;

    /// <summary>Player's current life; decremented on missed parry.</summary>
    private float _playerCurrentLife;

    /// <summary>Enemy's current life; decremented when all attacks in a combo are parried.</summary>
    private float _enemyCurrentLife;

    /// <summary>True while the player can input a parry for the current attack.</summary>
    private bool _parryWindowActive;

    /// <summary>Direction of the current enemy attack (expected parry is opposite).</summary>
    private Direction _currentAttaqueDirection;

    /// <summary>True while the parry window is in the wind-down phase (perfect parry possible).</summary>
    private bool _isInWindDown;

    /// <summary>Number of perfect parries in a row; reset on normal parry or miss.</summary>
    private int _perfectParriesInARow;

    /// <summary>Per-attack parry success in the current combo; one entry per attack.</summary>
    private readonly List<bool> _parriedInCombo = new List<bool>();

    /// <summary>Per-attack perfect parry in the current combo; one entry per attack (only true if parried and perfect).</summary>
    private readonly List<bool> _perfectParryInCombo = new List<bool>();

    /// <summary>Resolved config used for the run (from provider, fightConfig, or config).</summary>
    private GameplayConfig _effectiveConfig;

    /// <summary>Cached enemy definition for the current fight (from resolved FightConfig).</summary>
    private EnemyDefinition _enemyDefinition;

    /// <summary>Effective full-life music speed (from MusicConfig if set, else scene fallback).</summary>
    private float _effectiveFullLifeMusicSpeed;

    /// <summary>Effective empty-life music speed (from MusicConfig if set, else scene fallback).</summary>
    private float _effectiveEmptyLifeMusicSpeed;

    /// <summary>Set when the current combo has finished all attacks.</summary>
    private bool _comboComplete;

    /// <summary>True if we already ignored the first damage this combo (Gambeson). Reset at combo start.</summary>
    private bool _ignoredFirstDamageThisCombo;

    /// <summary>True if the player has done at least one perfect parry this combo (Surgeon's Edge: damage until combo end is boosted). Reset at combo start.</summary>
    private bool _hasPerfectParryThisCombo;

    /// <summary>True if Soul Anchor revive has been used this fight. Reset at fight start.</summary>
    private bool _reviveUsedThisFight;
    /// <summary>When true, player would have died but we defer until combo end and revive with X life (once per fight).</summary>
    private bool _pendingReviveAtComboEnd;

    /// <summary>Extra seconds to add to the next pause between combos (Respite Charm: each perfect parry adds value; consumed after use).</summary>
    private float _extraPauseBeforeNextCombo;

    /// <summary>Extra seconds to add to the next combo's wind-up duration (Watcher's Eye: each perfect parry adds value; consumed when next combo starts).</summary>
    private float _extraWindUpNextCombo;

    /// <summary>Cached handler for combo complete; used to subscribe/unsubscribe.</summary>
    private UnityAction _onComboCompleteHandler;

    /// <summary>Cached handler for parry window open.</summary>
    private UnityAction<Direction> _onParryWindowOpenHandler;

    /// <summary>Cached handler for parry window close.</summary>
    private UnityAction<Direction> _onParryWindowCloseHandler;

    /// <summary>Cached handler for wind-down start.</summary>
    private UnityAction _onWindDownStartHandler;

    /// <summary>Cached handler for swipe detected.</summary>
    private UnityAction<Direction> _onSwipeDetectedHandler;

    /// <summary>Active main loop coroutine; null when not running.</summary>
    private Coroutine _mainLoopCoroutine;

    /// <summary>Duration of the short enemy hurt blink when hurt by a single parry (normal or perfect).</summary>
    private const float EnemyShortHurtDuration = 0.4f;

    /// <summary>Coroutine that clears enemy isHurt after a single parry; null when not running.</summary>
    private Coroutine _enemyShortHurtCoroutine;

    /// <summary>When true, main loop does not auto-trigger combos; waits for StartTestCombo (editor tool).</summary>
    private bool _testMode;

    /// <summary>Pending outcomes to apply when parry window opens (test flow). Dequeued in OnParryWindowOpen.</summary>
    private readonly Queue<ParryOutcome> _testOutcomeQueue = new Queue<ParryOutcome>();

    /// <summary>When >= 0, main loop (in test mode) runs one combo with this many attacks then waits again.</summary>
    private int _testComboRequestedCount = -1;

    /// <summary>True when the current attack's outcome was already applied via test queue (skip miss in OnParryWindowClose).</summary>
    private bool _lastAttackOutcomeAppliedByTest;

    /// <summary>When true, fight simulation is running: auto-request combos with random outcomes until fight ends.</summary>
    private bool _fightSimulationActive;

    /// <summary>Simulation: probability (0–1) that each attack is a miss.</summary>
    private float _simulationMissRate;

    /// <summary>Simulation: among non-miss attacks, probability (0–1) of perfect parry.</summary>
    private float _simulationPerfectParryRate;

    /// <summary>Simulation: time scale and target FPS base (60) for restore.</summary>
    private float _simulationTimeScale;

    private const int DefaultTargetFrameRate = 60;

    /// <summary>Fight stats for simulation log: start time (realtime) or -1 if not started.</summary>
    private float _fightStartTime = -1f;

    /// <summary>Fight stats: total attacks, combos, parries, perfect parries this fight.</summary>
    private int _totalAttacks, _totalCombos, _totalParries, _totalPerfectParries;

    /// <summary>Raised when the game ends, with the result (player or enemy wins).</summary>
    public event System.Action<GameEndResult> GameEnded;

    /// <summary>Sets test mode (pause auto-combos; editor tool triggers combos via StartTestCombo).</summary>
    /// <param name="value">True to enable test mode (no auto-combos).</param>
    public void SetTestMode(bool value) { _testMode = value; }

    /// <summary>True when test mode is active.</summary>
    public bool IsTestMode() => _testMode;

    /// <summary>Enqueues outcomes to apply when each parry window opens (for test flows). Call before StartTestCombo.</summary>
    /// <param name="outcomes">Sequence of outcomes (e.g. Miss, NormalParry, PerfectParry) in order of attacks.</param>
    public void EnqueueTestOutcomes(IEnumerable<ParryOutcome> outcomes)
    {
        if (outcomes == null) return;
        foreach (var o in outcomes)
            _testOutcomeQueue.Enqueue(o);
    }

    /// <summary>Clears any pending test outcomes (call before EnqueueTestOutcomes when starting a new test sequence).</summary>
    public void ClearTestOutcomes() => _testOutcomeQueue.Clear();

    /// <summary>Requests one combo with the given attack count. Main loop (in test mode) will run it. Enqueue TestOutcomes first.</summary>
    /// <param name="attackCount">Number of attacks in the combo (minimum 1).</param>
    public void StartTestCombo(int attackCount)
    {
        if (attackCount < 1) return;
        _testComboRequestedCount = attackCount;
    }

    /// <summary>True when fight simulation is active (editor test tool).</summary>
    public bool IsFightSimulationActive() => _fightSimulationActive;

    /// <summary>Starts fight simulation: test mode, time scale and target FPS set, outcomes generated from rates until fight ends.</summary>
    /// <param name="missRate">Probability (0–1) that each attack is a miss.</param>
    /// <param name="perfectParryRate">Among non-miss attacks, probability (0–1) of perfect parry.</param>
    /// <param name="timeScale">Time.timeScale and target FPS = 60 * timeScale.</param>
    public void StartFightSimulation(float missRate, float perfectParryRate, float timeScale)
    {
        _fightSimulationActive = true;
        _simulationMissRate = Mathf.Clamp01(missRate);
        _simulationPerfectParryRate = Mathf.Clamp01(perfectParryRate);
        _simulationTimeScale = Mathf.Max(0.01f, timeScale);
        _testMode = true;
        Time.timeScale = _simulationTimeScale;
        int targetFps = Mathf.RoundToInt(DefaultTargetFrameRate * _simulationTimeScale);
        Application.targetFrameRate = targetFps;
        QualitySettings.vSyncCount = 0;
        GenerateAndRequestNextSimulationCombo();
    }

    /// <summary>Stops fight simulation and restores time scale and target frame rate.</summary>
    public void StopFightSimulation()
    {
        _fightSimulationActive = false;
        Time.timeScale = 1f;
        Application.targetFrameRate = DefaultTargetFrameRate;
    }

    /// <summary>Generates one combo worth of outcomes from simulation rates and requests the combo (test mode).</summary>
    private void GenerateAndRequestNextSimulationCombo()
    {
        if (_effectiveConfig == null || _playerCurrentLife <= 0 || _enemyCurrentLife <= 0)
            return;
        float lifeRatio = _enemyCurrentLife / _effectiveConfig.EnemyStartLife;
        int numberOfAttaques = (int)Mathf.Lerp(_effectiveConfig.EmptyLifeComboNumberOfAttaques, _effectiveConfig.FullLifeComboNumberOfAttaques, lifeRatio);
        numberOfAttaques = Mathf.Max(1, numberOfAttaques);
        var outcomes = new List<ParryOutcome>();
        for (int i = 0; i < numberOfAttaques; i++)
        {
            if (UnityEngine.Random.value < _simulationMissRate)
                outcomes.Add(ParryOutcome.Miss);
            else if (UnityEngine.Random.value < _simulationPerfectParryRate)
                outcomes.Add(ParryOutcome.PerfectParry);
            else
                outcomes.Add(ParryOutcome.NormalParry);
        }
        ClearTestOutcomes();
        EnqueueTestOutcomes(outcomes);
        _testComboRequestedCount = numberOfAttaques;
    }

    /// <summary>Sets player life to max (for editor test tool).</summary>
    public void HealPlayerToFull()
    {
        if (_effectiveConfig == null) return;
        _playerCurrentLife = _effectiveConfig.PlayerStartLife;
        UpdateLifebars();
    }

    /// <summary>Sets enemy life to max (for editor test tool).</summary>
    public void HealEnemyToFull()
    {
        if (_effectiveConfig == null) return;
        _enemyCurrentLife = _effectiveConfig.EnemyStartLife;
        UpdateLifebars();
        UpdateMusicPitch();
    }

    /// <summary>
    /// Validates required references, initializes life and lifebars, and starts the main loop coroutine.
    /// </summary>
    private void Start()
    {
        _effectiveConfig = FightConfigProvider.GetConfigForFight(
            fightConfig != null ? fightConfig.GetGameplayConfig() : config);

        if (_effectiveConfig == null)
        {
            Debug.LogError("GameplayLoopController: No GameplayConfig (assign config or fightConfig, or set FightConfigProvider).", this);
            return;
        }
        if (characterComboSequence == null || characterAttaqueSequence == null || slideDetection == null)
        {
            Debug.LogError("GameplayLoopController: CharacterComboSequence, CharacterAttaqueSequence or SlideDetection is not assigned.", this);
            return;
        }

        _playerCurrentLife = _effectiveConfig.PlayerStartLife;
        if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null && RogueliteRunState.Instance.TryGetPlayerLifeForNextFight(out float savedLife))
            _playerCurrentLife = Mathf.Clamp(savedLife, 0f, _effectiveConfig.PlayerStartLife);
        _enemyCurrentLife = _effectiveConfig.EnemyStartLife;
        UpdateLifebars();

        FightConfig resolvedFightConfig = FightConfigProvider.CurrentFightConfig != null ? FightConfigProvider.CurrentFightConfig : fightConfig;
        _enemyDefinition = resolvedFightConfig?.OptionalEnemyDefinition;
        if (_enemyDefinition?.SpriteSet != null && enemySpriteDirection != null)
            enemySpriteDirection.ApplySpriteConfig(_enemyDefinition.SpriteSet);
        if (characterAttaqueSequence != null)
            characterAttaqueSequence.SetWindDownFxIndex(_enemyDefinition != null ? _enemyDefinition.WindDownFxIndex : 0);

        _effectiveFullLifeMusicSpeed = fullLifeMusicSpeed;
        _effectiveEmptyLifeMusicSpeed = emptyLifeMusicSpeed;
        MusicConfig musicConfig = resolvedFightConfig?.GetMusicConfig();
        if (musicConfig != null)
        {
            _effectiveFullLifeMusicSpeed = musicConfig.FullLifeMusicSpeed;
            _effectiveEmptyLifeMusicSpeed = musicConfig.EmptyLifeMusicSpeed;
            if (musicConfig.MusicClip != null && musicSwitchManager != null)
                musicSwitchManager.SwitchMusic(musicConfig.MusicClip, 0f, true);
        }
        UpdateMusicPitch();

        EnvironmentConfig envConfig = resolvedFightConfig?.GetEnvironmentConfig();
        if (envConfig != null && envConfig.BackgroundSprite != null && fightBackgroundImage != null)
        {
            fightBackgroundImage.sprite = envConfig.BackgroundSprite;
            fightBackgroundImage.rectTransform.localPosition = envConfig.BackgroundPosition;
            fightBackgroundImage.rectTransform.localScale = envConfig.BackgroundScale;
        }

        _mainLoopCoroutine = StartCoroutine(MainLoopCoroutine());
    }

    /// <summary>
    /// Pushes current and max life values to the player and enemy lifebar managers.
    /// </summary>
    private void UpdateLifebars()
    {
        if (playerLifebarManager != null)
        {
            playerLifebarManager.MaxLifeValue = _effectiveConfig.PlayerStartLife;
            playerLifebarManager.CurrentLifeValue = _playerCurrentLife;
        }
        if (enemyLifebarManager != null)
        {
            enemyLifebarManager.MaxLifeValue = _effectiveConfig.EnemyStartLife;
            enemyLifebarManager.CurrentLifeValue = _enemyCurrentLife;
        }
    }

    /// <summary>
    /// Sets music pitch based on enemy life ratio (lerp between empty and full life speed).
    /// </summary>
    private void UpdateMusicPitch()
    {
        if (musicPitchManager == null)
            return;
        float lifeRatio = _enemyCurrentLife / _effectiveConfig.EnemyStartLife;
        musicPitchManager.pitch = Mathf.Lerp(_effectiveEmptyLifeMusicSpeed, _effectiveFullLifeMusicSpeed, lifeRatio);
    }

    /// <summary>
    /// Unsubscribes from combo and input events when the component is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeFromComboAndInput();
    }

    /// <summary>
    /// Returns the opposite cardinal direction for parry logic (e.g. Up -> Down).
    /// </summary>
    /// <param name="direction">The attack direction.</param>
    /// <returns>The opposite direction, or Neutral for Neutral.</returns>
    private static Direction GetOppositeDirection(Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.Neutral
        };
    }

    /// <summary>
    /// Subscribes to combo complete and parry window / swipe events for the current combo.
    /// </summary>
    private void SubscribeToComboAndInput()
    {
        _onComboCompleteHandler = OnComboComplete;
        _onParryWindowOpenHandler = OnParryWindowOpen;
        _onParryWindowCloseHandler = OnParryWindowClose;
        _onWindDownStartHandler = OnWindDownStart;
        _onSwipeDetectedHandler = OnSwipeDetected;

        characterComboSequence.onComboComplete.AddListener(_onComboCompleteHandler);
        characterAttaqueSequence.onParryWindowOpen.AddListener(_onParryWindowOpenHandler);
        characterAttaqueSequence.onParryWindowClose.AddListener(_onParryWindowCloseHandler);
        characterAttaqueSequence.onWindDownStart.AddListener(_onWindDownStartHandler);
        slideDetection.onSwipeDetected.AddListener(_onSwipeDetectedHandler);
    }

    /// <summary>
    /// Removes listeners from combo and input events and clears cached handlers.
    /// </summary>
    private void UnsubscribeFromComboAndInput()
    {
        if (characterComboSequence != null && _onComboCompleteHandler != null)
            characterComboSequence.onComboComplete.RemoveListener(_onComboCompleteHandler);
        if (characterAttaqueSequence != null)
        {
            if (_onParryWindowOpenHandler != null)
                characterAttaqueSequence.onParryWindowOpen.RemoveListener(_onParryWindowOpenHandler);
            if (_onParryWindowCloseHandler != null)
                characterAttaqueSequence.onParryWindowClose.RemoveListener(_onParryWindowCloseHandler);
            if (_onWindDownStartHandler != null)
                characterAttaqueSequence.onWindDownStart.RemoveListener(_onWindDownStartHandler);
        }
        if (slideDetection != null && _onSwipeDetectedHandler != null)
            slideDetection.onSwipeDetected.RemoveListener(_onSwipeDetectedHandler);

        _onComboCompleteHandler = null;
        _onParryWindowOpenHandler = null;
        _onParryWindowCloseHandler = null;
        _onWindDownStartHandler = null;
        _onSwipeDetectedHandler = null;
    }

    /// <summary>
    /// Called when a parry window opens; records the attack direction and adds a slot for this attack's parry result.
    /// In test mode, if a simulated outcome is queued, applies it immediately and marks so OnParryWindowClose skips.
    /// </summary>
    /// <param name="attaqueDirection">Direction of the incoming attack.</param>
    private void OnParryWindowOpen(Direction attaqueDirection)
    {
        _parryWindowActive = true;
        _isInWindDown = false;
        _currentAttaqueDirection = attaqueDirection;
        _parriedInCombo.Add(false);
        _perfectParryInCombo.Add(false);

        if (_testMode && _testOutcomeQueue.Count > 0)
        {
            ParryOutcome outcome = _testOutcomeQueue.Dequeue();
            _lastAttackOutcomeAppliedByTest = true;
            if (outcome == ParryOutcome.Miss)
                ApplyMissedParryForCurrentAttack();
            else
                ApplyParrySuccessForCurrentAttack(outcome == ParryOutcome.PerfectParry);
            _parryWindowActive = false;
            _isInWindDown = false;
            _currentAttaqueDirection = Direction.Neutral;
        }
    }

    /// <summary>Called when wind-down starts; parry during this phase is a perfect parry.</summary>
    private void OnWindDownStart()
    {
        _isInWindDown = true;
    }

    /// <summary>
    /// Applies missed-parry outcome for the current attack (last slot in _parriedInCombo). Used by OnParryWindowClose and test outcome injection.
    /// </summary>
    private void ApplyMissedParryForCurrentAttack()
    {
        if (_parriedInCombo.Count == 0 || _parriedInCombo[_parriedInCombo.Count - 1])
            return;

        bool ignoreDamage = false;
        if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null)
        {
            if (!_ignoredFirstDamageThisCombo && RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.IgnoreFirstDamagePerCombo) > 0f)
            {
                _ignoredFirstDamageThisCombo = true;
                ignoreDamage = true;
            }
            else if (RogueliteRunState.Instance.RollChanceForEffect(RogueliteEnhancementEffectType.ChanceIgnoreEachHit))
            {
                ignoreDamage = true;
            }
            else
            {
                float shieldThreshold = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.ShieldWhenComboExceedsN);
                shieldThreshold = 45.0f - shieldThreshold;
                if (shieldThreshold > 0f && _perfectParriesInARow > shieldThreshold)
                {
                    ignoreDamage = true;
                    _perfectParriesInARow = 0;
                    UpdatePerfectParryComboDisplay();
                }
            }
        }

        if (!ignoreDamage)
        {
            float damageTaken = 1f * GetDamageReceivedDecreasesWithComboMultiplier(_perfectParriesInARow) * GetReceiveMoreDealMoreMultiplier();
            _playerCurrentLife -= damageTaken;
            Debug.Log($"[Damage] Player took {damageTaken} damage. Current life: {_playerCurrentLife}");
            if (GameplayEvents.Instance != null)
            {
                GameplayEvents.Instance.InvokeMissParry();
                GameplayEvents.Instance.InvokePlayerLoseHealth();
            }
            if (playerLifebarManager != null)
                playerLifebarManager.NotifyDamage();
            UpdateLifebars();
            if (playerSpriteWinker != null)
                playerSpriteWinker.TriggerWink();
            if (playerSpriteManager != null)
                playerSpriteManager.TriggerHurt();
            if (playerSquashStretch != null)
                playerSquashStretch.PlayHit();
            if (gameplayPostProcessDriver != null)
                gameplayPostProcessDriver.NotifyPlayerDamaged();
            if (FxManager.Instance != null)
                FxManager.Instance.SpawnAtPosition(2, missedParryFxPosition);
            if (ScreenshackManager.Instance != null)
                ScreenshackManager.Instance.TriggerScreenShake(ScreenShakeStrength.High);
            if (floatingFeedback != null)
                floatingFeedback.ShowMiss(missedParryFxPosition);
            if (OptionManager.GetHapticEnabledFromPrefs() && SystemInfo.deviceType == DeviceType.Handheld)
                Handheld.Vibrate();
            _perfectParriesInARow = 0;
            UpdatePerfectParryComboDisplay();

            if (_playerCurrentLife <= 0 && characterComboSequence != null)
            {
                bool canRevive = RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null
                    && !_reviveUsedThisFight
                    && RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.ReviveAtEndOfComboWithXLives) > 0f;
                if (canRevive)
                    _pendingReviveAtComboEnd = true;
                else
                    characterComboSequence.StopComboAndNotifyComplete();
            }
        }

        if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null && characterComboSequence != null
            && RogueliteRunState.Instance.RollChanceForEffect(RogueliteEnhancementEffectType.ChanceDamageEndsCombo))
            characterComboSequence.StopComboAndNotifyComplete();
    }

    /// <summary>
    /// Called when a parry window closes; if the last attack was not parried (and not already applied by test), applies player damage and missed-parry FX.
    /// </summary>
    /// <param name="attaqueDirection">Direction of the attack that just closed.</param>
    private void OnParryWindowClose(Direction attaqueDirection)
    {
        if (_lastAttackOutcomeAppliedByTest)
        {
            _lastAttackOutcomeAppliedByTest = false;
            _parryWindowActive = false;
            _isInWindDown = false;
            _currentAttaqueDirection = Direction.Neutral;
            return;
        }

        if (_parriedInCombo.Count > 0 && !_parriedInCombo[_parriedInCombo.Count - 1])
            ApplyMissedParryForCurrentAttack();

        _parryWindowActive = false;
        _isInWindDown = false;
        _currentAttaqueDirection = Direction.Neutral;
    }

    /// <summary>
    /// Applies parry success for the current attack (last slot). Used by OnSwipeDetected and test outcome injection.
    /// </summary>
    /// <param name="isPerfectParry">True for perfect parry (wind-down), false for normal parry.</param>
    private void ApplyParrySuccessForCurrentAttack(bool isPerfectParry)
    {
        if (_parriedInCombo.Count == 0)
            return;

        _parriedInCombo[_parriedInCombo.Count - 1] = true;
        _perfectParryInCombo[_perfectParryInCombo.Count - 1] = isPerfectParry;
        _parryWindowActive = false;

        float parryDamage = isPerfectParry
            ? _effectiveConfig.DamageOnParry * _effectiveConfig.DamagePerfectRatio
            : _effectiveConfig.DamageOnParry;
        if (isPerfectParry)
        {
            parryDamage *= GetPerfectParryRatioBonusMultiplier();
            _hasPerfectParryThisCombo = true;
            if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null)
            {
                float pauseBonus = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.PerfectParryIncreasesPauseBeforeNextCombo);
                if (pauseBonus > 0f)
                    _extraPauseBeforeNextCombo += pauseBonus;
                float windUpBonus = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.PerfectParryIncreasesNextWindUp);
                if (windUpBonus > 0f)
                    _extraWindUpNextCombo += windUpBonus;
            }
        }
        else
            parryDamage *= GetDamageBonusBasicParryMultiplier();
        parryDamage *= GetDamageBonusPerfectParryComboMultiplier();
        parryDamage *= GetDamageScalesWithComboMultiplier(_parriedInCombo.Count);
        parryDamage *= GetDamageInverseToRemainingHealthMultiplier();
        parryDamage *= GetReceiveMoreDealMoreMultiplier();
        _enemyCurrentLife = Mathf.Max(0f, _enemyCurrentLife - parryDamage);
        Debug.Log($"[Damage] Enemy took {parryDamage} damage (parry). Current life: {_enemyCurrentLife}");
        UpdateLifebars();
        UpdateMusicPitch();
        if (enemySpriteWinker != null)
            enemySpriteWinker.TriggerWink(EnemyShortHurtDuration);
        if (enemySpriteDirection != null)
        {
            enemySpriteDirection.isHurt = true;
            if (_enemyShortHurtCoroutine != null)
                StopCoroutine(_enemyShortHurtCoroutine);
            _enemyShortHurtCoroutine = StartCoroutine(EnemyShortHurtClearCoroutine());
        }

        if (_enemyCurrentLife <= 0 && characterComboSequence != null)
            characterComboSequence.StopComboAndNotifyComplete();

        if (GameplayEvents.Instance != null)
        {
            GameplayEvents.Instance.InvokeParry();
            if (isPerfectParry)
                GameplayEvents.Instance.InvokePerfectParry();
            else
                GameplayEvents.Instance.InvokeNonPerfectParry();
        }

        StartCoroutine(HitStopCoroutine());
        if (playerSquashStretch != null)
            playerSquashStretch.PlayParrySuccess();
        if (FxManager.Instance != null)
            FxManager.Instance.SpawnAtPosition(isPerfectParry ? perfectParryFxIndex : 1, parryFxPosition);
        if (ScreenshackManager.Instance != null)
            ScreenshackManager.Instance.TriggerScreenShake(ScreenShakeStrength.Low);
        if (floatingFeedback != null)
        {
            if (isPerfectParry)
                floatingFeedback.ShowPerfectParry(parryFxPosition);
            else
                floatingFeedback.ShowParry(parryFxPosition);
        }
        if (isPerfectParry)
        {
            int perfectParryIncrement = 1;
            if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null
                && RogueliteRunState.Instance.RollChanceForEffect(RogueliteEnhancementEffectType.ChancePerfectParryMoreCombo))
                perfectParryIncrement = 2;
            _perfectParriesInARow += perfectParryIncrement;
            UpdatePerfectParryComboDisplay();
            TryApplyDamageEveryXPerfectParries();
            TryApplyAfterThreePerfectParriesBonusDamage();
            if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null
                && RogueliteRunState.Instance.RollChanceForEffect(RogueliteEnhancementEffectType.ChancePerfectParryReduceComboCount)
                && characterComboSequence != null)
                characterComboSequence.ReduceRemainingAttacks();
            if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null
                && RogueliteRunState.Instance.RollChanceForEffect(RogueliteEnhancementEffectType.ChancePerfectParryNextAttackFromGivenDirection)
                && characterComboSequence != null)
                characterComboSequence.SetNextAttackDirectionOverride(Direction.Up);
        }
        else
        {
            _perfectParriesInARow = 0;
            UpdatePerfectParryComboDisplay();
        }
        if (OptionManager.GetHapticEnabledFromPrefs() && SystemInfo.deviceType == DeviceType.Handheld)
            Handheld.Vibrate();
    }

    /// <summary>
    /// Called when the player swipes; if in a parry window and direction matches expected parry, marks current attack as parried and plays FX.
    /// </summary>
    /// <param name="swipeDirection">Direction of the player's swipe.</param>
    private void OnSwipeDetected(Direction swipeDirection)
    {
        if (!_parryWindowActive || _parriedInCombo.Count == 0)
            return;

        Direction expectedParryDirection = GetOppositeDirection(_currentAttaqueDirection);
        if (swipeDirection == expectedParryDirection)
        {
            bool isPerfectParry = _isInWindDown;
            if (!isPerfectParry && RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null
                && RogueliteRunState.Instance.RollChanceForEffect(RogueliteEnhancementEffectType.ChanceAutoParryOnFail))
                isPerfectParry = true;
            ApplyParrySuccessForCurrentAttack(isPerfectParry);
        }
    }

    /// <summary>Pushes current perfect parry count to the combo display.</summary>
    private void UpdatePerfectParryComboDisplay()
    {
        if (perfectParryComboDisplay != null)
            perfectParryComboDisplay.SetPerfectParryCount(_perfectParriesInARow);
    }

    /// <summary>
    /// Cadence Stone (DamageEveryXPerfectParries): applies bonus damage to the enemy when the current perfect-parries-in-a-row
    /// count is a multiple of (1f / value). E.g. value 0.1f => every 10 perfect parries in a row. Called after incrementing _perfectParriesInARow.
    /// </summary>
    private void TryApplyDamageEveryXPerfectParries()
    {
        if (_perfectParriesInARow <= 0 || !RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageEveryXPerfectParries);
        if (value <= 0f)
            return;
        int interval = Mathf.Max(1, Mathf.RoundToInt(1f / value));
        if (_perfectParriesInARow % interval != 0)
            return;
        float bonusDamage = _effectiveConfig.DamageOnParry * _effectiveConfig.DamagePerfectRatio * GetPerfectParryRatioBonusMultiplier() * GetDamageBonusPerfectParryComboMultiplier();
        bonusDamage *= GetDamageScalesWithComboMultiplier(_parriedInCombo.Count);
        bonusDamage *= GetDamageInverseToRemainingHealthMultiplier();
        bonusDamage *= GetReceiveMoreDealMoreMultiplier();
        _enemyCurrentLife = Mathf.Max(0f, _enemyCurrentLife - bonusDamage);
        Debug.Log($"[Damage] Enemy took {bonusDamage} damage (Cadence Stone bonus). Current life: {_enemyCurrentLife}");
        UpdateLifebars();
        UpdateMusicPitch();
        if (_enemyCurrentLife <= 0 && characterComboSequence != null)
            characterComboSequence.StopComboAndNotifyComplete();
    }

    /// <summary>
    /// Trinity Seal (AfterThreePerfectParriesNextFullParryBonusDamage): when perfect parries in a row is a multiple of 3 (but not 0),
    /// inflict damage to the enemy equal to the enhancement value. Called after incrementing _perfectParriesInARow.
    /// </summary>
    private void TryApplyAfterThreePerfectParriesBonusDamage()
    {
        if (_perfectParriesInARow <= 0 || _perfectParriesInARow % 3 != 0 || !RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return;
        float bonusDamage = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.AfterThreePerfectParriesNextFullParryBonusDamage);
        if (bonusDamage <= 0f)
            return;
        bonusDamage *= GetDamageBonusPerfectParryComboMultiplier();
        bonusDamage *= GetDamageScalesWithComboMultiplier(_parriedInCombo.Count);
        bonusDamage *= GetDamageInverseToRemainingHealthMultiplier();
        bonusDamage *= GetReceiveMoreDealMoreMultiplier();
        _enemyCurrentLife = Mathf.Max(0f, _enemyCurrentLife - bonusDamage);
        Debug.Log($"[Damage] Enemy took {bonusDamage} damage (Trinity Seal bonus). Current life: {_enemyCurrentLife}");
        UpdateLifebars();
        UpdateMusicPitch();
        if (_enemyCurrentLife <= 0 && characterComboSequence != null)
            characterComboSequence.StopComboAndNotifyComplete();
    }

    /// <summary>
    /// Returns the Veteran's Carapace (DamageReceivedDecreasesWithCombo) multiplier for player damage taken.
    /// Uses the same "combo" as PerfectParryComboDisplay: number of perfect parries in a row before this hit.
    /// Multiplier = 1 - value*count, clamped to 0. No run or no enhancement returns 1f.
    /// </summary>
    private float GetDamageReceivedDecreasesWithComboMultiplier(int perfectParriesInARow)
    {
        if (perfectParriesInARow <= 0 || !RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageReceivedDecreasesWithCombo);
        if (value <= 0f)
            return 1f;
        return Mathf.Max(0f, 1f - value * perfectParriesInARow);
    }

    /// <summary>
    /// Telegraph Bell (LongerWindUp): multiplier for wind-up duration. Returns 1f + enhancement value; no run or no enhancement returns 1f.
    /// </summary>
    private float GetLongerWindUpMultiplier()
    {
        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.LongerWindUp);
        return value <= 0f ? 1f : 1f + value;
    }

    /// <summary>
    /// Grace Period Ring (LongerWindDown): multiplier for wind-down duration. Returns 1f + enhancement value; no run or no enhancement returns 1f.
    /// </summary>
    private float GetLongerWindDownMultiplier()
    {
        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.LongerWindDown);
        return value <= 0f ? 1f : 1f + value;
    }

    /// <summary>
    /// Burden Stone (EnemySlowsWithCombo): multiplier for enemy wind-up duration. Returns 1f + value * perfectParriesInARow
    /// so wind-up is longer when the player has more perfect parries in a row. No run or no enhancement returns 1f.
    /// </summary>
    private float GetEnemySlowsWithComboWindUpMultiplier()
    {
        if (_perfectParriesInARow <= 0 || !RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.EnemySlowsWithCombo);
        if (value <= 0f)
            return 1f;
        return 1f + value * _perfectParriesInARow;
    }

    /// <summary>
    /// Executioner's Mark (DamageInverseToRemainingHealth): multiplier for enemy damage = 1 + (currentPlayerLife/maxPlayerLife) * enhancement value.
    /// More remaining player life = more damage to enemy. No run or no enhancement returns 1f.
    /// </summary>
    private float GetDamageInverseToRemainingHealthMultiplier()
    {
        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageInverseToRemainingHealth);
        if (value <= 0f)
            return 1f;
        float maxLife = _effectiveConfig.PlayerStartLife;
        if (maxLife <= 0f)
            return 1f;
        float ratio = _playerCurrentLife / maxLife;
        return 1f + (ratio / 1f) * value;
    }

    /// <summary>
    /// Surgeon's Edge (DamageBonusPerfectParryCombo): when the player has done a perfect parry this combo, all enemy damage until combo end is multiplied by 1 + enhancement value.
    /// </summary>
    private float GetDamageBonusPerfectParryComboMultiplier()
    {
        if (!_hasPerfectParryThisCombo || !RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageBonusPerfectParryCombo);
        return value <= 0f ? 1f : 1f + value;
    }

    /// <summary>
    /// Full combo parry (DamageBonusFullComboParry): multiplier for damage when the player parried all attacks of a combo = 1 + enhancement value.
    /// No run or no enhancement returns 1f.
    /// </summary>
    private float GetDamageBonusFullComboParryMultiplier()
    {
        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageBonusFullComboParry);
        return value <= 0f ? 1f : 1f + value;
    }

    /// <summary>
    /// Better Guard (PerfectParryRatioBonus): multiplier for perfect parry damage = 1 + enhancement value.
    /// No run or no enhancement returns 1f.
    /// </summary>
    private float GetPerfectParryRatioBonusMultiplier()
    {
        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.PerfectParryRatioBonus);
        return value <= 0f ? 1f : 1f + value;
    }

    /// <summary>
    /// Better Cutting Edge (DamageBonusBasicParry): multiplier for basic (non-perfect) parry damage = 1 + enhancement value.
    /// No run or no enhancement returns 1f.
    /// </summary>
    private float GetDamageBonusBasicParryMultiplier()
    {
        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageBonusBasicParry);
        return value <= 0f ? 1f : 1f + value;
    }

    /// <summary>
    /// Oil (ReceiveMoreDealMore): multiplier for both player and enemy damage = 1 + enhancement value.
    /// No run or no enhancement returns 1f.
    /// </summary>
    private float GetReceiveMoreDealMoreMultiplier()
    {
        if (!RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.ReceiveMoreDealMore);
        return value <= 0f ? 1f : 1f + value;
    }

    /// <summary>
    /// Returns the Crescendo Blade (DamageScalesWithCombo) multiplier for enemy damage.
    /// First hit in combo = 1x, second = 1+value, third = 1+2*value, etc. No run or no enhancement returns 1f.
    /// </summary>
    /// <param name="comboHitIndex1Based">1-based index of the hit in the current combo (e.g. first parry = 1).</param>
    private float GetDamageScalesWithComboMultiplier(int comboHitIndex1Based)
    {
        if (comboHitIndex1Based < 1 || !RogueliteRunState.IsRunActive || RogueliteRunState.Instance == null)
            return 1f;
        float value = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.DamageScalesWithCombo);
        if (value <= 0f)
            return 1f;
        return 1f + value * (comboHitIndex1Based - 1);
    }

    /// <summary>
    /// Brief time scale dip for parry hit stop.
    /// </summary>
    private IEnumerator HitStopCoroutine()
    {
        Time.timeScale = hitStopTimeScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = _fightSimulationActive ? _simulationTimeScale : 1f;
    }

    /// <summary>
    /// Clears enemy hurt state after the short single-parry hurt duration.
    /// </summary>
    private IEnumerator EnemyShortHurtClearCoroutine()
    {
        yield return new WaitForSeconds(EnemyShortHurtDuration);
        if (enemySpriteDirection != null)
            enemySpriteDirection.isHurt = false;
        _enemyShortHurtCoroutine = null;
    }

    /// <summary>
    /// Called when the combo sequence has finished all attacks; sets flag so the main loop can continue.
    /// </summary>
    private void OnComboComplete()
    {
        _comboComplete = true;
    }

    /// <summary>
    /// Runs the pre-fight countdown (5 to 1 then "Fight!") with scale/fade animation per step.
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator PreparationPhaseCoroutine()
    {
        if (countdownText == null)
        {
            Debug.LogWarning("GameplayLoopController: Countdown Text is not assigned. Skipping preparation phase.", this);
            yield break;
        }

        RectTransform countdownRect = countdownText.rectTransform;
        countdownText.gameObject.SetActive(true);

        for (int i = 5; i >= 1; i--)
        {
            countdownText.text = i.ToString();
            yield return AnimateCountdownStep(countdownRect, countdownText);
        }

        AsyncOperationHandle<string> fightOp = s_fightLabel.GetLocalizedStringAsync();
        if (!fightOp.IsDone)
            yield return fightOp;
        countdownText.text = (fightOp.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(fightOp.Result)) ? fightOp.Result : "Fight!";
        yield return AnimateCountdownStep(countdownRect, countdownText);

        if (GameplayEvents.Instance != null)
            GameplayEvents.Instance.InvokeFightStarted();

        countdownText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Animates one countdown step: fade in and scale from countdownScaleStart to countdownScaleEnd over countdownStepDuration.
    /// </summary>
    /// <param name="rect">RectTransform to scale.</param>
    /// <param name="text">Text whose alpha is animated.</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator AnimateCountdownStep(RectTransform rect, TMP_Text text)
    {
        float elapsed = 0f;
        Color color = text.color;

        text.color = new Color(color.r, color.g, color.b, 0f);
        rect.localScale = countdownScaleStart;

        while (elapsed < countdownStepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / countdownStepDuration);
            color.a = Mathf.Lerp(0f, 1f, t);
            text.color = color;
            rect.localScale = Vector3.Lerp(countdownScaleStart, countdownScaleEnd, t);
            yield return null;
        }

        text.color = new Color(color.r, color.g, color.b, 1f);
        rect.localScale = countdownScaleEnd;
    }

    /// <summary>
    /// Main loop: runs preparation countdown, then repeatedly runs combos (with parry logic), applies damage, and checks for game end.
    /// In test mode, does not auto-trigger combos; waits for StartTestCombo then runs one combo and waits again.
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator MainLoopCoroutine()
    {
        yield return StartCoroutine(PreparationPhaseCoroutine());

        _reviveUsedThisFight = false;

        while (_playerCurrentLife > 0 && _enemyCurrentLife > 0)
        {
            if (_fightStartTime < 0f)
            {
                _fightStartTime = Time.realtimeSinceStartup;
                _totalAttacks = 0;
                _totalCombos = 0;
                _totalParries = 0;
                _totalPerfectParries = 0;
            }

            int numberOfAttaques;
            float durationBetweenAttaque, windUpDuration, windDownDuration;

            if (_testMode)
            {
                while (_testMode && _testComboRequestedCount < 0 && _playerCurrentLife > 0 && _enemyCurrentLife > 0)
                {
                    if (_fightSimulationActive)
                        GenerateAndRequestNextSimulationCombo();
                    if (_testComboRequestedCount < 0)
                        yield return null;
                }
                if (_testComboRequestedCount < 0)
                    continue;
                numberOfAttaques = _testComboRequestedCount;
                _testComboRequestedCount = -1;
                float lifeRatio = _enemyCurrentLife / _effectiveConfig.EnemyStartLife;
                durationBetweenAttaque = Mathf.Lerp(_effectiveConfig.EmptyLifeDurationBetweenAttaque, _effectiveConfig.FullLifeDurationBetweenAttaque, lifeRatio);
                windUpDuration = Mathf.Lerp(_effectiveConfig.EmptyLifeWindUpDuration, _effectiveConfig.FullLifeWindUpDuration, lifeRatio);
                windDownDuration = Mathf.Lerp(_effectiveConfig.EmptyLifeWindDownDuration, _effectiveConfig.FullLifeWindDownDuration, lifeRatio);
                windUpDuration += _extraWindUpNextCombo;
                _extraWindUpNextCombo = 0f;
                windUpDuration *= GetEnemySlowsWithComboWindUpMultiplier();
                windUpDuration *= GetLongerWindUpMultiplier();
                windDownDuration *= GetLongerWindDownMultiplier();
            }
            else
            {
                if (enemySpriteDirection != null)
                    enemySpriteDirection.isHurt = false;
                float lifeRatio = _enemyCurrentLife / _effectiveConfig.EnemyStartLife;
                numberOfAttaques = (int)Mathf.Lerp(_effectiveConfig.EmptyLifeComboNumberOfAttaques, _effectiveConfig.FullLifeComboNumberOfAttaques, lifeRatio);
                durationBetweenAttaque = Mathf.Lerp(_effectiveConfig.EmptyLifeDurationBetweenAttaque, _effectiveConfig.FullLifeDurationBetweenAttaque, lifeRatio);
                windUpDuration = Mathf.Lerp(_effectiveConfig.EmptyLifeWindUpDuration, _effectiveConfig.FullLifeWindUpDuration, lifeRatio);
                windDownDuration = Mathf.Lerp(_effectiveConfig.EmptyLifeWindDownDuration, _effectiveConfig.FullLifeWindDownDuration, lifeRatio);
                windUpDuration += _extraWindUpNextCombo;
                _extraWindUpNextCombo = 0f;
                windUpDuration *= GetEnemySlowsWithComboWindUpMultiplier();
                windUpDuration *= GetLongerWindUpMultiplier();
                windDownDuration *= GetLongerWindDownMultiplier();
            }

            _parriedInCombo.Clear();
            _perfectParryInCombo.Clear();
            _parryWindowActive = false;
            _isInWindDown = false;
            _currentAttaqueDirection = Direction.Neutral;
            _comboComplete = false;
            _ignoredFirstDamageThisCombo = false;
            _hasPerfectParryThisCombo = false;
            _pendingReviveAtComboEnd = false;

            if (_testMode && enemySpriteDirection != null)
                enemySpriteDirection.isHurt = false;

            SubscribeToComboAndInput();
            characterComboSequence.TriggerComboWithParameters(numberOfAttaques, durationBetweenAttaque, windUpDuration, windDownDuration);

            while (!_comboComplete)
                yield return null;

            UnsubscribeFromComboAndInput();

            if (_pendingReviveAtComboEnd && RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null)
            {
                float reviveLife = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.ReviveAtEndOfComboWithXLives);
                if (reviveLife > 0f)
                {
                    _playerCurrentLife = reviveLife;
                    _reviveUsedThisFight = true;
                    _pendingReviveAtComboEnd = false;
                    UpdateLifebars();
                }
            }

            bool allParried = _parriedInCombo.Count > 0;
            bool allPerfectlyParried = _parriedInCombo.Count > 0 && _perfectParryInCombo.Count == _parriedInCombo.Count;
            for (int i = 0; i < _parriedInCombo.Count; i++)
            {
                if (!_parriedInCombo[i])
                {
                    allParried = false;
                    allPerfectlyParried = false;
                    break;
                }
                if (i >= _perfectParryInCombo.Count || !_perfectParryInCombo[i])
                    allPerfectlyParried = false;
            }

            if (GameplayEvents.Instance != null)
            {
                if (allParried)
                    GameplayEvents.Instance.InvokeComboEndAllParried();
                if (allPerfectlyParried)
                    GameplayEvents.Instance.InvokeComboEndAllPerfectlyParried();
                if (!allParried)
                    GameplayEvents.Instance.InvokeComboEndAtLeastOneNotParried();
                GameplayEvents.Instance.InvokeEnemyComboEnd();
            }

            if (allPerfectlyParried && RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null)
            {
                float regen = RogueliteRunState.Instance.GetTotalValueForEffect(RogueliteEnhancementEffectType.RegenOnFullPerfectParryCombo);
                if (regen > 0f)
                {
                    _playerCurrentLife = Mathf.Min(_effectiveConfig.PlayerStartLife, _playerCurrentLife + regen);
                    UpdateLifebars();
                }
            }

            if (allParried)
            {
                float comboParryDamage = _effectiveConfig.DamageOnComboParry * GetDamageBonusFullComboParryMultiplier() * GetDamageBonusPerfectParryComboMultiplier() * GetDamageScalesWithComboMultiplier(_parriedInCombo.Count) * GetDamageInverseToRemainingHealthMultiplier() * GetReceiveMoreDealMoreMultiplier();
                _enemyCurrentLife = Mathf.Max(0f, _enemyCurrentLife - comboParryDamage);
                Debug.Log($"[Damage] Enemy took {comboParryDamage} damage (full combo parry). Current life: {_enemyCurrentLife}");
                if (GameplayEvents.Instance != null)
                    GameplayEvents.Instance.InvokeEnemyLoseHealth();
                if (enemyLifebarManager != null)
                    enemyLifebarManager.NotifyDamage();
                UpdateLifebars();
                UpdateMusicPitch();
                if (enemySpriteWinker != null)
                    enemySpriteWinker.TriggerWink();
                if (enemySquashStretch != null)
                    enemySquashStretch.PlayHit();
                if (FxManager.Instance != null)
                    FxManager.Instance.SpawnAtPosition(_enemyDefinition != null ? _enemyDefinition.AllParriedComboFxIndex : 3, allParriedFxPosition);
                if (ScreenshackManager.Instance != null)
                    ScreenshackManager.Instance.TriggerScreenShake(ScreenShakeStrength.Medium);
                if (enemySpriteDirection != null)
                {
                    if (_enemyShortHurtCoroutine != null)
                    {
                        StopCoroutine(_enemyShortHurtCoroutine);
                        _enemyShortHurtCoroutine = null;
                    }
                    enemySpriteDirection.isHurt = true;
                }
                if (floatingFeedback != null)
                    floatingFeedback.ShowCombo(allParriedFxPosition);
                if (OptionManager.GetHapticEnabledFromPrefs() && SystemInfo.deviceType == DeviceType.Handheld)
                    Handheld.Vibrate();
            }

            _totalCombos++;
            _totalAttacks += numberOfAttaques;
            for (int i = 0; i < _parriedInCombo.Count; i++)
            {
                if (_parriedInCombo[i]) _totalParries++;
                if (i < _perfectParryInCombo.Count && _perfectParryInCombo[i]) _totalPerfectParries++;
            }

            float pauseDuration = _effectiveConfig.PauseBetweenComboDuration + _extraPauseBeforeNextCombo;
            _extraPauseBeforeNextCombo = 0f;
            if (!_testMode && _playerCurrentLife > 0 && _enemyCurrentLife > 0 && pauseDuration > 0f)
                yield return new WaitForSeconds(pauseDuration);
        }

        if (_enemyCurrentLife <= 0)
        {
            if (enemySpriteDirection != null)
                enemySpriteDirection.isDown = true;
            OnGameEnd(GameEndResult.PlayerWins);
        }
        else
        {
            if (playerSpriteManager != null)
                playerSpriteManager.isDead = true;
            OnGameEnd(GameEndResult.EnemyWins);
        }

        _mainLoopCoroutine = null;
    }

    /// <summary>
    /// Handles game end: switches music, logs result, and raises the GameEnded event.
    /// </summary>
    /// <param name="result">Whether the player or the enemy won.</param>
    private void OnGameEnd(GameEndResult result)
    {
        bool wasSimulation = _fightSimulationActive;
        if (_fightSimulationActive)
        {
            Time.timeScale = 1f;
            Application.targetFrameRate = DefaultTargetFrameRate;
            _fightSimulationActive = false;
        }
        if (wasSimulation && _effectiveConfig != null && _fightStartTime >= 0f)
            LogSimulationStats(result);
        if (musicSwitchManager != null)
        {
            AudioClip clip = result == GameEndResult.PlayerWins ? musicOnPlayerWins : musicOnEnemyWins;
            if (clip != null)
                musicSwitchManager.SwitchMusic(clip, gameEndMusicTransitionDuration, false);
        }

        if (result == GameEndResult.PlayerWins)
        {
            Debug.Log("Player wins");
            if (RogueliteRunState.IsRunActive && RogueliteRunState.Instance != null)
                RogueliteRunState.Instance.RecordPlayerLifeAfterFight(_playerCurrentLife);
        }
        else
            Debug.Log("Enemy wins");

        if (GameplayEvents.Instance != null)
            GameplayEvents.Instance.InvokeGameEnd(result);
        GameEnded?.Invoke(result);
    }

    /// <summary>Logs fight statistics after a simulation ends (duration, attacks, combos, parries, damage, rates).</summary>
    private void LogSimulationStats(GameEndResult result)
    {
        float durationRealtimeSec = Time.realtimeSinceStartup - _fightStartTime;
        float durationSimulationSec = durationRealtimeSec * _simulationTimeScale;
        int misses = _totalAttacks - _totalParries;
        float playerDamageTaken = _effectiveConfig.PlayerStartLife - _playerCurrentLife;
        float enemyDamageDealt = _effectiveConfig.EnemyStartLife - Mathf.Max(0f, _enemyCurrentLife);
        float parryRatePct = _totalAttacks > 0 ? (100f * _totalParries / _totalAttacks) : 0f;
        float perfectRateAmongParriesPct = _totalParries > 0 ? (100f * _totalPerfectParries / _totalParries) : 0f;
        float avgAttacksPerCombo = _totalCombos > 0 ? (float)_totalAttacks / _totalCombos : 0f;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== Fight simulation stats ===");
        sb.AppendLine("Result: " + (result == GameEndResult.PlayerWins ? "Player wins" : "Enemy wins"));
        sb.AppendLine("Duration: " + durationSimulationSec.ToString("F2") + " s (simulation @ " + _simulationTimeScale.ToString("F1") + "x) | " + durationRealtimeSec.ToString("F2") + " s realtime");
        sb.AppendLine("Combos: " + _totalCombos);
        sb.AppendLine("Attacks: " + _totalAttacks + " (avg " + avgAttacksPerCombo.ToString("F1") + " per combo)");
        sb.AppendLine("Parries: " + _totalParries + " (" + parryRatePct.ToString("F1") + "% of attacks)");
        sb.AppendLine("Perfect parries: " + _totalPerfectParries + " (" + perfectRateAmongParriesPct.ToString("F1") + "% of parries)");
        sb.AppendLine("Misses: " + misses);
        sb.AppendLine("Player damage taken: " + playerDamageTaken.ToString("F2") + " / " + _effectiveConfig.PlayerStartLife);
        sb.AppendLine("Enemy damage dealt: " + enemyDamageDealt.ToString("F2") + " / " + _effectiveConfig.EnemyStartLife);
        sb.AppendLine("Revive used: " + (_reviveUsedThisFight ? "yes" : "no"));
        sb.AppendLine("Player life remaining: " + _playerCurrentLife.ToString("F2"));
        sb.AppendLine("Enemy life remaining: " + Mathf.Max(0f, _enemyCurrentLife).ToString("F2"));
        sb.Append("==============================");
        Debug.Log(sb.ToString());
    }
}
