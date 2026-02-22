using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cardinal and neutral directions used for attacks, parries, and sprite facing.
/// </summary>
public enum Direction
{
    /// <summary>No direction; idle or default state.</summary>
    Neutral,

    /// <summary>Upward direction.</summary>
    Up,

    /// <summary>Left direction.</summary>
    Left,

    /// <summary>Right direction.</summary>
    Right,

    /// <summary>Downward direction.</summary>
    Down
}

/// <summary>
/// Updates an Image's sprite and scale based on a direction, hurt state, or down state.
/// Used for character (e.g. enemy) facing and state visualization.
/// </summary>
public class CharacterSpriteDirection : MonoBehaviour
{
    /// <summary>Sprite shown when direction is neutral.</summary>
    [SerializeField] private Sprite spriteNeutral;

    /// <summary>Sprite shown when facing up.</summary>
    [SerializeField] private Sprite spriteUp;

    /// <summary>Sprite shown when facing left.</summary>
    [SerializeField] private Sprite spriteLeft;

    /// <summary>Sprite shown when facing right.</summary>
    [SerializeField] private Sprite spriteRight;

    /// <summary>Sprite shown when facing down.</summary>
    [SerializeField] private Sprite spriteDown;

    /// <summary>Sprite shown when the character is hurt.</summary>
    [SerializeField] private Sprite spriteHurt;

    /// <summary>Sprite shown when the character is down/defeated.</summary>
    [SerializeField] private Sprite spriteDownState;

    /// <summary>When true, the hurt sprite is shown instead of the direction sprite.</summary>
    public bool isHurt;

    /// <summary>When true, the down/defeated sprite is shown.</summary>
    public bool isDown;

    /// <summary>Scale applied when direction is neutral.</summary>
    [SerializeField] private float scaleNeutral = 1f;

    /// <summary>Scale applied when facing up.</summary>
    [SerializeField] private float scaleUp = 1f;

    /// <summary>Scale applied when facing left.</summary>
    [SerializeField] private float scaleLeft = 1f;

    /// <summary>Scale applied when facing right.</summary>
    [SerializeField] private float scaleRight = 1f;

    /// <summary>Scale applied when facing down.</summary>
    [SerializeField] private float scaleDown = 1f;

    /// <summary>Scale applied when hurt.</summary>
    [SerializeField] private float scaleHurt = 1f;

    /// <summary>Scale applied when down/defeated.</summary>
    [SerializeField] private float scaleDownState = 1f;

    /// <summary>Current facing direction used to select sprite and scale.</summary>
    [SerializeField] private Direction _currentDirection;

    /// <summary>Image component that displays the sprite.</summary>
    [SerializeField] private Image _image;

    /// <summary>Last direction applied to avoid redundant updates.</summary>
    private Direction _lastAppliedDirection;

    /// <summary>Last hurt state applied.</summary>
    private bool _lastAppliedHurt;

    /// <summary>Last down state applied.</summary>
    private bool _lastAppliedDown;

    /// <summary>
    /// Initializes the displayed sprite and caches the current state.
    /// </summary>
    private void Start()
    {
        ApplySprite();
        _lastAppliedDirection = _currentDirection;
        _lastAppliedHurt = isHurt;
        _lastAppliedDown = isDown;
    }

    /// <summary>
    /// Reapplies sprite and scale when direction, hurt, or down state changes.
    /// </summary>
    private void Update()
    {
        if (_currentDirection != _lastAppliedDirection || isHurt != _lastAppliedHurt || isDown != _lastAppliedDown)
        {
            ApplySprite();
            _lastAppliedDirection = _currentDirection;
            _lastAppliedHurt = isHurt;
            _lastAppliedDown = isDown;
        }
    }

    /// <summary>
    /// Sets the current facing direction and updates the displayed sprite immediately.
    /// </summary>
    /// <param name="direction">The direction to face.</param>
    public void SetDirection(Direction direction)
    {
        _currentDirection = direction;
        _lastAppliedDirection = direction;
        ApplySprite();
    }

    /// <summary>
    /// Gets the current facing direction.
    /// </summary>
    public Direction CurrentDirection => _currentDirection;

    /// <summary>
    /// Applies a sprite set (e.g. from EnemyDefinition) to this component. Updates all direction/state sprites and scales, then refreshes the displayed sprite.
    /// </summary>
    /// <param name="set">The sprite set to apply; null is ignored.</param>
    public void ApplySpriteConfig(CharacterSpriteSet set)
    {
        if (set == null)
            return;

        spriteNeutral = set.SpriteNeutral;
        spriteUp = set.SpriteUp;
        spriteLeft = set.SpriteLeft;
        spriteRight = set.SpriteRight;
        spriteDown = set.SpriteDown;
        spriteHurt = set.SpriteHurt;
        spriteDownState = set.SpriteDownState;
        scaleNeutral = set.ScaleNeutral;
        scaleUp = set.ScaleUp;
        scaleLeft = set.ScaleLeft;
        scaleRight = set.ScaleRight;
        scaleDown = set.ScaleDown;
        scaleHurt = set.ScaleHurt;
        scaleDownState = set.ScaleDownState;
        ApplySprite();
    }

    /// <summary>
    /// Applies the appropriate sprite and scale to the image based on down, hurt, or direction state.
    /// </summary>
    private void ApplySprite()
    {
        if (_image == null)
            return;

        Sprite sprite;
        float scale;

        if (isDown)
        {
            sprite = spriteDownState;
            scale = scaleDownState;
        }
        else if (isHurt)
        {
            sprite = spriteHurt;
            scale = scaleHurt;
        }
        else
        {
            sprite = _currentDirection switch
            {
                Direction.Neutral => spriteNeutral,
                Direction.Up => spriteUp,
                Direction.Left => spriteLeft,
                Direction.Right => spriteRight,
                Direction.Down => spriteDown,
                _ => spriteNeutral
            };

            scale = _currentDirection switch
            {
                Direction.Neutral => scaleNeutral,
                Direction.Up => scaleUp,
                Direction.Left => scaleLeft,
                Direction.Right => scaleRight,
                Direction.Down => scaleDown,
                _ => scaleNeutral
            };
        }

        if (sprite != null)
            _image.sprite = sprite;

        _image.transform.localScale = Vector3.one * scale;
    }
}
