using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>Utility for drawing directional arrows in the Scene view using Gizmos for debugging orientation and paths.</summary>
    public static class DrawArrow
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>Draws an arrow gizmo starting at a position along a direction with a colored head.</summary>
        /// <param name="pos">Starting position of the arrow.</param>
        /// <param name="direction">Direction (and length) vector of the arrow shaft.</param>
        /// <param name="color">Arrow color (defaults to red if unspecified).</param>
        /// <param name="arrowHeadLength">Length of each arrow head side.</param>
        /// <param name="arrowHeadAngle">Angle (in degrees) between arrow head sides and shaft direction.</param>
        public static void ForGizmo(Vector3 pos, Vector3 direction, Color color = default, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (color == default) { color = Color.red; }
            Gizmos.color = color;
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }
        
    }
}