using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LightJson;
using BGC.IO;
using BGC.Users;
using BGC.Utility;

namespace BGC.Study
{
    public enum ProtocolStatus
    {
        Uninitialized = 0,
        InvalidProtocol,
        SessionReady,
        SessionLimitExceeded,
        SessionElementLimitExceeded,
        SessionFinished
    }

    public static class ProtocolManager
    {
        private const string protocolDataDir = "Protocols";
        public const int protocolDataVersion = 2;

        private static string loadedProtocol = "";

        public static class DataKeys
        {
            public const string SessionNumber = "SessionNumber";
            public const string ElementNumber = "ElementNumber";
            public const string SessionInProgress = "SessionInProgress";
        }

        public static Dictionary<int, Protocol> protocolDictionary = new Dictionary<int, Protocol>();
        public static Dictionary<int, Session> sessionDictionary = new Dictionary<int, Session>();
        public static Dictionary<int, SessionElement> sessionElementDictionary =
            new Dictionary<int, SessionElement>();

        public static Protocol currentProtocol = null;
        public static Session currentSession = null;
        public static SessionElement currentSessionElement = null;

        public static int nextSessionElementIndex = -1;

        public static int ElementNumber
        {
            get { return PlayerData.GetInt(DataKeys.ElementNumber, 0); }
            set { PlayerData.SetInt(DataKeys.ElementNumber, value); }
        }

        public static int SessionNumber
        {
            get { return PlayerData.GetInt(DataKeys.SessionNumber, 0); }
            set { PlayerData.SetInt(DataKeys.SessionNumber, value); }
        }

        public static bool SessionInProgress
        {
            get { return PlayerData.GetBool(DataKeys.SessionInProgress, false); }
            set { PlayerData.SetBool(DataKeys.SessionInProgress, value); }
        }

        public delegate void MigrateProtocols(ref JsonObject protocols);
        public delegate void SessionElementOverrun();
        public delegate void PrepareNextElement(bool resuming);
        public delegate SessionElement ParseSessionElement(JsonObject sessionElement);

        private static SessionElementOverrun sessionElementOverrun = null;
        private static PrepareNextElement prepareNextElement = null;
        private static MigrateProtocols migrateProtocols = null;
        private static ParseSessionElement parseSessionElement = null;

        public static void PrepareProtocol(string protocolName, int protocolID)
        {
            currentProtocol = null;
            currentSession = null;
            currentSessionElement = null;
            nextSessionElementIndex = -1;
            loadedProtocol = "";

            LoadProtocolSet(protocolName);
            if (protocolDictionary.ContainsKey(protocolID))
            {
                currentProtocol = protocolDictionary[protocolID];
            }
            else
            {
                Debug.LogError($"Loaded Protocol \"{loadedProtocol}\" does not contain requested protocolID {protocolID}.");
                return;
            }
        }

        public static ProtocolStatus TryUpdateProtocol(
            string protocolName,
            int protocolID,
            int sessionIndex,
            int sessionElementIndex = 0)
        {
            if (LoadProtocolSet(protocolName))
            {
                if (protocolDictionary.ContainsKey(protocolID))
                {
                    currentProtocol = protocolDictionary[protocolID];

                    return SetSession(sessionIndex, sessionElementIndex);
                }
                else
                {
                    Debug.LogError($"Loaded Protocol \"{loadedProtocol}\" does not contain requested protocolID {protocolID}.");
                    return ProtocolStatus.InvalidProtocol;
                }
            }

            return ProtocolStatus.Uninitialized;
        }

        public static ProtocolStatus SetSession(int session, int element = 0)
        {
            currentSession = null;
            currentSessionElement = null;
            nextSessionElementIndex = -1;

            if (loadedProtocol == "")
            {
                return ProtocolStatus.Uninitialized;
            }

            if (!protocolDictionary.ContainsKey(currentProtocol.id))
            {
                return ProtocolStatus.InvalidProtocol;
            }

            if (session >= currentProtocol.Count)
            {
                return ProtocolStatus.SessionLimitExceeded;
            }

            SessionNumber = session;
            currentSession = currentProtocol[session];

            if (element >= currentSession.Count)
            {
                return ProtocolStatus.SessionElementLimitExceeded;
            }

            //The next session element to run will be the current one
            nextSessionElementIndex = element;

            return ProtocolStatus.SessionReady;
        }

