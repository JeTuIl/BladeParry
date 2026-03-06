using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serializable data for a page category: optional id, optional localization key for the category label, and the list of pages in this category.
/// </summary>
[System.Serializable]
public class PageCategoryData
{
    /// <summary>Optional identifier for this category (e.g. for debugging or save data).</summary>
    [Tooltip("Optional identifier for this category (e.g. for debugging or save data).")]
    [SerializeField] private string categoryId;

    /// <summary>Optional key in the localization table for the category display name (e.g. for buttons/tabs).</summary>
    [Tooltip("Optional key in the localization table for the category display name (e.g. for buttons/tabs).")]
    [SerializeField] private string categoryNameLocalizationKey;

    /// <summary>Pages belonging to this category.</summary>
    [Tooltip("Pages belonging to this category.")]
    [SerializeField] private List<PageData> pages = new List<PageData>();

    /// <summary>Gets the optional category identifier.</summary>
    public string CategoryId => categoryId;

    /// <summary>Gets the optional localization key for the category display name.</summary>
    public string CategoryNameLocalizationKey => categoryNameLocalizationKey;

    /// <summary>Gets the list of pages in this category. Caller should not modify.</summary>
    public IReadOnlyList<PageData> Pages => pages != null ? pages : (IReadOnlyList<PageData>)new List<PageData>();
}
