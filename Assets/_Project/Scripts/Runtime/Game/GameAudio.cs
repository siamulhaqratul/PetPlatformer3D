using UnityEngine;

// Plays the game's sound effects and jingles in response to gameplay events. Button clicks and coin pickups
// stay with the UI manager. Volumes come from SoundSettings, which the settings panel sets.
public class GameAudio : MonoBehaviour
{
    // =========================== CONSTANTS ===========================

    // Landings slower than this make no sound, so small steps stay quiet.
    private const float QUIET_LANDING_SPEED = 4f;
    // Landings at this speed or faster play at full volume.
    private const float LOUD_LANDING_SPEED = 20f;
    private const float QUIET_LANDING_VOLUME = 0.35f;

    // ================= SERIALIZED FIELDS (INSPECTOR) =================

    [Header("References")]
    [Tooltip("Game manager whose lives and state changes play sounds.")]
    [SerializeField] private GameManager _gameManager;
    [Tooltip("Motor of the player's pet.")]
    [SerializeField] private PetMotor _motor;
    [Tooltip("Animal form of the pet, for the gate change sound.")]
    [SerializeField] private PetFormController _formController;
    [Tooltip("Source for short sound effects.")]
    [SerializeField] private AudioSource _effectsSource;
    [Tooltip("Source for the win and fail jingles.")]
    [SerializeField] private AudioSource _jingleSource;

    [Header("Pet")]
    [Tooltip("Played when the pet jumps.")]
    [SerializeField] private AudioClip _jumpClip;
    [Tooltip("Played when the pet lands, louder for harder landings.")]
    [SerializeField] private AudioClip _landClip;
    [Tooltip("Played when the pet starts a roll.")]
    [SerializeField] private AudioClip _rollClip;
    [Tooltip("Played when the pet loses a life.")]
    [SerializeField] private AudioClip _hitClip;
    [Tooltip("Played when the pet reappears at a checkpoint.")]
    [SerializeField] private AudioClip _respawnClip;

    [Header("Course")]
    [Tooltip("Played when a gate changes the pet's animal.")]
    [SerializeField] private AudioClip _formChangeClip;
    [Tooltip("Played when the pet reaches a checkpoint.")]
    [SerializeField] private AudioClip _checkpointClip;

    [Header("Run End")]
    [Tooltip("Jingle for crossing the finish line.")]
    [SerializeField] private AudioClip _winJingle;
    [Tooltip("Jingle for losing the last life.")]
    [SerializeField] private AudioClip _failJingle;

    [Header("Music")]
    [Tooltip("Source for the looping background music. Leave empty for no music.")]
    [SerializeField] private AudioSource _musicSource;
    [Tooltip("Background music track, looped for the whole session.")]
    [SerializeField] private AudioClip _musicClip;
    [Tooltip("Music volume with the Music slider at full, so the music sits under the sound effects.")]
    [Range(0f, 1f)]
    [SerializeField] private float _musicVolume = 0.5f;

    [Header("Variation")]
    [Tooltip("How far each effect's pitch may drift up or down, so repeated sounds do not feel mechanical.")]
    [Range(0f, 0.3f)]
    [SerializeField] private float _pitchVariation = 0.06f;

    // ======================= UNITY CALLBACKS =========================

    private void Start()
    {
        PlayMusic();
    }

    private void OnEnable()
    {
        _motor.OnJumped += HandleJumped;
        _motor.OnLanded += HandleLanded;
        _motor.OnRollStarted += HandleRollStarted;
        _gameManager.OnLifeLost += HandleLifeLost;
        _gameManager.OnStateChanged += HandleStateChanged;
        _formController.OnFormChanged += HandleFormChanged;
        GameEvents.OnCheckpointReached += HandleCheckpointReached;
        GameEvents.OnPetRespawned += HandlePetRespawned;
    }

    private void OnDisable()
    {
        _motor.OnJumped -= HandleJumped;
        _motor.OnLanded -= HandleLanded;
        _motor.OnRollStarted -= HandleRollStarted;
        _gameManager.OnLifeLost -= HandleLifeLost;
        _gameManager.OnStateChanged -= HandleStateChanged;
        _formController.OnFormChanged -= HandleFormChanged;
        GameEvents.OnCheckpointReached -= HandleCheckpointReached;
        GameEvents.OnPetRespawned -= HandlePetRespawned;
    }

    // ========================= PUBLIC METHODS ========================

    // Called by the settings panel when the BGM slider moves.
    public void RefreshMusicVolume()
    {
        if (_musicSource == null) return;

        _musicSource.volume = SoundSettings.BgmVolume * _musicVolume;
    }

    // ========================= PRIVATE METHODS =======================

    private void PlayMusic()
    {
        if (_musicSource == null || _musicClip == null) return;

        _musicSource.clip = _musicClip;
        _musicSource.loop = true;
        RefreshMusicVolume();
        _musicSource.Play();
    }

    private void HandleJumped()
    {
        PlayEffect(_jumpClip, 1f);
    }

    private void HandleLanded(float fallSpeed)
    {
        if (fallSpeed < QUIET_LANDING_SPEED) return;

        float loudness = Mathf.InverseLerp(QUIET_LANDING_SPEED, LOUD_LANDING_SPEED, fallSpeed);
        PlayEffect(_landClip, Mathf.Lerp(QUIET_LANDING_VOLUME, 1f, loudness));
    }

    private void HandleRollStarted()
    {
        PlayEffect(_rollClip, 1f);
    }

    private void HandleLifeLost()
    {
        PlayEffect(_hitClip, 1f);
    }

    // Pausing pauses every sound except the UI's, whose source ignores the listener pause.
    // Jingles only play on the win and fail screens, so leaving them for any other state cuts the jingle.
    // The music holds while a jingle plays and carries on from the same spot afterwards.
    private void HandleStateChanged(GameState state)
    {
        AudioListener.pause = state == GameState.Paused;

        if (state == GameState.Won)
        {
            PauseMusic();
            PlayJingle(_winJingle);
        }
        else if (state == GameState.Failed)
        {
            PauseMusic();
            PlayJingle(_failJingle);
        }
        else
        {
            _jingleSource.Stop();
            ResumeMusic();
        }
    }

    private void PauseMusic()
    {
        if (_musicSource == null) return;

        _musicSource.Pause();
    }

    private void ResumeMusic()
    {
        if (_musicSource == null || _musicSource.clip == null) return;

        _musicSource.UnPause();
    }

    private void HandleFormChanged(PetForm form, bool isFromGate)
    {
        if (!isFromGate) return;

        PlayEffect(_formChangeClip, 1f);
    }

    private void HandleCheckpointReached(Vector3 respawnPosition)
    {
        PlayEffect(_checkpointClip, 1f);
    }

    private void HandlePetRespawned(float respawnZ)
    {
        PlayEffect(_respawnClip, 1f);
    }

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (clip == null) return;

        _effectsSource.pitch = 1f + Random.Range(-_pitchVariation, _pitchVariation);
        _effectsSource.PlayOneShot(clip, volume * SoundSettings.SfxVolume);
    }

    private void PlayJingle(AudioClip clip)
    {
        if (clip == null) return;

        _jingleSource.Stop();
        _jingleSource.clip = clip;
        _jingleSource.volume = SoundSettings.SfxVolume;
        _jingleSource.Play();
    }
}
