using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using BGC.IO;
using BGC.Utility;
using LightJson;

namespace BGC.Users
{
    public static class PlayerData
    {
        public const string UserDataDir = "SaveData";

        private static DefaultData _defaultData = null;
        /// <summary> The default profile data. </summary>
        public static DefaultData DefaultData => _defaultData ?? (_defaultData = new DefaultData());

        private static UserData _currentUserData = null;
        /// <summary> Profile Data of the current user.  Or default if none are logged in. </summary>
        public static ProfileData ProfileData => _currentUserData as ProfileData ?? DefaultData;


        private static bool initialized = false;

        private static string previousUser = "";

        private static readonly List<string> users = new List<string>();

        #region Lock Accessors
        private const string LockStateKey = "LockState";
        private const string EverUnlockedKey = "EverUnlocked";

        /// <summary> Is the device currently in a Locked mode? </summary>
        public static bool IsLocked
        {
            get { return PlayerPrefs.GetInt(LockStateKey, 0) == 0; }
            set
            {
                if (value)
                {
                    PlayerPrefs.SetInt(LockStateKey, 0);
                }
                else
                {
                    EverUnlocked = true;
                    PlayerPrefs.SetInt(LockStateKey, 1);
                }
            }
        }

        /// <summary> Has the device ever been unlocked? </summary>
        public static bool EverUnlocked
        {
            get { return PlayerPrefs.GetInt(EverUnlockedKey, 0) != 0; }
            set
            {
                if (value)
                {
                    PlayerPrefs.SetInt(EverUnlockedKey, 1);
                }
            }
        }

        #endregion Lock Accessors
        #region Convenience ProfileData Access

        /// <summary> The Current Profile UserName </summary>
        public static string UserName => ProfileData.UserName;
        public static string LoggingName => ProfileData.LoggingName;

        /// <summary> The Current Profile IsDefault Status </summary>
        public static bool IsDefault => ProfileData.IsDefault;

        /// <summary> Serialize the current user's data </summary>
        public static void Save() => ProfileData.Serialize();

        public static bool HasKey(string key) => ProfileData.HasKey(key);
        public static void RemoveKey(string key) => ProfileData.RemoveKey(key);

        public static void SetInt(string key, int value) => ProfileData.SetInt(key, value);
        public static void SetBool(string key, bool value) => ProfileData.SetBool(key, value);
        public static void SetString(string key, string value) => ProfileData.SetString(key, value);
        public static void SetFloat(string key, float value) => ProfileData.SetFloat(key, value);
        public static void SetDouble(string key, double value) => ProfileData.SetDouble(key, value);
        public static void SetJsonValue(string key, JsonValue value) => ProfileData.SetJsonValue(key, value);
        public static void SetJsonArray(string key, JsonArray value) => ProfileData.SetJsonArray(key, value);

        public static int GetInt(string key, int defaultReturn = 0) => ProfileData.GetInt(key, defaultReturn);
        public static bool GetBool(string key, bool defaultReturn = false) => ProfileData.GetBool(key, defaultReturn);
        public static string GetString(string key, string defaultReturn = "") => ProfileData.GetString(key, defaultReturn);
        public static float GetFloat(string key, float defaultReturn = 0f) => ProfileData.GetFloat(key, defaultReturn);
        public static double GetDouble(string key, double defaultReturn = 0.0) => ProfileData.GetDouble(key, defaultReturn);
        public static JsonValue GetJsonValue(string key, JsonValue defaultReturn = default(JsonValue)) => ProfileData.GetJsonValue(key, defaultReturn);
        public static JsonArray GetJsonArray(string key, JsonArray defaultReturn = default(JsonArray)) => ProfileData.GetJsonArray(key, defaultReturn);

        #endregion Convenience Properties

        /// <summary> Load all usernames </summary>
        public static void DeserializeUsers()
        {
            //Load up saved information
            users.Clear();

            foreach (string fileName in DataManagement.GetDataFiles(UserDataDir))
            {
                if (Path.GetExtension(fileName) == FileExtensions.JSON)
                {
                    string newUserName = Path.GetFileNameWithoutExtension(fileName);

                    if (newUserName != "Default")
                    {
                        users.Add(newUserName);
                    }
                }
            }

            initialized = true;
        }

        /// <summary> Returns an enumeration of all loaded usernames </summary>
        public static IEnumerable<string> GetUserNames() => users.ToArray();

        /// <summary> Does the requested user already exist? </summary>
        public static bool UserExists(string userName) => users.Contains(userName);

        /// <summary>
        /// Attempts to log in with userName
        /// </summary>
        /// <param name="userName">Name of user to log into</param>
        /// <param name="userChangingCallback">Optional callback invoked when the user will change.
        /// Typically clearing logs, for example.</param>
        /// <returns>Whether the user was successfully logged in</returns>
        public static bool LogIn(
            string userName,
            Action userChangingCallback = null)
        {
            //Log out of current user (if still logged in)
            if (_currentUserData != null)
            {
                if (previousUser == userName)
                {
                    //Not changing user
                    return true;
                }

                LogOut();
            }

            //If we're logging into a new user...
            if (previousUser != userName)
            {
                userChangingCallback?.Invoke();
                previousUser = userName;
            }

            _currentUserData = new UserData(userName);

            if (_currentUserData.Deserialize() == false)
            {
                //Failed to load user
                _currentUserData = null;
                previousUser = "";

                Debug.LogError($"Failed to load selected user data: {userName}");
                return false;
            }

            return true;
        }

        /// <summary> Save and clear the current user </summary>
        public static void LogOut()
        {
            Save();
            _currentUserData = null;
        }

        /// <summary> Add a new user. </summary>
        /// <returns>Returns if the operation was successful</returns>
        public static bool AddUser(string userName, string loggingName = "")
        {
            if (!initialized)
            {
                //If deleteUser is attempted without first loading data
                Debug.LogError($"Tried to AddUser before initializing PlayerData.  userName: {userName}");
                return false;
            }

            if (users.Contains(userName))
            {
                return false;
            }

            UserData newUser = new UserData(userName, loggingName);
            users.Add(userName);
            newUser.Serialize();

            return true;
        }

        /// <summary> Deserialize, delete, and remove a user </summary>
        public static void DeleteUserData(string userName)
        {
            UserData tempUserData = new UserData(userName);
            if (tempUserData.Deserialize())
            {
                tempUserData.DeletePlayerData();
            }

            users.Remove(userName);
        }

        /// <summary> Get the migration preferences of all profiles. </summary>
        public static IEnumerable<MigrationData> GetMigrationData(
            string pushLogsKey,
            string organizationKey,
            string studyKey)
        {
            List<MigrationData> migrationData = new List<MigrationData>();

            if (initialized == false)
            {
                DeserializeUsers();
            }

            foreach (string userName in users)
            {
                UserData data = new UserData(userName);
                //Load in the data first

                if (data.Deserialize() && data.GetBool(pushLogsKey, false))
                {
                    migrationData.Add(new MigrationData
                    {
                        userName = data.LoggingName,
                        organization = data.GetString(organizationKey, "braingamecenter"),
                        study = data.GetString(studyKey, "default")
                    });
                }
            }

            return migrationData;
        }
    }
}
