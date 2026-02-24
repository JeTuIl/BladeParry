using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Detects swipe input from touch or mouse and invokes an event with the resolved cardinal direction (Up, Down, Left, Right).
/// </summary>
public class SlideDetection : MonoBehaviour
{
    /// <summary>Optional label to display the last detected direction (debug/UI).</summary>
    [SerializeField] private TMP_Text directionLabel;

    /// <summary>Minimum distance in pixels for a gesture to count as a swipe.</summary>
    [SerializeField] private float minSwipeDistance = 20f;

    /// <summary>Invoked when a valid swipe is detected; payload is the cardinal direction.</summary>
    [SerializeField] public UnityEvent<Direction> onSwipeDetected;

    /// <summary>Invoked each frame while dragging: (startPosition, currentPosition) in screen space.</summary>
    public event System.Action<Vector2, Vector2> onDragUpdate;

    /// <summary>Invoked when the drag ends (release or cancel).</summary>
    public event System.Action onDragEnd;

    /// <summary>Screen position where the touch or mouse press started.</summary>
    private Vector2 _touchStartPosition;

    /// <summary>True when a touch/mouse press has been recorded and we are waiting for release.</summary>
    private bool _touchTracked;

    private const string TableName = "BladeParry_LocalizationTable";
    private string _dirUp;
    private string _dirDown;
    private string _dirLeft;
    private string _dirRight;

    private void Start()
    {
        StartCoroutine(PreloadDirectionLabels());
    }

    private IEnumerator PreloadDirectionLabels()
    {
        var up = new LocalizedString(TableName, "Direction_Up").GetLocalizedStringAsync();
        yield return up;
        if (up.IsValid() && up.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(up.Result))
            _dirUp = up.Result;

        var down = new LocalizedString(TableName, "Direction_Down").GetLocalizedStringAsync();
        yield return down;
        if (down.IsValid() && down.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(down.Result))
            _dirDown = down.Result;

        var left = new LocalizedString(TableName, "Direction_Left").GetLocalizedStringAsync();
        yield return left;
        if (left.IsValid() && left.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(left.Result))
            _dirLeft = left.Result;

        var right = new LocalizedString(TableName, "Direction_Right").GetLocalizedStringAsync();
        yield return right;
        if (right.IsValid() && right.Status == AsyncOperationStatus.Succeeded && !string.IsNullOrEmpty(right.Result))
            _dirRight = right.Result;
    }

    /// <summary>
    /// Each frame, processes touch input if available, otherwise mouse input.
    /// </summary>
    void Update()
    {
        if (Touchscreen.current != null)
            ProcessTouch();
        else if (Mouse.current != null)
            ProcessMouse();
    }

    /// <summary>
    /// Handles touch input: records position on Began, commits swipe on Ended if distance is sufficient.
    /// </summary>
    private void ProcessTouch()
    {
        TouchControl primaryTouch = Touchscreen.current.primaryTouch;
        TouchPhase phase = primaryTouch.phase.ReadValue();

        switch (phase)
        {
            case TouchPhase.Began:
                _touchStartPosition = primaryTouch.position.ReadValue();
                _touchTracked = true;
                break;

            case TouchPhase.Moved:
                if (_touchTracked)
                    onDragUpdate?.Invoke(_touchStartPosition, primaryTouch.position.ReadValue());
                break;

            case TouchPhase.Ended:
                if (!_touchTracked)
                    break;
                onDragUpdate?.Invoke(_touchStartPosition, primaryTouch.position.ReadValue());
                TryCommitSwipe(primaryTouch.position.ReadValue());
                _touchTracked = false;
                onDragEnd?.Invoke();
                break;

            case TouchPhase.Canceled:
                if (_touchTracked)
                {
                    _touchTracked = false;
                    onDragEnd?.Invoke();
                }
                break;
        }
    }

    /// <summary>
    /// Handles mouse input: records position on left press, commits swipe on left release if distance is sufficient.
    /// </summary>
    private void ProcessMouse()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _touchStartPosition = Mouse.current.position.ReadValue();
            _touchTracked = true;
        }
        else if (Mouse.current.leftButton.isPressed && _touchTracked)
        {
            onDragUpdate?.Invoke(_touchStartPosition, Mouse.current.position.ReadValue());
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && _touchTracked)
        {
            Vector2 endPos = Mouse.current.position.ReadValue();
            onDragUpdate?.Invoke(_touchStartPosition, endPos);
            TryCommitSwipe(endPos);
            _touchTracked = false;
            onDragEnd?.Invoke();
        }
    }

    /// <summary>
    /// If the distance from start to end meets the minimum, resolves direction and invokes onSwipeDetected.
    /// </summary>
    /// <param name="endPosition">Screen position where the touch/mouse was released.</param>
    private void TryCommitSwipe(Vector2 endPosition)
    {
        Vector2 delta = endPosition - _touchStartPosition;
        float distance = delta.magnitude;

        if (distance >= minSwipeDistance)
        {
            Direction direction = GetDirectionFromDelta(delta);
            if (directionLabel != null)
            {
                string label = direction switch
                {
                    Direction.Up => _dirUp,
                    Direction.Down => _dirDown,
                    Direction.Left => _dirLeft,
                    Direction.Right => _dirRight,
                    _ => null
                };
                directionLabel.text = !string.IsNullOrEmpty(label) ? label : direction.ToString();
            }
            onSwipeDetected?.Invoke(direction);
        }
    }

    /// <summary>
    /// Maps a 2D delta to one of four cardinal directions based on angle (90° sectors).
    /// </summary>
    /// <param name="delta">Movement vector from start to end.</param>
    /// <returns>The cardinal direction (Right, Up, Left, or Down).</returns>
    private static Direction GetDirectionFromDelta(Vector2 delta)
    {
        // Atan2(y, x): Right=0°, Up=90°, Left=±180°, Down=-90°
        float angleDeg = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        // Normalize to 0..360
        angleDeg = Mathf.Repeat(angleDeg + 360f, 360f);
        // 4 cardinal sectors of 90°; boundaries at 45°, 135°, 225°, 315°
        int sector = (int)((angleDeg + 45f) / 90f) % 4;

        return sector switch
        {
            0 => Direction.Right,
            1 => Direction.Up,
            2 => Direction.Left,
            3 => Direction.Down,
            _ => Direction.Right
        };
    }
}
