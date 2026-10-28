// Render pipelines that utilize the DynamicResolutionHandler class can benefit from a simpler, more streamlined codepath.
// Prefer to leave this defined if possible, comment out only if your render pipeline does not support the feature.
// At the time of this script's last update, HDRP supports the DRH while URP and Built-in do not.
//#define PIPELINE_IMPLEMENTS_DRH

// Uncomment this for debugging tools.
// #define ENABLE_DYNAMIC_RESOLUTION_DEBUG

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if _2K__URP_UNITY__FOREVER_
using UnityEngine.Rendering.Universal;
#endif

namespace UnityExt.Optimization
{
    /// <summary>Runtime dynamic resolution scaler targeting a desired GPU frame time; supports DRH or manual buffer scaling.</summary>
    /// <remarks>This component adjusts the resolution of the game view in real-time to maintain a target frame rate, improving performance on lower-end hardware.</remarks>
    [DefaultExecutionOrder(-10000)]
    public class DynamicResolution : MonoBehaviour
    {
        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [Header("Android")] 
        [Tooltip("Enable dynamic resolution on Android.")]
        [SerializeField] private bool _enableInAndroid = false;
        [Tooltip("Minimum allowed scale on Android.")]
        [SerializeField] private float _androidMinScale = 0.6f;
        [Tooltip("Maximum allowed scale on Android.")]
        [SerializeField] private float _androidMaxScale = 0.85f;

        [Header("iOS")] 
        [Tooltip("Enable dynamic resolution on iOS.")]
        [SerializeField] private bool _enableInIOS = false;
        [Tooltip("Minimum allowed scale on iOS.")]
        [SerializeField] private float _iosMinScale = 0.6f;
        [Tooltip("Maximum allowed scale on iOS.")]
        [SerializeField] private float _iosMaxScale = 0.85f;

        [Header("Pipeline (URP)")]
        [Tooltip("URP pipeline assets to drive (only if URP symbol defined).")]
#if _2K__URP_UNITY__FOREVER_
        [SerializeField] private List<UniversalRenderPipelineAsset> _targets;
#endif

        // ======================= STATIC FIELDS ==========================

        private static DynamicResolution _instance;
        private static double _desiredFrameRate = 60.0;
        private static double _desiredFrameTime = 1000.0 / _desiredFrameRate; // ms
        private static float _currentScaleFactor = 1.0f; // 0-1 lerp factor
        private static bool _canUpdate = false;
        private static bool _systemEnabled = true;
        private static bool _platformSupported = true;
    private bool _useDrs = false;
#if PIPELINE_IMPLEMENTS_DRH
        private static bool _hasDoneOneTimeInit = false;
        private static uint _framesUntilInit = 1;
#endif

        // =========================== CONSTANTS ===========================

        private const uint SCALE_RAISE_COUNTER_LIMIT = 360;
        private const uint SCALE_RAISE_COUNTER_SMALL_INCREMENT = 3;
        private const uint SCALE_RAISE_COUNTER_BIG_INCREMENT = 10;
        private const double HEADROOM_THRESHOLD = 0.06; // proportion of target frame time
        private const double DELTA_THRESHOLD = 0.035;   // proportion of target frame time
        private const float SCALE_INCREASE_BASIS = (HEADROOM_THRESHOLD < DELTA_THRESHOLD) ? (float)HEADROOM_THRESHOLD : (float)DELTA_THRESHOLD;
        private const float SCALE_INCREASE_SMALL_FACTOR = 0.25f;
        private const float SCALE_INCREASE_BIG_FACTOR = 1.0f;
        private const float SCALE_HEADROOM_CLAMP_MIN = 0.1f;
        private const float SCALE_HEADROOM_CLAMP_MAX = 0.5f;
        private const uint NUM_FRAME_TIMINGS = 1;

        // =================== PRIVATE/PROTECTED FIELDS ====================

#if !PIPELINE_IMPLEMENTS_DRH
        private float _minScaleFactor = 0.65f;
        private float _maxScaleFactor = 0.8f;
#endif

        private uint _frameCount = 0;
        private readonly FrameTiming[] _frameTimings = new FrameTiming[NUM_FRAME_TIMINGS];
        private double _gpuFrameTime = 0.0;
        private double _cpuFrameTime = 0.0;
        private double _gpuTimeDelta = 0.0;
        private uint _scaleRaiseCounter = 0;

#if ENABLE_DYNAMIC_RESOLUTION_DEBUG
        private static GUIStyle _debugStyle;
#endif

