using BGC.IO;
using BGC.Users;
using BGC.Utility;
using LightJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Android.Gradle;
using UnityEngine;

namespace BGC.Study
{
    public enum ProtocolStatus
    {
        Uninitialized = 0,
        InvalidProtocol,
        SessionReady,
        SessionLimitExceeded,
        SessionElementLimitExceeded,
        SessionFinished,
        Locked,
        StepCompleted
    }

    public struct SequenceTime
    {
        public SequenceType type;
        public int id;
        public DateTime encounteredTime;
        public DateTime completedTime;

        public SequenceTime(SequenceType type, int id, DateTime encounteredTime, DateTime completedTime)
        {
            this.type = type;
            this.id = id;
            this.encounteredTime = encounteredTime;
            this.completedTime = completedTime;
        }

        public SequenceTime(JsonObject json)
        {
            if (json["type"].IsString)
            {
                if (!Enum.TryParse(json["type"].AsString, out type))
                {
                    Debug.LogError($"Unknown SequenceType: {json["type"].AsString}");
                    type = SequenceType.Session;
                }
            }
            else
            {
                type = (SequenceType)json["type"].AsInteger;
            }

            id = json["id"].AsInteger;
            encounteredTime = json["encounteredTime"].AsDateTime ?? DateTime.MinValue;
            completedTime = json["completedTime"].AsDateTime ?? DateTime.MinValue;
        }

        public JsonObject ToJson()
        {
            return new JsonObject
            {
                { "type", type.ToString() },
                { "id", id },
                { "encounteredTime", encounteredTime },
                { "completedTime", completedTime }
            };
        }
    }

    public static class ProtocolManager
    {
        private const string protocolDataDir = "Protocols";
        public const int protocolDataVersion = 3;

        private static string loadedProtocolSet = "";

        public static class DataKeys
        {
            public const string SessionNumber = "SessionNumber";
            public const string ElementNumber = "ElementNumber";
            public const string SessionInProgress = "SessionInProgress";
            public const string SequenceTimes = "SequenceTimes";
            public const string CurrentSequenceStartTime = "CurrentSequenceStartTime";
            public const string SequenceIndex = "SequenceIndex";
        }

        public static Dictionary<string, Protocol> protocolDictionary = new Dictionary<string, Protocol>();
        public static Dictionary<int, Session> sessionDictionary = new Dictionary<int, Session>();
        public static Dictionary<int, SessionElement> sessionElementDictionary =
            new Dictionary<int, SessionElement>();
        public static Dictionary<int, Lockout> lockoutDictionary = new Dictionary<int, Lockout>();
        public static Dictionary<int, LockoutElement> lockoutElementDictionary =
            new Dictionary<int, LockoutElement>();

        public static Protocol currentProtocol = null;
        public static Session currentSession = null;
        public static SessionElement currentSessionElement = null;
        public static Lockout currentLockout = null;

        public static IReadOnlyList<SequenceTime> SequenceTimes
        {
            get
            {
                JsonValue val = PlayerData.GetJsonValue(DataKeys.SequenceTimes);
                if (val.IsJsonArray)
                {
                    List<SequenceTime> times = new();
                    foreach (JsonValue v in val.AsJsonArray)
                    {
                        times.Add(new SequenceTime(v.AsJsonObject));
                    }
                    return times;
                }
                return new List<SequenceTime>();
            }
        }

        public static void AddSequenceTime(SequenceTime sequenceTime)
        {
            JsonValue val = PlayerData.GetJsonValue(DataKeys.SequenceTimes);
            JsonArray arr;
            if (val.IsJsonArray)
            {
                arr = val.AsJsonArray;
            }
            else
            {
                arr = new JsonArray();
            }
            arr.Add(sequenceTime.ToJson());
            PlayerData.SetJsonValue(DataKeys.SequenceTimes, arr);
        }

        public static DateTime CurrentSequenceStartTime
        {
            get => PlayerData.GetJsonValue(DataKeys.CurrentSequenceStartTime).AsDateTime ?? DateTime.MinValue;
            set => PlayerData.SetJsonValue(DataKeys.CurrentSequenceStartTime, value);
        }

