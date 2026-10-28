using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityExt
{
    /// <summary>
    /// Interface for objects that require a custom initialization step, similar to Unity's Awake.
    /// </summary>
    public interface ICustomAwakeHandler
    {
        /// <summary>
        /// Called to perform custom initialization.
        /// </summary>
        public void OnCustomAwake();
    }

    /// <summary>
    /// An empty class used as a lightweight flag or marker object to track state without holding data.
    /// </summary>
    public class MarkerObject { }

    /// <summary>
    /// A generic wrapper class that exposes a read-only value in the Unity Inspector.
    /// Useful for creating constant fields that can be set during edit-time but not changed at runtime.
    /// </summary>
    /// <typeparam name="T">The type of the value to hold.</typeparam>
    [System.Serializable]
    public class ConstField<T>
    {
        [SerializeField] T value;
        /// <summary>
        /// The read-only value of the field.
        /// </summary>
        public T Value { get { return value; } }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstField{T}"/> class with a default value.
        /// </summary>
        /// <param name="defaultValue">The value to initialize the field with.</param>
        public ConstField(T defaultValue)
        {
            value = defaultValue;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstField{T}"/> class with the type's default value.
        /// </summary>
        public ConstField()
        {
            value = default;
        }
    }


    /// <summary>
    /// Defines the type of interaction, typically for UI or game objects.
    /// </summary>
    public enum InteractionType
    {
        Open = 0,
        Close = 1
    }
    /// <summary>
    /// Specifies an axis for transform operations.
    /// </summary>
    public enum SetTransformAxis
    {
        X = 0,
        Y = 1
    }
    /// <summary>
    /// Specifies the dimensionality of a context, e.g., for physics or coordinate systems.
    /// </summary>
    public enum Dimension
    {
        TwoD = 0,
        ThreeD = 1
    }
    /// <summary>
    /// Represents the standard X, Y, Z axes in Unity.
    /// </summary>
    public enum UnityAxis
    {
        X = 0,
        Y = 1,
        Z = 2
    }
    /// <summary>
    /// A minimal class for creating and storing points along a cubic Bezier curve.
    /// </summary>
    public class BezierPathMinimal
    {
        /// <summary>
        /// The list of calculated points that form the Bezier path.
        /// </summary>
        public List<Vector3> pathPoints;
        private int segments;
        int pointCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BezierPathMinimal"/> class.
        /// </summary>
        /// <param name="ptCount">The total number of points to generate for the path.</param>
        public BezierPathMinimal(int ptCount)
        {
            pathPoints = new List<Vector3>();
            pointCount = ptCount;
        }

        /// <summary>
        /// Clears all points from the path.
        /// </summary>
        public void DeletePath()
        {
            pathPoints.Clear();
        }

        Vector3 BezierPathCalculation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            Vector3 B = new Vector3();
            B = uuu * p0;
            B += 3.0f * uu * t * p1;
            B += 3.0f * u * tt * p2;
            B += ttt * p3;

            return B;
        }

        /// <summary>
        /// Generates the Bezier curve points from a list of control points.
        /// </summary>
        /// <param name="controlPoints">A list of Vector3 control points. Must be in sets of 4 (start, handle1, handle2, end).</param>
        /// <param name="lastPt">The final point to ensure is added to the path.</param>
        public void CreateCurve(List<Vector3> controlPoints, Vector3 lastPt)
        {
            segments = controlPoints.Count / 3;

            for (int s = 0; s < controlPoints.Count - 3; s += 3)
            {
                Vector3 p0 = controlPoints[s];
                Vector3 p1 = controlPoints[s + 1];
                Vector3 p2 = controlPoints[s + 2];
                Vector3 p3 = controlPoints[s + 3];

                if (s == 0)
                {
                    pathPoints.Add(BezierPathCalculation(p0, p1, p2, p3, 0.0f));
                }

                for (int p = 0; p < (pointCount / segments); p++)
                {
                    float t = (1.0f / (pointCount / segments)) * p;
                    Vector3 point = new Vector3();
                    point = BezierPathCalculation(p0, p1, p2, p3, t);
                    pathPoints.Add(point);
                }
            }
            pathPoints.Add(lastPt);
        }
    }

    /// <summary>
    /// Defines a position and rotation offset relative to a parent transform.
    /// </summary>
    [System.Serializable]
    public class RelativeTransform
    {
        [SerializeField] float backward, upward, side;
        [SerializeField] RangedVector3Float rotationOffset;
        /// <summary>
        /// The distance to move backward from the transform's forward direction.
        /// </summary>
        public float Backward { get { return backward; } }
        /// <summary>
        /// The distance to move upward from the transform's up direction.
        /// </summary>
        public float Upward { get { return upward; } }
        /// <summary>
        /// The distance to move sideways from the transform's right direction.
        /// </summary>
        public float Side { get { return side; } }
        /// <summary>
        /// A normalized vector used to offset the look-at rotation.
        /// </summary>
        public RangedVector3Float RotationOffset { get { return rotationOffset; } }

        /// <summary>
        /// Calculates the relative position based on the offsets from a given transform.
        /// </summary>
        /// <param name="from">The transform to calculate the relative position from.</param>
        /// <returns>The calculated world position.</returns>
        public Vector3 GetRelativePosition(Transform from)
        {
            return from.position + from.forward * -backward + from.up * upward + from.right * side;
        }

        /// <summary>
        /// Calculates the rotation required for one transform to look at another, applying the rotation offset.
        /// </summary>
        /// <param name="thisTransform">The transform that will be rotated.</param>
        /// <param name="target">The transform to look at.</param>
        /// <param name="result">The resulting look-at rotation.</param>
        /// <returns>True if the rotation was successfully calculated; false if the target direction was a zero vector.</returns>
        public bool GetLookAtRotation(Transform thisTransform, Transform target, ref Quaternion result)
        {
            var toTarget = target.position - thisTransform.position;
            toTarget = toTarget.normalized + rotationOffset.Get();
            if (Mathf.Approximately(toTarget.magnitude, 0.0f) == false)
            {
                result = Quaternion.LookRotation(toTarget, Vector3.up);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// A serializable class representing a Vector3 where each component is clamped between -1 and 1 in the Inspector.
    /// </summary>
    [System.Serializable]
    public class RangedVector3Float
    {
        [SerializeField, Range(-1.0f, 1.0f)] float x, y, z;
        /// <summary>
        /// Gets the Vector3 value.
        /// </summary>
        /// <returns>The Vector3 composed of the x, y, and z components.</returns>
        public Vector3 Get()
        {
            return new Vector3(x, y, z);
        }
    }

    // Delegate definitions for events and callbacks.

    /// <summary>
    /// Delegate for an action that takes an integer level.
    /// </summary>
    public delegate void OnDoAnythingFunc2(int level);
    /// <summary>
    /// Delegate for an action that provides progress information.
    /// </summary>
    public delegate void OnDoAnythingFunc3(int level, float raise, float totalNow);
    /// <summary>
    /// Delegate for an action that provides detailed progress information.
    /// </summary>
    public delegate void OnDoAnythingFunc4(int level, float raise, float totalNow, float totalPrev);
    
    /// <summary>
    /// Delegate for a function that returns a boolean condition.
    /// </summary>
    public delegate bool WhenToDoFunc();
    /// <summary>
    /// Delegate for a function that takes one parameter and returns a boolean condition.
    /// </summary>
    public delegate bool WhenToDoFunc<T>(T data1);
    /// <summary>
    /// Delegate for a function that takes two parameters and returns a boolean condition.
    /// </summary>
    public delegate bool WhenToDoFunc<T1, T2>(T1 data1, T2 data2);
    /// <summary>
    /// Delegate for a function that takes three parameters and returns a boolean condition.
    /// </summary>
    public delegate bool WhenToDoFunc<T1, T2, T3>(T1 data1, T2 data2, T3 data3);

    /// <summary>
    /// Delegate for a parameterless action.
    /// </summary>
    public delegate void OnDoAnything();
    /// <summary>
    /// Delegate for an action that takes one parameter.
    /// </summary>
    public delegate void OnDoAnything<T>(T data);
    /// <summary>
    /// Delegate for an action that takes two parameters.
    /// </summary>
    public delegate void OnDoAnything<T1, T2>(T1 data1, T2 data2);
    /// <summary>
    /// Delegate for an action that takes three parameters.
    /// </summary>
    public delegate void OnDoAnything<T1, T2, T3>(T1 data1, T2 data2, T3 data3);
    /// <summary>
    /// Delegate for an action that takes four parameters.
    /// </summary>
    public delegate void OnDoAnything<T1, T2, T3, T4>(T1 data1, T2 data2, T3 data3, T4 data4);
    /// <summary>
    /// Delegate for an action that takes five parameters.
    /// </summary>
    public delegate void OnDoAnything<T1, T2, T3, T4, T5>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5);
    /// <summary>
    /// Delegate for an action that takes six parameters.
    /// </summary>
    public delegate void OnDoAnything<T1, T2, T3, T4, T5, T6>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6);
    /// <summary>
    /// Delegate for an action that takes seven parameters.
    /// </summary>
    public delegate void OnDoAnything<T1, T2, T3, T4, T5, T6, T7>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7);
    /// <summary>
    /// Delegate for an action that takes eight parameters.
    /// </summary>
    public delegate void OnDoAnything<T1, T2, T3, T4, T5, T6, T7, T8>(T1 data1, T2 data2, T3 data3, T4 data4, T5 data5, T6 data6, T7 data7, T8 data8);
}