using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    private const float NORMAL_TIME_SCALE = 1f;
    private const float PAUSED_TIME_SCALE = 0f;
    private const int FINISH_STARS = 1;
    private const int NO_DAMAGE_STARS = 1;
    private const int COIN_STARS = 1;
    private const string BEST_STARS_KEY = "BestStars";
    // The coin target is rounded down to a multiple of this, so the win screen asks for a round number.
    private const int COIN_TARGET_STEP = 5;

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Pet")]
    [Tooltip("Motor of the player's pet.")]
    [SerializeField] private PetMotor _motor;
    [Tooltip("Visuals of the player's pet.")]
    [SerializeField] private PetVisual _visual;
    [Tooltip("Input source, used for the pause key.")]
    [SerializeField] private PetInput _input;
    [Tooltip("Animal form of the pet, restored on respawn and reset on restart.")]
    [SerializeField] private PetFormController _formController;

    [Header("Course")]
    [Tooltip("Where the pet starts every run.")]
    [SerializeField] private Transform _spawnPoint;
    [Tooltip("Finish line. Passing it wins the run, even in mid-air, and it also measures course progress.")]
    [SerializeField] private Transform _finishPoint;
    [Tooltip("Hides coins and obstacles the pet has already passed.")]
    [SerializeField] private CourseCuller _courseCuller;
    [Tooltip("Follow camera, told about teleports so it cuts instead of swooping.")]
    [SerializeField] private CinemachineVirtualCamera _followCamera;

    [Header("Rules")]
    [Tooltip("Lives at the start of a run.")]
    [SerializeField] private int _maxLives = 3;
    [Tooltip("The pet loses a life when it falls below this height.")]
    [SerializeField] private float _fallDeathHeight = -6f;
    [Tooltip("Seconds between losing a life and respawning.")]
    [SerializeField] private float _respawnDelay = 0.9f;
    [Tooltip("Seconds after respawning before the pet runs again.")]
    [SerializeField] private float _resumeRunDelay = 0.4f;
    [Tooltip("Coins placed in the course, set by the level builder.")]
    [SerializeField] private int _totalCoins;
    [Tooltip("Share of the course's coins needed for the coin star, so the target follows the coin count.")]
    [Range(0f, 1f)]
    [SerializeField] private float _coinStarShare = 0.8f;
    [Tooltip("Seconds after a win or fail before jump restarts the run.")]
    [SerializeField] private float _quickRestartDelay = 0.8f;

    [Header("Performance")]
    [Tooltip("Frame rate asked for on phones, which otherwise run at 30. WebGL is left to follow the browser.")]
    [SerializeField] private int _mobileFrameRate = 60;

    [Header("Debug")]
    [Tooltip("Skip the start screen and run immediately, for testing.")]
    [SerializeField] private bool _autoStartRun;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private Transform _petTransform;
    private PetForm _checkpointForm;
    private GameState _state;
    private int _lives;
    private int _livesLost;
    private Vector3 _checkpointPosition;
    private Coroutine _loseLifeRoutine;
    private float _runEndTime;
    private int _coinsCollected;
    private float _furthestProgress;
    private bool _isNewBest;

    // ========================= INITIALIZE METHODS ========================

    private void InitializeGame()
    {
        // On WebGL a target frame rate swaps the browser's smooth frame callback for a timer, so it is left unset there.
        if (Application.isMobilePlatform)
        {
            Application.targetFrameRate = _mobileFrameRate;
        }

        _petTransform = _motor.transform;
        ResetRun();
        SetState(GameState.Start);

        if (_autoStartRun)
        {
            StartRun();
        }
    }

    // =========================== PROPERTIES ==========================

    public GameState State => _state;
    public int Lives => _lives;
    public int MaxLives => _maxLives;
    public int LivesLost => _livesLost;
    public int CoinsCollected => _coinsCollected;
    public int TotalCoins => _totalCoins;
    public int BestStars => PlayerPrefs.GetInt(BEST_STARS_KEY, 0);
    public bool IsNewBest => _isNewBest;
    public float FurthestProgress => _furthestProgress;
    public float CourseProgress => Mathf.Clamp01(Mathf.InverseLerp(_spawnPoint.position.z, _finishPoint.position.z, _petTransform.position.z));
    // 80% of 63 coins is 50.4, which rounds down to 50. Never below one step, so a course with few coins still needs some.
    public int CoinsForStar => Mathf.Max(COIN_TARGET_STEP, Mathf.FloorToInt(_totalCoins * _coinStarShare / COIN_TARGET_STEP) * COIN_TARGET_STEP);
    public bool HasCoinStar => _totalCoins > 0 && _coinsCollected >= CoinsForStar;
    public int EarnedStars => FINISH_STARS + (_livesLost == 0 ? NO_DAMAGE_STARS : 0) + (HasCoinStar ? COIN_STARS : 0);

    // ============================ EVENTS =============================

    public event Action<GameState> OnStateChanged;
    public event Action<int> OnLivesChanged;
    public event Action OnLifeLost;
    public event Action<int, int> OnCoinsChanged;

    // ======================= UNITY CALLBACKS =========================

    private void Start()
    {
        InitializeGame();
    }

    private void OnEnable()
    {
        GameEvents.OnPetHit += HandlePetHit;
        _motor.OnBonked += HandlePetHit;
        GameEvents.OnCheckpointReached += HandleCheckpointReached;
        GameEvents.OnFinishReached += HandleFinishReached;
        GameEvents.OnCoinCollected += HandleCoinCollected;
        _formController.OnFormChanged += HandleFormChanged;
        _input.OnPausePressed += TogglePause;
        _input.OnJumpPressed += HandleJumpPressed;
    }

    private void OnDisable()
    {
        GameEvents.OnPetHit -= HandlePetHit;
        _motor.OnBonked -= HandlePetHit;
        GameEvents.OnCheckpointReached -= HandleCheckpointReached;
        GameEvents.OnFinishReached -= HandleFinishReached;
        GameEvents.OnCoinCollected -= HandleCoinCollected;
        _formController.OnFormChanged -= HandleFormChanged;
        _input.OnPausePressed -= TogglePause;
        _input.OnJumpPressed -= HandleJumpPressed;
    }

    private void Update()
    {
        CheckFall();
        CheckFinish();
        TrackProgress();
    }

    // ========================= PUBLIC METHODS ========================

    public void StartRun()
    {
        ResetRun();
        SetState(GameState.Playing);
        _motor.SetRunning(true);
    }

    public void ReturnToTitle()
    {
        ResetRun();
        SetState(GameState.Start);
    }

    public void TogglePause()
    {
        if (_state == GameState.Playing)
        {
            Pause();
        }
        else if (_state == GameState.Paused)
        {
            Resume();
        }
    }

    public void Pause()
    {
        if (_state != GameState.Playing) return;

        Time.timeScale = PAUSED_TIME_SCALE;
        _motor.SetPaused(true);
        SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (_state != GameState.Paused) return;

        Time.timeScale = NORMAL_TIME_SCALE;
        _motor.SetPaused(false);
        SetState(GameState.Playing);
    }

    // ========================= PRIVATE METHODS =======================

    private void ResetRun()
    {
        if (_loseLifeRoutine != null)
        {
            StopCoroutine(_loseLifeRoutine);
            _loseLifeRoutine = null;
        }

        Time.timeScale = NORMAL_TIME_SCALE;
        _motor.SetPaused(false);
        _lives = _maxLives;
        _livesLost = 0;
        _coinsCollected = 0;
        _furthestProgress = 0f;
        _isNewBest = false;
        _checkpointPosition = _spawnPoint.position;
        _checkpointForm = _formController.StartForm;
        _formController.SetFormSilently(_checkpointForm);
        _motor.SetRunning(false);
        _motor.SetFrozen(false);
        MovePetTo(_checkpointPosition);
        _visual.ResetVisual();
        // Re-enable course objects first so the coins among them receive the reset event.
        _courseCuller.ShowFrom(_spawnPoint.position.z);
        GameEvents.RaiseRunReset();
        OnLivesChanged?.Invoke(_lives);
        OnCoinsChanged?.Invoke(_coinsCollected, _totalCoins);
    }

    private void HandleCoinCollected(Vector3 position)
    {
        _coinsCollected++;
        OnCoinsChanged?.Invoke(_coinsCollected, _totalCoins);
        Vibration.Light();
    }

    private void HandleFormChanged(PetForm form, bool isFromGate)
    {
        if (!isFromGate) return;

        Vibration.Medium();
    }

    private void SaveBestStars()
    {
        _isNewBest = EarnedStars > BestStars;
        if (!_isNewBest) return;

        PlayerPrefs.SetInt(BEST_STARS_KEY, EarnedStars);
        PlayerPrefs.Save();
    }

    private void TrackProgress()
    {
        if (_state != GameState.Playing) return;

        _furthestProgress = Mathf.Max(_furthestProgress, CourseProgress);
    }

    private void CheckFall()
    {
        bool hasFallen = _state == GameState.Playing && _petTransform.position.y < _fallDeathHeight;
        if (!hasFallen) return;

        HandlePetHit();
    }

    // The finish trigger has a fixed height, so a high jump or glide can clear it; passing its Z always counts.
    private void CheckFinish()
    {
        bool hasPassedFinish = _state == GameState.Playing && _petTransform.position.z >= _finishPoint.position.z;
        if (!hasPassedFinish) return;

        HandleFinishReached();
    }

    private void HandlePetHit()
    {
        bool canLoseLife = _state == GameState.Playing && _loseLifeRoutine == null;
        if (!canLoseLife) return;

        _loseLifeRoutine = StartCoroutine(LoseLifeRoutine());
    }

    private IEnumerator LoseLifeRoutine()
    {
        _motor.SetRunning(false);
        _motor.SetFrozen(true);
        _visual.PlayHit();
        Vibration.Heavy();
        _lives--;
        _livesLost++;
        OnLivesChanged?.Invoke(_lives);
        OnLifeLost?.Invoke();

        yield return new WaitForSeconds(_respawnDelay);

        if (_lives <= 0)
        {
            _loseLifeRoutine = null;
            SetState(GameState.Failed);
            yield break;
        }

        MovePetTo(_checkpointPosition);
        _courseCuller.ShowFrom(_checkpointPosition.z);
        _formController.SetFormSilently(_checkpointForm);
        GameEvents.RaisePetRespawned(_checkpointPosition.z);
        _motor.SetFrozen(false);
        _visual.PlayRespawn();

        yield return new WaitForSeconds(_resumeRunDelay);

        _loseLifeRoutine = null;
        _motor.SetRunning(true);
    }

    private void HandleJumpPressed()
    {
        if (_state == GameState.Start)
        {
            StartRun();
            return;
        }

        bool isRunOver = _state == GameState.Won || _state == GameState.Failed;
        bool canQuickRestart = isRunOver && Time.unscaledTime - _runEndTime >= _quickRestartDelay;
        if (!canQuickRestart) return;

        StartRun();
    }

    private void HandleCheckpointReached(Vector3 respawnPosition)
    {
        _checkpointPosition = respawnPosition;
        _checkpointForm = _formController.CurrentForm;
        Vibration.Light();
    }

    private void HandleFinishReached()
    {
        bool canWin = _state == GameState.Playing && _loseLifeRoutine == null;
        if (!canWin) return;

        _motor.SetRunning(false);
        _visual.PlayDance();
        Vibration.Medium();
        SaveBestStars();
        SetState(GameState.Won);
    }

    private void MovePetTo(Vector3 position)
    {
        Vector3 offset = position - _petTransform.position;
        _motor.TeleportTo(position);
        _followCamera.OnTargetObjectWarped(_petTransform, offset);
    }

    private void SetState(GameState state)
    {
        _state = state;

        if (state == GameState.Won || state == GameState.Failed)
        {
            _runEndTime = Time.unscaledTime;
        }

        OnStateChanged?.Invoke(state);
    }
}
