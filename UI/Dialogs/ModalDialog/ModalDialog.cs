using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private Dropdown optionDropdown = null;
        [SerializeField]
        private Text secondaryBodyText = null;
        [SerializeField]
        private InputField secondaryInputField = null;
        [SerializeField]
        private Text toggleText = null;
        [SerializeField]
        private Toggle toggleButton = null;

        private static ModalDialog instance;

        private bool closeImmediately = true;
        private string awaitingMessage = null;
        
        private const string defaultAwaitingMessage = "Working...";

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
            InputToggleConfirmCancel,
            InputAccept,
            LockDown,
            ABC,
            InputInputConfirmCancel,
            DropdownInput,
            InputABC
        }
        private Mode mode = Mode.ConfirmCancel;

        public delegate void ModalButtonCallback(Response response);
        ModalButtonCallback buttonCallback;

        public delegate Task ModalButtonCallbackAsync(Response response);
        ModalButtonCallbackAsync buttonCallbackAsync;

        public delegate void ModalInputCallback(Response response, string inputText);
        ModalInputCallback inputCallback;
        
        public delegate Task ModalInputCallbackAsync(Response response, string inputText);
        ModalInputCallbackAsync inputCallbackAsync;

        public delegate void ModalInputToggleCallback(Response response, string inputText, bool toggle);
        ModalInputToggleCallback inputToggleCallback;
        
        public delegate Task ModalInputToggleCallbackAsync(Response response, string inputText, bool toggle);
        ModalInputToggleCallbackAsync inputToggleCallbackAsync;        

        public delegate void ModalDoubleInputCallback(Response response, string primaryInput, string secondaryInput);
        ModalDoubleInputCallback doubleInputCallback;
        
        public delegate Task ModalDoubleInputCallbackAsync(Response response, string primaryInput, string secondaryInput);
        ModalDoubleInputCallbackAsync doubleInputCallbackAsync;

        public delegate void ModalDropdownInputCallback(Response response, int selectionIndex, string secondaryInput);
        ModalDropdownInputCallback dropdownInputCallback;
        
        public delegate Task ModalDropdownInputCallbackAsync(Response response, int selectionIndex, string secondaryInput);
        ModalDropdownInputCallbackAsync dropdownInputCallbackAsync;

        private void ResetCallbacks()
        {
            buttonCallback = null;
            buttonCallbackAsync = null;
            inputCallback = null;
            inputCallbackAsync = null;
            inputToggleCallback = null;
            inputToggleCallbackAsync = null;
            doubleInputCallback = null;
            doubleInputCallbackAsync = null;
            dropdownInputCallback = null;
            dropdownInputCallbackAsync = null;
        }

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
            buttonA.onClick.AddListener(async () => await HandleButtons(Response.A));
            buttonB.onClick.AddListener(async () => await HandleButtons(Response.B));
            buttonC.onClick.AddListener(async () => await HandleButtons(Response.C));
        }
        
        private async void Update()
        {
            switch (mode)
            {
                // Simple modal types
                case Mode.Accept:
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        await HandleButtons(Response.A);
                    }
                    break;
                case Mode.ConfirmCancel:
                case Mode.DropdownInput:
                case Mode.YesNo:
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        await HandleButtons(Response.A);
                    }
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        await HandleButtons(Response.B);
                    }
                    break;

                // Input types
                case Mode.InputAccept:
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        await HandleButtons(Response.A);
                    }
                    break;
                case Mode.InputConfirmCancel:
                case Mode.InputToggleConfirmCancel:
                case Mode.InputInputConfirmCancel:
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                    {
                        await HandleButtons(Response.A);
                    }
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        await HandleButtons(Response.B);
                    }
                    break;
            }
        }

        private void SetHeaderText(string text) =>
            headerText.text = text;

        private void SetBodyText(string primaryText, string secondaryText = "")
        {
            primaryBodyText.text = primaryText;
            secondaryBodyText.text = secondaryText;
        }

        private void SetToggleText(string text) =>
            toggleText.text = text;

        private void SetMode(Mode mode)
        {
            this.mode = mode;

            primaryInputField.text = "";
            primaryInputField.gameObject.SetActive(
                mode == Mode.InputAccept ||
                mode == Mode.InputConfirmCancel ||
                mode == Mode.InputToggleConfirmCancel ||
                mode == Mode.InputInputConfirmCancel ||
                mode == Mode.InputABC);

            secondaryBodyText.gameObject.SetActive(mode == Mode.InputInputConfirmCancel || mode == Mode.DropdownInput);

            secondaryInputField.text = "";
            secondaryInputField.gameObject.SetActive(mode == Mode.InputInputConfirmCancel || mode == Mode.DropdownInput);

            optionDropdown.gameObject.SetActive(mode == Mode.DropdownInput);

            toggleButton.gameObject.SetActive(mode == Mode.InputToggleConfirmCancel);

            //Set button text
            switch (mode)
            {
                case Mode.ConfirmCancel:
                case Mode.InputConfirmCancel:
                case Mode.InputInputConfirmCancel:
                case Mode.DropdownInput:
                case Mode.InputToggleConfirmCancel:
                    SetButtonText(a: "Confirm", b: "Cancel");
                    break;

                case Mode.YesNo:
                    SetButtonText(a: "Yes", b: "No");
                    break;

                case Mode.Accept:
                case Mode.InputAccept:
                    SetButtonText(c: "Ok");
                    break;

                case Mode.InputABC:
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
            ModalButtonCallback callback = null,
            ModalButtonCallbackAsync callbackAsync = null,
            bool closeImmediately = true,
            string awaitingMessage = null)
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
            instance.ResetCallbacks();
            instance.buttonCallback = callback;
            instance.buttonCallbackAsync = callbackAsync;

            //Set flag for closing modal and message for waiting on await
            instance.closeImmediately = closeImmediately;
            instance.awaitingMessage = awaitingMessage;
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
            ModalButtonCallback callback) =>
                ShowCustomSimpleModalHelper(
                    headerText: headerText, 
                    bodyText: bodyText, 
                    buttonALabel: buttonALabel, 
                    buttonBLabel: buttonBLabel, 
                    buttonCLabel: buttonCLabel,
                    callback: callback,
                    closeImmediately: true,
                    awaitingMessage: null);
        
        /// <summary>
        /// Show the modal dialog in the indicated mode, and await on the async callback when it receives a response
        /// </summary>
        public static void ShowCustomSimpleModal(
            string headerText,
            string bodyText,
            string buttonALabel,
            string buttonBLabel,
            string buttonCLabel,
            ModalButtonCallbackAsync callbackAsync,
            bool closeImmediately = true,
            string awaitingMessage = null) =>
                ShowCustomSimpleModalHelper(
                    headerText: headerText, 
                    bodyText: bodyText, 
                    buttonALabel: buttonALabel, 
                    buttonBLabel: buttonBLabel, 
                    buttonCLabel: buttonCLabel,
                    callbackAsync: callbackAsync,
                    closeImmediately: closeImmediately,
                    awaitingMessage: awaitingMessage);
        
        private static void ShowCustomSimpleModalHelper(
            string headerText,
            string bodyText,
            string buttonALabel,
            string buttonBLabel,
            string buttonCLabel,
            ModalButtonCallback callback = null,
            ModalButtonCallbackAsync callbackAsync = null,
            bool closeImmediately = true,
            string awaitingMessage = null)
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
            instance.ResetCallbacks();
            instance.buttonCallback = callback;
            instance.buttonCallbackAsync = callbackAsync;
            
            //Set flag for closing modal and message for waiting on await
            instance.closeImmediately = closeImmediately;
            instance.awaitingMessage = awaitingMessage;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowInputModal(
            Mode mode,
            string headerText,
            string bodyText,
            ModalInputCallback inputCallback,
            ModalButtonCallback buttonCallback = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric) =>
                ShowInputModalHelper(
                    mode: mode,
                    headerText: headerText,
                    bodyText: bodyText,
                    inputCallback: inputCallback,
                    buttonCallback: buttonCallback,
                    inputType: inputType,
                    closeImmediately: true,
                    awaitingMessage: null);
        
        /// <summary>
        /// Show the modal dialog in the indicated mode, and await on the async callback when it receives a response
        /// </summary>
        public static void ShowInputModal(
            Mode mode,
            string headerText,
            string bodyText,
            ModalInputCallbackAsync inputCallbackAsync,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null) =>
                ShowInputModalHelper(
                    mode: mode,
                    headerText: headerText,
                    bodyText: bodyText,
                    inputCallbackAsync: inputCallbackAsync, 
                    buttonCallbackAsync: buttonCallbackAsync,
                    inputType: inputType,
                    closeImmediately: closeImmediately,
                    awaitingMessage: awaitingMessage);

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowInputModal(
            string headerText,
            string primaryBodyText,
            string secondaryBodyText,
            ModalDoubleInputCallback inputCallback,
            ModalButtonCallback buttonCallback = null,
            InputField.ContentType primaryInputType = InputField.ContentType.Alphanumeric,
            InputField.ContentType secondaryInputType = InputField.ContentType.Alphanumeric) =>
                ShowInputModalHelper(
                    headerText: headerText,
                    primaryBodyText: primaryBodyText,
                    secondaryBodyText: secondaryBodyText,
                    inputCallback: inputCallback,
                    buttonCallback: buttonCallback,
                    primaryInputType: primaryInputType,
                    secondaryInputType: secondaryInputType,
                    closeImmediately: true,
                    awaitingMessage: null);
        
        /// <summary>
        /// Show the modal dialog in the indicated mode, and await on the callback when it receives a response
        /// </summary>
        public static void ShowInputModal(
            string headerText,
            string primaryBodyText,
            string secondaryBodyText,
            ModalDoubleInputCallbackAsync inputCallbackAsync,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType primaryInputType = InputField.ContentType.Alphanumeric,
            InputField.ContentType secondaryInputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null) =>
                ShowInputModalHelper(
                    headerText: headerText,
                    primaryBodyText: primaryBodyText,
                    secondaryBodyText: secondaryBodyText,
                    inputCallbackAsync: inputCallbackAsync,
                    buttonCallbackAsync: buttonCallbackAsync,
                    primaryInputType: primaryInputType,
                    secondaryInputType: secondaryInputType,
                    closeImmediately: closeImmediately,
                    awaitingMessage: awaitingMessage);

        private static void ShowInputModalHelper(
            Mode mode,
            string headerText,
            string bodyText,
            ModalInputCallback inputCallback = null,
            ModalInputCallbackAsync inputCallbackAsync = null,
            ModalButtonCallback buttonCallback = null,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null)
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
            instance.ResetCallbacks();
            instance.buttonCallback = buttonCallback;
            instance.buttonCallbackAsync = buttonCallbackAsync;
            instance.inputCallback = inputCallback;
            instance.inputCallbackAsync = inputCallbackAsync;

            instance.primaryInputField.contentType = inputType;
            
            //Set flag for closing modal and message for waiting on await
            instance.closeImmediately = closeImmediately;
            instance.awaitingMessage = awaitingMessage;
        }
        
        private static void ShowInputModalHelper(
            string headerText,
            string primaryBodyText,
            string secondaryBodyText,
            ModalDoubleInputCallback inputCallback = null,
            ModalDoubleInputCallbackAsync inputCallbackAsync = null,
            ModalButtonCallback buttonCallback = null,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType primaryInputType = InputField.ContentType.Alphanumeric,
            InputField.ContentType secondaryInputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null)
        {
            //Update text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(primaryBodyText, secondaryBodyText);

            //Update buttons
            instance.SetMode(Mode.InputInputConfirmCancel);

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update callbacks
            instance.ResetCallbacks();
            instance.buttonCallback = buttonCallback;
            instance.buttonCallbackAsync = buttonCallbackAsync;
            instance.doubleInputCallback = inputCallback;
            instance.doubleInputCallbackAsync = inputCallbackAsync;

            instance.primaryInputField.contentType = primaryInputType;
            instance.secondaryInputField.contentType = secondaryInputType;
            
            //Set flag for closing modal and message for waiting on await
            instance.closeImmediately = closeImmediately;
            instance.awaitingMessage = awaitingMessage;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowInputToggleModal(
            string headerText,
            string bodyText,
            string toggleText,
            ModalInputToggleCallback inputToggleCallback,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool initialToggleState = false) =>
            ShowInputToggleModalHelper(
                headerText: headerText,
                bodyText: bodyText,
                toggleText: toggleText,
                inputToggleCallback: inputToggleCallback,
                inputType: inputType,
                initialToggleState: initialToggleState,
                closeImmediately: true,
                awaitingMessage: null);
        
        /// <summary>
        /// Show the modal dialog in the indicated mode, and await on the async callback when it receives a response
        /// </summary>
        public static void ShowInputToggleModal(
            string headerText,
            string bodyText,
            string toggleText,
            ModalInputToggleCallbackAsync inputToggleCallbackAsync,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool initialToggleState = false,
            bool closeImmediately = true,
            string awaitingMessage = null) =>
            ShowInputToggleModalHelper(
                headerText: headerText,
                bodyText: bodyText,
                toggleText: toggleText,
                inputToggleCallbackAsync: inputToggleCallbackAsync,
                inputType: inputType,
                initialToggleState: initialToggleState,
                closeImmediately: closeImmediately,
                awaitingMessage: awaitingMessage);       
        
        private static void ShowInputToggleModalHelper(
            string headerText,
            string bodyText,
            string toggleText,
            ModalInputToggleCallback inputToggleCallback = null,
            ModalInputToggleCallbackAsync inputToggleCallbackAsync = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool initialToggleState = false,
            bool closeImmediately = true,
            string awaitingMessage = null)
        {
            //Update text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(bodyText);
            instance.SetToggleText(toggleText);
            instance.toggleButton.isOn = initialToggleState;

            //Update buttons
            instance.SetMode(Mode.InputToggleConfirmCancel);

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update callbacks
            instance.ResetCallbacks();
            instance.inputToggleCallback = inputToggleCallback;
            instance.inputToggleCallbackAsync = inputToggleCallbackAsync;

            instance.primaryInputField.contentType = inputType;
            
            //Set flag for closing modal and message for waiting on await
            instance.closeImmediately = closeImmediately;
            instance.awaitingMessage = awaitingMessage;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowDropdownInputModal(
            string headerText,
            string primaryBodyText,
            string secondaryBodyText,
            IEnumerable<string> dropdownOptions,
            ModalDropdownInputCallback inputCallback,
            ModalButtonCallback buttonCallback = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric) =>
            ShowDropdownInputModalHelper(
                headerText: headerText,
                primaryBodyText: primaryBodyText,
                secondaryBodyText: secondaryBodyText,
                dropdownOptions: dropdownOptions,
                inputCallback: inputCallback,
                buttonCallback: buttonCallback,
                inputType: inputType,
                closeImmediately: true,
                awaitingMessage: null);
        
        /// <summary>
        /// Show the modal dialog in the indicated mode, and await on the async callback when it receives a response
        /// </summary>
        public static void ShowDropdownInputModal(
            string headerText,
            string primaryBodyText,
            string secondaryBodyText,
            IEnumerable<string> dropdownOptions,
            ModalDropdownInputCallbackAsync inputCallbackAsync,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null) =>
            ShowDropdownInputModalHelper(
                headerText: headerText,
                primaryBodyText: primaryBodyText,
                secondaryBodyText: secondaryBodyText,
                dropdownOptions: dropdownOptions,
                inputCallbackAsync: inputCallbackAsync,
                buttonCallbackAsync: buttonCallbackAsync,
                inputType: inputType,
                closeImmediately: closeImmediately,
                awaitingMessage: awaitingMessage);       
        
        private static void ShowDropdownInputModalHelper(
            string headerText,
            string primaryBodyText,
            string secondaryBodyText,
            IEnumerable<string> dropdownOptions,
            ModalDropdownInputCallback inputCallback = null,
            ModalDropdownInputCallbackAsync inputCallbackAsync = null,
            ModalButtonCallback buttonCallback = null,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null)
        {
            //Update text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(primaryBodyText, secondaryBodyText);

            //Update buttons
            instance.SetMode(Mode.DropdownInput);

            //Update dropdown
            instance.optionDropdown.ClearOptions();
            instance.optionDropdown.AddOptions(dropdownOptions.ToList());
            instance.optionDropdown.value = 0;
            instance.optionDropdown.RefreshShownValue();

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update callbacks
            instance.ResetCallbacks();
            instance.buttonCallback = buttonCallback;
            instance.buttonCallbackAsync = buttonCallbackAsync;
            instance.dropdownInputCallback = inputCallback;
            instance.dropdownInputCallbackAsync = inputCallbackAsync;

            instance.secondaryInputField.contentType = inputType;
            
            //Set flag for closing modal and message for waiting on await
            instance.closeImmediately = closeImmediately;
            instance.awaitingMessage = awaitingMessage;
        }

        /// <summary>
        /// Show the modal dialog in the indicated mode, and call the callback when it receives a response
        /// </summary>
        public static void ShowInputModalABC(
            string headerText,
            string bodyText,
            string buttonALabel,
            string buttonBLabel,
            string buttonCLabel,
            ModalInputCallback inputCallback,
            ModalButtonCallback buttonCallback = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric) =>
            ShowInputModalABCHelper(
                headerText: headerText,
                bodyText: bodyText,
                buttonALabel: buttonALabel,
                buttonBLabel: buttonBLabel,
                buttonCLabel: buttonCLabel,
                inputCallback: inputCallback,
                buttonCallback: buttonCallback,
                inputType: inputType,
                closeImmediately: true,
                awaitingMessage: null);
        
        /// <summary>
        /// Show the modal dialog in the indicated mode, and await on the async callback when it receives a response
        /// </summary>
        public static void ShowInputModalABC(
            string headerText,
            string bodyText,
            string buttonALabel,
            string buttonBLabel,
            string buttonCLabel,
            ModalInputCallbackAsync inputCallbackAsync,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null) =>
            ShowInputModalABCHelper(
                headerText: headerText,
                bodyText: bodyText,
                buttonALabel: buttonALabel,
                buttonBLabel: buttonBLabel,
                buttonCLabel: buttonCLabel,
                inputCallbackAsync: inputCallbackAsync,
                buttonCallbackAsync: buttonCallbackAsync,
                inputType: inputType,
                closeImmediately: closeImmediately,
                awaitingMessage: awaitingMessage);       
        
        public static void ShowInputModalABCHelper(
            string headerText,
            string bodyText,
            string buttonALabel,
            string buttonBLabel,
            string buttonCLabel,
            ModalInputCallback inputCallback = null,
            ModalInputCallbackAsync inputCallbackAsync = null,
            ModalButtonCallback buttonCallback = null,
            ModalButtonCallbackAsync buttonCallbackAsync = null,
            InputField.ContentType inputType = InputField.ContentType.Alphanumeric,
            bool closeImmediately = true,
            string awaitingMessage = null)
        {
            //Update text
            instance.SetHeaderText(headerText);
            instance.SetBodyText(bodyText);

            //Update buttons
            instance.SetMode(Mode.InputABC);
            instance.SetButtonText(buttonALabel, buttonBLabel, buttonCLabel);

            //Set dialog visible
            instance.gameObject.SetActive(true);

            //Update callbacks
            instance.ResetCallbacks();
            instance.buttonCallback = buttonCallback;
            instance.buttonCallbackAsync = buttonCallbackAsync;
            instance.inputCallback = inputCallback;
            instance.inputCallbackAsync = inputCallbackAsync;

            instance.primaryInputField.contentType = inputType;
            
            //Set flag for closing modal and message for waiting on await
            instance.closeImmediately = closeImmediately;
            instance.awaitingMessage = awaitingMessage;
        }

        /// <summary>
        /// Accept the button repsonse as input, invoke and clear the callbacks, and hide the dialog
        /// </summary>
        private async Task HandleButtons(Response response)
        {
            //Temporary copy to allow for the calling of the dialog within a callback
            ModalButtonCallback tmpCallback = buttonCallback;
            ModalButtonCallbackAsync tmpCallbackAsync = buttonCallbackAsync;
            ModalInputCallback tmpInputCallback = inputCallback;
            ModalInputCallbackAsync tmpInputCallbackAsync = inputCallbackAsync;
            ModalInputToggleCallback tmpInputToggleCallback = inputToggleCallback;
            ModalInputToggleCallbackAsync tmpInputToggleCallbackAsync = inputToggleCallbackAsync;
            ModalDoubleInputCallback tmpDoubleInputCallback = doubleInputCallback;
            ModalDoubleInputCallbackAsync tmpDoubleInputCallbackAsync = doubleInputCallbackAsync;
            ModalDropdownInputCallback tmpDropdownInputCallback = dropdownInputCallback;
            ModalDropdownInputCallbackAsync tmpDropdownInputCallbackAsync = dropdownInputCallbackAsync;

            ResetCallbacks();

            gameObject.SetActive(false);
            if (!closeImmediately)
            {
                ShowSimpleModal(
                    mode: Mode.LockDown,
                    headerText: "",
                    bodyText: awaitingMessage ?? defaultAwaitingMessage);
            }

            tmpCallback?.Invoke(response);
            await (tmpCallbackAsync?.Invoke(response) ?? Task.CompletedTask);
            tmpInputCallback?.Invoke(response, primaryInputField.text);
            await (tmpInputCallbackAsync?.Invoke(response, primaryInputField.text) ?? Task.CompletedTask);
            tmpInputToggleCallback?.Invoke(response, primaryInputField.text, toggleButton.isOn);
            await (tmpInputToggleCallbackAsync?.Invoke(response, primaryInputField.text, toggleButton.isOn) ?? Task.CompletedTask);
            tmpDoubleInputCallback?.Invoke(response, primaryInputField.text, secondaryInputField.text);
            await (tmpDoubleInputCallbackAsync?.Invoke(response, primaryInputField.text, secondaryInputField.text) ?? Task.CompletedTask);
            tmpDropdownInputCallback?.Invoke(response, optionDropdown.value, secondaryInputField.text);
            await (tmpDropdownInputCallbackAsync?.Invoke(response, optionDropdown.value, secondaryInputField.text) ?? Task.CompletedTask);
            
            gameObject.SetActive(false);
        }
    }
}