        public static int nextSessionElementIndex = -1;

        public static int ElementNumber
        {
            get => PlayerData.GetInt(DataKeys.ElementNumber, 0);
            set => PlayerData.SetInt(DataKeys.ElementNumber, value);
        }

        public static int SessionNumber
        {
            get => PlayerData.GetInt(DataKeys.SessionNumber, 0);
            set => PlayerData.SetInt(DataKeys.SessionNumber, value);
        }

        public static int SequenceIndex
        {
            get => PlayerData.GetInt(DataKeys.SequenceIndex, 0);
            set => PlayerData.SetInt(DataKeys.SequenceIndex, value);
        }

        public static bool SessionInProgress
        {
            get => PlayerData.GetBool(DataKeys.SessionInProgress, false);
            set => PlayerData.SetBool(DataKeys.SessionInProgress, value);
        }

        public delegate void MigrateProtocols(ref JsonObject protocols);
        public delegate void SessionElementOverrun();
        public delegate void PrepareNextElement(bool resuming);
        public delegate SessionElement ParseSessionElement(JsonObject sessionElement);

        private static SessionElementOverrun sessionElementOverrun = null;
        private static PrepareNextElement prepareNextElement = null;
        private static MigrateProtocols migrateProtocols = null;
        private static ParseSessionElement parseSessionElement = null;

        private static int GetSessionOrdinalAtSequenceIndex(int sequenceIndex)
        {
            if (currentProtocol == null || sequenceIndex < 0)
            {
                return 0;
            }

            int sessionCount = 0;
            for (int i = 0; i <= sequenceIndex && i < currentProtocol.sequences.Count; i++)
            {
                if (currentProtocol.sequences[i].type == SequenceType.Session)
                {
                    sessionCount++;
                }
            }

            return Math.Max(0, sessionCount - 1);
        }

        private static void EnsureSequenceIndexMigrated()
        {
            JsonValue sequenceIndexValue = PlayerData.GetJsonValue(DataKeys.SequenceIndex);
            if (sequenceIndexValue.IsInteger)
            {
                SequenceIndex = sequenceIndexValue.AsInteger;
                return;
            }

            int legacySessionIndex = SessionNumber;
            int migratedIndex = GetSequenceIndexForSession(legacySessionIndex);
            SequenceIndex = migratedIndex < 0 ? 0 : migratedIndex;
        }

        private static IProtocolSequenceMember ResolveSequenceMember(SequenceElement sequence)
        {
            return sequence.type switch
            {
                SequenceType.Session => sequence.Session,
                SequenceType.Lockout => sequence.Lockout,
                _ => null
            };
        }

        private static ProtocolStatus SetSequence(int sequenceIndex, int element = 0)
        {
            currentSession = null;
            currentSessionElement = null;
            currentLockout = null;
            nextSessionElementIndex = -1;

            if (loadedProtocolSet == "" || currentProtocol == null)
            {
                return ProtocolStatus.Uninitialized;
            }

            if (sequenceIndex < 0)
            {
                return ProtocolStatus.InvalidProtocol;
            }

            if (sequenceIndex >= currentProtocol.sequences.Count)
            {
                SequenceIndex = currentProtocol.sequences.Count;
                return ProtocolStatus.SessionFinished;
            }

            SequenceIndex = sequenceIndex;

            SequenceElement sequence = currentProtocol.sequences[sequenceIndex];
            IProtocolSequenceMember member = ResolveSequenceMember(sequence);
            if (member == null)
            {
                return ProtocolStatus.InvalidProtocol;
            }

            member.OnEncountered();

            if (sequence.type == SequenceType.Session)
            {
                currentSession = sequence.Session;
                if (currentSession == null)
                {
                    return ProtocolStatus.InvalidProtocol;
                }

                SessionNumber = GetSessionOrdinalAtSequenceIndex(sequenceIndex);

                if (element >= currentSession.Count)
                {
                    return ProtocolStatus.SessionElementLimitExceeded;
                }

                nextSessionElementIndex = element;
                ElementNumber = element;

                if (element == 0)
                {
                    SessionInProgress = false;
                    CurrentSequenceStartTime = DateTime.Now;
                }

                ProtocolStatus status = member.CheckStatus();
                if (status == ProtocolStatus.Locked)
                {
                    return ProtocolStatus.Locked;
                }

                return ProtocolStatus.SessionReady;
            }

            ProtocolStatus lockoutStatus = member.CheckStatus();
            if (lockoutStatus == ProtocolStatus.Locked)
            {
                currentLockout = sequence.Lockout;
                return ProtocolStatus.Locked;
            }

            if (lockoutStatus == ProtocolStatus.StepCompleted)
            {
                member.OnCompleted();
                SequenceIndex = sequenceIndex + 1;
            }

            return ProtocolStatus.StepCompleted;
        }

