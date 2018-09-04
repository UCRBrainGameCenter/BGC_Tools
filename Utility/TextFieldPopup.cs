using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace BGC.Utility
{
#if UNITY_EDITOR
    public class TextPopupField : EditorWindow
    {
        /// <summary>
        /// Ensures that the Popup is initialized
        /// </summary>
        private bool Initialized = false;

        /// <summary>
        /// Text to display
        /// </summary>
        private string text;

        /// <summary>
        /// Text that user will input
        /// </summary>
        private string inputText;

        /// <summary>
        /// Callback that sends input field
        /// </summary>
        private Action<string> callback;

        /// <summary>
        /// Initializes the PopupField
        /// </summary>
        public void Init(string text, Action<string> callback)
        {
            Initialized = true;

            this.text = text;
            this.callback = callback;
        }

        void OnGUI()
        {
            Assert.IsTrue(Initialized);

            GUILayout.Label(text);

            inputText = EditorGUILayout.TextField(inputText);

            if (GUILayout.Button("Submit"))
            {
                callback(inputText);
                this.Close();
            }

            if (GUILayout.Button("Close"))
            {
                this.Close();
            }
        }
    }
#endif
}