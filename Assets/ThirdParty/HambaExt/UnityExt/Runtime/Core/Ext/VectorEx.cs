using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>Extension methods for geometric vector operations (line intersection, midpoints, directional tests, and interpolation).</summary>
    public static class ExVector
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Computes the intersection point of two infinite lines defined by two point pairs.
        /// </summary>
        /// <param name="thisLinePt1">First point of the first line.</param>
        /// <param name="thisLinePt2">Second point of the first line.</param>
        /// <param name="otherLinePt1">First point of the second line.</param>
        /// <param name="otherLinePt2">Second point of the second line.</param>
        /// <param name="found">Outputs true if a unique intersection was found (lines not parallel); otherwise false.</param>
        /// <returns>Intersection coordinates if found; otherwise Vector2.zero.</returns>
        public static Vector2 ExGetIntersectionPointCoordinates(this Vector2 thisLinePt1, Vector2 thisLinePt2, Vector2 otherLinePt1, Vector2 otherLinePt2, out bool found)
        {
            float tmp = (otherLinePt2.x - otherLinePt1.x) * (thisLinePt2.y - thisLinePt1.y) - (otherLinePt2.y - otherLinePt1.y) * (thisLinePt2.x - thisLinePt1.x);
            if (tmp == 0)
            {
                found = false;
                return Vector2.zero;
            }

            float mu = ((thisLinePt1.x - otherLinePt1.x) * (thisLinePt2.y - thisLinePt1.y) - (thisLinePt1.y - otherLinePt1.y) * (thisLinePt2.x - thisLinePt1.x)) / tmp;
            found = true;
            return new Vector2(
                otherLinePt1.x + (otherLinePt2.x - otherLinePt1.x) * mu,
                otherLinePt1.y + (otherLinePt2.y - otherLinePt1.y) * mu
            );
        }
        /// <summary>Returns the midpoint between a start point and target.</summary>
        /// <param name="pt">Start point.</param>
        /// <param name="target">Target point.</param>
        /// <returns>Midpoint between the two points.</returns>
        public static Vector3 ExGetMidPoint(this Vector3 pt, Vector3 target)
        {
            var toTarget = target - pt;
            var midDist = toTarget.magnitude * 0.5f;
            return pt + toTarget.normalized * midDist;
        }
        /// <summary>Determines whether movement from <paramref name="thisPt"/> in <paramref name="moveDirection"/> has passed the specified point.</summary>
        /// <param name="thisPt">Current position.</param>
        /// <param name="point">Target point to test against.</param>
        /// <param name="moveDirection">Direction of movement.</param>
        /// <returns>True if the point has been passed (dot product &lt; 0), otherwise false.</returns>
        public static bool ExHasPassedPoint(this Vector3 thisPt, Vector3 point, Vector3 moveDirection)
        {
            var toPointDirection = point - thisPt;
            return Vector3.Dot(toPointDirection.normalized, moveDirection.normalized) < 0.0f;
        }
        /// <summary>Gets a point along the line from <paramref name="pt"/> to <paramref name="target"/> at a normalized distance.</summary>
        /// <param name="pt">Start point.</param>
        /// <param name="normalizedDistance">Normalized distance (0 = start, 1 = target; values beyond 1 extrapolate).</param>
        /// <param name="target">Target point.</param>
        /// <returns>Point at normalized distance along the line.</returns>
        public static Vector3 ExGetPointAtNormalizedDistance(this Vector3 pt, float normalizedDistance, Vector3 target)
        {
            var toTarget = target - pt;
            var dist = toTarget.magnitude * normalizedDistance;
            return pt + toTarget.normalized * dist;
        }
    }
}