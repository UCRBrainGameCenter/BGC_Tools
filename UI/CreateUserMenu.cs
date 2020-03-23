using BGC.MonoUtility;
using BGC.UI.Dialogs;
using BGC.Users;
using BGC.Web;
using LightJson;
using LightJson.Serialization;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Plugins.BGC_Tools.UI
{
    public class CreateUserMenu
    {
        private readonly StatusPanel statusPanel;
        private readonly string gameName;
        private readonly Action<string> localUserCreationCallback;
        private readonly Func<string, JsonObject, bool> serverUserCreationCallback;

        /// <summary>Displays the Create User Menu. Note that this class uses the <see cref="ModalDialog"/> class</summary>
        /// <param name="statusPanel">Panel that displays loading status. Must be from the app.</param>
        /// <param name="gameName">The name of the game.</param>
        /// <param name="localUserCreationCallback">Callback that is executed when a local user needs to be created</param>
        /// <param name="serverUserCreationCallback">Callback that is executed when a server-code user needs to be created</param>
        public CreateUserMenu(
            StatusPanel statusPanel, 
            string gameName, 
            Action<string> localUserCreationCallback, 
            Func<string, JsonObject, bool> serverUserCreationCallback)
        {
            this.statusPanel = statusPanel;
            this.gameName = gameName;
            this.serverUserCreationCallback = serverUserCreationCallback;
            this.localUserCreationCallback = localUserCreationCallback;
            ShowCreateUserMenu();
        }

        /// <summary>Displays the create user menu. Handles all input flow internally.</summary>
        /// <param name="errorMessage">Optional error message to display on the menu.</param>
        private void ShowCreateUserMenu(string errorMessage = "")
        {
            ModalDialog.ShowInputModalABC(
                headerText: "Creating New User",
                bodyText: $"Please Enter Your Server Code.\n\n<i>If you have not been given a server code, click the <b>Create Offline User</b></i> button instead.\n\n<color=red>{errorMessage}</color>",
                buttonALabel: "Submit",
                buttonBLabel: "Cancel",
                buttonCLabel: "Create Offline User",
                inputCallback: CodeSubmitted,
                buttonCallback: null,
                inputType: InputField.ContentType.EmailAddress);
        }

        /// <summary>Callback that parses server code and executes further prompts if necessary</summary>
        private void CodeSubmitted(
            ModalDialog.Response response,
            string serverCode)
        {
            switch (response)
            {
                case ModalDialog.Response.Confirm:
                    SubmitServerCode(serverCode);
                    break;

                case ModalDialog.Response.Cancel:
                    break;

                case ModalDialog.Response.Accept:
                    // TODO: Create Offline User
                    PromptForOfflineUser();
                    break;

                default:
                    Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                    break;
            }
        }

        /// <summary>Displays an input window for putting in an offline user name</summary>
        /// <param name="bodyMessage">Additional information to display on the input menu. Generally used for errors</param>
        private  void PromptForOfflineUser(string errorMessage = "")
        {
            ModalDialog.ShowInputModal(
                ModalDialog.Mode.InputConfirmCancel,
                headerText: "Username",
                bodyText: $"Enter a Username\n\n<color=red>{errorMessage}</color>",
                inputCallback: (ModalDialog.Response response, string input) =>
                {
                    switch (response)
                    {
                        case ModalDialog.Response.Confirm:
                            input = input.Trim();

                        //Abort and warn if the name is bad or not unique
                        if (!ValidateName(input, out string error))
                            {
                                PromptForOfflineUser(error);
                                return;
                            }

                            Submit(input, "");

                            break;

                        case ModalDialog.Response.Cancel:
                            break;

                        case ModalDialog.Response.Accept:
                        default:
                            Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                            break;
                    }
                });
        }

        /// <summary>Validates usernames</summary>
        /// <param name="newName">Name to check</param>
        /// <param name="errorMessage">string the error message is stored into</param>
        /// <returns>TRUE if valid, FALSE if not</returns>
        private  bool ValidateName(string newName, out string errorMessage)
        {
            if (string.IsNullOrEmpty(newName))
            {
                errorMessage = "Invalid user name. You cannot add a user with an empty name";
                return false;
            }

            if (newName.Contains("/") || newName.Contains(".") || newName.Contains("\\"))
            {
                errorMessage = "Invalid user name. User name cannot contain characters: '/', '.', or '\\'.";
                return false;
            }

            //Check if user name is available
            if (PlayerData.UserExists(newName))
            {
                errorMessage = "Invalid user name. User already exists. Users must have unique names.";
                return false;
            }

            errorMessage = "";

            return true;
        }

        /// <summary>Submits a username and code for creation</summary>
        private  void Submit(string newName, string code)
        {
            newName = newName.Trim();
            code = code.Trim();

            //Abort and warn if the name is bad or not unique
            if (!TestNameValid(newName))
            {
                return;
            }

            if (code != "")
            {
                //Server User Creation

                //Abort if the code is malformed
                if (!TestCodeValid(code))
                {
                    return;
                }

                SetStudyConditionData(code);
            }
            else
            {
                //Local User Creation
                localUserCreationCallback(newName);
            }
        }

        private  bool TestNameValid(string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                //menuManager.PlayWarningSound();
                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Invalid User Name",
                    bodyText: "You cannot add a user with an empty name.");
                return false;
            }

            if (newName.Contains("/") || newName.Contains(".") || newName.Contains("\\"))
            {
                //menuManager.PlayWarningSound();
                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Invalid User Name",
                    bodyText: "Name is invalid. Cannot contain characters:'/', '.', or '\\'.");
                return false;
            }

            //Check if user name is available
            if (PlayerData.UserExists(newName))
            {
                //menuManager.PlayWarningSound();
                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Invalid User Name",
                    bodyText: "User already exists.  Users must have unique names.");
                return false;
            }

            return true;
        }

        private  bool TestCodeValid(string code)
        {
            if (code.Contains("/") || code.Contains("\\") || code.Contains(":"))
            {
                //menuManager.PlayWarningSound();
                //ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                //    headerText: "Invalid Code",
                //    bodyText: "Server code is invalid. It cannot contain any special characters.\n" +
                //        "If you have not been given a server code, leave it blank.");
                return false;
            }

            return true;
        }

        /// <summary>Validates server code and retrieves condition from S3 based on the server code</summary>
        private  void SubmitServerCode(string code)
        {
            code = code.Trim();

            if (code != "")
            {
                //Server User Creation

                //Abort if the code is malformed
                if (!TestCodeValid(code))
                {
                    //ModalDialog.ShowSimpleModal(
                    //	mode: ModalDialog.Mode.Accept,
                    //	headerText: "Error",
                    //	bodyText: "<color=red>Server code is invalid. It cannot contain any special characters.</color>",
                    //	callback: (resp) => CreateUserButtonPressed());

                    ShowCreateUserMenu("Server code is invalid. It cannot contain any special characters.");
                    return;
                }

                SetStudyConditionData(code);

                //HandleServerNameCreation(
                //    code: code,
                //    newName: newName,
                //    callback: () => menuManager.SetMenu(MenuHandler.MenuPanel.Main));
            }
            else
            {
                // Provide Error Response
                ShowCreateUserMenu("Server code cannot be empty");
            }
        }

        /// <summary>
        /// Pulls the condition from S3 along with study, organization, and logging name and adds a user if applicable.
        /// </summary>
        /// <param name="code">Server code to pull data from</param>
        private  void SetStudyConditionData(
            string code,
            Action callback = null)
        {
            code = code.ToLowerInvariant();
            statusPanel.gameObject.SetActive(true);

            AWSServer.GetConfiguration(
                code: code,
                game: gameName,
                statusPanel: statusPanel,
                callback: (body, statusCode) =>
                {
                    JsonValue responseBody = JsonReader.Parse(new StringReader(body));

                    if (statusCode == 200)
                    {
                    //statusPanel.Status = body;
                    statusPanel.gameObject.SetActive(false);

                        JsonObject data = responseBody["data"];
                        string loggingName = data["loggingName"];

                    // Check for logging name. If it's empty, then we have an anonymous config
                    // User will need to create a username.
                    // If it's not empty, then we use the logging name for their config
                    if (string.IsNullOrEmpty(loggingName))
                    {
                        // Prompt Username and send the callback
                        PromptForUsername(data, callback);
                        }
                        else
                        {
                        // If we have a logging name, then add the user using the code as the user name.
                        // The data will be parsed for the logging name
                        if (serverUserCreationCallback(code, data))
                            callback?.Invoke();
                        }
                    }
                    else
                    {
                        statusPanel.gameObject.SetActive(false);
                    //string errorMessage = $"<b>Server Error {statusCode}</b>\n" + responseBody["message"];
                    //CreateUserButtonPressed(errorMessage);
                    string errorMessage = "<color=red>" + responseBody["message"] + "</color>";
                        ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                            headerText: $"Server Error: {statusCode}",
                            bodyText: errorMessage,
                            (response) => ShowCreateUserMenu());
                    }
                });
        }

        /// <summary>Displays UI for inputting username after submitting an Anonymous Server Code</summary>
        private void PromptForUsername(JsonObject serverData, Action callback, string bodyMessage = "")
        {
            ModalDialog.ShowInputModal(
                ModalDialog.Mode.InputConfirmCancel,
                headerText: "Username",
                bodyText: $"Enter a Username\n\n<color=red>{bodyMessage}</color>",
                inputCallback: (ModalDialog.Response response, string input) =>
                {
                    switch (response)
                    {
                        case ModalDialog.Response.Confirm:
                            input = input.Trim();

                        //Abort and warn if the name is bad or not unique
                        if (!ValidateName(input, out string errorMessage))
                            {
                            //ModalDialog.ShowSimpleModal(
                            //	mode: ModalDialog.Mode.Accept,
                            //	headerText: "Error", 
                            //	bodyText: $"<color=red>{errorMessage}</color>",
                            //	callback: (resp) => PromptForUsername(serverData, callback));
                            PromptForUsername(serverData, callback, errorMessage);
                                return;
                            }

                            serverUserCreationCallback(input, serverData);
                            callback?.Invoke();
                            break;

                        case ModalDialog.Response.Cancel:
                            break;

                        case ModalDialog.Response.Accept:
                        default:
                            Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                            break;
                    }
                });
        }
    }
}
