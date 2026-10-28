using UnityEngine;
using UnityExt.Tons;
namespace UnityExt.Optimization
{
    [RequireComponent(typeof(Camera))]
    public class DeviceSupportForCamera : SingletonBehaviour<DeviceSupportForCamera>
    {
        public static DeviceSupportForCamera Instance { get; private set; }
        [Tooltip("The resolution the game was designed for. E.g., 1080x1920 for portrait.")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);
        [Tooltip("The initial orthographic size that matches the reference resolution height.")]
        [SerializeField] private float referenceOrthographicSize = 8f;
        private Camera mainCam;
        public Camera MainCam => mainCam;
        void Awake()
        {
            // 1. Robust Singleton Pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            mainCam = GetComponent<Camera>();
            //AdjustCamera();
        }
        /// <summary>
        /// Adjusts the camera's orthographic size to fit the screen without black bars.
        /// </summary>
        private void AdjustCamera()
        {
            if (mainCam == null || !mainCam.orthographic)
            {
                if (!mainCam.orthographic)
                {
                    Debug.LogWarning("DeviceSupportForCamera: This script is designed for an Orthographic camera and will not work correctly with a Perspective camera.");
                }
                return;
            }
            // 2. Calculate aspect ratios consistently (width / height)
            float referenceAspect = referenceResolution.x / referenceResolution.y;
            float screenAspect = (float)Screen.width / Screen.height;
            // 3. Adjust orthographic size
            if (screenAspect >= referenceAspect)
            {
                // If screen is wider than reference (or same), we match the reference height.
                // This is the case for landscape on a portrait-designed game.
                mainCam.orthographicSize = referenceOrthographicSize;
            }
            else
            {
                // If screen is taller than reference, we need to zoom out to fit the width.
                float scaleFactor = referenceAspect / screenAspect;
                mainCam.orthographicSize = referenceOrthographicSize * scaleFactor;
            }
        }
        public void ResetCamera()
        {
            mainCam.orthographicSize = referenceOrthographicSize;
            mainCam.transform.position = new Vector3(0f, 0f, mainCam.transform.position.z);
            AdjustCamera();
        }
    }
}