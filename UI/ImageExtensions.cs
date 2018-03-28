using UnityEngine.UI;

namespace BGC.UI
{
    /// <summary>
    /// Set of extensions to image that allow for easy modification of the
    /// image's color.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Set alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="a"></param>
        public static void SetA(this Image image, float a)
        {
            image.color = image.color.SetA(a);
        }

        /// <summary>
        /// Set red of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        public static void SetR(this Image image, float r)
        {
            image.color = image.color.SetR(r);
        }

        /// <summary>
        /// Set green of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="g"></param>
        public static void SetG(this Image image, float g)
        {
            image.color = image.color.SetG(g);
        }

        /// <summary>
        /// Set blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="b"></param>
        public static void SetB(this Image image, float b)
        {
            image.color = image.color.SetB(b);
        }

        /// <summary>
        /// Set red and blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        public static void SetRB(this Image image, float r, float b)
        {
            image.color = image.color.SetRB(r, b);
        }

        /// <summary>
        /// Set red and green of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        public static void SetRG(this Image image, float r, float g)
        {
            image.color = image.color.SetRG(r, g);
        }

        /// <summary>
        /// Set red and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="a"></param>
        public static void SetRA(this Image image, float r, float a)
        {
            image.color = image.color.SetRA(r, a);
        }

        /// <summary>
        /// Set green and blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="b"></param>
        /// <param name="g"></param>
        public static void SetGB(this Image image, float g, float b)
        {
            image.color = image.color.SetGB(g, b);
        }

        /// <summary>
        /// Set green and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void SetGA(this Image image, float g, float a)
        {
            image.color = image.color.SetGA(g, a);
        }

        /// <summary>
        /// Set blue and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetBA(this Image image, float b, float a)
        {
            image.color = image.color.SetBA(b, a);
        }

        /// <summary>
        /// set red, green, and blue of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetRGB(this Image image, float r, float g, float b)
        {
            image.color = image.color.SetRGB(r, g, b);
        }

        /// <summary>
        /// Set red, green, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="a"></param>
        public static void SetRGA(this Image image, float r, float g, float a)
        {
            image.color = image.color.SetRGA(r, g, a);
        }

        /// <summary>
        /// Set red, blue, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetRBA(this Image image, float r, float b, float a)
        {
            image.color = image.color.SetRBA(r, b, a);
        }

        /// <summary>
        /// Set green, blue, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetGBA(this Image image, float g, float b, float a)
        {
            image.color = image.color.SetGBA(g, b, a);
        }

        /// <summary>
        /// Set red, green, blue, and alpha of an image's color
        /// </summary>
        /// <param name="image"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetRGBA(this Image image, float r, float g, float b, float a)
        {
            image.color = image.color.SetRGBA(r, g, b, a);
        }
    }
}