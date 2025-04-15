using System;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.UI.Dialogs
{
    public class StringListInput : SimpleListInput
    {
        [SerializeField]
        private InputField inputField;

        public override object GetValue()
        {
            return inputField.text;
        }
        
        public override void SetValue(object value)
        {
            inputField.text = value.ToString();
        }
        
        public override void AddListener(Action<SimpleListInput> selectItem, Action valueChange)
        {
            base.AddListener(selectItem, valueChange);
            
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onValueChanged.AddListener(OnValueChanged);
        }
    }
}
