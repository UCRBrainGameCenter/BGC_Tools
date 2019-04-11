using UnityEngine;
using UnityEngine.UI;

namespace BGC.Audio.Visualization
{
    public class SpectrogramData : PlotData
    {
        public Texture2D legendTex;
        public Vector2 legendBounds;

        protected enum LabelPosition
        {
            LegendTop = 0,
            LegendMiddle,
            LegendBottom,
            XLower,
            XUpper,
            YLower,
            YUpper
        }

        protected GameObject CreateLabelWidget(string text, GameObject parent, LabelPosition position)
        {
            GameObject widget = new GameObject("TextElement");
            RectTransform transform = widget.AddComponent<RectTransform>();
            Text textItem = widget.AddComponent<Text>();

            textItem.font = Font.CreateDynamicFontFromOSFont("Arial", 40);
            textItem.fontSize = 40;
            textItem.text = text;

            transform.SetParent(parent.transform, false);

            switch (position)
            {
                case LabelPosition.LegendTop:
                    textItem.alignment = TextAnchor.UpperLeft;
                    transform.pivot = new Vector2(0f, 1f);
                    transform.anchorMin = new Vector2(1f, 1f);
                    transform.anchorMax = new Vector2(1f, 1f);
                    transform.offsetMin = new Vector2(20f, -50f);
                    transform.offsetMax = new Vector2(120f, 0f);
                    break;
                case LabelPosition.LegendMiddle:
                    textItem.alignment = TextAnchor.MiddleLeft;
                    transform.pivot = new Vector2(0f, 0.5f);
                    transform.anchorMin = new Vector2(1f, 0.5f);
                    transform.anchorMax = new Vector2(1f, 0.5f);
                    transform.offsetMin = new Vector2(20f, -25f);
                    transform.offsetMax = new Vector2(120f, 25f);
                    break;
                case LabelPosition.LegendBottom:
                    textItem.alignment = TextAnchor.LowerLeft;
                    transform.pivot = new Vector2(0f, 0f);
                    transform.anchorMin = new Vector2(1f, 0f);
                    transform.anchorMax = new Vector2(1f, 0f);
                    transform.offsetMin = new Vector2(20f, 0f);
                    transform.offsetMax = new Vector2(120f, 50f);
                    break;
                case LabelPosition.XLower:
                    textItem.alignment = TextAnchor.UpperLeft;
                    transform.pivot = new Vector2(0f, 1f);
                    transform.anchorMin = new Vector2(0f, 0f);
                    transform.anchorMax = new Vector2(0f, 0f);
                    transform.offsetMin = new Vector2(0f, -70f);
                    transform.offsetMax = new Vector2(100f, -20f);
                    break;
                case LabelPosition.XUpper:
                    textItem.alignment = TextAnchor.UpperRight;
                    transform.pivot = new Vector2(1f, 1f);
                    transform.anchorMin = new Vector2(1f, 0f);
                    transform.anchorMax = new Vector2(1f, 0f);
                    transform.offsetMin = new Vector2(-100f, -70f);
                    transform.offsetMax = new Vector2(0f, -20f);
                    break;
                case LabelPosition.YLower:
                    textItem.alignment = TextAnchor.LowerRight;
                    transform.pivot = new Vector2(1f, 0f);
                    transform.anchorMin = new Vector2(0f, 0f);
                    transform.anchorMax = new Vector2(0f, 0f);
                    transform.offsetMin = new Vector2(-180f, 0f);
                    transform.offsetMax = new Vector2(-20f, 50f);
                    break;
                case LabelPosition.YUpper:
                    textItem.alignment = TextAnchor.UpperRight;
                    transform.pivot = new Vector2(1f, 1f);
                    transform.anchorMin = new Vector2(0f, 1f);
                    transform.anchorMax = new Vector2(0f, 1f);
                    transform.offsetMin = new Vector2(-180f, -50f);
                    transform.offsetMax = new Vector2(-20f, 0f);
                    break;
                default:
                    Debug.LogError($"Unexpected position: {position}");
                    break;
            }

            //ThemeText themeText = widget.AddComponent<ThemeText>();
            //themeText.textType = ThemeText.ThemeTextType.SystemText;
            //themeText.FindTier();
            //themeText.UpdateFormatting();

            return widget;
        }

        public override void PopulateWidget(GameObject parent)
        {
            GameObject plotWidget = CreateTextureWidget(
                "Spectrogram Plot Widget", parent, plot,
                anchorMin: new Vector2(0f, 0f),
                anchorMax: new Vector2(0.8f, 1f),
                offsetMin: new Vector2(120f, 60f));

            GameObject legendWidget = CreateTextureWidget(
                "Spectrogram Legend Widget", parent, legendTex,
                anchorMin: new Vector2(0.85f, 0f),
                anchorMax: new Vector2(1f, 1f),
                offsetMax: new Vector2(-120f, 0f));

            CreateLabelWidget(legendBounds.x.ToString(), legendWidget, LabelPosition.LegendBottom);
            CreateLabelWidget(((legendBounds.x + legendBounds.y) / 2f).ToString(), legendWidget, LabelPosition.LegendMiddle);
            CreateLabelWidget(legendBounds.y.ToString(), legendWidget, LabelPosition.LegendTop);

            CreateLabelWidget(xBounds.x.ToString(), plotWidget, LabelPosition.XLower);
            CreateLabelWidget(xBounds.y.ToString(), plotWidget, LabelPosition.XUpper);
            CreateLabelWidget(yBounds.x.ToString(), plotWidget, LabelPosition.YLower);
            CreateLabelWidget(yBounds.y.ToString(), plotWidget, LabelPosition.YUpper);
        }
    }

}