        public static ProtocolStatus CheckSequenceStatus()
        {
            if (currentProtocol == null)
            {
                return ProtocolStatus.Uninitialized;
            }

            DateTime lockoutRelease = PlayerData.GetJsonValue("Lockout").AsDateTime ?? DateTime.MinValue;
            if (DateTime.Now < lockoutRelease)
            {
                return ProtocolStatus.Locked;
            }

            EnsureSequenceIndexMigrated();

            int seqIndex = SequenceIndex;

            while (seqIndex < currentProtocol.sequences.Count)
            {
                SequenceElement sequence = currentProtocol.sequences[seqIndex];
                IProtocolSequenceMember member = ResolveSequenceMember(sequence);

                if (member == null)
                {
                    return ProtocolStatus.InvalidProtocol;
                }

                member.OnEncountered();
                ProtocolStatus status = member.CheckStatus();

                if (status == ProtocolStatus.Locked)
                {
                    if (sequence.type == SequenceType.Lockout)
                    {
                        currentLockout = sequence.Lockout;
                    }
                    return ProtocolStatus.Locked;
                }

                if (sequence.type == SequenceType.Session && status == ProtocolStatus.SessionReady)
                {
                    currentSession = sequence.Session;
                    if (currentSession == null)
                    {
                        return ProtocolStatus.InvalidProtocol;
                    }

                    SessionNumber = GetSessionOrdinalAtSequenceIndex(seqIndex);

                    nextSessionElementIndex = Math.Min(ElementNumber, currentSession.Count);
                    if (nextSessionElementIndex >= currentSession.Count)
                    {
                        return ProtocolStatus.SessionElementLimitExceeded;
                    }

                    ElementNumber = nextSessionElementIndex;
                    SessionInProgress = false;
                    if (nextSessionElementIndex == 0)
                    {
                        CurrentSequenceStartTime = DateTime.Now;
                    }

                    return ProtocolStatus.SessionReady;
                }

                member.OnCompleted();
                SequenceIndex = seqIndex + 1;
                seqIndex = SequenceIndex;
            }

            return ProtocolStatus.SessionFinished;
        }

        public static ProtocolStatus AdvanceSequence(bool markCompletion = true)
        {
            if (currentProtocol != null && SequenceIndex < currentProtocol.sequences.Count)
            {
                IProtocolSequenceMember member = ResolveSequenceMember(currentProtocol.sequences[SequenceIndex]);
                if (markCompletion)
                {
                    member?.OnCompleted();
                }
            }

            SequenceIndex = SequenceIndex + 1;
            nextSessionElementIndex = -1;
            currentSession = null;
            currentSessionElement = null;
            currentLockout = null;
            SessionInProgress = false;
            ElementNumber = 0;

            return CheckSequenceStatus();
        }

        [Obsolete("Transition to string-based Protocol IDs when convenient")]
        public static void PrepareProtocol(
            string protocolName,
            int protocolID)
        {
            currentProtocol = null;
            currentSession = null;
            currentSessionElement = null;
            nextSessionElementIndex = -1;
            loadedProtocolSet = "";

            LoadProtocolSet(protocolName);
            if (protocolDictionary.ContainsKey(protocolID.ToString()))
            {
                currentProtocol = protocolDictionary[protocolID.ToString()];
            }
            else
            {
                Debug.LogError($"Loaded Protocol \"{loadedProtocolSet}\" does not contain requested protocolID {protocolID}.");
                return;
            }
        }

