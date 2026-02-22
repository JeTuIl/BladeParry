using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a line from swipe start to current position while the player is dragging (swipe preview).
/// </summary>
public class SwipeTrailUI : MonoBehaviour
{
    [SerializeField] private SlideDetection slideDetection;
    [Tooltip("RectTransform representing the line (e.g. a thin Image). Pivot at one end (e.g. 0, 0.5) for correct rotation.")]
    [SerializeField] private RectTransform lineRect;
    [Tooltip("Canvas used for screen-to-local conversion. If null, taken from line's parent Canvas.")]
    [SerializeField] private Canvas canvas;

    private void Awake()
    {
        if (lineRect != null && canvas == null)
            canvas = lineRect.GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        if (slideDetection != null)
        {
            slideDetection.onDragUpdate += OnDragUpdate;
            slideDetection.onDragEnd += OnDragEnd;
        }
        if (lineRect != null)
            lineRect.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (slideDetection != null)
        {
            slideDetection.onDragUpdate -= OnDragUpdate;
            slideDetection.onDragEnd -= OnDragEnd;
        }
        if (lineRect != null)
            lineRect.gameObject.SetActive(false);
    }

    private void OnDragUpdate(Vector2 startScreen, Vector2 currentScreen)
    {
        if (lineRect == null) return;

        lineRect.gameObject.SetActive(true);

        if (canvas == null) return;

        RectTransform lineParent = lineRect.parent as RectTransform;
        if (lineParent == null) return;

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lineParent, startScreen, cam, out Vector2 startLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lineParent, currentScreen, cam, out Vector2 currentLocal);

        Vector2 delta = currentLocal - startLocal;
        float distance = delta.magnitude;
        if (distance < 2f)
            distance = 2f;

        lineRect.anchoredPosition = startLocal;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);
    }

    private void OnDragEnd()
    {
        if (lineRect != null)
            lineRect.gameObject.SetActive(false);
    }
}
