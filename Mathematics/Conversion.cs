using UnityEngine;

namespace BGC.Mathematics
{
    public static class Conversion
    {
        public static float SecondsToMS(float seconds)
        {
            return seconds * 1000f;
        }

        public static float MSToSeconds(float ms)
        {
            return ms * 0.001f;
        }

        public static double SecondsToMS(double seconds)
        {
            return seconds * 1000;
        }

        public static double MSToSeconds(double ms)
        {
            return ms * 0.001;
        }

        public static Vector2 WorldToScreen(Vector3 worldPos, RectTransform screenRect)
        {
            Vector2 localPoint;
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(screenRect, screenPoint, Camera.main, out localPoint);

            return localPoint;
        }
    }
}
