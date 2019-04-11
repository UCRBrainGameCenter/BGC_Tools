using System;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.UI.Dialogs
{
    public class ModalDialog : MonoBehaviour
    {
        [Header("Dialog Buttons")]
        [SerializeField]
        private Button buttonA = null;
        [SerializeField]
        private Button buttonB = null;
        [SerializeField]
        private Button buttonC = null;

        [Header("Dialog Components")]
        [SerializeField]
        private Text headerText = null;
        [SerializeField]
        private Text primaryBodyText = null;
        [SerializeField]
        private InputField primaryInputField = null;
        [SerializeField]
        private Text secondaryBodyText = null;
        [SerializeField]
        private InputField secondaryInputField = null;

        private static ModalDialog instance;

        public enum Response
        {
            A = 0,
            Confirm = A,
            Yes = A,

            B,
            Cancel = B,
            No = B,

            C,
            Accept = C
        }

        public enum Mode
        {
            ConfirmCancel = 0,
            Accept,
            YesNo,
            InputConfirmCancel,
            InputAccept,
            LockDown,
            ABC,
            InputInputConfirmCancel
        }

        public delegate void ModalButtonCallback(Response response);
        ModalButtonCallback buttonCallback;

        public delegate void ModalInputCallback(Response response, string inputText);
        ModalInputCallback inputCallback;

        public delegate void ModalDoubleInputCallback(Response response, string primaryInput, string secondaryInput);
        ModalDoubleInputCallback doubleInputCallback;

        public ModalDialog()
        {
            instance = this;
        }

        public void Initialize()
        {
            instance = this;
        }

        private void Awake()
        {
            buttonA.onClick.AddListener(() => HandleButtons(Response.A));
            buttonB.onClick.AddListener(() => HandleButtons(Response.B));
            buttonC.onClick.AddListener(() => HandleButtons(Response.C));
        }

        private void SetHeaderText(string text)
        {
            headerText.text = text;
        }

        private void SetBodyText(string primaryText, string secondaryText = "")
        {
            primaryBodyText.text = primaryText;
            secondaryBodyText.text = secondaryText;
        }

        private void SetMode(Mode mode)
        {
            primaryInputField.text = "";
            primaryInputField.gameObject.SetActive(
                mode == Mode.InputAccept ||
                mode == Mode.InputConfirmCancel ||
                mode == Mode.InputInputConfirmCancel);

            secondaryBodyText.gameObject.SetActive(mode == Mode.InputInputConfirmCancel);

            secondaryInputField.text = "";
            secondaryInputField.gameObject.SetActive(mode == Mode.InputInputConfirmCancel);

            //Set button text
            switch (mode)
            {
                case Mode.ConfirmCancel:
                case Mode.InputConfirmCancel:
                case Mode.InputInputConfirmCancel:
                    SetButtonText(a: "Confirm", b: "Cancel");
                    break;

                case Mode.YesNo:
                    SetButtonText(a: "Yes", b: "No");
                    break;

                case Mode.Accept:
                case Mode.InputAccept:
                    SetButtonText(c: "Ok");
                    break;

                case Mode.ABC:
                    //Do nothing - Text set separately
                    break;

                case Mode.LockDown:
                    //Clear & Lock all buttons
                    SetButtonText(a: "", b: "", c: "");
                    break;

                default:
                    Debug.LogError($"Unexpected DialogMode: {mode}");
                    break;
            }

        }

        private void SetButtonText(string a = "", string b = "", string c = "")
        {
            buttonA.gameObject.SetActive(string.IsNullOrEmpty(a) == false);
            buttonB.gameObject.SetActive(string.IsNullOrEmpty(b) == false);
            buttonC.gameObject.SetActive(string.IsNullOrEmpty(c) == false);

            buttonA.GetComponentInChildren<Text>().text = a;
            buttonB.GetComponentInChildren<Text>().text = b;
            buttonC.GetComponentInChildren<Text>().text = c;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowSimpleModal(
            Mode mode,
            string headerText,
            string bodyText,
            ModalButtonCallback callback = null)
        {
            Debug.Assert(
                condition: mode != Mode.InputAccept && mode != Mode.InputConfirmCancel,
                message: "Incorrect method call.  Call ShowInputModal instead.");

            Debug.Assert(
                condition: mode != Mode.InputInputConfirmCancel,
                message: "Incorrect method call.  Call ShowInputModal instead.");


            //Update text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(bodyText);

            //Update buttons
            instance.SetMode(mode);

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update Callbacks
            instance.buttonCallback = callback;
            instance.inputCallback = null;
            instance.doubleInputCallback = null;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowCustomSimpleModal(
            string headerText,
            string bodyText,
            string buttonALabel,
            string buttonBLabel,
            string buttonCLabel,
            ModalButtonCallback callback)
        {
            //Update Text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(bodyText);

            //Update buttons
            instance.SetMode(Mode.ABC);
            instance.SetButtonText(buttonALabel, buttonBLabel, buttonCLabel);

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update Callbacks
            instance.buttonCallback = callback;
            instance.inputCallback = null;
            instance.doubleInputCallback = null;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowInputModal(
            Mode mode,
            string headerText,
            string bodyText,
            ModalInputCallback inputCallback,
            ModalButtonCallback buttonCallback = null)
        {
            Debug.Assert(
                condition: mode == Mode.InputAccept || mode == Mode.InputConfirmCancel,
                message: "Incorrect method call.  Don't call ShowInputModal if not using input.");

            //Update text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(bodyText);

            //Update buttons
            instance.SetMode(mode);

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update callbacks
            instance.buttonCallback = buttonCallback;
            instance.inputCallback = inputCallback;
            instance.doubleInputCallback = null;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowInputModal(
            string headerText,
            string primaryBodyText,
            string secondaryBodyText,
            ModalDoubleInputCallback inputCallback,
            ModalButtonCallback buttonCallback = null)
        {
            //Update text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(primaryBodyText, secondaryBodyText);

            //Update buttons
            instance.SetMode(Mode.InputInputConfirmCancel);

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update callbacks
            instance.buttonCallback = buttonCallback;
            instance.inputCallback = null;
            instance.doubleInputCallback = inputCallback;
        }

        /// <summary>
        /// Accept the button repsonse as input, invoke and clear the callbacks, and hide the dialog
        /// </summary>
        private void HandleButtons(Response response)
        {
            //Temporary copy to allow for the calling of the dialog within a callback
            ModalButtonCallback tmpCallback = buttonCallback;
            ModalInputCallback tmpInputCallback = inputCallback;
            ModalDoubleInputCallback tmpDoubleInputCallback = doubleInputCallback;

            buttonCallback = null;
            inputCallback = null;
            doubleInputCallback = null;

            gameObject.SetActive(false);

            tmpCallback?.Invoke(response);
            tmpInputCallback?.Invoke(response, primaryInputField.text);
            tmpDoubleInputCallback?.Invoke(response, primaryInputField.text, secondaryInputField.text);
        }
    }


}