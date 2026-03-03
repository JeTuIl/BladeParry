using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Spawns short-lived floating text for parry, miss, and combo feedback.
/// </summary>
public class FloatingFeedbackText : MonoBehaviour
{
    /// <summary>Prefab to instantiate for each feedback (must have TMP_Text and RectTransform).</summary>
    [SerializeField] private GameObject textPrefab;

    /// <summary>Parent RectTransform for spawned instances (e.g. under Canvas).</summary>
    [Tooltip("Parent RectTransform for spawned instances (e.g. under Canvas).")]
    [SerializeField] private RectTransform spawnParent;

    /// <summary>Camera used for world-to-screen conversion. If null, Camera.main is used.</summary>
    [Tooltip("Camera used for world-to-screen conversion. If null, Camera.main is used.")]
    [SerializeField] private Camera worldCamera;

    /// <summary>Duration in seconds before the text is destroyed.</summary>
    [SerializeField] private float displayDuration = 0.5f;

    /// <summary>Scale at spawn.</summary>
    [SerializeField] private float scaleStart = 0.5f;

    /// <summary>Peak scale at mid-duration.</summary>
    [SerializeField] private float scalePeak = 1.2f;

    /// <summary>Scale at end of lifetime.</summary>
    [SerializeField] private float scaleEnd = 1f;

    /// <summary>Localization table name.</summary>
    private const string TableName = "BladeParry_LocalizationTable";

    private static readonly LocalizedString s_parry = new LocalizedString(TableName, "UI_Parry");
    private static readonly LocalizedString s_perfectParry = new LocalizedString(TableName, "UI_PerfectParry");
    private static readonly LocalizedString s_miss = new LocalizedString(TableName, "UI_Miss");
    private static readonly LocalizedString s_combo = new LocalizedString(TableName, "UI_Combo");

    /// <summary>Canvas containing spawn parent (for screen space conversion).</summary>
    private Canvas _canvas;

    /// <summary>CanvasScaler from parent (optional).</summary>
    private CanvasScaler _scaler;

    /// <summary>Cached localized "Parry" string.</summary>
    private string _cachedParry;

    /// <summary>Cached localized "Perfect parry" string.</summary>
    private string _cachedPerfectParry;

    /// <summary>Cached localized "Miss" string.</summary>
    private string _cachedMiss;

    /// <summary>Cached localized "Combo" string.</summary>
    private string _cachedCombo;

    /// <summary>Caches Canvas and CanvasScaler from spawn parent.</summary>
    private void Awake()
    {
        if (spawnParent != null)
        {
            _canvas = spawnParent.GetComponentInParent<Canvas>();
            _scaler = spawnParent.GetComponentInParent<CanvasScaler>();
        }
    }

    /// <summary>Starts preloading localized strings.</summary>
    private void Start()
    {
        StartCoroutine(PreloadStrings());
    }

    /// <summary>Loads parry, perfect parry, miss, and combo strings asynchronously.</summary>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator PreloadStrings()
    {
        var opParry = s_parry.GetLocalizedStringAsync();
        yield return opParry;
        if (opParry.IsValid() && opParry.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(opParry.Result))
            _cachedParry = opParry.Result;

        var opPerfect = s_perfectParry.GetLocalizedStringAsync();
        yield return opPerfect;
        if (opPerfect.IsValid() && opPerfect.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(opPerfect.Result))
            _cachedPerfectParry = opPerfect.Result;

        var opMiss = s_miss.GetLocalizedStringAsync();
        yield return opMiss;
        if (opMiss.IsValid() && opMiss.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(opMiss.Result))
            _cachedMiss = opMiss.Result;

        var opCombo = s_combo.GetLocalizedStringAsync();
        yield return opCombo;
        if (opCombo.IsValid() && opCombo.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(opCombo.Result))
            _cachedCombo = opCombo.Result;
    }

