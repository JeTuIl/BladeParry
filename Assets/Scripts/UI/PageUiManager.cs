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
    [Tooltip("Sprite displayed on the page Image.")]
    [SerializeField] private Sprite sprite;

    [Tooltip("Key in the selected localization table for the page text.")]
    [SerializeField] private string localizationKey;

    public Sprite Sprite => sprite;
    public string LocalizationKey => localizationKey;
}

/// <summary>
/// Manages the display of several pages in a UI. Each page has a sprite (Image) and a localized string (TMP_Text).
/// Supports navigation (next/previous, show by index) and refreshes text when the locale changes.
/// </summary>
public class PageUiManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Name of the localization table used to resolve page text keys (e.g. BladeParry_LocalizationTable).")]
    [SerializeField] private string localizationTableName = "BladeParry_LocalizationTable";
    [SerializeField] private Image pageImage;
    [SerializeField] private TMP_Text pageText;
    [Tooltip("Optional: displays current page index and total (e.g. \"2 / 5\").")]
    [SerializeField] private TMP_Text pageCounterText;
    [Tooltip("Optional: button to go to the next page. Interactable is updated based on current page.")]
    [SerializeField] private Button nextPageButton;
    [Tooltip("Optional: button to go to the previous page. Interactable is updated based on current page.")]
    [SerializeField] private Button previousPageButton;

    [Header("Pages")]
    [SerializeField] private List<PageData> pages = new List<PageData>();

    private int _currentPageIndex;
    private Coroutine _refreshTextCoroutine;

    /// <summary>Current page index (0-based).</summary>
    public int CurrentPageIndex => _currentPageIndex;

    /// <summary>Total number of pages.</summary>
    public int PageCount => pages != null ? pages.Count : 0;

    /// <summary>True if there is a next page.</summary>
    public bool HasNext => pages != null && pages.Count > 0 && _currentPageIndex < pages.Count - 1;

    /// <summary>True if there is a previous page.</summary>
    public bool HasPrevious => pages != null && _currentPageIndex > 0;

    private void Start()
    {
        if (pages != null && pages.Count > 0)
        {
            _currentPageIndex = 0;
            RefreshCurrentPage();
        }
    }

    private void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    private void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        if (_refreshTextCoroutine != null)
        {
            StopCoroutine(_refreshTextCoroutine);
            _refreshTextCoroutine = null;
        }
    }

    private void OnSelectedLocaleChanged(UnityEngine.Localization.Locale locale)
    {
        RefreshCurrentPage();
    }

    /// <summary>
    /// Updates the Image sprite and TMP_Text with the current page data. Called on navigation and locale change.
    /// </summary>
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

    private void RefreshNavigationButtons()
    {
        if (nextPageButton != null)
            nextPageButton.interactable = HasNext;
        if (previousPageButton != null)
            previousPageButton.interactable = HasPrevious;
    }

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
