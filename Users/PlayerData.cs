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

        public static bool HasKey(string key)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) return c.ContainsKey(key);
            return ProfileData.HasKey(key);
        }

        public static void RemoveKey(string key)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c.Remove(key); return; }
            ProfileData.RemoveKey(key);
        }

        public static void SetInt(string key, int value)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c[key] = value; return; }
            ProfileData.SetInt(key, value);
        }

        public static void SetBool(string key, bool value)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c[key] = value; return; }
            ProfileData.SetBool(key, value);
        }

        public static void SetString(string key, string value)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c[key] = value; return; }
            ProfileData.SetString(key, value);
        }

        public static void SetFloat(string key, float value)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c[key] = value; return; }
            ProfileData.SetFloat(key, value);
        }

        public static void SetDouble(string key, double value)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c[key] = value; return; }
            ProfileData.SetDouble(key, value);
        }

        public static void SetJsonValue(string key, JsonValue value)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c[key] = value; return; }
            ProfileData.SetJsonValue(key, value);
        }

        public static void SetJsonArray(string key, JsonArray value)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null) { c[key] = value; return; }
            ProfileData.SetJsonArray(key, value);
        }

        public static int GetInt(string key, int defaultReturn = 0)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null)
            {
                return c.ContainsKey(key) && c[key].IsInteger ? c[key].AsInteger : defaultReturn;
            }
            return ProfileData.GetInt(key, defaultReturn);
        }

        public static bool GetBool(string key, bool defaultReturn = false)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null)
            {
                return c.ContainsKey(key) && c[key].IsBoolean ? c[key].AsBoolean : defaultReturn;
            }
            return ProfileData.GetBool(key, defaultReturn);
        }

        public static string GetString(string key, string defaultReturn = "")
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null)
            {
                return c.ContainsKey(key) && c[key].IsString ? c[key].AsString : defaultReturn;
            }
            return ProfileData.GetString(key, defaultReturn);
        }

        public static float GetFloat(string key, float defaultReturn = 0f)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null)
            {
                return c.ContainsKey(key) && c[key].IsNumber ? (float)c[key].AsNumber : defaultReturn;
            }
            return ProfileData.GetFloat(key, defaultReturn);
        }

        public static double GetDouble(string key, double defaultReturn = 0.0)
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null)
            {
                return c.ContainsKey(key) && c[key].IsNumber ? c[key].AsNumber : defaultReturn;
            }
            return ProfileData.GetDouble(key, defaultReturn);
        }

        public static JsonValue GetJsonValue(string key, JsonValue defaultReturn = default(JsonValue))
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null)
            {
                return c.ContainsKey(key) ? c[key] : defaultReturn;
            }
            return ProfileData.GetJsonValue(key, defaultReturn);
        }

        public static JsonArray GetJsonArray(string key, JsonArray defaultReturn = default(JsonArray))
        {
            JsonObject c = GetTrackUserStateOrNull(key);
            if (c != null)
            {
                return c.ContainsKey(key) && c[key].IsJsonArray ? c[key].AsJsonArray : defaultReturn;
            }
            return ProfileData.GetJsonArray(key, defaultReturn);
        }

        #endregion Convenience Properties

        #region Per-Track User-State Routing

        // Keys that always read/write at the flat user-data root regardless of active track.
        // These are system-owned and used by BGCScience / ProtocolManager / Localization.
        // Routing them into a track's UserState would break the system reading them back.
        private static readonly HashSet<string> systemKeys = new HashSet<string>(StringComparer.Ordinal)
        {
            // BGCScience.Keys
            "StudyName", "Lockout", "SessionCount", "Password",
            "ProtocolSet", "ProtocolKey", "ProtocolKeys",
            "AllDownloaded", "PushLogs", "Organization", "Study",
            "ServerCode", "HasUserBatteries", "ConditionFileName", "ProtocolFileName",
            // ProtocolManager.DataKeys (legacy fallbacks; modern path uses ActiveTrack)
            "SessionNumber", "ElementNumber", "SessionInProgress", "SequenceTimes",
            "CurrentSequenceStartTime", "SequenceIndex", "LockoutExpiration",
            "LockoutHasBypassPassword", "LastEncounteredSequenceIndex",
            // Localization
            "Language",
        };

        private static bool IsSystemKey(string key)
        {
            if (key == null) return true;
            // Any "ProtocolManager.*" key (covers ProtocolTrack.TracksDataKey and
            // ProtocolManager.ExtensionState by convention).
            if (key.StartsWith("ProtocolManager.", StringComparison.Ordinal)) return true;
            return systemKeys.Contains(key);
        }

        /// <summary>
        /// Returns the per-track UserState JsonObject for the active parallel-protocol track,
        /// creating it on demand. Returns null (meaning "use the flat root via ProfileData")
        /// when:
        ///  - the key is a known system key (must stay at root for BGCScience/ProtocolManager)
        ///  - the user is not running a parallel protocol
        ///  - no active track is set
        /// Mutations to the returned JsonObject propagate to disk on the next PlayerData.Save()
        /// because we never copy — we hand out the same reference ProtocolTrack mounts at
        /// "ProtocolManager.Tracks".<trackKey>.UserState (sibling to ExtensionState).
        /// </summary>
        private static JsonObject GetTrackUserStateOrNull(string key)
        {
            if (IsSystemKey(key)) return null;
            if (!ProtocolManager.IsParallelProtocol) return null;

            string trackKey = ProtocolManager.ActiveTrackKey;
            if (string.IsNullOrEmpty(trackKey)) return null;

            JsonValue tracksVal = ProfileData.GetJsonValue(ProtocolTrack.TracksDataKey);
            JsonObject tracksRoot;
            if (tracksVal.IsJsonObject)
            {
                tracksRoot = tracksVal.AsJsonObject;
            }
            else
            {
                tracksRoot = new JsonObject();
                ProfileData.SetJsonValue(ProtocolTrack.TracksDataKey, tracksRoot);
            }

            if (!tracksRoot.ContainsKey(trackKey) || !tracksRoot[trackKey].IsJsonObject)
            {
                tracksRoot[trackKey] = new JsonObject();
            }
            JsonObject trackObj = tracksRoot[trackKey];

            const string userStateKey = "UserState";
            if (!trackObj.ContainsKey(userStateKey) || !trackObj[userStateKey].IsJsonObject)
            {
                trackObj[userStateKey] = new JsonObject();
            }
            return trackObj[userStateKey];
        }

        #endregion Per-Track User-State Routing

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
