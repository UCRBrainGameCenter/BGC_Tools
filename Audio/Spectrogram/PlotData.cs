using UnityEngine;
using UnityEngine.UI;

namespace BGC.Audio.Visualization
{
    public abstract class PlotData
    {
        public Texture2D plot;
        public Vector2 xBounds;
        public Vector2 yBounds;

        public abstract void PopulateWidget(GameObject parent);

        protected GameObject CreateTextureWidget(
            string name,
            GameObject parent,
            Texture2D tex,
            Vector2 anchorMin,
            Vector2 anchorMax,
            bool fixAspectRatio = false,
            Vector2 offsetMin = new Vector2(),
            Vector2 offsetMax = new Vector2())
        {
            GameObject widget = new GameObject(name);
            RectTransform transform = widget.AddComponent<RectTransform>();
            Image plotImage = widget.AddComponent<Image>();

            transform.SetParent(parent.transform, false);

            transform.anchorMin = anchorMin;
            transform.anchorMax = anchorMax;
            transform.offsetMin = offsetMin;
            transform.offsetMax = offsetMax;

            plotImage.overrideSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 100f);

            if (fixAspectRatio)
            {
                AspectRatioFitter fitter = widget.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fitter.aspectRatio = tex.width / (float)tex.height;
            }

            return widget;
        }
    }

}