        // ======================= UNITY CALLBACKS =========================

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            _useDrs = false;
#if !PIPELINE_IMPLEMENTS_DRH
            _minScaleFactor = 0.6f;
            _maxScaleFactor = 1.0f;
#endif
#if UNITY_ANDROID
            _useDrs = _enableInAndroid;
#if !PIPELINE_IMPLEMENTS_DRH
            _minScaleFactor = _androidMinScale;
            _maxScaleFactor = _androidMaxScale;
#endif
#elif UNITY_IOS
            _useDrs = _enableInIOS;
#if !PIPELINE_IMPLEMENTS_DRH
            _minScaleFactor = _iosMinScale;
            _maxScaleFactor = _iosMaxScale;
#endif
#endif
        }

        private void Start()
        {
#if UNITY_IOS
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Metal)
#endif
            {
                if (FrameTimingManager.GetCpuTimerFrequency() == 0 || FrameTimingManager.GetGpuTimerFrequency() == 0)
                {
                    _platformSupported = false;
                    _systemEnabled = false;
                }
            }

#if !PIPELINE_IMPLEMENTS_DRH
            _canUpdate = true;
#endif

#if ENABLE_DYNAMIC_RESOLUTION_DEBUG
            if (_debugStyle == null)
            {
                _debugStyle = new GUIStyle();
            }
#endif
        }

        private void Update()
        {
            if (!_systemEnabled) {
#if ENABLE_DYNAMIC_RESOLUTION_DEBUG
                if (_platformSupported) { GetFrameStats(); }
#endif
                return; }

#if PIPELINE_IMPLEMENTS_DRH
            if (!_hasDoneOneTimeInit)
            {
                if (_framesUntilInit == 0)
                {
                    _canUpdate = true;
                    DynamicResolutionHandler.SetDynamicResScaler(ScalerDelegate, DynamicResScalePolicyType.ReturnsMinMaxLerpFactor);
                    _hasDoneOneTimeInit = true;
                }
                --_framesUntilInit;
            }
#endif

            if (!_canUpdate) return;

            GetFrameStats();
            double headroom = _desiredFrameTime - _gpuFrameTime;

            if (headroom < 0.0)
            {
                _scaleRaiseCounter = 0;
                float scaleDecreaseFactor = (float)(headroom / _desiredFrameTime);
                _currentScaleFactor = Mathf.Clamp01(_currentScaleFactor + scaleDecreaseFactor);
#if !PIPELINE_IMPLEMENTS_DRH
                SetNewScale();
#endif
                return;
            }

            if (_gpuTimeDelta > headroom)
            {
                _scaleRaiseCounter = 0;
                float scaleDecreaseFactor = (float)(_gpuTimeDelta / _desiredFrameTime);
                _currentScaleFactor = Mathf.Clamp01(_currentScaleFactor - scaleDecreaseFactor);
#if !PIPELINE_IMPLEMENTS_DRH
                SetNewScale();
#endif
                return;
            }

            if (_gpuTimeDelta < 0.0)
            {
                _scaleRaiseCounter += SCALE_RAISE_COUNTER_BIG_INCREMENT;
            }
            else
            {
                double headroomThreshold = _desiredFrameTime * HEADROOM_THRESHOLD;
                double deltaThreshold = _desiredFrameTime * DELTA_THRESHOLD;
                if ((headroom > headroomThreshold) && (_gpuTimeDelta < deltaThreshold))
                {
                    _scaleRaiseCounter += SCALE_RAISE_COUNTER_SMALL_INCREMENT;
                }
            }

            if (_scaleRaiseCounter >= SCALE_RAISE_COUNTER_LIMIT)
            {
                _scaleRaiseCounter = 0;
                float headroomPercent = (float)(headroom / _desiredFrameTime);
                float clampedHeadroom = Mathf.Clamp(headroomPercent, SCALE_HEADROOM_CLAMP_MIN, SCALE_HEADROOM_CLAMP_MAX);
                float remappedHeadroom = (clampedHeadroom - SCALE_HEADROOM_CLAMP_MIN) / (SCALE_HEADROOM_CLAMP_MAX - SCALE_HEADROOM_CLAMP_MIN);
                float scaleIncreaseFactor = SCALE_INCREASE_BASIS * Mathf.Lerp(SCALE_INCREASE_SMALL_FACTOR, SCALE_INCREASE_BIG_FACTOR, remappedHeadroom);
                _currentScaleFactor = Mathf.Clamp01(_currentScaleFactor + scaleIncreaseFactor);
#if !PIPELINE_IMPLEMENTS_DRH
                SetNewScale();
#endif
            }
        }

        // ========================= PRIVATE METHODS =======================

#if _2K__URP_UNITY__FOREVER_
        /// <summary>Applies URP renderScale to all targeted pipeline assets.</summary>
        private void SetURP_DRS(float drsValue)
        {
            int len = _targets.Count;
            for (int i = 0; i < len; i++)
            {
                if (_targets[i] != null) _targets[i].renderScale = drsValue;
            }
        }
#endif

#if PIPELINE_IMPLEMENTS_DRH
        /// <summary>DRH scaler delegate returning current 0-1 factor.</summary>
        private static float ScalerDelegate() => _currentScaleFactor;
