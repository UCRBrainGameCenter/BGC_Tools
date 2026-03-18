using BGC.IO;
using BGC.Users;
using LightJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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

        /// <summary>
        /// Legacy flat PlayerData key names. Retained for migration from older data formats.
        /// New per-track state is stored in nested JSON under ProtocolTrack.TracksDataKey.
        /// </summary>
        public static class DataKeys
        {
            public const string SessionNumber = "SessionNumber";
            public const string ElementNumber = "ElementNumber";
            public const string SessionInProgress = "SessionInProgress";
            public const string SequenceTimes = "SequenceTimes";
            public const string CurrentSequenceStartTime = "CurrentSequenceStartTime";
            public const string SequenceIndex = "SequenceIndex";
            public const string LockoutExpiration = "LockoutExpiration";
            public const string LockoutHasBypassPassword = "LockoutHasBypassPassword";
            public const string LastEncounteredSequenceIndex = "LastEncounteredSequenceIndex";

            public const string ExtensionState = "ProtocolManager.ExtensionState";
        }

        public static Dictionary<string, Protocol> protocolDictionary = new Dictionary<string, Protocol>();
        public static Dictionary<int, Session> sessionDictionary = new Dictionary<int, Session>();
        public static Dictionary<int, SessionElement> sessionElementDictionary =
            new Dictionary<int, SessionElement>();
        public static Dictionary<int, Lockout> lockoutDictionary = new Dictionary<int, Lockout>();
        public static Dictionary<int, LockoutElement> lockoutElementDictionary =
            new Dictionary<int, LockoutElement>();

        #region Track Infrastructure

        private static Dictionary<string, ProtocolTrack> tracks = new Dictionary<string, ProtocolTrack>();
        private static string activeTrackKey = null;

        /// <summary>
        /// The key of the currently active track. Null when no track is loaded.
        /// </summary>
        public static string ActiveTrackKey => activeTrackKey;

        /// <summary>
        /// The currently active track. Null when no track is loaded.
        /// </summary>
        public static ProtocolTrack ActiveTrack =>
            activeTrackKey != null && tracks.TryGetValue(activeTrackKey, out ProtocolTrack track)
                ? track
                : null;

        /// <summary>
        /// All loaded tracks. Empty when no protocol is loaded.
        /// </summary>
        public static IReadOnlyDictionary<string, ProtocolTrack> Tracks => tracks;

        /// <summary>
        /// True when multiple protocol tracks are loaded in parallel.
        /// </summary>
        public static bool IsParallelProtocol => tracks.Count > 1;

        /// <summary>
        /// Switches the active track. The new track key must already exist in the tracks dictionary.
        /// </summary>
        public static void SetActiveTrack(string trackKey)
        {
            if (!tracks.ContainsKey(trackKey))
            {
                Debug.LogError($"SetActiveTrack: Track \"{trackKey}\" does not exist.");
                return;
            }

            activeTrackKey = trackKey;
        }

        /// <summary>
        /// Sets up a single track for the given protocol key, creating it if it doesn't exist.
        /// Migrates legacy flat PlayerData keys on first use.
        /// </summary>
        private static void EnsureSingleTrack(string protocolKey)
        {
            if (!tracks.ContainsKey(protocolKey))
            {
                tracks.Clear();
                ProtocolTrack track = new ProtocolTrack(protocolKey);
                track.MigrateFromFlatKeys();
                tracks[protocolKey] = track;
            }

            activeTrackKey = protocolKey;
        }

        #endregion

        #region Delegated Properties — runtime state

        public static Protocol currentProtocol
        {
            get => ActiveTrack?.CurrentProtocol;
            set { if (ActiveTrack != null) ActiveTrack.CurrentProtocol = value; }
        }

        public static Session currentSession
        {
            get => ActiveTrack?.CurrentSession;
            set { if (ActiveTrack != null) ActiveTrack.CurrentSession = value; }
        }

        public static SessionElement currentSessionElement
        {
            get => ActiveTrack?.CurrentSessionElement;
            set { if (ActiveTrack != null) ActiveTrack.CurrentSessionElement = value; }
        }

        public static Lockout currentLockout
        {
            get => ActiveTrack?.CurrentLockout;
            set { if (ActiveTrack != null) ActiveTrack.CurrentLockout = value; }
        }

        public static int nextSessionElementIndex
        {
            get => ActiveTrack?.NextSessionElementIndex ?? -1;
            set { if (ActiveTrack != null) ActiveTrack.NextSessionElementIndex = value; }
        }

        /// <summary>
        /// Set to true by CheckSequenceStatus() when it detects that SessionInProgress
        /// was already true on disk — indicating someone force-closed during an active session.
        /// Consumers can check this to log a force-close interruption event.
        /// Reset to false after being read.
        /// </summary>
        public static bool WasForceCloseInterrupted
        {
            get => ActiveTrack?.WasForceCloseInterrupted ?? false;
            set { if (ActiveTrack != null) ActiveTrack.WasForceCloseInterrupted = value; }
        }

        #endregion

        #region Delegated Properties — persisted per-track state

        public static IReadOnlyList<SequenceTime> SequenceTimes =>
            ActiveTrack?.SequenceTimes ?? (IReadOnlyList<SequenceTime>)new List<SequenceTime>();

        public static void AddSequenceTime(SequenceTime sequenceTime) =>
            ActiveTrack?.AddSequenceTime(sequenceTime);

        public static DateTime CurrentSequenceStartTime
        {
            get => ActiveTrack?.CurrentSequenceStartTime ?? DateTime.MinValue;
            set { if (ActiveTrack != null) ActiveTrack.CurrentSequenceStartTime = value; }
        }

        /// <summary>
        /// Cached lockout expiration for UI display purposes only.
        /// This allows OnlineUserButton to show lockout times without loading the protocol.
        /// The actual lockout logic is handled by each LockoutElement using its own persisted state.
        /// </summary>
        public static DateTime LockoutExpiration
        {
            get => ActiveTrack?.LockoutExpiration ?? DateTime.MinValue;
            set { if (ActiveTrack != null) ActiveTrack.LockoutExpiration = value; }
        }

        /// <summary>
        /// Cached flag indicating whether the current lockout has a bypass password.
        /// This allows OnlineUserButton to show the bypass button without loading the protocol.
        /// </summary>
        public static bool LockoutHasBypassPassword
        {
            get => ActiveTrack?.LockoutHasBypassPassword ?? false;
            set { if (ActiveTrack != null) ActiveTrack.LockoutHasBypassPassword = value; }
        }

        public static int ElementNumber
        {
            get => ActiveTrack?.ElementNumber ?? 0;
            set { if (ActiveTrack != null) ActiveTrack.ElementNumber = value; }
        }

        public static int SessionNumber
        {
            get => ActiveTrack?.SessionNumber ?? 0;
            set { if (ActiveTrack != null) ActiveTrack.SessionNumber = value; }
        }

        public static int SequenceIndex
        {
            get => ActiveTrack?.SequenceIndex ?? 0;
            set { if (ActiveTrack != null) ActiveTrack.SequenceIndex = value; }
        }

        /// <summary>
        /// Tracks the last sequence index for which OnEncountered() was called.
        /// Used to prevent calling OnEncountered() multiple times on the same sequence element.
        /// </summary>
        private static int LastEncounteredSequenceIndex
        {
            get => ActiveTrack?.LastEncounteredSequenceIndex ?? -1;
            set { if (ActiveTrack != null) ActiveTrack.LastEncounteredSequenceIndex = value; }
        }

        public static bool SessionInProgress
        {
            get => ActiveTrack?.SessionInProgress ?? false;
            set { if (ActiveTrack != null) ActiveTrack.SessionInProgress = value; }
        }

        #endregion

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
            if (ActiveTrack == null)
            {
                return;
            }

            // If SequenceIndex has been explicitly stored in the track state, use it.
            if (ActiveTrack.HasExplicitSequenceIndex)
            {
                return;
            }

            // Legacy migration: derive SequenceIndex from SessionNumber for
            // very old users who pre-date the SequenceIndex system.
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

            // Clear stale lockout/session state from previous calls.
            // This ensures we don't return stale references if the sequence has advanced.
            currentLockout = null;
            currentSession = null;

            // Detect if a previous session was interrupted by a force-close.
            // SessionInProgress is persisted to disk, so if it's still true here
            // it means the app exited without properly ending the session.
            if (SessionInProgress)
            {
                WasForceCloseInterrupted = true;
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

                // Only call OnEncountered() if this is a new sequence element
                // This prevents resetting timers when re-checking status after app restart
                if (seqIndex != LastEncounteredSequenceIndex)
                {
                    member.OnEncountered();
                    LastEncounteredSequenceIndex = seqIndex;
                }
                
                ProtocolStatus status = member.CheckStatus();

                if (status == ProtocolStatus.Locked)
                {
                    if (sequence.type == SequenceType.Lockout)
                    {
                        currentLockout = sequence.Lockout;
                        
                        // Update SessionNumber to reflect the number of completed sessions.
                        // At a Lockout, we've completed all sessions before this point.
                        SessionNumber = GetSessionOrdinalAtSequenceIndex(seqIndex) + 1;
                        
                        // Cache the lockout expiration for UI display (e.g., OnlineUserButton)
                        // Each LockoutElement manages its own persisted state internally
                        DateTime maxExpiration = DateTime.MinValue;
                        bool hasBypassPassword = false;
                        foreach (LockoutElementID elementId in currentLockout)
                        {
                            LockoutElement element = elementId.Element;
                            if (element != null)
                            {
                                DateTime? expiration = element.GetLockoutExpiration();
                                if (expiration.HasValue && expiration.Value > maxExpiration)
                                {
                                    maxExpiration = expiration.Value;
                                }

                                if (!string.IsNullOrEmpty(element.GetBypassPassword()))
                                {
                                    hasBypassPassword = true;
                                }
                            }
                        }
                        if (maxExpiration > DateTime.MinValue)
                        {
                            LockoutExpiration = maxExpiration;
                        }
                        LockoutHasBypassPassword = hasBypassPassword;
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

            // All sequences completed - update SessionNumber to reflect total completed sessions
            if (currentProtocol != null)
            {
                SessionNumber = currentProtocol.SessionCount;
            }

            return ProtocolStatus.SessionFinished;
        }

        public static ProtocolStatus AdvanceSequence(bool markCompletion = true)
        {
            if (currentProtocol != null && SequenceIndex < currentProtocol.sequences.Count)
            {
                SequenceElement seq = currentProtocol.sequences[SequenceIndex];
                IProtocolSequenceMember member = ResolveSequenceMember(seq);
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
            
            // Clear the stored lockout expiration since we're advancing past the lockout
            LockoutExpiration = DateTime.MinValue;

            ProtocolStatus result = CheckSequenceStatus();
            return result;
        }

        [Obsolete("Transition to string-based Protocol IDs when convenient")]
        public static void PrepareProtocol(
            string protocolName,
            int protocolID)
        {
            PrepareProtocol(protocolName, protocolID.ToString());
        }

        /// <summary>
        /// Loads a protocol set and sets up a single in-memory track.
        /// Does NOT touch PlayerData (safe for batch user creation where no user is logged in).
        /// For full track initialization with persisted state, use TryUpdateProtocol instead.
        /// </summary>
        public static void PrepareProtocol(
            string protocolSetName,
            string protocolKey)
        {
            tracks.Clear();
            activeTrackKey = null;
            loadedProtocolSet = "";

            LoadProtocolSet(protocolSetName);
            if (protocolDictionary.ContainsKey(protocolKey))
            {
                // Use transient constructor — no PlayerData access
                ProtocolTrack track = new ProtocolTrack(protocolKey, new JsonObject());
                track.CurrentProtocol = protocolDictionary[protocolKey];
                tracks[protocolKey] = track;
                activeTrackKey = protocolKey;
            }
            else
            {
                Debug.LogError($"Loaded Protocol \"{loadedProtocolSet}\" does not contain requested protocolKey {protocolKey}.");
                return;
            }
        }

        /// <summary>
        /// Sets up multiple in-memory protocol tracks for parallel execution.
        /// Does NOT touch PlayerData (safe for batch user creation).
        /// For full track initialization with persisted state, use TryUpdateParallelProtocols instead.
        /// </summary>
        public static void PrepareParallelProtocols(
            string protocolSetName,
            IEnumerable<string> protocolKeys)
        {
            tracks.Clear();
            activeTrackKey = null;
            loadedProtocolSet = "";

            LoadProtocolSet(protocolSetName);

            string firstKey = null;

            foreach (string protocolKey in protocolKeys)
            {
                if (!protocolDictionary.ContainsKey(protocolKey))
                {
                    Debug.LogError($"Protocol set \"{loadedProtocolSet}\" does not contain protocolKey \"{protocolKey}\". Skipping.");
                    continue;
                }

                // Use transient constructor — no PlayerData access
                ProtocolTrack track = new ProtocolTrack(protocolKey, new JsonObject());
                track.CurrentProtocol = protocolDictionary[protocolKey];
                tracks[protocolKey] = track;

                firstKey ??= protocolKey;
            }

            activeTrackKey = firstKey;
        }

        public static int GetSequenceIndexForSession(int sessionNumber)
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
                
        public static ProtocolStatus TryUpdateProtocol(
            string protocolSetName,
            string protocolKey,
            int sequenceIndex,
            int sessionElementIndex = 0)
        {
            if (LoadProtocolSet(protocolSetName))
            {
                if (protocolDictionary.ContainsKey(protocolKey))
                {
                    EnsureSingleTrack(protocolKey);
                    ActiveTrack.CurrentProtocol = protocolDictionary[protocolKey];
                    // The track's persisted state (from migration or previous runs)
                    // is the source of truth. The sequenceIndex/sessionElementIndex
                    // parameters are retained for API compatibility but no longer
                    // override the track's stored values.
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

        /// <summary>
        /// Loads a protocol set and sets up multiple tracks for parallel execution.
        /// Calls CheckSequenceStatus on the active track.
        /// </summary>
        public static ProtocolStatus TryUpdateParallelProtocols(
            string protocolSetName,
            IEnumerable<string> protocolKeys)
        {
            if (LoadProtocolSet(protocolSetName))
            {
                tracks.Clear();
                string firstKey = null;

                foreach (string protocolKey in protocolKeys)
                {
                    if (!protocolDictionary.ContainsKey(protocolKey))
                    {
                        Debug.LogError(
                            $"Protocol set \"{loadedProtocolSet}\" does not contain protocolKey \"{protocolKey}\". Skipping.");
                        continue;
                    }

                    ProtocolTrack track = new ProtocolTrack(protocolKey);
                    track.MigrateFromFlatKeys();
                    track.CurrentProtocol = protocolDictionary[protocolKey];
                    tracks[protocolKey] = track;

                    firstKey ??= protocolKey;
                }

                if (tracks.Count == 0)
                {
                    return ProtocolStatus.InvalidProtocol;
                }

                activeTrackKey = firstKey;
                return CheckSequenceStatus();
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
            // If session state isn't initialized, check the sequence status to determine
            // the actual protocol state (could be Locked, SessionFinished, etc.)
            if (nextSessionElementIndex == -1 || currentSession == null)
            {
                return CheckSequenceStatus();
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

            // Save immediately so that if the app is force-closed during element
            // execution, the on-disk state correctly reflects the running element
            // and a resume will be offered on next launch.
            PlayerData.Save();

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

            // Clear all tracks
            tracks.Clear();
            activeTrackKey = null;

            // Clear runtime history stored in PlayerData
            ClearExtensionState();

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

        /// <summary>
        /// Legacy fallback: reads the extension state root from the flat PlayerData key.
        /// Only reached when no track is loaded (ActiveTrack == null).
        /// </summary>
        private static JsonObject GetExtensionStateRoot()
        {
            JsonValue val = PlayerData.GetJsonValue(DataKeys.ExtensionState);
            return val.IsJsonObject ? val.AsJsonObject : new JsonObject();
        }

        /// <summary>
        /// Legacy fallback: writes the extension state root to the flat PlayerData key.
        /// Only reached when no track is loaded (ActiveTrack == null).
        /// </summary>
        private static void SetExtensionStateRoot(JsonObject root)
        {
            PlayerData.SetJsonValue(DataKeys.ExtensionState, root ?? new JsonObject());
        }

        public static JsonValue GetExtensionState(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default(JsonValue);
            }

            if (ActiveTrack != null)
            {
                return ActiveTrack.GetExtensionState(key);
            }

            JsonObject root = GetExtensionStateRoot();
            return root.ContainsKey(key) ? root[key] : default(JsonValue);
        }

        public static JsonObject GetExtensionStateObject(string key)
        {
            JsonValue val = GetExtensionState(key);
            return val.IsJsonObject ? val.AsJsonObject : null;
        }

        public static void SetExtensionState(string key, JsonValue value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (ActiveTrack != null)
            {
                ActiveTrack.SetExtensionState(key, value);
                return;
            }

            JsonObject root = GetExtensionStateRoot();
            root[key] = value;
            SetExtensionStateRoot(root);
        }

        public static void RemoveExtensionState(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (ActiveTrack != null)
            {
                ActiveTrack.RemoveExtensionState(key);
                return;
            }

            JsonObject root = GetExtensionStateRoot();
            root.Remove(key);
            SetExtensionStateRoot(root);
        }

        public static void ClearExtensionState(string prefix = null)
        {
            if (ActiveTrack != null)
            {
                ActiveTrack.ClearExtensionState(prefix);
                return;
            }

            if (string.IsNullOrEmpty(prefix))
            {
                SetExtensionStateRoot(new JsonObject());
                return;
            }

            JsonObject root = GetExtensionStateRoot();
            List<string> keysToRemove = new List<string>();

            foreach (var kvp in root)
            {
                if (kvp.Key != null && kvp.Key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (string k in keysToRemove)
            {
                root.Remove(k);
            }

            SetExtensionStateRoot(root);
        }

        /// <summary>
        /// Skips/clears the current lockout if the current sequence element is a lockout.
        /// This is an admin feature for when the device is unlocked.
        /// Returns true if a lockout was skipped, false otherwise.
        /// </summary>
        public static bool SkipCurrentLockout()
        {
            if (currentProtocol == null)
            {
                Debug.LogWarning("SkipCurrentLockout: No protocol loaded");
                return false;
            }

            int seqIndex = SequenceIndex;
            if (seqIndex < 0 || seqIndex >= currentProtocol.sequences.Count)
            {
                Debug.LogWarning($"SkipCurrentLockout: Invalid sequence index {seqIndex}");
                return false;
            }

            SequenceElement element = currentProtocol.sequences[seqIndex];
            if (element.type != SequenceType.Lockout)
            {
                Debug.LogWarning($"SkipCurrentLockout: Current sequence element is not a lockout (type: {element.type})");
                return false;
            }

            if (!lockoutDictionary.TryGetValue(element.id, out Lockout lockout))
            {
                Debug.LogWarning($"SkipCurrentLockout: Lockout {element.id} not found in dictionary");
                return false;
            }

            // Clear all lockout elements in this lockout
            foreach (LockoutElementID elementId in lockout)
            {
                LockoutElement lockoutElement = elementId.Element;
                if (lockoutElement != null)
                {
                    lockoutElement.ClearLockout();
                }
            }

            // Clear cached lockout expiration
            LockoutExpiration = DateTime.MinValue;

            Debug.Log($"SkipCurrentLockout: Cleared lockout {element.id}");
            return true;
        }
    }
}
