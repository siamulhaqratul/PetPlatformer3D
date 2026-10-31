using UnityEngine;
using UnityEngine.UI;

// Settings overlay opened from the title and pause screens: sound effects volume, music volume and, on Android,
// vibration. Volumes apply while a slider moves and are written to disk when the panel closes.
public class SettingsPanel : MonoBehaviour
{
    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("Screen")]
    [Tooltip("Screen holding the panel, drawn above every other screen.")]
    [SerializeField] private UIScreen _screen;

    [Header("Controls")]
    [Tooltip("Sound effects volume, 0 to 1.")]
    [SerializeField] private Slider _sfxSlider;
    [Tooltip("Music volume, 0 to 1.")]
    [SerializeField] private Slider _bgmSlider;
    [Tooltip("Row with the vibration toggle, hidden where the device cannot vibrate.")]
    [SerializeField] private GameObject _vibrationRow;
    [Tooltip("Turns vibration on or off.")]
    [SerializeField] private Button _vibrationButton;
    [Tooltip("Checkbox icon on the vibration toggle.")]
    [SerializeField] private Image _vibrationCheck;
    [Tooltip("Sprite for a ticked checkbox.")]
    [SerializeField] private Sprite _checkOnSprite;
    [Tooltip("Sprite for an empty checkbox.")]
    [SerializeField] private Sprite _checkOffSprite;
    [Tooltip("Closes the panel.")]
    [SerializeField] private Button _closeButton;

    [Header("Audio")]
    [Tooltip("Plays the music whose volume the music slider sets.")]
    [SerializeField] private GameAudio _gameAudio;

    // =========================== PROPERTIES ==========================

    public Button CloseButton => _closeButton;
    public Button VibrationButton => _vibrationButton;

    // Only Android builds vibrate; the Editor shows the row too so it can be tested.
    private static bool CanVibrate => Application.platform == RuntimePlatform.Android || Application.isEditor;

    // ======================= UNITY CALLBACKS =========================

    private void Awake()
    {
        _sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
        _bgmSlider.onValueChanged.AddListener(HandleBgmChanged);
    }

    // ========================= PUBLIC METHODS ========================

    public void Open()
    {
        _sfxSlider.SetValueWithoutNotify(SoundSettings.SfxVolume);
        _bgmSlider.SetValueWithoutNotify(SoundSettings.BgmVolume);
        _vibrationRow.SetActive(CanVibrate);
        UpdateVibrationCheck();
        _screen.Show();
    }

    public void Close()
    {
        if (!_screen.IsVisible) return;

        SoundSettings.Save();
        _screen.Hide();
    }

    public void HideImmediate()
    {
        _screen.HideImmediate();
    }

    public void ToggleVibration()
    {
        Vibration.IsEnabled = !Vibration.IsEnabled;
        UpdateVibrationCheck();
        Vibration.Medium();
    }

    // ========================= PRIVATE METHODS =======================

    private void HandleSfxChanged(float volume)
    {
        SoundSettings.SfxVolume = volume;
    }

    private void HandleBgmChanged(float volume)
    {
        SoundSettings.BgmVolume = volume;
        _gameAudio.RefreshMusicVolume();
    }

    private void UpdateVibrationCheck()
    {
        _vibrationCheck.sprite = Vibration.IsEnabled ? _checkOnSprite : _checkOffSprite;
    }
}
