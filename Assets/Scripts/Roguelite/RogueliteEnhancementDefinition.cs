using UnityEngine;

/// <summary>
/// ScriptableObject defining a single roguelite enhancement: sprite, localized name/description,
/// base value × level for effect, and effect type for gameplay linking.
/// Use the Enhancements localization table (table name "Enhancements") with keys: Id_Name, Id_Desc.
/// </summary>
[CreateAssetMenu(fileName = "RogueliteEnhancement", menuName = "BladeParry/Roguelite Enhancement Definition")]
public class RogueliteEnhancementDefinition : ScriptableObject
{
    public const string LocalizationTableName = "Enhancements";

    [Tooltip("Stable ID for save/load and upgrade logic (e.g. Gambeson). Must match localization keys: Id_Name, Id_Desc.")]
    [SerializeField] private string id = "";

    [Tooltip("Icon shown in the enhancement choice UI.")]
    [SerializeField] private Sprite sprite;

    [Tooltip("Localization key for display name (Enhancements table). If empty, uses Id + \"_Name\".")]
    [SerializeField] private string nameKey = "";

    [Tooltip("Localization key for description (Enhancements table). If empty, uses Id + \"_Desc\".")]
    [SerializeField] private string descriptionKey = "";

    [Tooltip("Base numerical value; effective value = baseValue * level.")]
    [SerializeField] private float baseValue = 1f;

    [Tooltip("Maximum level this enhancement can be upgraded to.")]
    [SerializeField] private int maxLevel = 3;

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

    /// <summary>Effective value at a given level (baseValue * level).</summary>
    public float GetValueAtLevel(int level)
    {
        int clamped = Mathf.Clamp(level, 1, maxLevel);
        return baseValue * clamped;
    }
}
