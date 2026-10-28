using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Makes the attached GameObject face towards the main camera every frame.
    /// Optionally, rotates the object 180 degrees around the Y axis after facing the camera,
    /// depending on the <see cref="IsNotRotate"/> flag.
    /// </summary>
    /// <remarks>
    /// Attach this component to any GameObject that should always face the camera (billboarding).
    /// </remarks>
    public class CameraFacer : MonoBehaviour
    {
        // =================== PRIVATE FIELDS ====================

        /// <summary>
        /// The target camera's transform (usually the main camera).
        /// </summary>
        private Transform _target;

        /// <summary>
        /// Cached reference to this object's transform.
        /// </summary>
        private Transform _transform;

        // ================= SERIALIZED FIELDS (INSPECTOR) =================

        [Tooltip("If true, disables the 180-degree Y rotation after facing the camera.")]
        [SerializeField] private bool _isNotRotate;

        // ======================= UNITY CALLBACKS =========================

        /// <summary>
        /// Unity Start callback. Caches the transform and finds the main camera.
        /// </summary>
        private void Start()
        {
            _transform = transform;
            if (Camera.main != null)
                _target = Camera.main.transform;
        }

        /// <summary>
        /// Unity Update callback. Makes the object face the camera every frame.
        /// </summary>
        private void Update()
        {
            FaceToCamera();
        }

        // ========================= PRIVATE METHODS =======================

        /// <summary>
        /// Rotates the object to face the camera.
        /// If <see cref="IsNotRotate"/> is false, applies an additional 180-degree Y rotation.
        /// </summary>
        private void FaceToCamera()
        {
            if (_target == null) return;
            _transform.LookAt(_target, Vector3.up);
            if (_isNotRotate) return;
            _transform.Rotate(Vector3.up, 180);
        }

        // ======================== PROPERTIES =======================

        /// <summary>
        /// If true, disables the 180-degree Y rotation after facing the camera.
        /// </summary>
        public bool IsNotRotate => _isNotRotate;
    }
}