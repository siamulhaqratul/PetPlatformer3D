using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// Turns touch or mouse-drag strokes into swipe directions; strokes that start on UI are ignored.
// A swipe fires as soon as the finger has moved a short physical distance, so a quick flick works. The finger can
// stay down and drag again to swipe again: each swipe restarts measuring from where the finger is.
public class SwipeDetector
{
    // =========================== CONSTANTS ===========================

    // About 4 mm on any phone, measured from Screen.dpi.
    private const float MIN_SWIPE_INCHES = 0.16f;
    // Used where Screen.dpi is unknown (0), such as some WebGL browsers.
    private const float FALLBACK_SWIPE_SCREEN_FRACTION = 0.03f;
    // A swipe in the same direction as the last one waits this long, so one long flick moves only one lane.
    // A change of direction fires straight away.
    private const float SAME_DIRECTION_REPEAT_DELAY = 0.25f;

    // ========================= READONLY FIELDS ======================

    private readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private bool _isTracking;
    private bool _isHoldingUpSwipe;
    private Vector2 _startPosition;
    private Vector2Int _lastDirection;
    private float _lastSwipeTime;
    private PointerEventData _pointerData;

    // ========================= PUBLIC METHODS ========================

    // True while the finger (or mouse button) whose last swipe went up is still held, which counts as holding jump.
    public bool IsHoldingUpSwipe => _isHoldingUpSwipe;

    public bool TryGetSwipe(out Vector2Int direction)
    {
        direction = Vector2Int.zero;
        if (!TryReadPointer(out ButtonControl press, out Vector2 position))
        {
            _isHoldingUpSwipe = false;
            return false;
        }

        if (press.wasPressedThisFrame)
        {
            _startPosition = position;
            _lastDirection = Vector2Int.zero;
            _isTracking = !IsOverUI(position);
        }

        bool isReleased = !press.isPressed;
        if (isReleased)
        {
            _isHoldingUpSwipe = false;
        }

        if (!_isTracking) return false;

        // The release frame still counts, so a flick that lifts in the same frame it crosses the distance fires.
        if (isReleased)
        {
            _isTracking = false;
        }

        Vector2 delta = position - _startPosition;
        float minDistance = MinSwipeDistance();
        if (delta.sqrMagnitude < minDistance * minDistance) return false;

        Vector2Int swipe = ToDirection(delta);
        bool isRepeat = swipe == _lastDirection && Time.unscaledTime - _lastSwipeTime < SAME_DIRECTION_REPEAT_DELAY;
        _startPosition = position;
        if (isRepeat) return false;

        _lastDirection = swipe;
        _lastSwipeTime = Time.unscaledTime;
        _isHoldingUpSwipe = swipe.y > 0 && !isReleased;
        direction = swipe;
        return true;
    }

    // ========================= PRIVATE METHODS =======================

    private static float MinSwipeDistance()
    {
        return Screen.dpi > 0f ? Screen.dpi * MIN_SWIPE_INCHES : Screen.height * FALLBACK_SWIPE_SCREEN_FRACTION;
    }

    private static Vector2Int ToDirection(Vector2 delta)
    {
        bool isHorizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
        return isHorizontal
            ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
            : new Vector2Int(0, (int)Mathf.Sign(delta.y));
    }

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
