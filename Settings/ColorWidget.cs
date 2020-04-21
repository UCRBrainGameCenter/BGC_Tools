using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.Settings
{
    public class ColorWidget : MonoBehaviour
    {
        [SerializeField]
        private Text label = null;
        [SerializeField]
        private Image colorSwatch = null;
        [SerializeField]
        private Button settingButton = null;
        [SerializeField]
        private RectTransform alphaSwatchFiller = null;
        [SerializeField]
        private Text valueText = null;

        public Text LabelText => label;
        public Text ValueText => valueText;
        public Button SettingButton => settingButton;

        public void SetColor(Color newColor)
        {
            Color tempColor = newColor;
            tempColor.a = 1;

            colorSwatch.color = tempColor;

            alphaSwatchFiller.anchorMax = new Vector2(1f, newColor.a);
        }
    }
}
