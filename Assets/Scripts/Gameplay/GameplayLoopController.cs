using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum GameEndResult
{
    PlayerWins,
    EnemyWins
}

public class GameplayLoopController : MonoBehaviour
{
    [SerializeField] private GameplayConfig config;
    [SerializeField] private CharacterComboSequence characterComboSequence;
    [SerializeField] private CharacterAttaqueSequence characterAttaqueSequence;
    [SerializeField] private SlideDetection slideDetection;
    [SerializeField] private LifebarManager playerLifebarManager;
    [SerializeField] private LifebarManager enemyLifebarManager;
    [SerializeField] private CharacterSpriteDirection enemySpriteDirection;
    [SerializeField] private Vector3 parryFxPosition;
    [SerializeField] private Vector3 missedParryFxPosition;
    [SerializeField] private Vector3 allParriedFxPosition;
    [SerializeField] private MusicPitchManager musicPitchManager;
    [SerializeField] private float fullLifeMusicSpeed = 1f;
    [SerializeField] private float emptyLifeMusicSpeed = 1f;

    private int _playerCurrentLife;
    private int _enemyCurrentLife;
    private bool _parryWindowActive;
    private Direction _currentAttaqueDirection;
    private readonly List<bool> _parriedInCombo = new List<bool>();
    private bool _comboComplete;
    private UnityAction _onComboCompleteHandler;
    private UnityAction<Direction> _onParryWindowOpenHandler;
    private UnityAction<Direction> _onParryWindowCloseHandler;
    private UnityAction<Direction> _onSwipeDetectedHandler;
    private Coroutine _mainLoopCoroutine;

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("GameplayLoopController: GameplayConfig is not assigned.", this);
            return;
        }
        if (characterComboSequence == null || characterAttaqueSequence == null || slideDetection == null)
        {
            Debug.LogError("GameplayLoopController: CharacterComboSequence, CharacterAttaqueSequence or SlideDetection is not assigned.", this);
            return;
        }

        _playerCurrentLife = config.PlayerStartLife;
        _enemyCurrentLife = config.EnemyStartLife;
        UpdateLifebars();
        UpdateMusicPitch();
        _mainLoopCoroutine = StartCoroutine(MainLoopCoroutine());
    }

    private void UpdateLifebars()
    {
        if (playerLifebarManager != null)
        {
            playerLifebarManager.MaxLifeValue = config.PlayerStartLife;
            playerLifebarManager.CurrentLifeValue = _playerCurrentLife;
        }
        if (enemyLifebarManager != null)
        {
            enemyLifebarManager.MaxLifeValue = config.EnemyStartLife;
            enemyLifebarManager.CurrentLifeValue = _enemyCurrentLife;
        }
    }

    private void UpdateMusicPitch()
    {
        if (musicPitchManager == null)
            return;
        float lifeRatio = (float)_enemyCurrentLife / config.EnemyStartLife;
        musicPitchManager.pitch = Mathf.Lerp(emptyLifeMusicSpeed, fullLifeMusicSpeed, lifeRatio);
    }

    private void OnDestroy()
    {
        UnsubscribeFromComboAndInput();
    }

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

    private void SubscribeToComboAndInput()
    {
        _onComboCompleteHandler = OnComboComplete;
        _onParryWindowOpenHandler = OnParryWindowOpen;
        _onParryWindowCloseHandler = OnParryWindowClose;
        _onSwipeDetectedHandler = OnSwipeDetected;

        characterComboSequence.onComboComplete.AddListener(_onComboCompleteHandler);
        characterAttaqueSequence.onParryWindowOpen.AddListener(_onParryWindowOpenHandler);
        characterAttaqueSequence.onParryWindowClose.AddListener(_onParryWindowCloseHandler);
        slideDetection.onSwipeDetected.AddListener(_onSwipeDetectedHandler);
    }

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
        }
        if (slideDetection != null && _onSwipeDetectedHandler != null)
            slideDetection.onSwipeDetected.RemoveListener(_onSwipeDetectedHandler);

        _onComboCompleteHandler = null;
        _onParryWindowOpenHandler = null;
        _onParryWindowCloseHandler = null;
        _onSwipeDetectedHandler = null;
    }

    private void OnParryWindowOpen(Direction attaqueDirection)
    {
        _parryWindowActive = true;
        _currentAttaqueDirection = attaqueDirection;
        _parriedInCombo.Add(false);
    }

    private void OnParryWindowClose(Direction attaqueDirection)
    {
        if (_parriedInCombo.Count > 0 && !_parriedInCombo[_parriedInCombo.Count - 1])
        {
            _playerCurrentLife--;
            UpdateLifebars();
            if (FxManager.Instance != null)
                FxManager.Instance.SpawnAtPosition(2, missedParryFxPosition);
        }

        _parryWindowActive = false;
        _currentAttaqueDirection = Direction.Neutral;
    }

    private void OnSwipeDetected(Direction swipeDirection)
    {
        if (!_parryWindowActive || _parriedInCombo.Count == 0)
            return;

        Direction expectedParryDirection = GetOppositeDirection(_currentAttaqueDirection);
        if (swipeDirection == expectedParryDirection)
        {
            _parriedInCombo[_parriedInCombo.Count - 1] = true;
            _parryWindowActive = false;
            if (FxManager.Instance != null)
                FxManager.Instance.SpawnAtPosition(1, parryFxPosition);
        }
    }

    private void OnComboComplete()
    {
        _comboComplete = true;
    }

    private IEnumerator MainLoopCoroutine()
    {
        while (_playerCurrentLife > 0 && _enemyCurrentLife > 0)
        {
            if (enemySpriteDirection != null)
                enemySpriteDirection.isHurt = false;

            int numberOfAttaques;
            float durationBetweenAttaque, windUpDuration, windDownDuration;

            float lifeRatio = (float)_enemyCurrentLife / config.EnemyStartLife;

            numberOfAttaques = (int)Mathf.Lerp(config.EmptyLifeComboNumberOfAttaques, config.FullLifeComboNumberOfAttaques, lifeRatio);
            durationBetweenAttaque = Mathf.Lerp(config.EmptyLifeDurationBetweenAttaque, config.FullLifeDurationBetweenAttaque, lifeRatio);
            windUpDuration = Mathf.Lerp(config.EmptyLifeWindUpDuration, config.FullLifeWindUpDuration, lifeRatio);
            windDownDuration = Mathf.Lerp(config.EmptyLifeWindDownDuration, config.FullLifeWindDownDuration, lifeRatio);

            _parriedInCombo.Clear();
            _parryWindowActive = false;
            _currentAttaqueDirection = Direction.Neutral;
            _comboComplete = false;

            SubscribeToComboAndInput();
            characterComboSequence.TriggerComboWithParameters(numberOfAttaques, durationBetweenAttaque, windUpDuration, windDownDuration);

            while (!_comboComplete)
                yield return null;

            UnsubscribeFromComboAndInput();

            bool allParried = _parriedInCombo.Count > 0;
            for (int i = 0; i < _parriedInCombo.Count; i++)
            {
                if (!_parriedInCombo[i])
                {
                    allParried = false;
                    break;
                }
            }
            if (allParried)
            {
                _enemyCurrentLife--;
                UpdateLifebars();
                UpdateMusicPitch();
                if (FxManager.Instance != null)
                    FxManager.Instance.SpawnAtPosition(3, allParriedFxPosition);
                if (enemySpriteDirection != null)
                    enemySpriteDirection.isHurt = true;
            }

            float pauseDuration = config.PauseBetweenComboDuration;
            if (pauseDuration > 0f)
                yield return new WaitForSeconds(pauseDuration);
        }

        if (_enemyCurrentLife <= 0)
        {
            if (enemySpriteDirection != null)
                enemySpriteDirection.isDown = true;
            OnGameEnd(GameEndResult.PlayerWins);
        }
        else
            OnGameEnd(GameEndResult.EnemyWins);

        _mainLoopCoroutine = null;
    }

    private void OnGameEnd(GameEndResult result)
    {
        if (result == GameEndResult.PlayerWins)
            Debug.Log("Player wins");
        else
            Debug.Log("Enemy wins");
    }
}
