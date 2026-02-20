using UnityEngine;
using UnityEngine.UI;

public enum Direction
{
    Neutral,
    Up,
    Left,
    Right,
    Down
}

public class CharacterSpriteDirection : MonoBehaviour
{
    [SerializeField] private Sprite spriteNeutral;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;
    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteHurt;
    [SerializeField] private Sprite spriteDownState;

    public bool isHurt;
    public bool isDown;

    [SerializeField] private float scaleNeutral = 1f;
    [SerializeField] private float scaleUp = 1f;
    [SerializeField] private float scaleLeft = 1f;
    [SerializeField] private float scaleRight = 1f;
    [SerializeField] private float scaleDown = 1f;
    [SerializeField] private float scaleHurt = 1f;
    [SerializeField] private float scaleDownState = 1f;

    [SerializeField] private Direction _currentDirection;
    [SerializeField] private Image _image;

    private Direction _lastAppliedDirection;
    private bool _lastAppliedHurt;
    private bool _lastAppliedDown;

    private void Start()
    {
        ApplySprite();
        _lastAppliedDirection = _currentDirection;
        _lastAppliedHurt = isHurt;
        _lastAppliedDown = isDown;
    }

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

    public void SetDirection(Direction direction)
    {
        _currentDirection = direction;
        _lastAppliedDirection = direction;
        ApplySprite();
    }

    public Direction CurrentDirection => _currentDirection;

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
