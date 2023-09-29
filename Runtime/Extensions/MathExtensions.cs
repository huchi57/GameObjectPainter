using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanFox.GameObjectPainter
{
    public static class MathExtensions
    {
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count <= 0;
        }

        public static bool IsInRange(this int value, int rangeStartInclusive, int rangeEndInclusive)
        {
            var minInclusive = Mathf.Min(rangeStartInclusive, rangeEndInclusive);
            var maxInclusive = Mathf.Max(rangeStartInclusive, rangeEndInclusive);
            return minInclusive <= value && value <= maxInclusive;
        }

        public static bool IsInRange<T>(this int index, ICollection<T> collection)
        {
            return !collection.IsNullOrEmpty() && index.IsInRange(0, collection.Count - 1);
        }

        public static bool IsApproximately(this float value, float compareValue)
        {
            // Consider 8 * Mathf.Epsilon a very small number
            return Mathf.Abs(value - compareValue) < 8 * Mathf.Epsilon;
        }

        public static bool IsApproximatelyZero(this float value)
        {
            return value.IsApproximately(0);
        }

        public static float Angle360(this float angle)
        {
            return angle > 360 ? Angle360(angle - 360) : angle < 0 ? Angle360(angle + 360) : angle;
        }

        public static Vector3 ChangeLength(this Vector3 vector, float newLength)
        {
            return newLength * vector.normalized;
        }

        public static Vector3 RotateVectorAlongAxis(this Vector3 vector, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * vector;
        }

        public static Vector3 GetPerpendicularVector(this Vector3 normal)
        {
            var normalMagnitude = normal.magnitude;
            if (normalMagnitude.IsApproximatelyZero())
            {
                return Vector3.zero;
            }

            var projectVector = Vector3.ProjectOnPlane(Vector3.forward, normal);

            // If a project vector is 0, then the surface is a horizontal plane.
            return projectVector.magnitude.IsApproximatelyZero() ? normalMagnitude * Vector3.up : projectVector.ChangeLength(normalMagnitude);
        }

        public static Vector3 GetRandomPerpendicularVector(this Vector3 normal)
        {
            if (normal.magnitude.IsApproximatelyZero())
            {
                return Vector3.zero;
            }

            var randomUnitVector = Random.onUnitSphere;
            var projectVector = Vector3.ProjectOnPlane(randomUnitVector, normal);

            // (Brute-force, unlikely) Get another random perpendicular vector again if the cross product is parallel to the normal.
            return projectVector.magnitude.IsApproximatelyZero() ? normal.GetRandomPerpendicularVector() : projectVector.ChangeLength(normal.magnitude);
        }
    }
}
