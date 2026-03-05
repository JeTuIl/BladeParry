using UnityEngine;

/// <summary>
/// ScriptableObject defining a single roguelite enhancement: sprite, localized name/description,
/// base value × level for effect, and effect type for gameplay linking.
/// Use the Enhancements localization table (table name "Enhancements") with keys: Id_Name, Id_Desc.
/// </summary>
[CreateAssetMenu(fileName = "RogueliteEnhancement", menuName = "BladeParry/Roguelite Enhancement Definition")]
public class RogueliteEnhancementDefinition : ScriptableObject
{
    /// <summary>Localization table name for enhancement name/description keys.</summary>
    public const string LocalizationTableName = "Enhancements";

    /// <summary>Stable ID for save/load and upgrade logic (e.g. Gambeson). Must match localization keys: Id_Name, Id_Desc.</summary>
    [Tooltip("Stable ID for save/load and upgrade logic (e.g. Gambeson). Must match localization keys: Id_Name, Id_Desc.")]
    [SerializeField] private string id = "";

    /// <summary>Icon shown in the enhancement choice UI.</summary>
    [Tooltip("Icon shown in the enhancement choice UI.")]
    [SerializeField] private Sprite sprite;

    /// <summary>Localization key for display name (Enhancements table). If empty, uses Id + "_Name".</summary>
    [Tooltip("Localization key for display name (Enhancements table). If empty, uses Id + \"_Name\".")]
    [SerializeField] private string nameKey = "";

    /// <summary>Localization key for description (Enhancements table). If empty, uses Id + "_Desc".</summary>
    [Tooltip("Localization key for description (Enhancements table). If empty, uses Id + \"_Desc\".")]
    [SerializeField] private string descriptionKey = "";

    /// <summary>Base numerical value; effective value = baseValue * level.</summary>
    [Tooltip("Base numerical value; effective value = baseValue * level.")]
    [SerializeField] private float baseValue = 1f;

    /// <summary>Maximum level this enhancement can be upgraded to.</summary>
    [Tooltip("Maximum level this enhancement can be upgraded to.")]
    [SerializeField] private int maxLevel = 3;

    /// <summary>Effect type used by the central reader to apply gameplay modifiers.</summary>
    [Tooltip("Effect type used by the central reader to apply gameplay modifiers.")]
    [SerializeField] private RogueliteEnhancementEffectType effectType = RogueliteEnhancementEffectType.None;

    /// <summary>Stable ID for save/load and for "offer upgrade" logic.</summary>
    public string Id => string.IsNullOrEmpty(id) ? name : id;

    /// <summary>Icon for the enhancement choice UI.</summary>
    public Sprite Sprite => sprite;

    /// <summary>Localization key for name (Enhancements table).</summary>
    public string NameKey => string.IsNullOrEmpty(nameKey) ? Id + "_Name" : nameKey;

    /// <summary>Localization key for description (Enhancements table).</summary>
    public string DescriptionKey => string.IsNullOrEmpty(descriptionKey) ? Id + "_Desc" : descriptionKey;

    /// <summary>Base value; effective value = baseValue * level.</summary>
    public float BaseValue => baseValue;

    /// <summary>Maximum upgrade level.</summary>
    public int MaxLevel => maxLevel;

    /// <summary>Effect type for gameplay linking.</summary>
    public RogueliteEnhancementEffectType EffectType => effectType;

    /// <summary>Effective value at a given level (baseValue * level). Level 0 means not owned; returns 0.</summary>
    /// <param name="level">Enhancement level (0 = not owned, 1..MaxLevel = owned).</param>
    /// <returns>baseValue * level, or 0 when level &lt; 1.</returns>
    public float GetValueAtLevel(int level)
    {
        if (level < 1) return 0f;
        int clamped = Mathf.Clamp(level, 1, maxLevel);
        return baseValue * clamped;
    }
}
