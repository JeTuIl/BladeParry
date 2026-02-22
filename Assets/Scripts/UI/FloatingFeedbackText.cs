using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Spawns short-lived floating text for parry, miss, and combo feedback.
/// </summary>
public class FloatingFeedbackText : MonoBehaviour
{
    [SerializeField] private GameObject textPrefab;
    [Tooltip("Parent RectTransform for spawned instances (e.g. under Canvas).")]
    [SerializeField] private RectTransform spawnParent;
    [Tooltip("Camera used for world-to-screen conversion. If null, Camera.main is used.")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private float displayDuration = 0.5f;
    [SerializeField] private float scaleStart = 0.5f;
    [SerializeField] private float scalePeak = 1.2f;
    [SerializeField] private float scaleEnd = 1f;

    private Canvas _canvas;
    private CanvasScaler _scaler;

    private void Awake()
    {
        if (spawnParent != null)
        {
            _canvas = spawnParent.GetComponentInParent<Canvas>();
            _scaler = spawnParent.GetComponentInParent<CanvasScaler>();
        }
    }

    /// <summary>
    /// Shows "PARRY!" at the given world position (converted to canvas space).
    /// </summary>
    public void ShowParry(Vector3 worldPosition)
    {
        ShowAt("PARRY!", worldPosition);
    }

    /// <summary>
    /// Shows "MISS" at the given world position.
    /// </summary>
    public void ShowMiss(Vector3 worldPosition)
    {
        ShowAt("MISS", worldPosition);
    }

    /// <summary>
    /// Shows "COMBO!" at the given world position.
    /// </summary>
    public void ShowCombo(Vector3 worldPosition)
    {
        ShowAt("COMBO!", worldPosition);
    }

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
