using UnityEngine;

/// <summary>
/// Serializable set of sprites and scales used by CharacterSpriteDirection.
/// Used by EnemyDefinition to define enemy appearance per direction and state.
/// </summary>
[System.Serializable]
public class CharacterSpriteSet
{
    [Tooltip("Sprite shown when direction is neutral.")]
    [SerializeField] private Sprite spriteNeutral;

    [Tooltip("Sprite shown when facing up.")]
    [SerializeField] private Sprite spriteUp;

    [Tooltip("Sprite shown when facing left.")]
    [SerializeField] private Sprite spriteLeft;

    [Tooltip("Sprite shown when facing right.")]
    [SerializeField] private Sprite spriteRight;

    [Tooltip("Sprite shown when facing down.")]
    [SerializeField] private Sprite spriteDown;

    [Tooltip("Sprite shown when the character is hurt.")]
    [SerializeField] private Sprite spriteHurt;

    [Tooltip("Sprite shown when the character is down/defeated.")]
    [SerializeField] private Sprite spriteDownState;

    [Tooltip("Scale applied when direction is neutral.")]
    [SerializeField] private float scaleNeutral = 1f;

    [Tooltip("Scale applied when facing up.")]
    [SerializeField] private float scaleUp = 1f;

    [Tooltip("Scale applied when facing left.")]
    [SerializeField] private float scaleLeft = 1f;

    [Tooltip("Scale applied when facing right.")]
    [SerializeField] private float scaleRight = 1f;

    [Tooltip("Scale applied when facing down.")]
    [SerializeField] private float scaleDown = 1f;

    [Tooltip("Scale applied when hurt.")]
    [SerializeField] private float scaleHurt = 1f;

    [Tooltip("Scale applied when down/defeated.")]
    [SerializeField] private float scaleDownState = 1f;

    public Sprite SpriteNeutral => spriteNeutral;
    public Sprite SpriteUp => spriteUp;
    public Sprite SpriteLeft => spriteLeft;
    public Sprite SpriteRight => spriteRight;
    public Sprite SpriteDown => spriteDown;
    public Sprite SpriteHurt => spriteHurt;
    public Sprite SpriteDownState => spriteDownState;
    public float ScaleNeutral => scaleNeutral;
    public float ScaleUp => scaleUp;
    public float ScaleLeft => scaleLeft;
    public float ScaleRight => scaleRight;
    public float ScaleDown => scaleDown;
    public float ScaleHurt => scaleHurt;
    public float ScaleDownState => scaleDownState;
}
