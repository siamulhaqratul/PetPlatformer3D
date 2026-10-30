using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using CandyCoded.HapticFeedback;
#endif

public static class Vibration
{
    // =========================== CONSTANTS ===========================

    private const string VIBRATION_KEY = "VibrationOn";
    private const int VIBRATION_ON = 1;
    private const int VIBRATION_OFF = 0;

    // =========================== PROPERTIES ==========================

    public static bool IsEnabled
    {
        get => PlayerPrefs.GetInt(VIBRATION_KEY, VIBRATION_ON) == VIBRATION_ON;
        set
        {
            PlayerPrefs.SetInt(VIBRATION_KEY, value ? VIBRATION_ON : VIBRATION_OFF);
            PlayerPrefs.Save();
        }
    }

    // ========================= STATIC METHODS ========================

    // The plugin only vibrates on Android, so every other platform (WebGL, the Editor) skips the call.
    public static void Light()
    {
        if (!IsEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        HapticFeedback.LightFeedback();
#endif
    }

    public static void Medium()
    {
        if (!IsEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        HapticFeedback.MediumFeedback();
#endif
    }

    public static void Heavy()
    {
        if (!IsEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        HapticFeedback.HeavyFeedback();
#endif
    }
}
