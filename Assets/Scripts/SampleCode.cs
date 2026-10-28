using System;
using UnityEngine;

/// <summary>Sample MonoBehaviour demonstrating Hamba Structure and Documentation.</summary>
public class SampleCode : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    // Maximum health value for a player.
    private const int MAX_HEALTH = 100;
    // Input axis used for horizontal movement.
    private const string HORIZONTAL_AXIS = "Horizontal";
    // Input button used for jumping.
    private const string JUMP_BUTTON = "Jump";

    // ======================= STATIC FIELDS ==========================

    // Tracks total number of players.
    private static int _totalPlayers;

    // ========================= READONLY FIELDS ======================

    // Unique identifier for the player.
    private readonly string _playerId = Guid.NewGuid().ToString();

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Player Settings")]
    [Tooltip("Current health value of the player.")]
    [SerializeField] private int _health = MAX_HEALTH;
    [Tooltip("Current score of the player.")]
    [SerializeField] private int _score;

    [Header("Movement Settings")]
    [Tooltip("Movement speed of the player.")]
    [SerializeField] private float _moveSpeed = 5f;
    [Tooltip("Jump force applied to the player.")]
    [SerializeField] private float _jumpForce = 5f;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    // Cached reference to the Rigidbody component.
    private Rigidbody _rigidbody;
    // Horizontal input read this frame.
    private float _horizontalInput;
    // Whether a jump was requested since the last physics step.
    private bool _isJumpRequested;

    // ========================= INITIALIZE METHODS ========================

    /// <summary>Caches components and resets player health and score.</summary>
    private void InitializePlayer()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _score = 0;
        _health = MAX_HEALTH;
        _totalPlayers++;
    }

    // =========================== PROPERTIES ==========================

    /// <summary>Total number of players in the game.</summary>
    public static int TotalPlayers => _totalPlayers;
    /// <summary>Unique identifier of this player.</summary>
    public string PlayerId => _playerId;
    /// <summary>Current health value.</summary>
    public int Health => _health;
    /// <summary>Current score value.</summary>
    public int Score => _score;

    // ============================ EVENTS =============================

    /// <summary>Event triggered when the player dies.</summary>
    public event Action OnPlayerDeath;

    // ======================= UNITY CALLBACKS =========================

    private void Start()
    {
        InitializePlayer();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    // ========================= PUBLIC METHODS ========================

    /// <summary>Reduces health by damage amount and triggers death event if health is zero or less.</summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    // ========================= STATIC METHODS ========================

    /// <summary>Resets the total player count to zero.</summary>
    public static void ResetTotalPlayers()
    {
        _totalPlayers = 0;
    }

    // ========================= PRIVATE METHODS =======================

    /// <summary>Reads movement and jump input for the next physics step.</summary>
    private void HandleInput()
    {
        _horizontalInput = Input.GetAxis(HORIZONTAL_AXIS);

        if (Input.GetButtonDown(JUMP_BUTTON))
        {
            _isJumpRequested = true;
        }
    }

    /// <summary>Applies horizontal movement and any pending jump to the Rigidbody.</summary>
    private void ApplyMovement()
    {
        float horizontalVelocity = _horizontalInput * _moveSpeed;
        _rigidbody.velocity = new Vector3(horizontalVelocity, _rigidbody.velocity.y, 0f);

        if (!_isJumpRequested) return;

        Jump();
        _isJumpRequested = false;
    }

    /// <summary>Applies jump force to the player.</summary>
    private void Jump()
    {
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }
}
