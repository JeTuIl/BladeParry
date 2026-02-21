using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the player's sprite: direction (from swipe), hurt and dead states, and attack animation with FX.
/// Subscribes to SlideDetection for direction and drives attack wind-up/wind-down with FxManager.
/// </summary>
public class PlayerSpriteManager : MonoBehaviour
{
    /// <summary>Image that displays the player sprite.</summary>
    [SerializeField] private Image _image;

    /// <summary>Current parry/attack direction set by swipe; Neutral when idle.</summary>
    public Direction Direction;

    /// <summary>Sprite when idle (Neutral).</summary>
    [SerializeField] private Sprite spriteNeutral;

    /// <summary>Sprite when facing or attacking up.</summary>
    [SerializeField] private Sprite spriteUp;

    /// <summary>Sprite when facing or attacking left.</summary>
    [SerializeField] private Sprite spriteLeft;

    /// <summary>Sprite when facing or attacking right.</summary>
    [SerializeField] private Sprite spriteRight;

    /// <summary>Sprite when facing or attacking down.</summary>
    [SerializeField] private Sprite spriteDown;

    /// <summary>Sprite shown during hurt state.</summary>
    [SerializeField] private Sprite spriteHurt;

    /// <summary>Sprite shown when dead.</summary>
    [SerializeField] private Sprite spriteDead;

    /// <summary>Duration of the attack wind-up phase.</summary>
    [SerializeField] private float windUpDuration = 0.3f;

    /// <summary>Duration of the attack wind-down phase.</summary>
    [SerializeField] private float windDownDuration = 0.2f;

    /// <summary>Duration the hurt sprite is shown when TriggerHurt is called.</summary>
    [SerializeField] private float hurtDuration = 0.5f;

    /// <summary>World position where the player attack FX is spawned.</summary>
    [SerializeField] private Vector3 attackFxSpawnPosition;

    /// <summary>When true, the hurt sprite is shown.</summary>
    public bool isHurt;

    /// <summary>When true, the dead sprite is shown.</summary>
    public bool isDead;

    /// <summary>Source of swipe direction for parry/attack.</summary>
    [SerializeField] private SlideDetection slideDetection;

    /// <summary>Active attack coroutine; null when not attacking.</summary>
    private Coroutine _attackCoroutine;

    /// <summary>Active hurt coroutine; null when not in hurt state.</summary>
    private Coroutine _hurtCoroutine;

    /// <summary>Direction of the attack currently playing.</summary>
    private Direction _attackDirection;

    /// <summary>Current phase of the attack: 0 = wind-up, 1 = wind-down.</summary>
    private int _attackPhase; // 0 = wind-up, 1 = wind-down

    /// <summary>
    /// Subscribes to slide detection for direction and applies the initial sprite.
    /// </summary>
    private void Start()
    {
        if (slideDetection != null)
            slideDetection.onSwipeDetected.AddListener(SetDirection);
        ApplySprite();
    }

    /// <summary>
    /// Sets the current direction (e.g. from a swipe).
    /// </summary>
    /// <param name="direction">The direction to set.</param>
    public void SetDirection(Direction direction)
    {
        Direction = direction;
    }

    /// <summary>
    /// Triggers the hurt state: shows hurt sprite for hurtDuration then clears it.
    /// </summary>
    public void TriggerHurt()
    {
        if (_hurtCoroutine != null)
            StopCoroutine(_hurtCoroutine);
        _hurtCoroutine = StartCoroutine(HurtSequenceCoroutine());
    }

    /// <summary>
    /// Sets isHurt true, waits for hurtDuration, then sets isHurt false.
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator HurtSequenceCoroutine()
    {
        isHurt = true;
        float duration = Mathf.Max(hurtDuration, 0f);
        yield return new WaitForSeconds(duration);
        isHurt = false;
        _hurtCoroutine = null;
    }

    /// <summary>
    /// Applies the current sprite and starts an attack sequence when direction is non-Neutral and not hurt/dead.
    /// </summary>
    private void Update()
    {
        ApplySprite();

        if (!isDead && !isHurt && Direction != Direction.Neutral && _attackCoroutine == null)
        {
            _attackCoroutine = StartCoroutine(AttackSequenceCoroutine());
        }
    }

    /// <summary>
    /// Chooses sprite based on dead, hurt, attack phase, or direction and assigns it to the image.
    /// </summary>
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

    /// <summary>
    /// Returns the sprite for the given direction.
    /// </summary>
    /// <param name="d">The direction.</param>
    /// <returns>The corresponding sprite, or spriteNeutral for Neutral/unknown.</returns>
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

    /// <summary>
    /// Returns the opposite cardinal direction.
    /// </summary>
    /// <param name="d">The direction.</param>
    /// <returns>The opposite direction, or Neutral for Neutral.</returns>
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

    /// <summary>
    /// Returns a quaternion for orienting attack FX based on direction (Z rotation).
    /// </summary>
    /// <param name="direction">The attack direction.</param>
    /// <returns>Rotation for the FX.</returns>
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

    /// <summary>
    /// Returns the scale to apply to attack FX for the given direction.
    /// </summary>
    /// <param name="direction">The attack direction.</param>
    /// <returns>Scale vector for the FX.</returns>
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

    /// <summary>
    /// Runs the attack: wind-up (inverse sprite), spawn FX, wind-down, then reset direction to Neutral.
    /// </summary>
    /// <returns>Enumerator for the coroutine.</returns>
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
