using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    private const float STAR_POP_DELAY = 0.35f;
    private const float STAR_POP_DURATION = 0.45f;
    private const float STAR_SPIN_ANGLE = 180f;
    private const float LIFE_PUNCH_SCALE = 0.4f;
    private const float LIFE_PUNCH_DURATION = 0.35f;
    private const float COIN_PUNCH_SCALE = 0.25f;
    private const float COIN_PUNCH_DURATION = 0.2f;
    private const float TITLE_BOB_HEIGHT = 14f;
    private const float TITLE_BOB_DURATION = 1.4f;
    private const float PLAY_PULSE_SCALE = 1.06f;
    private const float PLAY_PULSE_DURATION = 0.7f;
    private const float BADGE_POP_DELAY = 1.3f;
    private const float OBJECTIVE_POP_DELAY = 0.35f;
    private const float OBJECTIVE_PUNCH_SCALE = 0.3f;
    private const float OBJECTIVE_PUNCH_DURATION = 0.3f;
    private const float TOAST_FADE_DURATION = 0.2f;
    private const float TOAST_POP_SCALE = 0.7f;
    private const float FAIL_BAR_DELAY = 0.3f;
    private const float FAIL_BAR_DURATION = 0.8f;
    private const float SCORE_COUNT_DELAY = 0.3f;
    private const float SCORE_COUNT_DURATION = 0.9f;
    private const float HALFWAY_PROGRESS = 0.45f;
    private const float CLOSE_PROGRESS = 0.8f;
    private const string EARLY_FAIL_MESSAGE = "Good start! Try again.";
    private const string HALFWAY_FAIL_MESSAGE = "Over halfway! Try again.";
    private const string CLOSE_FAIL_MESSAGE = "So close! Try again.";

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Game")]
    [Tooltip("Game manager whose state drives the screens.")]
    [SerializeField] private GameManager _gameManager;

    [Tooltip("Pet form controller whose changes update the form badge and banner.")]
    [SerializeField] private PetFormController _formController;

    [Header("Screens")]
    [Tooltip("Title screen shown before a run.")]
    [SerializeField] private UIScreen _startScreen;
    [Tooltip("In-game HUD.")]
    [SerializeField] private UIScreen _hud;
    [Tooltip("Pause menu.")]
    [SerializeField] private UIScreen _pauseScreen;
    [Tooltip("Result screen after reaching the flag.")]
    [SerializeField] private UIScreen _winScreen;
    [Tooltip("Result screen after losing every life.")]
    [SerializeField] private UIScreen _failScreen;

    [Header("HUD")]
    [Tooltip("One icon per life, left to right.")]
    [SerializeField] private Image[] _lifeIcons;
    [Tooltip("Sprite for a life still left.")]
    [SerializeField] private Sprite _lifeFullSprite;
    [Tooltip("Sprite for a life already lost.")]
    [SerializeField] private Sprite _lifeEmptySprite;
    [Tooltip("Coin counter text.")]
    [SerializeField] private TMP_Text _coinText;
    [Tooltip("Coin icon, punched on every pickup.")]
    [SerializeField] private RectTransform _coinIcon;
    [Tooltip("Fill of the course progress bar, stretched by progress.")]
    [SerializeField] private RectTransform _progressFill;
    [Tooltip("Marker that slides along the progress bar.")]
    [SerializeField] private RectTransform _progressMarker;
    [Tooltip("Pause button in the HUD.")]
    [SerializeField] private Button _pauseButton;

    [Header("Form")]
    [Tooltip("Icon of the current animal in the HUD badge.")]
    [SerializeField] private Image _formIcon;
    [Tooltip("Name of the current animal in the HUD badge.")]
    [SerializeField] private TMP_Text _formNameText;
    [Tooltip("Banner shown when a gate changes the animal.")]
    [SerializeField] private CanvasGroup _formToast;
    [Tooltip("Animal icon on the banner.")]
    [SerializeField] private Image _toastIcon;
    [Tooltip("Animal name on the banner.")]
    [SerializeField] private TMP_Text _toastNameText;
    [Tooltip("Ability description on the banner.")]
    [SerializeField] private TMP_Text _toastAbilityText;
    [Tooltip("Seconds the banner stays fully visible.")]
    [SerializeField] private float _toastHoldDuration = 1.8f;

    [Header("Start")]
    [Tooltip("Game title, bobs gently on the title screen.")]
    [SerializeField] private RectTransform _title;
    [Tooltip("Starts a run.")]
    [SerializeField] private Button _playButton;
    [Tooltip("Wrapper around the play button that pulses to invite a click.")]
    [SerializeField] private RectTransform _playButtonPulse;
    [Tooltip("Stars showing the best result so far.")]
    [SerializeField] private Image[] _bestStars;
    [Tooltip("Best score so far, under the best stars.")]
    [SerializeField] private TMP_Text _bestScoreText;
    [Tooltip("Sprite for an earned star.")]
    [SerializeField] private Sprite _starFullSprite;
    [Tooltip("Sprite for a star not yet earned.")]
    [SerializeField] private Sprite _starEmptySprite;
    [Tooltip("Tint for a star not yet earned.")]
    [SerializeField] private Color _emptyStarColor = Color.grey;
    [Tooltip("Control hints for keyboard players.")]
    [SerializeField] private GameObject _keyboardHints;
    [Tooltip("Control hints for touch players.")]
    [SerializeField] private GameObject _touchHints;

    [Header("Pause")]
    [Tooltip("Closes the pause menu.")]
    [SerializeField] private Button _resumeButton;
    [Tooltip("Restarts the run from the pause menu.")]
    [SerializeField] private Button _pauseRestartButton;
    [Tooltip("Returns to the title from the pause menu.")]
    [SerializeField] private Button _pauseHomeButton;

    [Header("Win")]
    [Tooltip("Seconds of celebration (the pet's dance and confetti) before the win screen appears.")]
    [SerializeField] private float _winScreenDelay = 1.2f;
    [Tooltip("Filled stars, shown one by one for each star earned.")]
    [SerializeField] private Image[] _winStars;
    [Tooltip("Badge shown when the run beats the best result.")]
    [SerializeField] private RectTransform _newBestBadge;
    [Tooltip("Coins collected on the win screen.")]
    [SerializeField] private TMP_Text _winCoinText;
    [Tooltip("Run score on the win screen, counted up when it appears.")]
    [SerializeField] private TMP_Text _winScoreText;
    [Tooltip("Checkboxes for finishing, collecting coins and losing no lives, in star order.")]
    [SerializeField] private Image[] _objectiveChecks;
    [Tooltip("Label of the coin objective, filled with the coins needed.")]
    [SerializeField] private TMP_Text _coinObjectiveText;
    [Tooltip("Sprite for a completed objective.")]
    [SerializeField] private Sprite _checkOnSprite;
    [Tooltip("Sprite for an objective not completed.")]
    [SerializeField] private Sprite _checkOffSprite;
    [Tooltip("Plays the course again from the win screen.")]
    [SerializeField] private Button _winRetryButton;
    [Tooltip("Returns to the title from the win screen.")]
    [SerializeField] private Button _winHomeButton;

    [Header("Fail")]
    [Tooltip("Encouraging line on the fail screen, chosen by how far the run got.")]
    [SerializeField] private TMP_Text _failMessageText;
    [Tooltip("Fill of the fail screen bar showing how far the run got.")]
    [SerializeField] private RectTransform _failProgressFill;
    [Tooltip("Coins collected on the fail screen.")]
    [SerializeField] private TMP_Text _failCoinText;
    [Tooltip("Run score on the fail screen, counted up when it appears.")]
    [SerializeField] private TMP_Text _failScoreText;
    [Tooltip("Retries the course from the fail screen.")]
    [SerializeField] private Button _failRetryButton;
    [Tooltip("Returns to the title from the fail screen.")]
    [SerializeField] private Button _failHomeButton;

    [Header("Settings")]
    [Tooltip("Settings overlay with the volume sliders and the vibration toggle.")]
    [SerializeField] private SettingsPanel _settingsPanel;
    [Tooltip("Buttons that open the settings panel, on the title and pause screens.")]
    [SerializeField] private Button[] _settingsButtons;

    [Header("Audio")]
    [Tooltip("Source that plays UI sounds.")]
    [SerializeField] private AudioSource _audioSource;
    [Tooltip("Sound played on every button click.")]
    [SerializeField] private AudioClip _clickClip;
    [Tooltip("Sound played when a coin is collected.")]
    [SerializeField] private AudioClip _coinClip;

    // =================== PRIVATE/PROTECTED FIELDS ====================

    private int _shownCoins;
    private Tween _winScreenDelayTween;

    // ========================= INITIALIZE METHODS ========================

    private void InitializeUI()
    {
        // Pausing pauses game sounds through the listener; clicks on the pause menu still need to play.
        // Set here because Unity does not save this setting with the scene.
        _audioSource.ignoreListenerPause = true;
        _startScreen.HideImmediate();
        _hud.HideImmediate();
        _pauseScreen.HideImmediate();
        _winScreen.HideImmediate();
        _failScreen.HideImmediate();
        _settingsPanel.HideImmediate();

        BindButton(_playButton, _gameManager.StartRun);
        BindButton(_pauseButton, _gameManager.Pause);
        BindButton(_resumeButton, _gameManager.Resume);
        BindButton(_pauseRestartButton, _gameManager.StartRun);
        BindButton(_pauseHomeButton, _gameManager.ReturnToTitle);
        BindButton(_winRetryButton, _gameManager.StartRun);
        BindButton(_winHomeButton, _gameManager.ReturnToTitle);
        BindButton(_failRetryButton, _gameManager.StartRun);
        BindButton(_failHomeButton, _gameManager.ReturnToTitle);

        foreach (Button settingsButton in _settingsButtons)
        {
            BindButton(settingsButton, _settingsPanel.Open);
        }

        BindButton(_settingsPanel.CloseButton, _settingsPanel.Close);
        BindButton(_settingsPanel.VibrationButton, _settingsPanel.ToggleVibration);
        UpdateControlHints();
        _coinObjectiveText.text = $"COLLECT {_gameManager.CoinsForStar} COINS";
        StartIdleAnimations();
        UpdateLives(_gameManager.Lives, false);
        SetCoins(_gameManager.CoinsCollected);
        ShowScreensFor(_gameManager.State);
    }

    // ======================= UNITY CALLBACKS =========================

    private void Start()
    {
        InitializeUI();
    }

    private void OnEnable()
    {
        _gameManager.OnStateChanged += HandleStateChanged;
        _gameManager.OnLivesChanged += HandleLivesChanged;
        _gameManager.OnCoinsChanged += HandleCoinsChanged;
        _formController.OnFormChanged += HandleFormChanged;
        InputSystem.onDeviceChange += HandleDeviceChange;
    }

    private void OnDisable()
    {
        _gameManager.OnStateChanged -= HandleStateChanged;
        _gameManager.OnLivesChanged -= HandleLivesChanged;
        _gameManager.OnCoinsChanged -= HandleCoinsChanged;
        _formController.OnFormChanged -= HandleFormChanged;
        InputSystem.onDeviceChange -= HandleDeviceChange;
    }

    private void Update()
    {
        if (_gameManager.State != GameState.Playing) return;

        SetProgress(_gameManager.CourseProgress);
    }

    // ========================= PRIVATE METHODS =======================

    private void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        button.onClick.AddListener(PlayClick);
        button.onClick.AddListener(action);
    }

    private void PlayClick()
    {
        _audioSource.PlayOneShot(_clickClip, SoundSettings.SfxVolume);
        Vibration.Light();
    }

    private void StartIdleAnimations()
    {
        _title.DOAnchorPosY(_title.anchoredPosition.y + TITLE_BOB_HEIGHT, TITLE_BOB_DURATION)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        _playButtonPulse.DOScale(PLAY_PULSE_SCALE, PLAY_PULSE_DURATION)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    // On a win the HUD goes first and the win screen waits, so the dance and confetti have the screen to themselves.
    // Any other state change, such as a quick restart during the celebration, cancels the wait.
    // Settings belong to the screen they were opened from, so leaving it (Esc to resume, say) closes them.
    private void HandleStateChanged(GameState state)
    {
        _winScreenDelayTween?.Kill();
        _settingsPanel.Close();

        if (state == GameState.Won)
        {
            SetVisible(_hud, false);
            // Game time, like the dance and confetti it waits for (DelayedCall ignores the time scale by default).
            _winScreenDelayTween = DOVirtual.DelayedCall(_winScreenDelay, () => ShowScreensFor(state), false);
            return;
        }

        ShowScreensFor(state);
    }

    private void HandleLivesChanged(int lives)
    {
        UpdateLives(lives, true);
    }

    private void HandleCoinsChanged(int collected, int total)
    {
        bool isPickup = collected > _shownCoins;
        SetCoins(collected);

        if (!isPickup) return;

        _audioSource.PlayOneShot(_coinClip, SoundSettings.SfxVolume);
        _coinIcon.DOKill();
        _coinIcon.localScale = Vector3.one;
        _coinIcon.DOPunchScale(Vector3.one * COIN_PUNCH_SCALE, COIN_PUNCH_DURATION);
    }

    private void ShowScreensFor(GameState state)
    {
        SetVisible(_startScreen, state == GameState.Start);
        SetVisible(_hud, state == GameState.Playing || state == GameState.Paused);
        SetVisible(_pauseScreen, state == GameState.Paused);
        SetVisible(_winScreen, state == GameState.Won);
        SetVisible(_failScreen, state == GameState.Failed);

        switch (state)
        {
            case GameState.Start:
                UpdateControlHints();
                ShowBestStars(_gameManager.BestStars);
                _bestScoreText.text = _gameManager.BestScore.ToString();
                SetProgress(0f);
                break;
            case GameState.Won:
                PlayStars(_gameManager.EarnedStars);
                PlayObjectives(true, _gameManager.HasCoinStar, _gameManager.LivesLost == 0);
                PlayNewBestBadge(_gameManager.IsNewBest || _gameManager.IsNewBestScore);
                PlayScore(_winScoreText, _gameManager.Score);
                break;
            case GameState.Failed:
                PlayFailProgress(_gameManager.FurthestProgress);
                PlayScore(_failScoreText, _gameManager.Score);
                break;
        }
    }

    private void SetVisible(UIScreen screen, bool isVisible)
    {
        if (isVisible == screen.IsVisible) return;

        if (isVisible)
        {
            screen.Show();
        }
        else
        {
            screen.Hide();
        }
    }

    private void SetCoins(int collected)
    {
        string coins = collected.ToString();
        _coinText.text = coins;
        _winCoinText.text = coins;
        _failCoinText.text = coins;
        _shownCoins = collected;
    }

    // Counts the score up from zero, on unscaled time like the rest of the result screens.
    private void PlayScore(TMP_Text scoreText, int score)
    {
        scoreText.DOKill();
        scoreText.text = "0";
        int shownScore = 0;
        DOTween.To(() => shownScore, value =>
            {
                shownScore = value;
                scoreText.text = value.ToString();
            }, score, SCORE_COUNT_DURATION)
            .SetDelay(SCORE_COUNT_DELAY).SetEase(Ease.OutCubic).SetUpdate(true).SetTarget(scoreText);
    }

    private void SetProgress(float progress)
    {
        _progressFill.anchorMax = new Vector2(progress, 1f);
        _progressMarker.anchorMin = new Vector2(progress, 0.5f);
        _progressMarker.anchorMax = new Vector2(progress, 0.5f);
    }

    private void UpdateLives(int lives, bool animate)
    {
        for (int index = 0; index < _lifeIcons.Length; index++)
        {
            Image icon = _lifeIcons[index];
            bool isFull = index < lives;
            bool justLost = animate && index == lives;
            icon.sprite = isFull ? _lifeFullSprite : _lifeEmptySprite;

            if (!justLost) continue;

            icon.transform.DOKill();
            icon.transform.localScale = Vector3.one;
            icon.transform.DOPunchScale(Vector3.one * LIFE_PUNCH_SCALE, LIFE_PUNCH_DURATION).SetUpdate(true);
        }
    }

    private void ShowBestStars(int bestStars)
    {
        for (int index = 0; index < _bestStars.Length; index++)
        {
            bool isEarned = index < bestStars;
            _bestStars[index].sprite = isEarned ? _starFullSprite : _starEmptySprite;
            _bestStars[index].color = isEarned ? Color.white : _emptyStarColor;
        }
    }

    private void PlayStars(int earnedStars)
    {
        for (int index = 0; index < _winStars.Length; index++)
        {
            Transform star = _winStars[index].transform;
            star.DOKill();
            star.localScale = Vector3.zero;
            star.localRotation = Quaternion.Euler(0f, 0f, -STAR_SPIN_ANGLE);

            if (index >= earnedStars) continue;

            float delay = STAR_POP_DELAY * (index + 1);
            star.DOScale(Vector3.one, STAR_POP_DURATION).SetDelay(delay).SetEase(Ease.OutBack).SetUpdate(true);
            star.DOLocalRotate(Vector3.zero, STAR_POP_DURATION).SetDelay(delay).SetEase(Ease.OutQuad).SetUpdate(true);
        }
    }

    private void PlayObjectives(params bool[] completed)
    {
        for (int index = 0; index < _objectiveChecks.Length; index++)
        {
            Image check = _objectiveChecks[index];
            bool isDone = completed[index];
            check.sprite = isDone ? _checkOnSprite : _checkOffSprite;
            check.transform.DOKill();
            check.transform.localScale = Vector3.one;

            if (!isDone) continue;

            check.transform.DOPunchScale(Vector3.one * OBJECTIVE_PUNCH_SCALE, OBJECTIVE_PUNCH_DURATION)
                .SetDelay(OBJECTIVE_POP_DELAY * (index + 1)).SetUpdate(true);
        }
    }

    private void HandleFormChanged(PetForm form, bool isFromGate)
    {
        _formIcon.sprite = form.Icon;
        _formNameText.text = form.DisplayName;

        if (!isFromGate)
        {
            _formToast.DOKill();
            _formToast.alpha = 0f;
            return;
        }

        _formIcon.transform.DOKill();
        _formIcon.transform.localScale = Vector3.one;
        _formIcon.transform.DOPunchScale(Vector3.one * LIFE_PUNCH_SCALE, LIFE_PUNCH_DURATION);
        ShowFormToast(form);
    }

    private void ShowFormToast(PetForm form)
    {
        bool hasTouchscreen = Touchscreen.current != null;
        _toastIcon.sprite = form.Icon;
        _toastNameText.text = $"{form.DisplayName}!";
        _toastAbilityText.text = hasTouchscreen ? form.TouchAbilityText : form.AbilityText;

        Transform toastTransform = _formToast.transform;
        _formToast.DOKill();
        toastTransform.DOKill();
        _formToast.alpha = 0f;
        toastTransform.localScale = Vector3.one * TOAST_POP_SCALE;

        Sequence toast = DOTween.Sequence().SetTarget(_formToast);
        toast.Append(_formToast.DOFade(1f, TOAST_FADE_DURATION));
        toast.Join(toastTransform.DOScale(Vector3.one, TOAST_FADE_DURATION * 2f).SetEase(Ease.OutBack));
        toast.AppendInterval(_toastHoldDuration);
        toast.Append(_formToast.DOFade(0f, TOAST_FADE_DURATION * 1.5f));
    }

    // Touch hints whenever a touchscreen exists (phones, tablets, touch laptops); keyboard hints otherwise.
    private void UpdateControlHints()
    {
        bool hasTouchscreen = Touchscreen.current != null;
        _keyboardHints.SetActive(!hasTouchscreen);
        _touchHints.SetActive(hasTouchscreen);
    }

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Touchscreen) return;

        UpdateControlHints();
    }

    private void PlayFailProgress(float progress)
    {
        _failMessageText.text = FailMessageFor(progress);
        _failProgressFill.DOKill();
        _failProgressFill.anchorMax = new Vector2(0f, 1f);
        _failProgressFill.DOAnchorMax(new Vector2(progress, 1f), FAIL_BAR_DURATION)
            .SetDelay(FAIL_BAR_DELAY).SetEase(Ease.OutCubic).SetUpdate(true);
    }

    private static string FailMessageFor(float progress)
    {
        if (progress >= CLOSE_PROGRESS) return CLOSE_FAIL_MESSAGE;
        if (progress >= HALFWAY_PROGRESS) return HALFWAY_FAIL_MESSAGE;

        return EARLY_FAIL_MESSAGE;
    }

    private void PlayNewBestBadge(bool isNewBest)
    {
        _newBestBadge.DOKill();
        _newBestBadge.localScale = Vector3.zero;

        if (!isNewBest) return;

        _newBestBadge.DOScale(Vector3.one, STAR_POP_DURATION).SetDelay(BADGE_POP_DELAY).SetEase(Ease.OutBack).SetUpdate(true);
    }
}