#else
        /// <summary>Applies buffer resize using computed scale range.</summary>
        private void SetNewScale()
        {
            if (!_useDrs) return;
            float finalScaleFactor = Mathf.Lerp(_minScaleFactor, _maxScaleFactor, _currentScaleFactor);
#if _2K__URP_UNITY__FOREVER_
            SetURP_DRS(finalScaleFactor);
#else
            ScalableBufferManager.ResizeBuffers(finalScaleFactor, finalScaleFactor);
#endif
        }
#endif

        /// <summary>Resets scale back to 1 (full resolution) and applies resize.</summary>
        private static void ResetScale()
        {
            if (_instance == null || !_instance._useDrs) return;
            _currentScaleFactor = 1.0f;
#if !PIPELINE_IMPLEMENTS_DRH
#if _2K__URP_UNITY__FOREVER_
            _instance.SetURP_DRS(_instance._maxScaleFactor);
#else
            ScalableBufferManager.ResizeBuffers(_instance._maxScaleFactor, _instance._maxScaleFactor);
#endif
#endif
        }

        /// <summary>Placeholder for platform-specific initialization hook.</summary>
        private void SetDRS() { /* Reserved for future platform gating logic. */ }

        /// <summary>Capture recent frame timings and update GPU delta.</summary>
        private void GetFrameStats()
        {
            if (_frameCount < NUM_FRAME_TIMINGS)
            {
                _frameCount++;
                return;
            }

            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings(NUM_FRAME_TIMINGS, _frameTimings);
            if (_frameTimings.Length < NUM_FRAME_TIMINGS) return;
            if (_frameTimings[0].cpuTimeFrameComplete < _frameTimings[0].cpuTimePresentCalled) return;

            if (_gpuFrameTime != 0.0)
            {
                _gpuTimeDelta = _frameTimings[0].gpuFrameTime - _gpuFrameTime;
            }

            _gpuFrameTime = _frameTimings[0].gpuFrameTime;
            _cpuFrameTime = _frameTimings[0].cpuFrameTime;
        }

        // ========================= PUBLIC METHODS ========================

        /// <summary>Enable dynamic resolution if the platform supports it.</summary>
        public static void Enable()
        {
            if (_platformSupported) _systemEnabled = true;
        }

        /// <summary>Disable dynamic resolution and reset scale.</summary>
        public static void Disable()
        {
            if (_platformSupported)
            {
                _systemEnabled = false;
                ResetScale();
            }
        }

        /// <summary>Returns true if dynamic resolution is supported on this platform.</summary>
        public static bool IsSupportedOnPlatform() => _platformSupported;
        /// <summary>Returns true if the system is currently enabled.</summary>
        public static bool IsEnabled() => _systemEnabled;
        /// <summary>Gets the target frame rate.</summary>
        public static double GetTargetFramerate() => _desiredFrameRate;
        /// <summary>Sets a new target frame rate and resets scaling.</summary>
        /// <param name="target">Desired frames per second.</param>
        public static void SetTargetFramerate(double target)
        {
            _desiredFrameRate = target;
            _desiredFrameTime = 1000.0 / target;
            ResetScale();
        }

        // ======================= UNITY DEBUG GUI ========================

#if ENABLE_DYNAMIC_RESOLUTION_DEBUG
        private void OnGUI()
        {
#if PIPELINE_IMPLEMENTS_DRH
            float curScale = DynamicResolutionHandler.instance.GetCurrentScale();
            int rezWidth = (int)Mathf.Ceil(curScale * Screen.currentResolution.width);
            int rezHeight = (int)Mathf.Ceil(curScale * Screen.currentResolution.height);
#else
            int rezWidth = (int)Mathf.Ceil(ScalableBufferManager.widthScaleFactor * Screen.currentResolution.width);
            int rezHeight = (int)Mathf.Ceil(ScalableBufferManager.heightScaleFactor * Screen.currentResolution.height);
#endif
            _debugStyle = GUI.skin.box;
            _debugStyle.fontSize = 20;
            _debugStyle.alignment = TextAnchor.MiddleLeft;
            GUILayout.Label(string.Format(
                "Enabled: {0}\nResolution: {1} x {2}\nScaleFactor: {3:F3}\nGPU: {4:F3} CPU: {5:F3}",
                _systemEnabled,
                rezWidth,
                rezHeight,
#if PIPELINE_IMPLEMENTS_DRH
                curScale,
#else
#if !PIPELINE_IMPLEMENTS_DRH
                Mathf.Lerp(_minScaleFactor, _maxScaleFactor, _currentScaleFactor),
#else
                _currentScaleFactor,
#endif
#endif
                _gpuFrameTime,
                _cpuFrameTime), _debugStyle);
        }
#endif

        private void OnDestroy()
        {
            if (_systemEnabled) { ResetScale(); }
        }
    }
}