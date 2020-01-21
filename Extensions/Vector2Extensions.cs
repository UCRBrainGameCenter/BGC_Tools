using System;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Extensions
{
    /// <summary>
    /// Set of extensions for unity data structure Vector2
    /// </summary>
    public static class Vector2Extension
    {
        /// <summary>
        /// Rotate a 2d vector by specified number of degrees
        /// </summary>
        /// <param name="v"></param>
        /// <param name="degrees">angle to rotate by in degrees</param>
        /// <returns></returns>
        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            double radians = degrees * GeneralMath.dDeg2Rad;
            float sin = (float)Math.Sin(radians);
            float cos = (float)Math.Cos(radians);

            return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        /// <summary>
        /// get the minimum of the x or y values in the vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static float Min(this Vector2 v) => Math.Min(v.x, v.y);
    }
}