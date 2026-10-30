using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PetInput : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    private const string STEER_ACTION = "Steer";
    private const string JUMP_ACTION = "Jump";
    private const string PAUSE_ACTION = "Pause";
    private const string DUCK_ACTION = "Duck";
    private const string AXIS_COMPOSITE = "1DAxis";
    private const string NEGATIVE_PART = "Negative";
    private const string POSITIVE_PART = "Positive";

    private const string KEY_A = "<Keyboard>/a";
    private const string KEY_D = "<Keyboard>/d";
    private const string KEY_LEFT = "<Keyboard>/leftArrow";
    private const string KEY_RIGHT = "<Keyboard>/rightArrow";
    private const string KEY_SPACE = "<Keyboard>/space";
    private const string KEY_W = "<Keyboard>/w";
    private const string KEY_UP = "<Keyboard>/upArrow";
    private const string KEY_ESCAPE = "<Keyboard>/escape";
    private const string KEY_P = "<Keyboard>/p";
    private const string KEY_S = "<Keyboard>/s";
    private const string KEY_DOWN = "<Keyboard>/downArrow";
    private const string PAD_EAST = "<Gamepad>/buttonEast";
    private const string PAD_STICK_X = "<Gamepad>/leftStick/x";
    private const string PAD_SOUTH = "<Gamepad>/buttonSouth";
    private const string PAD_START = "<Gamepad>/start";

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private InputAction _steerAction;
    private InputAction _jumpAction;
    private InputAction _pauseAction;
    private InputAction _duckAction;
    private SwipeDetector _swipeDetector;

    // ========================= INITIALIZE METHODS ========================

    private void InitializeActions()
    {
        _steerAction = new InputAction(STEER_ACTION, InputActionType.Value);
        _steerAction.AddCompositeBinding(AXIS_COMPOSITE).With(NEGATIVE_PART, KEY_A).With(POSITIVE_PART, KEY_D);
        _steerAction.AddCompositeBinding(AXIS_COMPOSITE).With(NEGATIVE_PART, KEY_LEFT).With(POSITIVE_PART, KEY_RIGHT);
        _steerAction.AddBinding(PAD_STICK_X);

        _jumpAction = new InputAction(JUMP_ACTION, InputActionType.Button);
        _jumpAction.AddBinding(KEY_SPACE);
        _jumpAction.AddBinding(KEY_W);
        _jumpAction.AddBinding(KEY_UP);
        _jumpAction.AddBinding(PAD_SOUTH);

        _pauseAction = new InputAction(PAUSE_ACTION, InputActionType.Button);
        _pauseAction.AddBinding(KEY_ESCAPE);
        _pauseAction.AddBinding(KEY_P);
        _pauseAction.AddBinding(PAD_START);

        _duckAction = new InputAction(DUCK_ACTION, InputActionType.Button);
        _duckAction.AddBinding(KEY_S);
        _duckAction.AddBinding(KEY_DOWN);
        _duckAction.AddBinding(PAD_EAST);

        _swipeDetector = new SwipeDetector();
    }

    // =========================== PROPERTIES ==========================

    public float Steer => Mathf.Clamp(_steerAction.ReadValue<float>(), -1f, 1f);
    public bool IsJumpHeld => _jumpAction.IsPressed() || _swipeDetector.IsHoldingUpSwipe;

    // ============================ EVENTS =============================

    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnPausePressed;
    public event Action OnDuckPressed;
    public event Action<int> OnSteerSwipe;

    // ======================= UNITY CALLBACKS =========================

    // Awake is needed because OnEnable runs before Start and uses the actions.
    private void Awake()
    {
        InitializeActions();
    }

    private void OnEnable()
    {
        _jumpAction.started += HandleJumpStarted;
        _jumpAction.canceled += HandleJumpCanceled;
        _pauseAction.started += HandlePauseStarted;
        _duckAction.started += HandleDuckStarted;

        _steerAction.Enable();
        _jumpAction.Enable();
        _pauseAction.Enable();
        _duckAction.Enable();
    }

    private void OnDisable()
    {
        _jumpAction.started -= HandleJumpStarted;
        _jumpAction.canceled -= HandleJumpCanceled;
        _pauseAction.started -= HandlePauseStarted;
        _duckAction.started -= HandleDuckStarted;

        _steerAction.Disable();
        _jumpAction.Disable();
        _pauseAction.Disable();
        _duckAction.Disable();
    }

    private void Update()
    {
        ProcessSwipe();
    }

    private void OnDestroy()
    {
        _steerAction.Dispose();
        _jumpAction.Dispose();
        _pauseAction.Dispose();
        _duckAction.Dispose();
    }

    // ========================= PUBLIC METHODS ========================

    public void SimulateSwipe(Vector2Int direction)
    {
        RaiseSwipe(direction);
    }

    // ========================= PRIVATE METHODS =======================

    private void HandleJumpStarted(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
    }

    private void HandleJumpCanceled(InputAction.CallbackContext context)
    {
        OnJumpReleased?.Invoke();
    }

    private void HandlePauseStarted(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
    }

    private void HandleDuckStarted(InputAction.CallbackContext context)
    {
        OnDuckPressed?.Invoke();
    }

    private void ProcessSwipe()
    {
        if (!_swipeDetector.TryGetSwipe(out Vector2Int direction)) return;

        RaiseSwipe(direction);
    }

    // A swipe up is a full jump: no release follows, so the jump is never cut short.
    private void RaiseSwipe(Vector2Int direction)
    {
        if (direction.x != 0)
        {
            OnSteerSwipe?.Invoke(direction.x);
        }
        else if (direction.y > 0)
        {
            OnJumpPressed?.Invoke();
        }
        else
        {
            OnDuckPressed?.Invoke();
        }
    }
}
