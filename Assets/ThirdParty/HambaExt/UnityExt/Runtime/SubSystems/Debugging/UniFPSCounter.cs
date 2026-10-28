using UnityEngine;

namespace UnityExt
{
    /// <summary>Lightweight FPS counter that calculates and displays frames per second; optionally persists across scenes.</summary>
    /// <remarks>This component can be attached to any GameObject in the scene to display a real-time FPS counter on the screen. It uses OnGUI for rendering and updates the frame rate every half second. If configured to persist, it will not be destroyed when loading new scenes.</remarks>
    public class UniFPSCounter : MonoBehaviour
    {
        // ======================= STATIC FIELDS ==========================

        // Singleton-style instance reference (optional persistence across scenes).
        private static UniFPSCounter _instance = null;

        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [Header("Settings")]
        [Tooltip("If true, this FPS counter persists across scene loads (DontDestroyOnLoad).")]
        [SerializeField] private bool _persists = false;

        // =================== PRIVATE/PROTECTED FIELDS ====================

        // Cached screen longer side dimension for adaptive UI sizing.
        private int _screenLongSide;
        // Rectangle area for the FPS GUI box.
        private Rect _boxRect;
        // GUI style used for the FPS label.
        private GUIStyle _style = new GUIStyle();
        // Frames counted within the current sampling window.
        private int _frameCount;
        // Accumulated time within the current sampling window.
        private float _elapsedTime;
        // Last computed frame rate (frames per second).
        private double _frameRate;

        // =========================== PROPERTIES ==========================

        /// <summary>Latest calculated frames per second.</summary>
        public double FrameRate => _frameRate;

        // ======================= UNITY CALLBACKS =========================

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                UpdateUISize();
                if (_persists)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                if (_persists)
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        private void Update()
        {
            _frameCount++;
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime > 0.5f)
            {
                _frameRate = System.Math.Round(_frameCount / _elapsedTime, 1, System.MidpointRounding.AwayFromZero);
                _frameCount = 0;
                _elapsedTime = 0;

                if (_screenLongSide != Mathf.Max(Screen.width, Screen.height))
                {
                    UpdateUISize();
                }
            }
        }

        // ========================= PRIVATE METHODS =======================

        private void UpdateUISize()
        {
            _screenLongSide = Mathf.Max(Screen.width, Screen.height);
            var rectLongSide = _screenLongSide / 10;
            _boxRect = new Rect(10f, ((float)Screen.height / 1280f) * 80f, rectLongSide, rectLongSide / 3);
            _style.fontSize = (int)(_screenLongSide / 36.8);
            _style.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            GUI.Box(_boxRect, "");
            GUI.Label(_boxRect, " " + _frameRate + "fps", _style);
        }
    }
}