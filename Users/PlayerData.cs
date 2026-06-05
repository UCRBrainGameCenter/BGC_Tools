using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using BGC.IO;
using BGC.Localization;
using BGC.Study;
using BGC.Utility;
using UnityEngine.SocialPlatforms;

namespace BGC.Users
{
    public static class PlayerData
    {
        public const string UserDataDir = "SaveData";

        private static DefaultData _defaultData = null;
        /// <summary> The default profile data. </summary>
        public static DefaultData DefaultData => _defaultData ?? (_defaultData = new DefaultData());

        private static GlobalData _globalData = null;
        /// <summary> The global profile data. </summary>
        public static GlobalData GlobalData => _globalData ?? (_globalData = new GlobalData());

        private static UserData _currentUserData = null;
        /// <summary> Profile Data of the current user.  Or default if none are logged in. </summary>
        public static ProfileData ProfileData => _currentUserData as ProfileData ?? DefaultData;

        /// <summary>
        /// The currently-logged-in user (or null). Used by <see cref="ProfileData.RoutingActive"/>
        /// so only the current user's profile routes per active track — never DefaultData/GlobalData
        /// or a transient non-current UserData instance.
        /// </summary>
        internal static UserData CurrentUserOrNull => _currentUserData;

        /// <summary>Returns the language the user is using.</summary>
        public static LocalizationSystem.Language Language
        {
            get
            {
                int playerLanguage = ProfileData.GetInt("Language", (int) LocalizationSystem.Language.English);
                return Enum.IsDefined(typeof(LocalizationSystem.Language), playerLanguage)
                    ? (LocalizationSystem.Language) playerLanguage
                    : LocalizationSystem.Language.English;
            }
        }

        private static bool initialized = false;

        private static string previousUser = "";

        private static readonly List<string> users = new List<string>();

        #region Convenience ProfileData Access

        /// <summary> The Current Profile UserName </summary>
        public static string UserName => ProfileData.UserName;
        public static string LoggingName => ProfileData.LoggingName;

        /// <summary> The Current Profile IsDefault Status </summary>
        public static bool IsDefault => ProfileData.IsDefault;

        /// <summary> Serialize the current user's data </summary>
        public static void Save()
        {
            ProfileData.Serialize();
            DefaultData.Serialize();
            GlobalData.Serialize();
        }

        // The per-track routing now lives in ProfileData's instance accessors (so it is transparent:
        // PlayerData.X and PlayerData.ProfileData.X resolve identically). These statics are thin
        // delegations to the current profile.
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

        #region Per-Track Branched Routing

        /// <summary>
        /// Locates a specific track's JsonObject by key for the …ForTrack API. Never
        /// auto-creates: callers are expected to enumerate <see cref="ProtocolManager.Tracks"/>
        /// for valid track keys first. Logs an error and returns null on miss. Reads the raw
        /// root (<see cref="ProfileData.UserDicts"/>) directly so it is unaffected by routing.
        /// </summary>
        private static JsonObject ExistingTrackJsonOrLogError(string trackKey, string caller)
        {
            if (string.IsNullOrEmpty(trackKey))
            {
                Debug.LogError($"PlayerData.{caller}: trackKey is null or empty.");
                return null;
            }

            JsonObject root = ProfileData.UserDicts;
            if (!root.ContainsKey(ProtocolTrack.TracksDataKey) || !root[ProtocolTrack.TracksDataKey].IsJsonObject)
            {
                Debug.LogError($"PlayerData.{caller}: no tracks loaded (\"{ProtocolTrack.TracksDataKey}\" missing).");
                return null;
            }

            JsonObject tracksRoot = root[ProtocolTrack.TracksDataKey].AsJsonObject;
            if (!tracksRoot.ContainsKey(trackKey) || !tracksRoot[trackKey].IsJsonObject)
            {
                Debug.LogError($"PlayerData.{caller}: track \"{trackKey}\" does not exist.");
                return null;
            }

            return tracksRoot[trackKey].AsJsonObject;
        }

        #endregion Per-Track Branched Routing

        #region Per-Track Explicit API (...ForTrack)

        // Optional API for inspecting / mutating a specific track regardless of the
        // current active track. Use case: TrackHub displaying every track's lockout
        // / password / progress state without switching active track.
        // Behavior on missing track: no-op write (with Debug.LogError) / default-return read.

