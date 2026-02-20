using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterAttaqueSequence : MonoBehaviour
{
    [SerializeField] private CharacterSpriteDirection characterSpriteDirection;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private float movementDistance = 50f;
    [SerializeField] private float testWindUpDuration = 0.3f;
    [SerializeField] private float testWindDownDuration = 0.2f;
    [SerializeField] private UnityEvent<Direction> onAttaqueEnd;

    private Coroutine _attaqueCoroutine;

    private static readonly Direction[] AttackDirections = { Direction.Up, Direction.Left, Direction.Right, Direction.Down };

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

    private IEnumerator AttaqueSequenceCoroutine(Direction attaqueDirection, float windUpDuration, float windDownDuration)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 oppositeVector = GetDirectionVector(GetOppositeDirection(attaqueDirection));
        Vector2 windUpEndPosition = startPosition + oppositeVector * movementDistance;

        // Step 1: Wind-up
        characterSpriteDirection.SetDirection(GetOppositeDirection(attaqueDirection));
        float elapsed = 0f;
        while (elapsed < windUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / windUpDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, windUpEndPosition, t);
            yield return null;
        }
        rectTransform.anchoredPosition = windUpEndPosition;

        // Step 2: Wind-down
        characterSpriteDirection.SetDirection(attaqueDirection);
        elapsed = 0f;
        while (elapsed < windDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / windDownDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(windUpEndPosition, startPosition, t);
            yield return null;
        }
        rectTransform.anchoredPosition = startPosition;

        // Step 3: Attaque end
        characterSpriteDirection.SetDirection(Direction.Neutral);
        rectTransform.anchoredPosition = startPosition;
        onAttaqueEnd?.Invoke(attaqueDirection);
        _attaqueCoroutine = null;
    }

    [ContextMenu("Test Attaque (Random Direction)")]
    private void TestAttaqueRandomDirection()
    {
        Direction randomDirection = AttackDirections[Random.Range(0, AttackDirections.Length)];
        StartAttaque(randomDirection, testWindUpDuration, testWindDownDuration);
    }
}
