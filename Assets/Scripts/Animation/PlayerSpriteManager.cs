using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpriteManager : MonoBehaviour
{
    [SerializeField] private Image _image;

    public Direction Direction;

    [SerializeField] private Sprite spriteNeutral;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteHurt;
    [SerializeField] private Sprite spriteDead;

    [SerializeField] private float windUpDuration = 0.3f;
    [SerializeField] private float windDownDuration = 0.2f;

    [SerializeField] private float hurtDuration = 0.5f;

    [SerializeField] private Vector3 attackFxSpawnPosition;

    public bool isHurt;
    public bool isDead;

    [SerializeField] private SlideDetection slideDetection;

    private Coroutine _attackCoroutine;
    private Coroutine _hurtCoroutine;
    private Direction _attackDirection;
    private int _attackPhase; // 0 = wind-up, 1 = wind-down

    private void Start()
    {
        if (slideDetection != null)
            slideDetection.onSwipeDetected.AddListener(SetDirection);
        ApplySprite();
    }

    public void SetDirection(Direction direction)
    {
        Direction = direction;
    }

    public void TriggerHurt()
    {
        if (_hurtCoroutine != null)
            StopCoroutine(_hurtCoroutine);
        _hurtCoroutine = StartCoroutine(HurtSequenceCoroutine());
    }

    private IEnumerator HurtSequenceCoroutine()
    {
        isHurt = true;
        float duration = Mathf.Max(hurtDuration, 0f);
        yield return new WaitForSeconds(duration);
        isHurt = false;
        _hurtCoroutine = null;
    }

    private void Update()
    {
        ApplySprite();

        if (!isDead && !isHurt && Direction != Direction.Neutral && _attackCoroutine == null)
        {
            _attackCoroutine = StartCoroutine(AttackSequenceCoroutine());
        }
    }

    private void ApplySprite()
    {
        if (_image == null)
            return;

        Sprite sprite = null;

        if (isDead)
        {
            sprite = spriteDead;
        }
        else if (isHurt)
        {
            sprite = spriteHurt;
        }
        else if (_attackCoroutine != null)
        {
            sprite = _attackPhase == 0
                ? GetSpriteForDirection(GetInverseDirection(_attackDirection))
                : GetSpriteForDirection(_attackDirection);
        }
        else if (Direction == Direction.Neutral)
        {
            sprite = spriteNeutral;
        }
        else
        {
            // About to start attack: show wind-up (inverse) sprite
            sprite = GetSpriteForDirection(GetInverseDirection(Direction));
        }

        if (sprite != null)
            _image.sprite = sprite;
    }

    private Sprite GetSpriteForDirection(Direction d)
    {
        return d switch
        {
            Direction.Neutral => spriteNeutral,
            Direction.Up => spriteUp,
            Direction.Left => spriteLeft,
            Direction.Right => spriteRight,
            Direction.Down => spriteDown,
            _ => spriteNeutral
        };
    }

    private static Direction GetInverseDirection(Direction d)
    {
        return d switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.Neutral
        };
    }

    private static Quaternion GetRotationForAttackDirection(Direction direction)
    {
        float zAngle = direction switch
        {
            Direction.Down => 0f,
            Direction.Right => 180f,
            Direction.Up => 90f,
            Direction.Left => -90f,
            Direction.Neutral => 0f,
            _ => 0f
        };
        return Quaternion.Euler(0f, 0f, zAngle);
    }

    private static Vector3 GetScaleForAttackDirection(Direction direction)
    {
        Vector3 baseScale = direction switch
        {
            Direction.Down => new Vector3(1f, 1f, 1f),
            Direction.Right => new Vector3(1f, 1f, 1f),
            Direction.Up => new Vector3(-1f, 1f, 1f),
            Direction.Left => new Vector3(-1f, 1f, 1f),
            Direction.Neutral => Vector3.one,
            _ => Vector3.one
        };
        return baseScale * 2f;
    }

    private IEnumerator AttackSequenceCoroutine()
    {
        _attackDirection = Direction;
        _attackPhase = 0;

        float windUp = Mathf.Max(windUpDuration, 0.001f);
        float windDown = Mathf.Max(windDownDuration, 0.001f);

        yield return new WaitForSeconds(windUp);

        _attackPhase = 1;
        if (FxManager.Instance != null)
            FxManager.Instance.SpawnAtPosition(4, attackFxSpawnPosition, GetRotationForAttackDirection(_attackDirection), GetScaleForAttackDirection(_attackDirection));

        yield return new WaitForSeconds(windDown);

        if (_image != null && spriteNeutral != null)
            _image.sprite = spriteNeutral;

        Direction = Direction.Neutral;
        _attackCoroutine = null;
    }
}