        public static JsonValue GetEnvValue(string key, JsonValue defaultReturn = default(JsonValue))
        {
            if (loadedProtocol == "" ||
                currentProtocol == null || currentSession == null || currentSessionElement == null)
            {
                Debug.Log($"EnvVal not ready for query: {key}");
                return defaultReturn;
            }

            if (currentSessionElement.envVals.ContainsKey(key))
            {
                return currentSessionElement.envVals[key];
            }

            if (currentSession.envVals.ContainsKey(key))
            {
                return currentSession.envVals[key];
            }

            if (currentProtocol.envVals.ContainsKey(key))
            {
                return currentProtocol.envVals[key];
            }

            Debug.Log($"EnvVal not found: {key}");
            return defaultReturn;
        }

        public static bool GetEnvBool(string key, bool defaultValue = false)
        {
            JsonValue val = GetEnvValue(key, defaultValue);

            if (val.IsBoolean)
            {
                return val.AsBoolean;
            }

            if (val.IsInteger)
            {
                Debug.LogWarning($"EnvVal type mismatch: Found {val.Type}, Expected: {defaultValue.GetType().Name}.  Converting.");
                return val.AsInteger != 0;
            }

            Debug.LogError($"EnvVal type mismatch: Found {val.Type}, Expected: {defaultValue.GetType().Name}");
            return defaultValue;
        }

        public static int GetEnvInt(string key, int defaultValue = 0)
        {
            JsonValue val = GetEnvValue(key, defaultValue);

            if (val.IsInteger)
            {
                return val.AsInteger;
            }

            Debug.LogError($"EnvVal type mismatch: Found {val.Type}, Expected: {defaultValue.GetType().Name}");
            return defaultValue;
        }

        public static string GetEnvStr(string key, string defaultValue = "")
        {
            JsonValue val = GetEnvValue(key, defaultValue);

            if (val.IsString)
            {
                return val.AsString;
            }

            Debug.LogError($"EnvVal type mismatch: Found {val.Type}, Expected: {defaultValue.GetType().Name}");
            return defaultValue;
        }

        public static float GetEnvFloat(string key, float defaultValue = 0f)
        {
            JsonValue val = GetEnvValue(key, defaultValue);

            if (val.IsNumber)
            {
                return (float)val.AsNumber;
            }

            Debug.LogError($"EnvVal type mismatch: Found {val.Type}, Expected: {defaultValue.GetType().Name}");
            return defaultValue;
        }

        public static void RegisterSpecializers(
            SessionElementOverrun sessionElementOverrun,
            PrepareNextElement prepareNextElement,
            ParseSessionElement parseSessionElement,
            MigrateProtocols migrateProtocols)
        {
            ProtocolManager.sessionElementOverrun = sessionElementOverrun;
            ProtocolManager.prepareNextElement = prepareNextElement;
            ProtocolManager.parseSessionElement = parseSessionElement;
            ProtocolManager.migrateProtocols = migrateProtocols;
        }

        public static ProtocolStatus ExecuteNextElement(bool resuming = false)
        {
            if (nextSessionElementIndex == -1)
            {
                return ProtocolStatus.Uninitialized;
            }

            if (nextSessionElementIndex == currentSession.Count)
            {
                //In principle, this should only happen if an EndSessionElement was forgotten
                ElementNumber = 0;
                SessionInProgress = false;
                ++SessionNumber;

                sessionElementOverrun?.Invoke();

                PlayerData.Save();

                return ProtocolStatus.SessionFinished;
            }

            //This only happens when the last element was running
            currentSessionElement?.CleanupElement();

            currentSessionElement = currentSession[nextSessionElementIndex];

            ElementNumber = nextSessionElementIndex;
            SessionInProgress = true;

            prepareNextElement?.Invoke(resuming);

            currentSessionElement.ExecuteElement(resuming);

            if (nextSessionElementIndex + 1 == currentSession.Count)
            {
                double lastCompleteSessionTime = PlayerData.GetDouble("LastCompleteSessionTime", -1d);

                if (lastCompleteSessionTime != -1d)
                {
                    PlayerData.SetDouble("SecondToLastCompleteSessionTime", lastCompleteSessionTime);
                }

                PlayerData.SetDouble("LastCompleteSessionTime", Epoch.Time);
            }

            PlayerData.Save();

            ++nextSessionElementIndex;

            return ProtocolStatus.SessionReady;
        }

        public static void SaveAs(string protocolName)
        {
            loadedProtocol = protocolName;
            RemoveRedundancies();
            SerializeAll();
        }

