using System;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.UI
{
    public static class ColorUIExtensions
    {
        private const string red = "Red";
        private const string green = "Green";
        private const string blue = "Blue";
        private const string alpha = "Alpha";

        /// <summary>
        /// Set Red of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        public static Color SetR(this Color color, float r)
        {
            updateColorValue(ref r, red);
            Color c = color;
            c.r = r;
            return c;
        }

        /// <summary>
        /// Set Green of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        public static Color SetG(this Color color, float g)
        {
            updateColorValue(ref g, red);
            Color c = color;
            c.g = g;
            return c;
        }


        /// <summary>
        /// Set blue of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="b"></param>
        public static Color SetB(this Color color, float b)
        {
            updateColorValue(ref b, red);
            Color c = color;
            color.a = b;
            return c;
        }

        /// <summary>
        /// Set alpha of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="a"></param>
        public static Color SetA(this Color color, float a)
        {
            updateColorValue(ref a, red);
            Color c = color;
            c.a = a;
            return c;
        }

        /// <summary>
        /// Set red and green of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        public static Color SetRG(this Color color, float r, float g)
        {
            return color.SetR(r).SetG(g);
        }

        /// <summary>
        /// set red and blue of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        public static Color SetRB(this Color color, float r, float b)
        {
            return color.SetR(r).SetB(b);
        }

        /// <summary>
        /// set red and alpha of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="a"></param>
        public static Color SetRA(this Color color, float r, float a)
        {
            return color.SetR(r).SetA(a);
        }

        /// <summary>
        /// set green and blue of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static Color SetGB(this Color color, float g, float b)
        {
            return color.SetG(g).SetB(b);
        }

        /// <summary>
        /// set green and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static Color SetGA(this Color color, float g, float a)
        {
            return color.SetG(g).SetA(a);
        }

        /// <summary>
        /// set blue and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static Color SetBA(this Color color, float b, float a)
        {
            return color.SetB(b).SetA(a);
        }

        /// <summary>
        /// set red, blue, and green
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static Color SetRGB(this Color color, float r, float g, float b)
        {
            return color.SetR(r).SetG(g).SetB(b);
        }

        /// <summary>
        /// set red, green, and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static Color SetRGA(this Color color, float r, float g, float a)
        {
            return color.SetR(r).SetG(g).SetA(a);
        }

        /// <summary>
        /// set red, blue, and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static Color SetRBA(this Color color, float r, float b, float a)
        {
            return color.SetR(r).SetB(b).SetA(a);
        }

        /// <summary>
        /// set green, blue, and alhpa
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static Color SetGBA(this Color color, float g, float b, float a)
        {
            return color.SetG(g).SetB(b).SetA(a);
        }

        /// <summary>
        /// set red, green, blue, and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static Color SetRGBA(this Color color, float r, float g, float b, float a)
        {
            return color.SetR(r).SetG(g).SetB(b).SetA(a);
        }

        /// <summary>
        /// Update color value to be between 0 and 1
        /// </summary>
        /// <param name="color"></param>
        /// <param name="colorType"></param>
        private static void updateColorValue(ref float color, string colorType)
        {
            if (color > 1f || color < 0f)
            {
                Debug.LogWarning($"{colorType} should be set between 0 and 1");
                color = GeneralMath.Clamp(color, 0f, 1f);
            }
        }
    }
}
