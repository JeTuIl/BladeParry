using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Runs a single attack sequence: wind-up (sprite and RectTransform move), parry window with FX, then wind-down.
/// Fires events when the parry window opens and closes and when the attack ends.
/// </summary>
public class CharacterAttaqueSequence : MonoBehaviour
{
    /// <summary>Controls the character's facing sprite during the attack.</summary>
    [SerializeField] private CharacterSpriteDirection characterSpriteDirection;

    /// <summary>RectTransform that is moved during wind-up and wind-down.</summary>
    [SerializeField] private RectTransform rectTransform;

    /// <summary>Distance in units the RectTransform moves during wind-up.</summary>
    [SerializeField] private float movementDistance = 50f;

    /// <summary>Wind-up duration used by the context menu test.</summary>
    [SerializeField] private float testWindUpDuration = 0.3f;

    /// <summary>Wind-down duration used by the context menu test.</summary>
    [SerializeField] private float testWindDownDuration = 0.2f;

    /// <summary>Position where attack FX are spawned.</summary>
    [SerializeField] private Vector3 fxSpawnPosition;

    /// <summary>Easing curve for wind-up (time 0-1). If null, linear is used.</summary>
    [SerializeField] private AnimationCurve windUpCurve;

    /// <summary>Easing curve for wind-down (time 0-1). Ease-out makes the strike snap at the end. If null, linear is used.</summary>
    [SerializeField] private AnimationCurve windDownCurve;

    /// <summary>Invoked when the attack sequence has fully completed.</summary>
    [SerializeField] private UnityEvent<Direction> onAttaqueEnd;

    /// <summary>Invoked when the parry window opens (start of wind-down).</summary>
    [SerializeField] public UnityEvent<Direction> onParryWindowOpen;

    /// <summary>Invoked when the parry window closes (end of wind-down).</summary>
    [SerializeField] public UnityEvent<Direction> onParryWindowClose;

    /// <summary>Invoked when wind-down starts (player can now perfect parry).</summary>
    [SerializeField] public UnityEvent onWindDownStart;

    /// <summary>Active attack coroutine; null when no attack is running.</summary>
    private Coroutine _attaqueCoroutine;

    /// <summary>Cardinal directions used for random test attacks (excludes Neutral).</summary>
    private static readonly Direction[] AttackDirections = { Direction.Up, Direction.Left, Direction.Right, Direction.Down };

    /// <summary>
    /// Returns a unit Vector2 for the given cardinal direction.
    /// </summary>
    /// <param name="direction">The direction.</param>
    /// <returns>Unit vector (or zero for Neutral).</returns>
    private static Vector2 GetDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Vector2(0f, 1f),
            Direction.Down => new Vector2(0f, -1f),
            Direction.Left => new Vector2(-1f, 0f),
            Direction.Right => new Vector2(1f, 0f),
            _ => Vector2.zero
        };
    }

    /// <summary>
    /// Returns the opposite cardinal direction.
    /// </summary>
    /// <param name="direction">The direction.</param>
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
    /// Starts a single attack in the given direction with the specified wind-up and wind-down durations.
    /// Stops any running attack first.
    /// </summary>
    /// <param name="attaqueDirection">Direction of the attack (and parry window).</param>
    /// <param name="windUpDuration">Duration of the wind-up phase in seconds.</param>
    /// <param name="windDownDuration">Duration of the wind-down (parry window) phase in seconds.</param>
    public void StartAttaque(Direction attaqueDirection, float windUpDuration, float windDownDuration)
    {
        if (characterSpriteDirection == null || rectTransform == null)
        {
            Debug.LogWarning("CharacterAttaqueSequence: CharacterSpriteDirection or RectTransform is null.", this);
            return;
        }
        if (windUpDuration <= 0f || windDownDuration <= 0f)
        {
            Debug.LogWarning("CharacterAttaqueSequence: windUpDuration and windDownDuration must be positive.", this);
            return;
        }

        if (_attaqueCoroutine != null)
            StopCoroutine(_attaqueCoroutine);

        _attaqueCoroutine = StartCoroutine(AttaqueSequenceCoroutine(attaqueDirection, windUpDuration, windDownDuration));
    }

    /// <summary>
    /// Coroutine: opens parry window, wind-up, then wind-down with FX, then closes parry window and invokes end event.
    /// </summary>
    /// <param name="attaqueDirection">Direction of the attack.</param>
    /// <param name="windUpDuration">Wind-up duration in seconds.</param>
    /// <param name="windDownDuration">Wind-down duration in seconds.</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator AttaqueSequenceCoroutine(Direction attaqueDirection, float windUpDuration, float windDownDuration)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 oppositeVector = GetDirectionVector(GetOppositeDirection(attaqueDirection));
        Vector2 windUpEndPosition = startPosition + oppositeVector * movementDistance;

        onParryWindowOpen?.Invoke(attaqueDirection);

        // Step 1: Wind-up
        characterSpriteDirection.SetDirection(GetOppositeDirection(attaqueDirection));
        float elapsed = 0f;
        while (elapsed < windUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / windUpDuration);
            float eased = windUpCurve != null ? windUpCurve.Evaluate(t) : t;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, windUpEndPosition, eased);
            yield return null;
        }
        rectTransform.anchoredPosition = windUpEndPosition;

        onWindDownStart?.Invoke();

        // Step 2: Wind-down (parry window)
        characterSpriteDirection.SetDirection(attaqueDirection);
        if (FxManager.Instance != null)
            FxManager.Instance.SpawnAtPosition(0, fxSpawnPosition, attaqueDirection);
        elapsed = 0f;
        while (elapsed < windDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / windDownDuration);
            float eased = windDownCurve != null ? windDownCurve.Evaluate(t) : t;
            rectTransform.anchoredPosition = Vector2.Lerp(windUpEndPosition, startPosition, eased);
            yield return null;
        }
        rectTransform.anchoredPosition = startPosition;

        // Step 3: Attaque end
        onParryWindowClose?.Invoke(attaqueDirection);
        characterSpriteDirection.SetDirection(Direction.Neutral);
        rectTransform.anchoredPosition = startPosition;
        onAttaqueEnd?.Invoke(attaqueDirection);
        _attaqueCoroutine = null;
    }

    /// <summary>
    /// Context menu: starts a test attack in a random cardinal direction using test wind-up/wind-down durations.
    /// </summary>
    [ContextMenu("Test Attaque (Random Direction)")]
    private void TestAttaqueRandomDirection()
    {
        Direction randomDirection = AttackDirections[Random.Range(0, AttackDirections.Length)];
        StartAttaque(randomDirection, testWindUpDuration, testWindDownDuration);
    }
}