        private static void RemoveRedundancies()
        {
            Dictionary<int, int> sessionElementRemapping = new Dictionary<int, int>();
            Dictionary<string, int> sessionElements = new Dictionary<string, int>();

            foreach (KeyValuePair<int, SessionElement> sessionElement in sessionElementDictionary)
            {
                JsonObject serializedElement = sessionElement.Value.SerializeElement();
                serializedElement.Remove(ProtocolKeys.SessionElement.Id);

                string serialization = serializedElement.ToString();
                if (sessionElements.ContainsKey(serialization))
                {
                    //Collision - Add it to be remapped
                    int newID = sessionElements[serialization];
                    sessionElementRemapping.Add(sessionElement.Key, newID);
                }
                else
                {
                    //It's a different sessionElement - Add it to the dictionary
                    sessionElements.Add(serialization, sessionElement.Key);
                }
            }

            Dictionary<int, int> sessionRemapping = new Dictionary<int, int>();
            Dictionary<string, int> sessions = new Dictionary<string, int>();

            foreach (KeyValuePair<int, Session> session in sessionDictionary)
            {
                //Apply Remapping To Sessions
                List<SessionElementID> elementIDs = session.Value.sessionElements;

                for (int i = 0; i < elementIDs.Count; i++)
                {
                    if (sessionElementRemapping.ContainsKey(elementIDs[i].id))
                    {
                        elementIDs[i] = new SessionElementID(sessionElementRemapping[elementIDs[i].id]);
                    }
                }

                JsonObject serializedSession = session.Value.SerializeSession();
                serializedSession.Remove(ProtocolKeys.Session.Id);

                string serialization = serializedSession.ToString();
                if (sessions.ContainsKey(serialization))
                {
                    //Collision - Add it to be remapped
                    int newID = sessions[serialization];
                    sessionRemapping.Add(session.Key, newID);
                }
                else
                {
                    //It's a different session - Add it to the dictionary
                    sessions.Add(serialization, session.Key);
                }
            }

            foreach (KeyValuePair<int, Protocol> protocol in protocolDictionary)
            {
                //Apply Remapping To Protocols
                List<SessionID> sessionIDs = protocol.Value.sessions;

                for (int i = 0; i < sessionIDs.Count; i++)
                {
                    if (sessionRemapping.ContainsKey(sessionIDs[i].id))
                    {
                        sessionIDs[i] = new SessionID(sessionRemapping[sessionIDs[i].id]);
                    }
                }
            }

            //Remove eliminated SessionElements
            foreach (int sessionElementID in sessionElementRemapping.Keys)
            {
                sessionElementDictionary.Remove(sessionElementID);
            }

            //Remove eliminated Sessions
            foreach (int sessionID in sessionRemapping.Keys)
            {
                sessionDictionary.Remove(sessionID);
            }
        }

        public static void SerializeAll()
        {
            if (loadedProtocol == "")
            {
                Debug.LogError("Can't serialize protocol - none loaded");
                return;
            }

            FileWriter.WriteJson(
                path: Path.Combine(DataManagement.PathForDataDirectory(protocolDataDir), $"{loadedProtocol}.json"),
                createJson: () => new JsonObject()
                {
                    { ProtocolKeys.Version, protocolDataVersion },
                    { ProtocolKeys.Protocols, SerializeProtocols() },
                    { ProtocolKeys.Sessions, SerializeSessions() },
                    { ProtocolKeys.SessionElements, SerializeSessionElements() }
                },
                pretty: false);
        }

        public static bool LoadProtocolSet(string protocolName)
        {
            if (protocolName == "")
            {
                protocolName = "DefaultSet";
            }

            if (loadedProtocol == protocolName)
            {
                return true;
            }

            string previousLoadedProtocol = loadedProtocol;
            loadedProtocol = protocolName;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                loadedProtocol = loadedProtocol.Replace(c.ToString(), "");
            }

            return FileReader.ReadJsonFile(
                path: Path.Combine(DataManagement.PathForDataDirectory(protocolDataDir), $"{loadedProtocol}.json"),
                successCallback: (JsonObject jsonProtocols) =>
                {
                    migrateProtocols?.Invoke(ref jsonProtocols);

                    DeserializeProtocols(jsonProtocols[ProtocolKeys.Protocols]);
                    DeserializeSessions(jsonProtocols[ProtocolKeys.Sessions]);
                    DeserializeSessionElements(jsonProtocols[ProtocolKeys.SessionElements]);
                },
                failCallback: () =>
                {
                    loadedProtocol = "";
                    protocolDictionary.Clear();
                    sessionDictionary.Clear();
                    sessionElementDictionary.Clear();
                },
                fileNotFoundCallback: () => loadedProtocol = previousLoadedProtocol);
        }

