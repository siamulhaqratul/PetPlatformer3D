using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Extension methods for rotating a Transform around its local axes with delta tracking.
    /// </summary>
    public static class ExRotation
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Rotates the Transform around its local X axis by the specified angle delta.
        /// </summary>
        /// <param name="transform">The Transform to rotate.</param>
        /// <param name="angle">The new angle to apply (delta is computed from lastDeltaAngle).</param>
        /// <param name="lastDeltaAngle">Reference to the last applied angle (updated after rotation).</param>
        public static void ExSetRotationAroundLocalXAxis(this Transform transform, float angle, ref float lastDeltaAngle)
        {
            angle *= -1f;
            Quaternion localRotation = Quaternion.Euler(angle - lastDeltaAngle, 0f, 0f);
            transform.rotation = transform.rotation * localRotation;
            lastDeltaAngle = angle;
        }

        /// <summary>
        /// Rotates the Transform around its local Y axis by the specified angle delta.
        /// </summary>
        /// <param name="transform">The Transform to rotate.</param>
        /// <param name="angle">The new angle to apply (delta is computed from lastDeltaAngle).</param>
        /// <param name="lastDeltaAngle">Reference to the last applied angle (updated after rotation).</param>
        public static void ExSetRotationAroundLocalYAxis(this Transform transform, float angle, ref float lastDeltaAngle)
        {
            angle *= -1f;
            Quaternion localRotation = Quaternion.Euler(0f, angle - lastDeltaAngle, 0f);
            transform.rotation = transform.rotation * localRotation;
            lastDeltaAngle = angle;
        }

        /// <summary>
        /// Rotates the Transform around its local Z axis by the specified angle delta.
        /// </summary>
        /// <param name="transform">The Transform to rotate.</param>
        /// <param name="angle">The new angle to apply (delta is computed from lastDeltaAngle).</param>
        /// <param name="lastDeltaAngle">Reference to the last applied angle (updated after rotation).</param>
        public static void ExSetRotationAroundLocalZAxis(this Transform transform, float angle, ref float lastDeltaAngle)
        {
            angle *= -1f;
            Quaternion localRotation = Quaternion.Euler(0f, 0f, angle - lastDeltaAngle);
            transform.rotation = transform.rotation * localRotation;
            lastDeltaAngle = angle;
        }
    }
}