        public static bool HasKeyForTrack(string trackKey, string key)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(HasKeyForTrack));
            return t != null && t.ContainsKey(key);
        }

        public static void RemoveKeyForTrack(string trackKey, string key)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(RemoveKeyForTrack));
            if (t == null) return;
            if (t.ContainsKey(key)) t.Remove(key);
        }

        public static void SetIntForTrack(string trackKey, string key, int value)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(SetIntForTrack));
            if (t == null) return;
            t[key] = value;
        }

        public static void SetBoolForTrack(string trackKey, string key, bool value)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(SetBoolForTrack));
            if (t == null) return;
            t[key] = value;
        }

        public static void SetStringForTrack(string trackKey, string key, string value)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(SetStringForTrack));
            if (t == null) return;
            t[key] = value;
        }

        public static void SetFloatForTrack(string trackKey, string key, float value)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(SetFloatForTrack));
            if (t == null) return;
            t[key] = value;
        }

        public static void SetDoubleForTrack(string trackKey, string key, double value)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(SetDoubleForTrack));
            if (t == null) return;
            t[key] = value;
        }

        public static void SetJsonValueForTrack(string trackKey, string key, JsonValue value)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(SetJsonValueForTrack));
            if (t == null) return;
            t[key] = value;
        }

        public static void SetJsonArrayForTrack(string trackKey, string key, JsonArray value)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(SetJsonArrayForTrack));
            if (t == null) return;
            t[key] = value;
        }

        public static int GetIntForTrack(string trackKey, string key, int defaultReturn = 0)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(GetIntForTrack));
            if (t == null) return defaultReturn;
            return t.ContainsKey(key) && t[key].IsInteger ? t[key].AsInteger : defaultReturn;
        }

        public static bool GetBoolForTrack(string trackKey, string key, bool defaultReturn = false)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(GetBoolForTrack));
            if (t == null) return defaultReturn;
            return t.ContainsKey(key) && t[key].IsBoolean ? t[key].AsBoolean : defaultReturn;
        }

        public static string GetStringForTrack(string trackKey, string key, string defaultReturn = "")
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(GetStringForTrack));
            if (t == null) return defaultReturn;
            return t.ContainsKey(key) && t[key].IsString ? t[key].AsString : defaultReturn;
        }

        public static float GetFloatForTrack(string trackKey, string key, float defaultReturn = 0f)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(GetFloatForTrack));
            if (t == null) return defaultReturn;
            return t.ContainsKey(key) && t[key].IsNumber ? (float)t[key].AsNumber : defaultReturn;
        }

        public static double GetDoubleForTrack(string trackKey, string key, double defaultReturn = 0.0)
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(GetDoubleForTrack));
            if (t == null) return defaultReturn;
            return t.ContainsKey(key) && t[key].IsNumber ? t[key].AsNumber : defaultReturn;
        }

        public static JsonValue GetJsonValueForTrack(string trackKey, string key, JsonValue defaultReturn = default(JsonValue))
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(GetJsonValueForTrack));
            if (t == null) return defaultReturn;
            return t.ContainsKey(key) ? t[key] : defaultReturn;
        }

        public static JsonArray GetJsonArrayForTrack(string trackKey, string key, JsonArray defaultReturn = default(JsonArray))
        {
            JsonObject t = ExistingTrackJsonOrLogError(trackKey, nameof(GetJsonArrayForTrack));
            if (t == null) return defaultReturn;
            return t.ContainsKey(key) && t[key].IsJsonArray ? t[key].AsJsonArray : defaultReturn;
        }

        #endregion Per-Track Explicit API

        /// <summary> Load all usernames </summary>
        public static void DeserializeUsers()
        {
            //Load up saved information
            users.Clear();

            //Extra safety to guarantee deserialization and conversion of Default and Global profiles
            _defaultData = new DefaultData();
            _globalData = new GlobalData();

            foreach (string fileName in DataManagement.GetDataFiles(UserDataDir))
            {
                if (Path.GetExtension(fileName) == FileExtensions.JSON)
                {
                    users.Add(Path.GetFileNameWithoutExtension(fileName));
                }
            }

            initialized = true;
        }

        /// <summary> Returns an enumeration of all loaded usernames </summary>
        public static IEnumerable<string> GetUserNames() => users.ToArray();

        /// <summary> Returns an enumeration of all UserData </summary>
        public static IEnumerable<UserData> GetAllUserData()
        {
            foreach (string userName in users)
            {
                UserData data = new UserData(userName);
                //Load in the data first

                if (data.Deserialize())
                {
                    yield return data;
                }
            }
        }

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
