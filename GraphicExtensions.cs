using UnityEngine.UI;
using UnityEngine;

namespace BGCTools
{
    /// <summary>
    /// Set of extensions to image that allow for easy modification of the
    /// image's color.
    /// </summary>
    public static class GraphicExtensions
    {
        /// <summary>
        /// Set alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="a"></param>
        public static void SetA(this Graphic image, float a)
        {
            Color c = image.color;
            updateAlpha(ref c, a);
            image.color = c;
        }

        /// <summary>
        /// Set red of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        public static void SetR(this Graphic image, float r)
        {
            Color c = image.color;
            updateRed(ref c, r);
            image.color = c;
        }

        /// <summary>
        /// Set green of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="g"></param>
        public static void SetG(this Graphic image, float g)
        {
            Color c = image.color;
            updateGreen(ref c, g);
            image.color = c;
        }

        /// <summary>
        /// Set blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="b"></param>
        public static void SetB(this Graphic image, float b)
        {

            Color c = image.color;
            updateBlue(ref c, b);
            image.color = c;
        }

        /// <summary>
        /// Set red and blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        public static void SetRB(this Graphic image, float r, float b)
        {
            Color c = image.color;
            updateRed(ref c, r);
            updateBlue(ref c, b);
            image.color = c;
        }

        /// <summary>
        /// Set red and green of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        public static void SetRG(this Graphic image, float r, float g)
        {
            Color c = image.color;
            updateRed(ref c, r);
            updateGreen(ref c, g);
            image.color = c;
        }

        /// <summary>
        /// Set red and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="a"></param>
        public static void SetRA(this Graphic image, float r, float a)
        {
            Color c = image.color;
            updateRed(ref c, r);
            updateAlpha(ref c, a);
            image.color = c;
        }

        /// <summary>
        /// Set green and blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="b"></param>
        /// <param name="g"></param>
        public static void SetGB(this Graphic image, float b, float g)
        {
            Color c = image.color;
            updateGreen(ref c, g);
            updateBlue(ref c, b);
            image.color = c;
        }

        /// <summary>
        /// Set green and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void SetGA(this Graphic image, float g, float a)
        {
            Color c = image.color;
            updateGreen(ref c, g);
            updateAlpha(ref c, a);
            image.color = c;
        }

        /// <summary>
        /// Set blue and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetBA(this Graphic image, float b, float a)
        {
            Color c = image.color;
            updateBlue(ref c, b);
            updateAlpha(ref c, a);
            image.color = c;
        }

        /// <summary>
        /// set red, green, and blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetRGB(this Graphic image, float r, float g, float b)
        {
            Color c = image.color;
            updateRed(ref c, r);
            updateGreen(ref c, g);
            updateBlue(ref c, b);
            image.color = c;
        }

        /// <summary>
        /// Set red, green, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void SetRGA(this Graphic image, float r, float g, float a)
        {
            Color c = image.color;
            updateRed(ref c, r);
            updateGreen(ref c, g);
            updateAlpha(ref c, a);
            image.color = c;
        }

        /// <summary>
        /// Set red, blue, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetRBA(this Graphic image, float r, float b, float a)
        {
            Color c = image.color;
            updateRed(ref c, r);
            updateBlue(ref c, b);
            updateAlpha(ref c, a);
            image.color = c;
        }

        /// <summary>
        /// Set green, blue, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetGBA(this Graphic image, float g, float b, float a)
        {
            Color c = image.color;
            updateGreen(ref c, g);
            updateBlue(ref c, b);
            updateAlpha(ref c, a);
            image.color = c;
        }

        /// <summary>
        /// Set red, green, blue, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetRGBA(this Graphic image, float r, float g, float b, float a)
        {
            Color c = image.color;
            updateRed(ref c, r);
            updateBlue(ref c, b);
            updateGreen(ref c, g);
            updateAlpha(ref c, a);
            image.color = c;
        }

        private static void updateRed(ref Color color, float r)
        {
            updateColorValue(ref r, "Red");
            color.r = r;
        }

        private static void updateGreen(ref Color color, float g)
        {
            updateColorValue(ref g, "Green");
            color.g = g;
        }

        private static void updateBlue(ref Color color, float b)
        {
            updateColorValue(ref b, "Blue");
            color.b = b;
        }

        private static void updateAlpha(ref Color color, float a)
        {
            updateColorValue(ref a, "Alpha");
            color.a = a;
        }

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