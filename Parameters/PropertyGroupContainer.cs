using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BGC.UI.Dialogs;

namespace BGC.Parameters
{
#pragma warning disable UNT0013 // Remove invalid SerializeField attribute
    public class PropertyGroupContainer : MonoBehaviour
    {
        [SerializeField]
        public Text label;

        [SerializeField]
        public Dropdown options;

        [SerializeField]
        public GameObject propertyFrame;

        [SerializeField]
        public Transform titleBox;

        [SerializeField]
        public Button infoButton;

        private string groupInfoText = null;
        public string GroupInfoText
        {
            get => groupInfoText;
            set
            {
                groupInfoText = value;
                infoButton.gameObject.SetActive(
                    !string.IsNullOrEmpty(value) || !string.IsNullOrEmpty(ChoiceInfoText));
            }
        }

        private string choiceInfoText = null;
        public string ChoiceInfoText
        {
            get => choiceInfoText;
            set
            {
                choiceInfoText = value;
                infoButton.gameObject.SetActive(
                    !string.IsNullOrEmpty(GroupInfoText) || !string.IsNullOrEmpty(value));
            }
        }

        private void Awake()
        {
            infoButton.onClick.AddListener(InfoClick);

            if (string.IsNullOrEmpty(groupInfoText) && string.IsNullOrEmpty(choiceInfoText))
            {
                infoButton.gameObject.SetActive(false);
            }
        }

        private void InfoClick()
        {
            string infoText;
            if (string.IsNullOrEmpty(ChoiceInfoText))
            {
                infoText = $"{label.text}\n" +
                    $"{GroupInfoText ?? "Undocumented"}";
            }
            else
            {
                infoText = $"{label.text}\n" +
                    $"{GroupInfoText ?? "Undocumented"}\n" +
                    $"{options.captionText.text}:\n" +
                    $"{ChoiceInfoText}";
            }

            ModalDialog.ShowSimpleModal(
                mode: ModalDialog.Mode.Accept,
                headerText: "Info",
                bodyText: infoText);
        }
    }
}

#pragma warning restore UNT0013 // Remove invalid SerializeField attribute
