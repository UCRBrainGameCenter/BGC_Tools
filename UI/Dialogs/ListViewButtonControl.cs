using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.UI.Dialogs
{
    public class ListViewButtonControl : MonoBehaviour
    {
        private static readonly Color DefaultSelectedItemBG = new Color32(0x00, 0x00, 0xFF, 0xC3);
        private static readonly Color DefaultSelectedItemText = Color.white;
        private static readonly Color DefaultEnabledItemBG = new Color32(0xFF, 0xFF, 0xFF, 0x82);
        private static readonly Color DefaultEnabledItemText = Color.black;
        private static readonly Color DefaultDisabledItemBG = new Color32(0xDD, 0xDD, 0xDD, 0x30);
        private static readonly Color DefaultDisabledItemText = new Color32(0x7C, 0x7C, 0x7C, 0x9B);

        protected virtual Color SelectedItemBG => DefaultSelectedItemBG;
        protected virtual Color SelectedItemText => DefaultSelectedItemText;
        protected virtual Color EnabledItemBG => DefaultEnabledItemBG;
        protected virtual Color EnabledItemText => DefaultEnabledItemText;
        protected virtual Color DisabledItemBG => DefaultDisabledItemBG;
        protected virtual Color DisabledItemText => DefaultDisabledItemText;

        public void SetButtonState(bool activated)
        {
            if (activated)
            {
                GetComponent<Image>().color = SelectedItemBG;
                GetComponentInChildren<Text>().color = SelectedItemText;
            }
            else
            {
                if (GetComponent<Button>().interactable)
                {
                    GetComponent<Image>().color = EnabledItemBG;
                    GetComponentInChildren<Text>().color = EnabledItemText;
                }
                else
                {
                    GetComponent<Image>().color = DisabledItemBG;
                    GetComponentInChildren<Text>().color = DisabledItemText;
                }
            }
        }
    }
}