        public static JsonArray SerializeProtocols()
        {
            JsonArray protocols = new JsonArray();

            foreach (Protocol protocol in protocolDictionary.Values)
            {
                JsonArray jsonSessionIDs = new JsonArray();
                foreach (SessionID sessionID in protocol.sessions)
                {
                    jsonSessionIDs.Add(sessionID.id);
                }

                JsonObject newProtocol = new JsonObject()
                {
                    { ProtocolKeys.Protocol.Id, protocol.id },
                    { ProtocolKeys.Protocol.Name, protocol.name },
                    { ProtocolKeys.Protocol.SessionIDs, jsonSessionIDs }
                };

                if (protocol.envVals.Count > 0)
                {
                    newProtocol.Add(ProtocolKeys.Protocol.EnvironmentValues, protocol.envVals);
                }

                protocols.Add(newProtocol);
            }

            return protocols;
        }

        public static void DeserializeProtocols(JsonArray protocols)
        {
            protocolDictionary.Clear();

            foreach (JsonObject protocol in protocols)
            {
                List<SessionID> sessions = new List<SessionID>();
                foreach (int sessionID in protocol[ProtocolKeys.Protocol.SessionIDs].AsJsonArray)
                {
                    sessions.Add(sessionID);
                }

                JsonObject envVals;
                if (protocol.ContainsKey(ProtocolKeys.Protocol.EnvironmentValues))
                {
                    envVals = protocol[ProtocolKeys.Protocol.EnvironmentValues].AsJsonObject;
                }
                else
                {
                    envVals = new JsonObject();
                }

                protocolDictionary.Add(
                    protocol[ProtocolKeys.Protocol.Id],
                    new Protocol(protocol[ProtocolKeys.Protocol.Id].AsInteger)
                    {
                        name = protocol[ProtocolKeys.Protocol.Name],
                        sessions = sessions,
                        envVals = envVals
                    });
            }
        }

        public static JsonArray SerializeSessions()
        {
            JsonArray sessions = new JsonArray();

            foreach (Session session in sessionDictionary.Values)
            {
                sessions.Add(session.SerializeSession());
            }

            return sessions;
        }

        public static void DeserializeSessions(JsonArray sessions)
        {
            sessionDictionary.Clear();

            foreach (JsonObject sessionData in sessions)
            {
                Session session = new Session(sessionData);

                sessionDictionary.Add(session.id, session);
            }
        }

        public static JsonArray SerializeSessionElements()
        {
            JsonArray sessions = new JsonArray();

            foreach (SessionElement element in sessionElementDictionary.Values)
            {
                sessions.Add(element.SerializeElement());
            }

            return sessions;
        }

        public static void DeserializeSessionElements(JsonArray elements)
        {
            sessionElementDictionary.Clear();

            foreach (JsonObject element in elements)
            {
                SessionElement parsedElement = parseSessionElement?.Invoke(element);

                if (parsedElement == null)
                {
                    Debug.LogError($"Failed to parse SessionElement: {element.ToString()}");
                    continue;
                }

                sessionElementDictionary.Add(parsedElement.id, parsedElement);
            }
        }

        public static void UpdateDefaults()
        {
            string path = DataManagement.PathForDataDirectory(protocolDataDir);
            TextAsset[] assets = Resources.LoadAll<TextAsset>(protocolDataDir);
            foreach (TextAsset asset in assets)
            {
                string currentFile = Path.Combine(path, FileExtensions.AddJsonExtension(asset.name));

                File.WriteAllText(currentFile, asset.text);
            }
        }

        public static string GetProtocolName(int protocolID)
        {
            if (protocolDictionary.ContainsKey(protocolID))
            {
                return protocolDictionary[protocolID].name;
            }

            return "Invalid";
        }

        public static void HardClearAll()
        {
            loadedProtocol = "";
            protocolDictionary.Clear();
            sessionDictionary.Clear();
            sessionElementDictionary.Clear();

            Protocol.HardClear();
            Session.HardClear();
            SessionElement.HardClear();
        }
    }
}
