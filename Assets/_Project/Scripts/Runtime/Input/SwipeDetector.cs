using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Turns each touch or mouse-drag stroke into at most one swipe direction; strokes that start on UI are ignored.
public class SwipeDetector
{
    // =========================== CONSTANTS ===========================

    private const float MIN_SWIPE_SCREEN_FRACTION = 0.05f;

    // ========================= READONLY FIELDS ======================

    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private bool _isTracking;
    private bool _isHoldingUpSwipe;
    private Vector2 _startPosition;
    private PointerEventData _pointerData;

    // ========================= PUBLIC METHODS ========================

    // True while the finger (or mouse button) that swiped up is still held, which counts as holding jump.
    public bool IsHoldingUpSwipe => _isHoldingUpSwipe;

    public bool TryGetSwipe(out Vector2Int direction)
    {
        direction = Vector2Int.zero;
        if (!TryReadPointer(out ButtonControl press, out Vector2 position))
        {
            _isHoldingUpSwipe = false;
            return false;
        }

        if (!press.isPressed)
        {
            _isHoldingUpSwipe = false;
        }

        if (press.wasPressedThisFrame)
        {
            _startPosition = position;
            _isTracking = !IsOverUI(position);
        }

        if (!_isTracking) return false;

        if (!press.isPressed)
        {
            _isTracking = false;
        }

        Vector2 delta = position - _startPosition;
        float minDistance = Screen.height * MIN_SWIPE_SCREEN_FRACTION;
        if (delta.sqrMagnitude < minDistance * minDistance) return false;

        _isTracking = false;
        bool isHorizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
        direction = isHorizontal
            ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
            : new Vector2Int(0, (int)Mathf.Sign(delta.y));
        _isHoldingUpSwipe = direction.y > 0 && press.isPressed;
        return true;
    }

    // ========================= PRIVATE METHODS =======================

    // A finger on the screen wins; otherwise the left mouse button acts as a finger.
    private static bool TryReadPointer(out ButtonControl press, out Vector2 position)
    {
        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null)
        {
            TouchControl touch = touchscreen.primaryTouch;
            bool isTouchActive = touch.press.isPressed || touch.press.wasReleasedThisFrame;
            if (isTouchActive)
            {
                press = touch.press;
                position = touch.position.ReadValue();
                return true;
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            press = mouse.leftButton;
            position = mouse.position.ReadValue();
            return true;
        }

        press = null;
        position = Vector2.zero;
        return false;
    }

    private bool IsOverUI(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;

        _pointerData ??= new PointerEventData(eventSystem);
        _pointerData.position = screenPosition;
        _uiHits.Clear();
        eventSystem.RaycastAll(_pointerData, _uiHits);
        return _uiHits.Count > 0;
    }
}
