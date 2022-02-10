using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SH.RoadCreator.Algorithm
{
    /// <summary>
    /// This class is used to create bezier curves.
    /// </summary>
    public static class Bezier
    {
        /// <summary>
        /// Generate quadratic bezier curve.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">Tangent.</param>
        /// <param name="c">End point.</param>
        /// <param name="t">Selected time on curve.</param>
        /// <returns>Returns the position on the curve with the selected time (time is not evenly distributed).</returns>
        public static Vector3 Quadratic(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 valueA = Vector3.Lerp(a, b, t);
            Vector3 valueB = Vector3.Lerp(b, c, t);
            return Vector3.Lerp(valueA, valueB, t);
        }

        /// <summary>
        /// Generate cubic bezier curve.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">Tangent start.</param>
        /// <param name="c">Tangent end.</param>
        /// <param name="d">End point.</param>
        /// <param name="t">Selected time on curve.</param>
        /// <returns>Returns the position on the curve with the selected time (time is not evenly distributed).</returns>
        public static Vector3 Cubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 valueA = Quadratic(a, b, c, t);
            Vector3 valueB = Quadratic(b, c, d, t);
            return Vector3.Lerp(valueA, valueB, t);
        }

        /// <summary>
        /// Calculate the approximate length (for performance reasons) of the cubic bezier curve.
        /// </summary>
        /// <param name="a">Start point.</param>
        /// <param name="b">Tangent start.</param>
        /// <param name="c">Tangent end.</param>
        /// <param name="d">End point.</param>
        /// <returns>Returns the approximate length of the curve.</returns>
        public static float ApproximateCubicLength(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            float chord = (d - a).sqrMagnitude;
            float tangents = (a - c).sqrMagnitude + (b - c).sqrMagnitude + (d - b).sqrMagnitude;
            float length = (tangents + chord) / 2f;
            return Mathf.Sqrt(length);
        }
    }
}