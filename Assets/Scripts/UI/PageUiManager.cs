using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Serializable data for a single page: sprite for the image and localization key for the text.
/// </summary>
[System.Serializable]
public class PageData
{
    /// <summary>Sprite displayed on the page Image.</summary>
    [Tooltip("Sprite displayed on the page Image.")]
    [SerializeField] private Sprite sprite;

    /// <summary>Key in the selected localization table for the page text.</summary>
    [Tooltip("Key in the selected localization table for the page text.")]
    [SerializeField] private string localizationKey;

    /// <summary>Gets the sprite for this page.</summary>
    public Sprite Sprite => sprite;

    /// <summary>Gets the localization key for the page text.</summary>
    public string LocalizationKey => localizationKey;
}

/// <summary>
/// Manages the display of several pages in a UI. Each page has a sprite (Image) and a localized string (TMP_Text).
/// Supports navigation (next/previous, show by index) and refreshes text when the locale changes.
/// </summary>
public class PageUiManager : MonoBehaviour
{
    [Header("References")]
    /// <summary>Name of the localization table used to resolve page text keys (e.g. BladeParry_LocalizationTable).</summary>
    [Tooltip("Name of the localization table used to resolve page text keys (e.g. BladeParry_LocalizationTable).")]
    [SerializeField] private string localizationTableName = "BladeParry_LocalizationTable";

    /// <summary>Image that displays the current page sprite.</summary>
    [SerializeField] private Image pageImage;

    /// <summary>Text that displays the current page localized string.</summary>
    [SerializeField] private TMP_Text pageText;

    /// <summary>Optional: displays current page index and total (e.g. "2 / 5").</summary>
    [Tooltip("Optional: displays current page index and total (e.g. \"2 / 5\").")]
    [SerializeField] private TMP_Text pageCounterText;

    /// <summary>Optional: button to go to the next page. Interactable is updated based on current page.</summary>
    [Tooltip("Optional: button to go to the next page. Interactable is updated based on current page.")]
    [SerializeField] private Button nextPageButton;

    /// <summary>Optional: button to go to the previous page. Interactable is updated based on current page.</summary>
    [Tooltip("Optional: button to go to the previous page. Interactable is updated based on current page.")]
    [SerializeField] private Button previousPageButton;

    [Header("Pages")]
    /// <summary>List of pages (sprite + localization key per page).</summary>
    [SerializeField] private List<PageData> pages = new List<PageData>();

    /// <summary>Current page index (0-based).</summary>
    private int _currentPageIndex;

    /// <summary>Active coroutine loading localized text; null when idle.</summary>
    private Coroutine _refreshTextCoroutine;

    /// <summary>Current page index (0-based).</summary>
    public int CurrentPageIndex => _currentPageIndex;

    /// <summary>Total number of pages.</summary>
    public int PageCount => pages != null ? pages.Count : 0;

    /// <summary>True if there is a next page.</summary>
    public bool HasNext => pages != null && pages.Count > 0 && _currentPageIndex < pages.Count - 1;

    /// <summary>True if there is a previous page.</summary>
    public bool HasPrevious => pages != null && _currentPageIndex > 0;

    /// <summary>Initializes to the first page and refreshes display.</summary>
    private void Start()
    {
        if (pages != null && pages.Count > 0)
        {
            _currentPageIndex = 0;
            RefreshCurrentPage();
        }
    }

    /// <summary>Subscribes to SelectedLocaleChanged so page text refreshes when language changes.</summary>
    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    /// <summary>Unsubscribes from SelectedLocaleChanged and stops any running text refresh coroutine.</summary>
    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        if (_refreshTextCoroutine != null)
        {
            StopCoroutine(_refreshTextCoroutine);
            _refreshTextCoroutine = null;
        }
    }

    /// <summary>Handler for locale change: refreshes the current page (reloads localized text).</summary>
    /// <param name="locale">The newly selected locale (not used; RefreshCurrentPage reloads from table).</param>
    private void OnSelectedLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        RefreshCurrentPage();
    }

    /// <summary>
    /// Updates the Image sprite and TMP_Text with the current page data. Called on navigation and locale change.
    /// </summary>
    /// <remarks>Starts an async coroutine to load localized text; next/previous buttons and page counter are updated.</remarks>
    public void RefreshCurrentPage()
    {
        if (pages == null || pages.Count == 0)
            return;

        int index = Mathf.Clamp(_currentPageIndex, 0, pages.Count - 1);
        PageData page = pages[index];

        if (pageImage != null)
        {
            if (page.Sprite != null)
            {
                pageImage.sprite = page.Sprite;
                pageImage.enabled = true;
            }
            else
            {
                pageImage.enabled = false;
            }
        }

        if (_refreshTextCoroutine != null)
            StopCoroutine(_refreshTextCoroutine);
        _refreshTextCoroutine = StartCoroutine(RefreshTextCoroutine(page.LocalizationKey));

        RefreshPageCounter();
        RefreshNavigationButtons();
    }

    /// <summary>Enables or disables next/previous buttons based on HasNext and HasPrevious.</summary>
    private void RefreshNavigationButtons()
    {
        if (nextPageButton != null)
            nextPageButton.interactable = HasNext;
        if (previousPageButton != null)
            previousPageButton.interactable = HasPrevious;
    }

    /// <summary>Updates the page counter text (e.g. "2 / 5") or clears it if there are no pages.</summary>
    private void RefreshPageCounter()
    {
        if (pageCounterText == null)
            return;
        int count = PageCount;
        if (count <= 0)
        {
            pageCounterText.text = string.Empty;
            return;
        }
        int current = Mathf.Clamp(_currentPageIndex, 0, count - 1);
        pageCounterText.text = (current + 1).ToString() + " / " + count.ToString();
    }

    /// <summary>Loads the localized string for the given key and assigns it to pageText.</summary>
    /// <param name="localizationKey">Key in the localization table for the page text.</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator RefreshTextCoroutine(string localizationKey)
    {
        if (pageText == null)
        {
            _refreshTextCoroutine = null;
            yield break;
        }

        if (string.IsNullOrEmpty(localizationKey))
        {
            pageText.text = string.Empty;
            _refreshTextCoroutine = null;
            yield break;
        }

        string tableName = string.IsNullOrEmpty(localizationTableName) ? "BladeParry_LocalizationTable" : localizationTableName;
        var localizedString = new LocalizedString(tableName, localizationKey);
        var op = localizedString.GetLocalizedStringAsync();
        if (!op.IsDone)
            yield return op;

        if (op.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(op.Result))
            pageText.text = op.Result;
        else
            pageText.text = string.Empty;
        _refreshTextCoroutine = null;
    }

    /// <summary>
    /// Shows the page at the given index. Index is clamped to valid range.
    /// </summary>
    /// <param name="index">0-based page index; clamped to [0, PageCount - 1].</param>
    public void ShowPage(int index)
    {
        if (pages == null || pages.Count == 0)
            return;
        _currentPageIndex = Mathf.Clamp(index, 0, pages.Count - 1);
        RefreshCurrentPage();
    }

    /// <summary>
    /// Shows the next page if available. Does nothing on the last page.
    /// </summary>
    public void NextPage()
    {
        if (!HasNext)
            return;
        _currentPageIndex++;
        RefreshCurrentPage();
    }

    /// <summary>
    /// Shows the previous page if available. Does nothing on the first page.
    /// </summary>
    public void PreviousPage()
    {
        if (!HasPrevious)
            return;
        _currentPageIndex--;
        RefreshCurrentPage();
    }
}
