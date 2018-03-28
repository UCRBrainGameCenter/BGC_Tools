using UnityEngine;

namespace BGC.UI
{
    public static class ColorExtensions
    {
        private const string red   = "Red";
        private const string green = "Green";
        private const string blue  = "Blue";
        private const string alpha = "Alpha";

        /// <summary>
        /// Set Red of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        public static void SetR(this Color color, float r)
        {
            updateColorValue(ref r, red);
            color.r = r;
        }

        /// <summary>
        /// Set Green of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        public static void SetG(this Color color, float g)
        {
            updateColorValue(ref g, red);
            color.g = g;
        }


        /// <summary>
        /// Set blue of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="b"></param>
        public static void SetB(this Color color, float b)
        {
            updateColorValue(ref b, red);
            color.a = b;
        }

        /// <summary>
        /// Set alpha of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="a"></param>
        public static void SetA(this Color color, float a)
        {
            updateColorValue(ref a, red);
            color.a = a;
        }

        /// <summary>
        /// Set red and green of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        public static void SetRG(this Color color, float r, float g)
        {
            color.SetR(r);
            color.SetG(g);
        }

        /// <summary>
        /// set red and blue of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        public static void SetRB(this Color color, float r, float b)
        {
            color.SetR(r);
            color.SetB(b);
        }

        /// <summary>
        /// set red and alpha of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="a"></param>
        public static void SetRA(this Color color, float r, float a)
        {
            color.SetR(r);
            color.SetA(a);
        }

        /// <summary>
        /// set green and blue of the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetGB(this Color color, float g, float b)
        {
            color.SetG(g);
            color.SetB(b);
        }

        /// <summary>
        /// set green and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void SetGA(this Color color, float g, float a)
        {
            color.SetG(g);
            color.SetA(a);
        }

        /// <summary>
        /// set blue and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetBA(this Color color, float b, float a)
        {
            color.SetB(b);
            color.SetA(a);
        }

        /// <summary>
        /// set red, blue, and green
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetRGB(this Color color, float r, float g, float b)
        {
            color.SetR(r);
            color.SetG(g);
            color.SetB(b);
        }

        /// <summary>
        /// set red, green, and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void SetRGA(this Color color, float r, float g, float a)
        {
            color.SetR(r);
            color.SetG(g);
            color.SetA(a);
        }

        /// <summary>
        /// set red, blue, and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetRBA(this Color color, float r, float b, float a)
        {
            color.SetR(r);
            color.SetB(b);
            color.SetA(a);
        }

        /// <summary>
        /// set green, blue, and alhpa
        /// </summary>
        /// <param name="color"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetGBA(this Color color, float g, float b, float a)
        {
            color.SetG(g);
            color.SetB(b);
            color.SetA(a);
        }

        /// <summary>
        /// set red, green, blue, and alpha
        /// </summary>
        /// <param name="color"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetRGBA(this Color color, float r, float g, float b, float a)
        {
            color.SetR(r);
            color.SetG(g);
            color.SetB(b);
            color.SetA(a);
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
                Debug.LogWarning(colorType + " should be set between 0 and 1");
                Mathf.Clamp(color, 0f, 1f);
            }
        }
    }
}