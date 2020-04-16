using System;
using System.IO;
using UnityEngine;
using LightJson;
using BGC.IO;

namespace BGC.Users
{
    public abstract class ProfileData
    {
        public string UserName { get; }
        private string loggingName;
        public string LoggingName => loggingName;

        private JsonObject userData = new JsonObject();

        private const int userDataSerializationVersion = 1;

        public ProfileData(string userName, string loggingName)
        {
            UserName = userName;
            this.loggingName = string.IsNullOrEmpty(loggingName) ? userName : loggingName;
        }

        /// <summary> Is this an instance of default data? </summary>
        public abstract bool IsDefault { get; }

        /// <summary> Path of the user datafile </summary>
        protected virtual string PlayerFilePath => DataManagement.PathForDataFile(
            dataDirectory: PlayerData.UserDataDir,
            fileName: FileExtensions.AddJsonExtension(UserName));

        /// <summary> Clear all values and keys </summary>
        public virtual void Clear()
        {
            userData.Clear();
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetInt(string key, int value)
        {
            if (userData.ContainsKey(key) == false)
            {
                //Data did not exist - add it
                userData.Add(key, value);
            }
            else
            {
                //Check existing data for type match
                if (userData[key].IsInteger == false)
                {
                    Debug.LogError($"PlayerData \"{key}\" Datatype changed from {userData[key].Type} to Int");
                }

                //Set data
                userData[key] = value;
            }
        }


        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetBool(string key, bool value)
        {
            if (userData.ContainsKey(key) == false)
            {
                //Data did not exist - add it
                userData.Add(key, value);
            }
            else
            {
                //Check existing data for type match
                if (userData[key].IsBoolean == false)
                {
                    if (userData[key].IsInteger)
                    {
                        Debug.LogWarning($"PlayerData \"{key}\" Datatype changed from {userData[key].Type} to Bool");
                    }
                    else
                    {
                        Debug.LogError($"PlayerData \"{key}\" Datatype changed from {userData[key].Type} to Bool");
                    }
                }

                //Set data
                userData[key] = value;
            }
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetString(string key, string value)
        {
            if (userData.ContainsKey(key) == false)
            {
                //Data did not exist - add it
                userData.Add(key, value);
            }
            else
            {
                //Check existing data for type match
                if (userData[key].IsString == false)
                {
                    Debug.LogError($"PlayerData \"{key}\" Datatype changed from {userData[key].Type} to String");
                }

                //Set data
                userData[key] = value;
            }
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetFloat(string key, float value)
        {
            if (userData.ContainsKey(key) == false)
            {
                //Data did not exist - add it
                userData.Add(key, value);
            }
            else
            {
                //Check existing data for type match
                if (userData[key].IsNumber == false)
                {
                    Debug.LogError($"PlayerData \"{key}\" Datatype changed from {userData[key].Type} to Number");
                }

                //Set data
                userData[key] = value;
            }
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetDouble(string key, double value)
        {
            if (userData.ContainsKey(key) == false)
            {
                //Data did not exist - add it
                userData.Add(key, value);
            }
            else
            {
                //Check existing data for type match
                if (userData[key].IsNumber == false)
                {
                    Debug.LogError($"PlayerData \"{key}\" Datatype changed from {userData[key].Type} to Number");
                }

                //Set data
                userData[key] = value;
            }
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetJsonValue(string key, JsonValue value)
        {
            if (userData.ContainsKey(key) == false)
            {
                //Data did not exist - add it
                userData.Add(key, value);
            }
            else
            {
                //Set data
                userData[key] = value;
            }
        }

                /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetJsonArray(string key, JsonArray value)
        {
            if (userData.ContainsKey(key) == false)
            {
                //Data did not exist - add it
                userData.Add(key, value);
            }
            else
            {
                //Set data
                userData[key] = value;
            }
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public int GetInt(string key, int defaultReturn = 0)
        {
            if (userData.ContainsKey(key) && userData[key].IsInteger)
            {
                return userData[key].AsInteger;
            }

            return defaultReturn;
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public bool GetBool(string key, bool defaultReturn = false)
        {
            if (userData.ContainsKey(key))
            {
                if (userData[key].IsBoolean)
                {
                    return userData[key].AsBoolean;
                }
                else if (userData[key].IsInteger)
                {
                    return userData[key].AsInteger != 0;
                }
            }

            return defaultReturn;
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public float GetFloat(string key, float defaultReturn = 0f)
        {
            if (userData.ContainsKey(key) && userData[key].IsNumber)
            {
                return (float)userData[key].AsNumber;
            }

            return defaultReturn;
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public double GetDouble(string key, double defaultReturn = 0.0)
        {
            if (userData.ContainsKey(key) && userData[key].IsNumber)
            {
                return userData[key].AsNumber;
            }

            return defaultReturn;
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public string GetString(string key, string defaultReturn = "")
        {
            if (userData.ContainsKey(key) && userData[key].IsString)
            {
                return userData[key].AsString;
            }

            return defaultReturn;
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public JsonValue GetJsonValue(string key, JsonValue defaultReturn = default(JsonValue))
        {
            if (userData.ContainsKey(key))
            {
                return userData[key];
            }

            return defaultReturn;
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public JsonArray GetJsonArray(string key, JsonArray defaultReturn = default(JsonArray))
        {
            if (userData.ContainsKey(key))
            {
                return userData[key];
            }

            return defaultReturn;
        }

        /// <summary> Get if any value is associated with indicated <paramref name="key"/> </summary>
        public bool HasKey(string key) => userData.ContainsKey(key);

        /// <summary> Remove any value is associated with indicated <paramref name="key"/> </summary>
        public void RemoveKey(string key) => userData.Remove(key);

        /// <summary> Save contents to file </summary>
        public void Serialize()
        {
            //Don't serialize out blank usernames
            if (string.IsNullOrEmpty(UserName))
            {
                Debug.LogError("Tried to serialize out a profile with a blank username.");
                return;
            }

            FileWriter.WriteJson(
                path: PlayerFilePath,
                createJson: () => new JsonObject
                {
                    { "Version", userDataSerializationVersion },
                    { "UserName", UserName },
                    { "LoggingName", loggingName },
                    { "UserDicts", userData }
                },
                pretty: true);
        }

        /// <summary> Load user contents from file </summary>
        public bool Deserialize()
        {
            return FileReader.ReadJsonFile(
                path: PlayerFilePath,
                //If it is parsable, mark it as successfully loaded
                successCallback: (JsonObject readData) =>
                {
                    if (readData.ContainsKey("LoggingName"))
                    {
                        string deserializedLoggingName = readData["LoggingName"];

                        // Only assign logging name if it's not empty. Otherwise, this is an anonymous user and we use their user name
                        loggingName = string.IsNullOrEmpty(deserializedLoggingName) ? UserName : deserializedLoggingName;
                    }
                    else
                    {
                        // Handle backwards compatibility with older profiles that don't have logging names
                        loggingName = UserName;
                    }

                    if (readData.ContainsKey("UserDicts"))
                    {
                        userData = readData["UserDicts"];
                    }
                });
        }

        public virtual void DeletePlayerData()
        {
            if (File.Exists(PlayerFilePath))
            {
                File.Delete(PlayerFilePath);
            }

            Clear();
        }
    }
}
