using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using TMPro;

public class SlideDetection : MonoBehaviour
{
    [SerializeField] private TMP_Text directionLabel;
    [SerializeField] private float minSwipeDistance = 20f;
    [SerializeField] public UnityEvent<Direction> onSwipeDetected;

    private Vector2 _touchStartPosition;
    private bool _touchTracked;

    void Update()
    {
        if (Touchscreen.current != null)
            ProcessTouch();
        else if (Mouse.current != null)
            ProcessMouse();
    }

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
