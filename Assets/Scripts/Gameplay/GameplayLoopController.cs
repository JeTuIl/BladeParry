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
    [SerializeField] private float hitStopTimeScale = 0.92f;
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

    /// <summary>Raised when the game ends, with the result (player or enemy wins).</summary>
    public event System.Action<GameEndResult> GameEnded;

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
    /// </summary>
    /// <param name="attaqueDirection">Direction of the incoming attack.</param>
    private void OnParryWindowOpen(Direction attaqueDirection)
    {
        _parryWindowActive = true;
        _isInWindDown = false;
        _currentAttaqueDirection = attaqueDirection;
        _parriedInCombo.Add(false);
        _perfectParryInCombo.Add(false);
    }

    /// <summary>Called when wind-down starts; parry during this phase is a perfect parry.</summary>
    private void OnWindDownStart()
    {
        _isInWindDown = true;
    }

    /// <summary>
    /// Called when a parry window closes; if the last attack was not parried, applies player damage and missed-parry FX.
    /// </summary>
    /// <param name="attaqueDirection">Direction of the attack that just closed.</param>
    private void OnParryWindowClose(Direction attaqueDirection)
    {
        if (_parriedInCombo.Count > 0 && !_parriedInCombo[_parriedInCombo.Count - 1])
        {
            _playerCurrentLife -= 1f;
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
                characterComboSequence.StopComboAndNotifyComplete();
        }

        _parryWindowActive = false;
        _isInWindDown = false;
        _currentAttaqueDirection = Direction.Neutral;
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
            _parriedInCombo[_parriedInCombo.Count - 1] = true;
            bool isPerfectParry = _isInWindDown;
            _perfectParryInCombo[_perfectParryInCombo.Count - 1] = isPerfectParry;
            _parryWindowActive = false;

            float parryDamage = isPerfectParry
                ? _effectiveConfig.DamageOnParry * _effectiveConfig.DamagePerfectRatio
                : _effectiveConfig.DamageOnParry;
            _enemyCurrentLife = Mathf.Max(0f, _enemyCurrentLife - parryDamage);
            UpdateLifebars();
            UpdateMusicPitch();

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
                _perfectParriesInARow++;
                UpdatePerfectParryComboDisplay();
            }
            else
            {
                _perfectParriesInARow = 0;
                UpdatePerfectParryComboDisplay();
            }
            if (OptionManager.GetHapticEnabledFromPrefs() && SystemInfo.deviceType == DeviceType.Handheld)
                Handheld.Vibrate();
        }
    }

    /// <summary>Pushes current perfect parry count to the combo display.</summary>
    private void UpdatePerfectParryComboDisplay()
    {
        if (perfectParryComboDisplay != null)
            perfectParryComboDisplay.SetPerfectParryCount(_perfectParriesInARow);
    }

    /// <summary>
    /// Brief time scale dip for parry hit stop.
    /// </summary>
    private IEnumerator HitStopCoroutine()
    {
        Time.timeScale = hitStopTimeScale;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
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
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator MainLoopCoroutine()
    {
        yield return StartCoroutine(PreparationPhaseCoroutine());

        while (_playerCurrentLife > 0 && _enemyCurrentLife > 0)
        {
            if (enemySpriteDirection != null)
                enemySpriteDirection.isHurt = false;

            int numberOfAttaques;
            float durationBetweenAttaque, windUpDuration, windDownDuration;

            float lifeRatio = _enemyCurrentLife / _effectiveConfig.EnemyStartLife;

            numberOfAttaques = (int)Mathf.Lerp(_effectiveConfig.EmptyLifeComboNumberOfAttaques, _effectiveConfig.FullLifeComboNumberOfAttaques, lifeRatio);
            durationBetweenAttaque = Mathf.Lerp(_effectiveConfig.EmptyLifeDurationBetweenAttaque, _effectiveConfig.FullLifeDurationBetweenAttaque, lifeRatio);
            windUpDuration = Mathf.Lerp(_effectiveConfig.EmptyLifeWindUpDuration, _effectiveConfig.FullLifeWindUpDuration, lifeRatio);
            windDownDuration = Mathf.Lerp(_effectiveConfig.EmptyLifeWindDownDuration, _effectiveConfig.FullLifeWindDownDuration, lifeRatio);

            _parriedInCombo.Clear();
            _perfectParryInCombo.Clear();
            _parryWindowActive = false;
            _isInWindDown = false;
            _currentAttaqueDirection = Direction.Neutral;
            _comboComplete = false;

            SubscribeToComboAndInput();
            characterComboSequence.TriggerComboWithParameters(numberOfAttaques, durationBetweenAttaque, windUpDuration, windDownDuration);

            while (!_comboComplete)
                yield return null;

            UnsubscribeFromComboAndInput();

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

            if (allParried)
            {
                _enemyCurrentLife = Mathf.Max(0f, _enemyCurrentLife - _effectiveConfig.DamageOnComboParry);
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
                    enemySpriteDirection.isHurt = true;
                if (floatingFeedback != null)
                    floatingFeedback.ShowCombo(allParriedFxPosition);
                if (OptionManager.GetHapticEnabledFromPrefs() && SystemInfo.deviceType == DeviceType.Handheld)
                    Handheld.Vibrate();
            }

            float pauseDuration = _effectiveConfig.PauseBetweenComboDuration;
            if (_playerCurrentLife > 0 && _enemyCurrentLife > 0 && pauseDuration > 0f)
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
        if (musicSwitchManager != null)
        {
            AudioClip clip = result == GameEndResult.PlayerWins ? musicOnPlayerWins : musicOnEnemyWins;
            if (clip != null)
                musicSwitchManager.SwitchMusic(clip, gameEndMusicTransitionDuration, false);
        }

        if (result == GameEndResult.PlayerWins)
            Debug.Log("Player wins");
        else
            Debug.Log("Enemy wins");

        if (GameplayEvents.Instance != null)
            GameplayEvents.Instance.InvokeGameEnd(result);
        GameEnded?.Invoke(result);
    }
}