        public static void PrepareProtocol(
            string protocolSetName,
            string protocolKey)
        {
            currentProtocol = null;
            currentSession = null;
            currentSessionElement = null;
            nextSessionElementIndex = -1;
            loadedProtocolSet = "";

            LoadProtocolSet(protocolSetName);
            if (protocolDictionary.ContainsKey(protocolKey))
            {
                currentProtocol = protocolDictionary[protocolKey];
            }
            else
            {
                Debug.LogError($"Loaded Protocol \"{loadedProtocolSet}\" does not contain requested protocolKey {protocolKey}.");
                return;
            }
        }

        private static int GetSequenceIndexForSession(int sessionNumber)
        {
            if (currentProtocol == null) return -1;
            int sessionCount = 0;
            for (int i = 0; i < currentProtocol.sequences.Count; i++)
            {
                if (currentProtocol.sequences[i].type == SequenceType.Session)
                {
                    if (sessionCount == sessionNumber) return i;
                    sessionCount++;
                }
            }
            return -1;
        }

        public static ProtocolStatus CheckLockoutStatus()
        {
            currentLockout = null;

            // 1. Old Time Lockout
            DateTime lockoutRelease = PlayerData.GetJsonValue("Lockout").AsDateTime ?? DateTime.MinValue;
            if (DateTime.Now < lockoutRelease)
            {
                return ProtocolStatus.Locked;
            }

            // 2. Old Password
            if (currentSession != null)
            {
                string password = currentSession.GetPassword();
                if (!string.IsNullOrEmpty(password))
                {
                    return ProtocolStatus.Locked;
                }
            }

            // 3. New Lockouts
            int currentSequenceIndex = GetSequenceIndexForSession(SessionNumber);
            // If we can't find the session, we might be at the end or uninitialized.
            // If SessionNumber is valid (checked elsewhere), this should be valid.
            // If SessionNumber >= SessionCount, we are finished.
            if (currentSequenceIndex == -1)
            {
                // If SessionNumber is valid but not found, it might be because we are past the last session?
                // But GetSequenceIndexForSession iterates all sequences.
                // If SessionNumber is valid, it should be found.
                // Unless SessionNumber is out of bounds.
                // TryUpdateProtocol checks bounds.
                return ProtocolStatus.Uninitialized;
            }

            for (int i = currentSequenceIndex - 1; i >= 0; i--)
            {
                if (currentProtocol.sequences[i].type == SequenceType.Session) break;
                
                if (currentProtocol.sequences[i].type == SequenceType.Lockout)
                {
                    Lockout lockout = currentProtocol.sequences[i].Lockout;
                    if (lockout != null)
                    {
                        foreach (LockoutElementID elementId in lockout)
                        {
                            LockoutElement element = elementId.Element;
                            if (element == null)
                            {
                                Debug.LogError($"Lockout element is null for ID: {elementId.id}");
                                continue;
                            }

                            if (element.CheckLockout(DateTime.Now, SequenceTimes))
                            {
                                currentLockout = lockout;
                                return ProtocolStatus.Locked;
                            }
                        }
                    }
                }
            }

            return ProtocolStatus.SessionReady;
        }

        [Obsolete("Transition to string-based Protocol IDs when convenient")]
        public static ProtocolStatus TryUpdateProtocol(
            string protocolName,
            int protocolID,
            int sessionIndex,
            int sessionElementIndex = 0)
        {
            if (LoadProtocolSet(protocolName))
            {
                if (protocolDictionary.ContainsKey(protocolID.ToString()))
                {
                    currentProtocol = protocolDictionary[protocolID.ToString()];
                    ElementNumber = sessionElementIndex;

                    int seqIndex = GetSequenceIndexForSession(sessionIndex);
                    if (seqIndex < 0)
                    {
                        return ProtocolStatus.SessionLimitExceeded;
                    }

                    SequenceIndex = seqIndex;
                    return CheckSequenceStatus();
                }
                else
                {
                    Debug.LogError($"Loaded Protocol \"{loadedProtocolSet}\" does not contain requested protocolID {protocolID}.");
                    return ProtocolStatus.InvalidProtocol;
                }
            }

            return ProtocolStatus.Uninitialized;
        }

