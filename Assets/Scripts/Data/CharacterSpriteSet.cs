using UnityEngine;

/// <summary>
/// Serializable set of sprites and scales used by CharacterSpriteDirection.
/// Used by EnemyDefinition to define enemy appearance per direction and state.
/// </summary>
[System.Serializable]
public class CharacterSpriteSet
{
    /// <summary>Sprite shown when direction is neutral.</summary>
    [Tooltip("Sprite shown when direction is neutral.")]
    [SerializeField] private Sprite spriteNeutral;

    /// <summary>Sprite shown when facing up.</summary>
    [Tooltip("Sprite shown when facing up.")]
    [SerializeField] private Sprite spriteUp;

    /// <summary>Sprite shown when facing left.</summary>
    [Tooltip("Sprite shown when facing left.")]
    [SerializeField] private Sprite spriteLeft;

    /// <summary>Sprite shown when facing right.</summary>
    [Tooltip("Sprite shown when facing right.")]
    [SerializeField] private Sprite spriteRight;

    /// <summary>Sprite shown when facing down.</summary>
    [Tooltip("Sprite shown when facing down.")]
    [SerializeField] private Sprite spriteDown;

    /// <summary>Sprite shown when the character is hurt.</summary>
    [Tooltip("Sprite shown when the character is hurt.")]
    [SerializeField] private Sprite spriteHurt;

    /// <summary>Sprite shown when the character is down/defeated.</summary>
    [Tooltip("Sprite shown when the character is down/defeated.")]
    [SerializeField] private Sprite spriteDownState;

    /// <summary>Scale applied when direction is neutral.</summary>
    [Tooltip("Scale applied when direction is neutral.")]
    [SerializeField] private float scaleNeutral = 1f;

    /// <summary>Scale applied when facing up.</summary>
    [Tooltip("Scale applied when facing up.")]
    [SerializeField] private float scaleUp = 1f;

    /// <summary>Scale applied when facing left.</summary>
    [Tooltip("Scale applied when facing left.")]
    [SerializeField] private float scaleLeft = 1f;

    /// <summary>Scale applied when facing right.</summary>
    [Tooltip("Scale applied when facing right.")]
    [SerializeField] private float scaleRight = 1f;

    /// <summary>Scale applied when facing down.</summary>
    [Tooltip("Scale applied when facing down.")]
    [SerializeField] private float scaleDown = 1f;

    /// <summary>Scale applied when hurt.</summary>
    [Tooltip("Scale applied when hurt.")]
    [SerializeField] private float scaleHurt = 1f;

    /// <summary>Scale applied when down/defeated.</summary>
    [Tooltip("Scale applied when down/defeated.")]
    [SerializeField] private float scaleDownState = 1f;

    /// <summary>Gets the sprite shown when direction is neutral.</summary>
    public Sprite SpriteNeutral => spriteNeutral;

    /// <summary>Gets the sprite shown when facing up.</summary>
    public Sprite SpriteUp => spriteUp;

    /// <summary>Gets the sprite shown when facing left.</summary>
    public Sprite SpriteLeft => spriteLeft;

    /// <summary>Gets the sprite shown when facing right.</summary>
    public Sprite SpriteRight => spriteRight;

    /// <summary>Gets the sprite shown when facing down.</summary>
    public Sprite SpriteDown => spriteDown;

    /// <summary>Gets the sprite shown when the character is hurt.</summary>
    public Sprite SpriteHurt => spriteHurt;

    /// <summary>Gets the sprite shown when the character is down/defeated.</summary>
    public Sprite SpriteDownState => spriteDownState;

    /// <summary>Gets the scale applied when direction is neutral.</summary>
    public float ScaleNeutral => scaleNeutral;

    /// <summary>Gets the scale applied when facing up.</summary>
    public float ScaleUp => scaleUp;

    /// <summary>Gets the scale applied when facing left.</summary>
    public float ScaleLeft => scaleLeft;

    /// <summary>Gets the scale applied when facing right.</summary>
    public float ScaleRight => scaleRight;

    /// <summary>Gets the scale applied when facing down.</summary>
    public float ScaleDown => scaleDown;

    /// <summary>Gets the scale applied when hurt.</summary>
    public float ScaleHurt => scaleHurt;

    /// <summary>Gets the scale applied when down/defeated.</summary>
    public float ScaleDownState => scaleDownState;
}
