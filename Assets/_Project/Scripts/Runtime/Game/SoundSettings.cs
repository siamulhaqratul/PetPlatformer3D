using UnityEngine;

public static class SoundSettings
{
    // =========================== CONSTANTS ===========================

    private const string SFX_VOLUME_KEY = "SfxVolume";
    private const string BGM_VOLUME_KEY = "BgmVolume";
    // Saved by the old main-menu sound toggle; read once so a player who muted keeps silence.
    private const string LEGACY_SOUND_KEY = "SoundOn";
    private const int LEGACY_SOUND_ON = 1;
    private const int LEGACY_SOUND_OFF = 0;
    private const float FULL_VOLUME = 1f;
    private const float MUTED_VOLUME = 0f;

    // =========================== PROPERTIES ==========================

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SFX_VOLUME_KEY, DefaultVolume);
        set => SaveVolume(SFX_VOLUME_KEY, value);
    }

    public static float BgmVolume
    {
        get => PlayerPrefs.GetFloat(BGM_VOLUME_KEY, DefaultVolume);
        set => SaveVolume(BGM_VOLUME_KEY, value);
    }

    private static float DefaultVolume => PlayerPrefs.GetInt(LEGACY_SOUND_KEY, LEGACY_SOUND_ON) == LEGACY_SOUND_OFF ? MUTED_VOLUME : FULL_VOLUME;

    // ========================= STATIC METHODS ========================

    private static void SaveVolume(string key, float volume)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }
}
