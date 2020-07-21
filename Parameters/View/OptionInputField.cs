using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.Parameters.View
{
    [RequireComponent(typeof(InputField))]
    public class OptionInputField : MonoBehaviour
    {
        [SerializeField]
        public Text InputFieldText;
        [SerializeField]
        public Text PostfixText;

        void Awake()
        {
            GetComponent<InputField>().onValueChanged.AddListener(DelayedUpdatePostfixPosition);
        }

        void Start()
        {
            DelayedUpdatePostfixPosition("");
        }

        private void OnEnable()
        {
            DelayedUpdatePostfixPosition("");
        }

        IEnumerator UpdatePostfixPosition()
        {
            yield return new WaitForEndOfFrame();

            PostfixText.GetComponent<RectTransform>().localPosition =
                new Vector2(10f + InputFieldText.preferredWidth, 0f);
        }

        private void DelayedUpdatePostfixPosition(string newString)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(UpdatePostfixPosition());
            }
        }
    }
}
