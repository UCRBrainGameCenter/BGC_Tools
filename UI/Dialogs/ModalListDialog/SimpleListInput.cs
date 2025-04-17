using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BGC.UI.Dialogs
{
    public class SimpleListInput : MonoBehaviour, IPointerClickHandler
    {
        private static readonly Color DefaultSelectedItemBG = new Color32(0x00, 0x00, 0xFF, 0xC3);
        private static readonly Color DefaultSelectedItemText = Color.white;
        private static readonly Color DefaultEnabledItemBG = new Color32(0xFF, 0xFF, 0xFF, 0x82);
        private static readonly Color DefaultEnabledItemText = Color.black;

        protected virtual Color SelectedItemBG => DefaultSelectedItemBG;
        protected virtual Color SelectedItemText => DefaultSelectedItemText;
        protected virtual Color EnabledItemBG => DefaultEnabledItemBG;
        protected virtual Color EnabledItemText => DefaultEnabledItemText;
        
        [SerializeField]
        private Image background;
        [SerializeField]
        private Text text;
        
        protected Action<SimpleListInput> onSelectCallback;
        protected Action onValueChangeCallback;

        public virtual object GetValue()
        {
            return null;
        }
        
        public virtual void SetValue(object value)
        {
            
        }

        public void SetButtonState(bool activated)
        {
            if (activated)
            {
                background.color = SelectedItemBG;
                
                if(text != null)
                    text.color = SelectedItemText;
            }
            else
            {
                background.color = EnabledItemBG;
               
                if(text != null)
                    text.color = EnabledItemText;
            }
        }
        
        public virtual void AddListener(Action<SimpleListInput> selectItem, Action valueChange)
        {
            onSelectCallback = selectItem;
            onValueChangeCallback = valueChange;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onSelectCallback?.Invoke(this);
        }

        protected void OnValueChanged(object value)
        {
            onValueChangeCallback?.Invoke();
        }
    }
}