    /// <summary>Returns the localized string, using cache or loading synchronously if needed.</summary>
    /// <param name="localized">LocalizedString to resolve.</param>
    /// <param name="fallback">Fallback when localization fails.</param>
    /// <param name="cache">Cached result (updated when loaded).</param>
    /// <returns>Localized or fallback string.</returns>
    private static string GetLocalized(LocalizedString localized, string fallback, ref string cache)
    {
        if (!string.IsNullOrEmpty(cache))
            return cache;
        var op = localized.GetLocalizedStringAsync();
        if (op.IsDone && op.IsValid() && op.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(op.Result))
        {
            cache = op.Result;
            return cache;
        }
        op.WaitForCompletion();
        if (op.IsValid() && op.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(op.Result))
        {
            cache = op.Result;
            return cache;
        }
        return fallback;
    }

    /// <summary>
    /// Shows the parry label at the given world position (converted to canvas space).
    /// </summary>
    /// <param name="worldPosition">World position to display the text at.</param>
    public void ShowParry(Vector3 worldPosition)
    {
        string s = GetLocalized(s_parry, "PARRY!", ref _cachedParry);
        ShowAt(s, worldPosition);
    }

    /// <summary>
    /// Shows the perfect parry label at the given world position (perfect parry during wind-down).
    /// </summary>
    /// <param name="worldPosition">World position to display the text at.</param>
    public void ShowPerfectParry(Vector3 worldPosition)
    {
        string s = GetLocalized(s_perfectParry, "Perfect !", ref _cachedPerfectParry);
        ShowAt(s, worldPosition);
    }

    /// <summary>
    /// Shows the miss label at the given world position.
    /// </summary>
    /// <param name="worldPosition">World position to display the text at.</param>
    public void ShowMiss(Vector3 worldPosition)
    {
        string s = GetLocalized(s_miss, "MISS", ref _cachedMiss);
        ShowAt(s, worldPosition);
    }

    /// <summary>
    /// Shows the combo label at the given world position.
    /// </summary>
    /// <param name="worldPosition">World position to display the text at.</param>
    public void ShowCombo(Vector3 worldPosition)
    {
        string s = GetLocalized(s_combo, "COMBO!", ref _cachedCombo);
        ShowAt(s, worldPosition);
    }

    /// <summary>Instantiates the text prefab at the world position (converted to canvas space) and starts the animate-and-destroy coroutine.</summary>
    /// <param name="text">String to display.</param>
    /// <param name="worldPosition">World position for placement.</param>
    private void ShowAt(string text, Vector3 worldPosition)
    {
        if (textPrefab == null || spawnParent == null)
            return;

        Camera cam = worldCamera != null ? worldCamera : Camera.main;
        if (cam == null)
            return;

        GameObject instance = Instantiate(textPrefab, spawnParent);
        TMP_Text tmp = instance.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
            tmp.text = text;

        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect == null)
            rect = instance.GetComponentInChildren<RectTransform>(true);
        if (rect != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPosition);
            Vector2 local;
            if (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                RectTransformUtility.ScreenPointToLocalPointInRectangle(spawnParent, screenPoint, _canvas.worldCamera, out local);
            else
                RectTransformUtility.ScreenPointToLocalPointInRectangle(spawnParent, screenPoint, null, out local);
            rect.anchoredPosition = local;
        }

        StartCoroutine(AnimateAndDestroy(instance, rect ?? instance.transform));
    }

    /// <summary>Animates scale and alpha over displayDuration then destroys the instance.</summary>
    /// <param name="instance">Spawned feedback GameObject to destroy.</param>
    /// <param name="scaleTarget">Transform to scale (instance or child).</param>
    /// <returns>Enumerator for the coroutine.</returns>
    private IEnumerator AnimateAndDestroy(GameObject instance, Transform scaleTarget)
    {
        float elapsed = 0f;
        float half = displayDuration * 0.5f;

        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / displayDuration);
            float scale;
            if (t < 0.5f)
                scale = Mathf.Lerp(scaleStart, scalePeak, t * 2f);
            else
                scale = Mathf.Lerp(scalePeak, scaleEnd, (t - 0.5f) * 2f);
            scaleTarget.localScale = Vector3.one * scale;

            if (instance.TryGetComponent<CanvasGroup>(out var cg))
                cg.alpha = 1f - t;
            else if (scaleTarget.TryGetComponent<TMP_Text>(out var tmp))
            {
                Color c = tmp.color;
                c.a = 1f - t;
                tmp.color = c;
            }

            yield return null;
        }

        Destroy(instance);
    }
}
