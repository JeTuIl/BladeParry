using System.Collections;
using UnityEngine;
using ReGolithSystems.UI;

/// <summary>
/// Orchestrates transitions between the category selection panel and the page reader panel using UiFaders.
/// Call SelectCategory(index) when the user picks a category; call BackToCategories() from the Back button on the pages panel.
/// Default fader states: set the category panel's UiFader Default State to Enabled and the pages panel's to Disabled so the flow starts with category selection visible.
/// </summary>
public class PageCategoryFlowManager : MonoBehaviour
{
    [Header("Faders")]
    /// <summary>Fader for the category selection panel (buttons/tabs). Fades out when opening pages, fades in when returning. Use Default State = Enabled so it is visible at start.</summary>
    [Tooltip("Fader for the category selection panel. Set UiFader Default State to Enabled so the panel is visible at start. Fader must be on a GameObject with a CanvasGroup.")]
    [SerializeField] private UiFader categorySelectionFader;

    /// <summary>Fader for the page reader panel. Fades in when a category is selected, fades out when going back. Use Default State = Disabled so it is hidden at start.</summary>
    [Tooltip("Fader for the page reader panel. Set UiFader Default State to Disabled so the panel is hidden at start. Fader must be on a GameObject with a CanvasGroup.")]
    [SerializeField] private UiFader pagesFader;

    [Header("Page manager")]
    /// <summary>Page UI manager that displays pages for the selected category.</summary>
    [Tooltip("Page UI manager that displays pages for the selected category.")]
    [SerializeField] private PageUiManager pageUiManager;

    /// <summary>
    /// Opens the page reader for the given category. Fades out the category panel, then fades in the pages panel and sets the category on the page manager.
    /// Call from each category button's OnClick with the category index (e.g. 0, 1, 2).
    /// </summary>
    /// <param name="categoryIndex">0-based index of the category.</param>
    public void SelectCategory(int categoryIndex)
    {
        StartCoroutine(SelectCategoryCoroutine(categoryIndex));
    }

    /// <summary>
    /// Returns to the category selection. Fades out the pages panel, then fades in the category panel.
    /// Call from the Back button on the page reader panel.
    /// </summary>
    public void BackToCategories()
    {
        StartCoroutine(BackToCategoriesCoroutine());
    }

    private IEnumerator SelectCategoryCoroutine(int categoryIndex)
    {
        if (pageUiManager != null)
            pageUiManager.SetCategory(categoryIndex, resetPageIndex: true);

        if (categorySelectionFader != null)
        {
            categorySelectionFader.FadeOut();
            yield return new WaitUntil(() => categorySelectionFader == null || !categorySelectionFader.IsFading);
        }

        if (pagesFader != null)
            pagesFader.FadeIn();
    }

    private IEnumerator BackToCategoriesCoroutine()
    {
        if (pagesFader != null)
        {
            pagesFader.FadeOut();
            yield return new WaitUntil(() => pagesFader == null || !pagesFader.IsFading);
        }

        if (categorySelectionFader != null)
            categorySelectionFader.FadeIn();
    }
}
