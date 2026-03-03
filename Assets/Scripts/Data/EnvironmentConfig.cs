using UnityEngine;

/// <summary>
/// ScriptableObject for fight environment: background (sprite, position, scale).
/// Used by FightConfig; applied at fight setup in GameplayLoopController.
/// </summary>
[CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BladeParry/Environment Config")]
public class EnvironmentConfig : ScriptableObject
{
    [Header("Background")]
    /// <summary>Sprite used for the fight background.</summary>
    [Tooltip("Sprite used for the fight background.")]
    [SerializeField] private Sprite backgroundSprite;

    /// <summary>Position for the background (local position of the SpriteRenderer transform).</summary>
    [Tooltip("Position for the background (local position of the SpriteRenderer transform).")]
    [SerializeField] private Vector3 backgroundPosition;

    /// <summary>Scale for the background (local scale of the SpriteRenderer transform).</summary>
    [Tooltip("Scale for the background (local scale of the SpriteRenderer transform).")]
    [SerializeField] private Vector3 backgroundScale = Vector3.one;

    /// <summary>Sprite for the fight background.</summary>
    public Sprite BackgroundSprite => backgroundSprite;

    /// <summary>Position for the background.</summary>
    public Vector3 BackgroundPosition => backgroundPosition;

    /// <summary>Scale for the background.</summary>
    public Vector3 BackgroundScale => backgroundScale;

    /// <summary>Sets the background sprite (for runtime-created config).</summary>
    /// <param name="value">The sprite to use for the fight background.</param>
    public void SetBackgroundSprite(Sprite value) => backgroundSprite = value;

    /// <summary>Sets the background position (for runtime-created config).</summary>
    /// <param name="value">Local position for the SpriteRenderer transform.</param>
    public void SetBackgroundPosition(Vector3 value) => backgroundPosition = value;

    /// <summary>Sets the background scale (for runtime-created config).</summary>
    /// <param name="value">Local scale for the SpriteRenderer transform.</param>
    public void SetBackgroundScale(Vector3 value) => backgroundScale = value;
}
