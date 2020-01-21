using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.Settings
{
    public class ColorWidget : MonoBehaviour
    {
        public Text label;
        public Image colorSwatch;
        public Button settingButton;
        public RectTransform alphaSwatchFiller;

        public void SetColor(Color newColor)
        {
            Color tempColor = newColor;
            tempColor.a = 1;

            colorSwatch.color = tempColor;

            alphaSwatchFiller.anchorMax = new Vector2(1f, newColor.a);
        }
    }
}