        public static ProtocolStatus TryUpdateProtocol(
            string protocolSetName,
            string protocolKey,
            int sessionIndex,
            int sessionElementIndex = 0)
        {
            if (LoadProtocolSet(protocolSetName))
            {
                if (protocolDictionary.ContainsKey(protocolKey))
                {
                    currentProtocol = protocolDictionary[protocolKey];
                    ElementNumber = sessionElementIndex;

                    int seqIndex = GetSequenceIndexForSession(sessionIndex);
                    if (seqIndex < 0)
                    {
                        return ProtocolStatus.SessionLimitExceeded;
                    }

                    SequenceIndex = seqIndex;
                    return CheckSequenceStatus();
                }
                else
                {
                    Debug.LogError($"Loaded Protocol \"{loadedProtocolSet}\" does not contain requested protocolKey {protocolKey}.");
                    return ProtocolStatus.InvalidProtocol;
                }
            }

            return ProtocolStatus.Uninitialized;
        }

        public static ProtocolStatus SetSession(int session, int element = 0)
        {
            int sequenceIndex = GetSequenceIndexForSession(session);
            if (sequenceIndex < 0)
            {
                return ProtocolStatus.SessionLimitExceeded;
            }

            SequenceIndex = sequenceIndex;
            ElementNumber = element;
            return SetSequence(sequenceIndex, element);
        }

