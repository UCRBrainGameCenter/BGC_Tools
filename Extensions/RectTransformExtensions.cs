using UnityEngine;

namespace BGC.Extensions
{
    public static class RectTransformExtensions
    {
        private static int CountCornersVisibleWithin(this RectTransform rectTransform, RectTransform child)
        {
            Vector3[] parentCorners = new Vector3[4];
            rectTransform.GetWorldCorners(parentCorners);

            Rect worldRect = new Rect(parentCorners[0], parentCorners[2] - parentCorners[0]);

            Vector3[] childCorners = new Vector3[4];
            child.GetWorldCorners(childCorners);

            int visibleCorners = 0;
            for (var i = 0; i < childCorners.Length; i++)
            {
                if (worldRect.Contains(childCorners[i]))
                {
                    visibleCorners++;
                }
            }
            return visibleCorners;
        }


        public static bool IsChildFullyVisible(this RectTransform rectTransform, RectTransform child)
        {
            return CountCornersVisibleWithin(rectTransform, child) == 4;
        }


        public static bool IsChildVisible(this RectTransform rectTransform, RectTransform child)
        {
            return CountCornersVisibleWithin(rectTransform, child) > 0;
        }
    }
}