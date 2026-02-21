using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using TMPro;

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

    /// <summary>Screen position where the touch or mouse press started.</summary>
    private Vector2 _touchStartPosition;

    /// <summary>True when a touch/mouse press has been recorded and we are waiting for release.</summary>
    private bool _touchTracked;

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

            case TouchPhase.Ended:
                if (!_touchTracked)
                    break;

                TryCommitSwipe(primaryTouch.position.ReadValue());
                _touchTracked = false;
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
        else if (Mouse.current.leftButton.wasReleasedThisFrame && _touchTracked)
        {
            TryCommitSwipe(Mouse.current.position.ReadValue());
            _touchTracked = false;
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
                directionLabel.text = direction.ToString();
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