        public static JsonValue GetEnvValue(string key, JsonValue defaultReturn = default(JsonValue))
        {
            if (loadedProtocolSet == "" ||
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

        public static async Task<ProtocolStatus> ExecuteNextElement(bool resuming = false)
        {
            if (nextSessionElementIndex == -1)
            {
                return ProtocolStatus.Uninitialized;
            }

            if (currentSession == null)
            {
                return ProtocolStatus.Uninitialized;
            }

            if (nextSessionElementIndex == currentSession.Count)
            {
                SessionInProgress = false;
                sessionElementOverrun?.Invoke();

                ProtocolStatus status = AdvanceSequence();

                PlayerData.Save();

                return status;
            }

            //This only happens when the last element was running
            currentSessionElement?.CleanupElement();

            currentSessionElement = currentSession[nextSessionElementIndex];

            ElementNumber = nextSessionElementIndex;
            ++nextSessionElementIndex;
            SessionInProgress = true;

            prepareNextElement?.Invoke(resuming);

            await currentSessionElement.ExecuteElement(resuming);

            if (nextSessionElementIndex == -1)
            {
                PlayerData.Save();
                return CheckSequenceStatus();
            }

            PlayerData.Save();

            return ProtocolStatus.SessionReady;
        }

        public static void SaveAs(string protocolName)
        {
            loadedProtocolSet = protocolName;
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

            Dictionary<int, int> lockoutElementRemapping = new Dictionary<int, int>();
            Dictionary<string, int> lockoutElements = new Dictionary<string, int>();

            foreach (KeyValuePair<int, LockoutElement> lockoutElement in lockoutElementDictionary)
            {
                JsonObject serializedElement = lockoutElement.Value.SerializeElement();
                serializedElement.Remove(ProtocolKeys.LockoutElement.Id);

                string serialization = serializedElement.ToString();
                if (lockoutElements.ContainsKey(serialization))
                {
                    //Collision - Add it to be remapped
                    int newID = lockoutElements[serialization];
                    lockoutElementRemapping.Add(lockoutElement.Key, newID);
                }
                else
                {
                    //It's a different lockoutElement - Add it to the dictionary
                    lockoutElements.Add(serialization, lockoutElement.Key);
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
                serializedSession.Remove(Session.Keys.Id);

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

            Dictionary<int, int> lockoutRemapping = new Dictionary<int, int>();
            Dictionary<string, int> lockouts = new Dictionary<string, int>();

            foreach (KeyValuePair<int, Lockout> lockout in lockoutDictionary)
            {
                //Apply Remapping To Lockouts
                List<LockoutElementID> elementIDs = lockout.Value.lockoutElements;

                for (int i = 0; i < elementIDs.Count; i++)
                {
                    if (lockoutElementRemapping.ContainsKey(elementIDs[i].id))
                    {
                        elementIDs[i] = new LockoutElementID(lockoutElementRemapping[elementIDs[i].id]);
                    }
                }

                JsonObject serializedLockout = lockout.Value.SerializeLockout();
                serializedLockout.Remove(Lockout.Keys.Id);

                string serialization = serializedLockout.ToString();
                if (lockouts.ContainsKey(serialization))
                {
                    //Collision - Add it to be remapped
                    int newID = lockouts[serialization];
                    lockoutRemapping.Add(lockout.Key, newID);
                }
                else
                {
                    //It's a different lockout - Add it to the dictionary
                    lockouts.Add(serialization, lockout.Key);
                }
            }

            foreach (Protocol protocol in protocolDictionary.Values)
            {
                //Apply Remapping To Protocols
                List<SequenceElement> sequences = protocol.sequences;

                for (int i = 0; i < sequences.Count; i++)
                {
                    if (sequences[i].type == SequenceType.Session && sessionRemapping.ContainsKey(sequences[i].id))
                    {
                        sequences[i] = new SequenceElement(sessionRemapping[sequences[i].id], SequenceType.Session);
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

            //Remove eliminated LockoutElements
            foreach (int lockoutElementID in lockoutElementRemapping.Keys)
            {
                lockoutElementDictionary.Remove(lockoutElementID);
            }

            //Remove eliminated Lockouts
            foreach (int lockoutID in lockoutRemapping.Keys)
            {
                lockoutDictionary.Remove(lockoutID);
            }
        }

        public static void SerializeAll()
        {
            if (loadedProtocolSet == "")
            {
                Debug.LogError("Can't serialize protocol - none loaded");
                return;
            }

            FileWriter.WriteJson(
                path: Path.Combine(DataManagement.PathForDataDirectory(protocolDataDir), $"{loadedProtocolSet}.json"),
                createJson: () => new JsonObject()
                {
                    { ProtocolKeys.Version, protocolDataVersion },
                    { ProtocolKeys.Protocols, SerializeProtocols() },
                    { ProtocolKeys.Sessions, SerializeSessions() },
                    { ProtocolKeys.Lockouts, SerializeLockouts() },
                    { ProtocolKeys.SessionElements, SerializeSessionElements() },
                    { ProtocolKeys.LockoutElements, SerializeLockoutElements() }
                },
                pretty: false);
        }

        public static bool LoadProtocolSet(string protocolSet)
        {
            if (protocolSet == "")
            {
                protocolSet = "DefaultSet";
            }

            if (loadedProtocolSet == protocolSet)
            {
                return true;
            }

            string previousLoadedProtocol = loadedProtocolSet;
            loadedProtocolSet = protocolSet;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                loadedProtocolSet = loadedProtocolSet.Replace(c.ToString(), "");
            }

            return FileReader.ReadJsonFile(
                path: Path.Combine(DataManagement.PathForDataDirectory(protocolDataDir), $"{loadedProtocolSet}.json"),
                successCallback: (JsonObject jsonProtocols) =>
                {
                    migrateProtocols?.Invoke(ref jsonProtocols);

                    DeserializeProtocols(jsonProtocols[ProtocolKeys.Protocols]);
                    DeserializeSessions(jsonProtocols[ProtocolKeys.Sessions]);
                    if (jsonProtocols.ContainsKey(ProtocolKeys.Lockouts))
                    {
                        DeserializeLockouts(jsonProtocols[ProtocolKeys.Lockouts]);
                    }
                    DeserializeSessionElements(jsonProtocols[ProtocolKeys.SessionElements]);
                    if (jsonProtocols.ContainsKey(ProtocolKeys.LockoutElements))
                    {
                        DeserializeLockoutElements(jsonProtocols[ProtocolKeys.LockoutElements]);
                    }
                },
                failCallback: () =>
                {
                    loadedProtocolSet = "";
                    protocolDictionary.Clear();
                    sessionDictionary.Clear();
                    sessionElementDictionary.Clear();
                    lockoutDictionary.Clear();
                    lockoutElementDictionary.Clear();
                },
                fileNotFoundCallback: () => loadedProtocolSet = previousLoadedProtocol);
        }

        public static JsonObject SerializeProtocols()
        {
            JsonObject protocols = new JsonObject();

            foreach (var protocolKVP in protocolDictionary)
            {
                protocols.Add(protocolKVP.Key, protocolKVP.Value.ToJson());
            }

            return protocols;
        }

        public static void DeserializeProtocols(JsonObject protocols)
        {
            protocolDictionary.Clear();

            foreach (var protocolKVP in protocols)
            {
                protocolDictionary.Add(
                    protocolKVP.Key,
                    new Protocol(protocolKVP.Value.AsJsonObject, protocolKVP.Key));
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

        public static JsonArray SerializeLockouts()
        {
            JsonArray lockouts = new JsonArray();

            foreach (Lockout lockout in lockoutDictionary.Values)
            {
                lockouts.Add(lockout.SerializeLockout());
            }

            return lockouts;
        }

        public static void DeserializeLockouts(JsonArray lockouts)
        {
            lockoutDictionary.Clear();

            foreach (JsonObject lockoutData in lockouts)
            {
                Lockout lockout = new Lockout(lockoutData);

                lockoutDictionary.Add(lockout.id, lockout);
            }
        }

        public static JsonArray SerializeLockoutElements()
        {
            JsonArray lockoutElements = new JsonArray();

            foreach (LockoutElement element in lockoutElementDictionary.Values)
            {
                lockoutElements.Add(element.SerializeElement());
            }

            return lockoutElements;
        }

        public delegate LockoutElement ParseLockoutElement(JsonObject lockoutElement);
        private static ParseLockoutElement parseLockoutElement = null;

        public static void RegisterLockoutElementParser(ParseLockoutElement parseLockoutElement)
        {
            ProtocolManager.parseLockoutElement = parseLockoutElement;
        }

        public static void DeserializeLockoutElements(JsonArray elements)
        {
            lockoutElementDictionary.Clear();

            foreach (JsonObject element in elements)
            {
                LockoutElement parsedElement = parseLockoutElement?.Invoke(element);

                if (parsedElement == null)
                {
                    string type = element[ProtocolKeys.LockoutElement.Type].AsString;
                    switch (type)
                    {
                        case "FixedTime":
                            parsedElement = new FixedTimeLockout(element);
                            break;
                        case "Password":
                            parsedElement = new PasswordLockout(element);
                            break;
                        case "Window":
                            parsedElement = new WindowLockout(element);
                            break;
                        default:
                            Debug.LogError($"Unknown LockoutElement Type: {type}");
                            break;
                    }
                }

                if (parsedElement == null)
                {
                    Debug.LogError($"Failed to parse LockoutElement: {element.ToString()}");
                    continue;
                }

                lockoutElementDictionary.Add(parsedElement.id, parsedElement);
            }
        }

        [Obsolete("Transition to string-based Protocol IDs")]
        public static string GetProtocolName(int protocolID) => GetProtocolName(protocolID.ToString());

        public static string GetProtocolName(string protocolKey)
        {
            if (protocolDictionary.ContainsKey(protocolKey))
            {
                return protocolDictionary[protocolKey].name;
            }

            return "Invalid";
        }

        public static void HardClearAll()
        {
            loadedProtocolSet = "";
            protocolDictionary.Clear();
            sessionDictionary.Clear();
            sessionElementDictionary.Clear();
            lockoutDictionary.Clear();
            lockoutElementDictionary.Clear();

            Protocol.HardClear();
            Session.HardClear();
            SessionElement.HardClear();
            Lockout.HardClear();
            LockoutElement.HardClear();
        }

        public static void DeleteProtocol(string protocolSetName)
        {
            string path = Path.Combine(DataManagement.PathForDataDirectory(protocolDataDir, false), $"{protocolSetName}.json");
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
