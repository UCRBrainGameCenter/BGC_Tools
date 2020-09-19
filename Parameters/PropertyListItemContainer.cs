using UnityEngine;
using UnityEngine.UI;
using BGC.UI.Dialogs;

namespace BGC.Parameters
{
#pragma warning disable UNT0013 // Remove invalid SerializeField attribute

    public class PropertyListItemContainer : MonoBehaviour
    {
        [SerializeField]
        public Text typeLabel;

        [SerializeField]
        public Text nameLabel;

        [SerializeField]
        public GameObject propertyFrame;

        [SerializeField]
        public Transform titleBox;

        [SerializeField]
        public Button infoButton;

        private string choiceInfoText = null;
        public string ChoiceInfoText
        {
            get => choiceInfoText;
            set
            {
                choiceInfoText = value;
                infoButton.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        private void Awake()
        {
            infoButton.onClick.AddListener(InfoClick);

            if (string.IsNullOrEmpty(choiceInfoText))
            {
                infoButton.gameObject.SetActive(false);
            }
        }

        private void InfoClick()
        {
            ModalDialog.ShowSimpleModal(
                mode: ModalDialog.Mode.Accept,
                headerText: "Info",
                bodyText: $"{typeLabel.text}\n{ChoiceInfoText ?? "Undocumented"}");
        }
    }

#pragma warning restore UNT0013 // Remove invalid SerializeField attribute
}

