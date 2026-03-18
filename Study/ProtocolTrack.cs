using BGC.Users;
using LightJson;
using System;
using System.Collections.Generic;

namespace BGC.Study
{
    /// <summary>
    /// Encapsulates all per-protocol-track state. In single-protocol mode, one track exists.
    /// In parallel-protocol mode, multiple tracks exist with independent state.
    ///
    /// State is stored as a nested JSON object within PlayerData under
    /// "ProtocolManager.Tracks" → { trackKey: { ...state... } }.
    /// The track holds a direct reference to its JSON sub-object, so in-place
    /// modifications are automatically included when PlayerData.Save() runs.
    /// </summary>
    public class ProtocolTrack
    {
        public const string TracksDataKey = "ProtocolManager.Tracks";

        public string TrackKey { get; }

        /// <summary>
        /// Direct reference to this track's JSON state within PlayerData.
        /// Modifications are in-place and automatically serialized by PlayerData.Save().
        /// </summary>
        private readonly JsonObject state;

        // Runtime state (not persisted — reconstructed from protocol data + indices)
        public Protocol CurrentProtocol { get; set; }
        public Session CurrentSession { get; set; }
        public SessionElement CurrentSessionElement { get; set; }
        public Lockout CurrentLockout { get; set; }
        public int NextSessionElementIndex { get; set; } = -1;
        public bool WasForceCloseInterrupted { get; set; } = false;

        public ProtocolTrack(string trackKey)
        {
            TrackKey = trackKey;
            state = EnsureTrackState(trackKey);
        }

        /// <summary>
        /// Internal constructor for transient/non-persistent tracks.
        /// Used when loading protocol metadata without an active PlayerData user
        /// (e.g., batch user creation).
        /// </summary>
        internal ProtocolTrack(string trackKey, JsonObject existingState)
        {
            TrackKey = trackKey;
            state = existingState ?? new JsonObject();
        }

        /// <summary>
        /// Ensures the Tracks root and this track's sub-object exist in PlayerData.
        /// Returns a direct reference to the track's JsonObject.
        /// </summary>
        private static JsonObject EnsureTrackState(string trackKey)
        {
            JsonValue tracksVal = PlayerData.GetJsonValue(TracksDataKey);
            JsonObject root;

            if (tracksVal.IsJsonObject)
            {
                root = tracksVal.AsJsonObject;
            }
            else
            {
                root = new JsonObject();
                PlayerData.SetJsonValue(TracksDataKey, root);
            }

            if (root.ContainsKey(trackKey) && root[trackKey].IsJsonObject)
            {
                return root[trackKey].AsJsonObject;
            }

            JsonObject trackState = new JsonObject();
            root[trackKey] = trackState;
            return trackState;
        }

        #region Persisted Properties

        public int SequenceIndex
        {
            get => state.ContainsKey("SequenceIndex") && state["SequenceIndex"].IsInteger
                ? state["SequenceIndex"].AsInteger : 0;
            set => state["SequenceIndex"] = value;
        }

        public int SessionNumber
        {
            get => state.ContainsKey("SessionNumber") && state["SessionNumber"].IsInteger
                ? state["SessionNumber"].AsInteger : 0;
            set => state["SessionNumber"] = value;
        }

        public int ElementNumber
        {
            get => state.ContainsKey("ElementNumber") && state["ElementNumber"].IsInteger
                ? state["ElementNumber"].AsInteger : 0;
            set => state["ElementNumber"] = value;
        }

        public bool SessionInProgress
        {
            get => state.ContainsKey("SessionInProgress") && state["SessionInProgress"].IsBoolean
                && state["SessionInProgress"].AsBoolean;
            set => state["SessionInProgress"] = value;
        }

        public int LastEncounteredSequenceIndex
        {
            get => state.ContainsKey("LastEncounteredSequenceIndex") && state["LastEncounteredSequenceIndex"].IsInteger
                ? state["LastEncounteredSequenceIndex"].AsInteger : -1;
            set => state["LastEncounteredSequenceIndex"] = value;
        }

        public DateTime LockoutExpiration
        {
            get
            {
                if (state.ContainsKey("LockoutExpiration"))
                {
                    return state["LockoutExpiration"].AsDateTime ?? DateTime.MinValue;
                }

                return DateTime.MinValue;
            }
            set => state["LockoutExpiration"] = value;
        }

        public bool LockoutHasBypassPassword
        {
            get => state.ContainsKey("LockoutHasBypassPassword") && state["LockoutHasBypassPassword"].IsBoolean
                && state["LockoutHasBypassPassword"].AsBoolean;
            set => state["LockoutHasBypassPassword"] = value;
        }

        public DateTime CurrentSequenceStartTime
        {
            get
            {
                if (state.ContainsKey("CurrentSequenceStartTime"))
                {
                    return state["CurrentSequenceStartTime"].AsDateTime ?? DateTime.MinValue;
                }

                return DateTime.MinValue;
            }
            set => state["CurrentSequenceStartTime"] = value;
        }

        public IReadOnlyList<SequenceTime> SequenceTimes
        {
            get
            {
                if (state.ContainsKey("SequenceTimes") && state["SequenceTimes"].IsJsonArray)
                {
                    List<SequenceTime> times = new();
                    foreach (JsonValue v in state["SequenceTimes"].AsJsonArray)
                    {
                        times.Add(new SequenceTime(v.AsJsonObject));
                    }
                    return times;
                }

                return new List<SequenceTime>();
            }
        }

        public void AddSequenceTime(SequenceTime sequenceTime)
        {
            JsonArray arr;

            if (state.ContainsKey("SequenceTimes") && state["SequenceTimes"].IsJsonArray)
            {
                arr = state["SequenceTimes"].AsJsonArray;
            }
            else
            {
                arr = new JsonArray();
                state["SequenceTimes"] = arr;
            }

            arr.Add(sequenceTime.ToJson());
        }

        #endregion

        #region Extension State

        private JsonObject EnsureExtensionStateRoot()
        {
            if (state.ContainsKey("ExtensionState") && state["ExtensionState"].IsJsonObject)
            {
                return state["ExtensionState"].AsJsonObject;
            }

            JsonObject root = new JsonObject();
            state["ExtensionState"] = root;
            return root;
        }

        public JsonValue GetExtensionState(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default;
            }

            if (!state.ContainsKey("ExtensionState") || !state["ExtensionState"].IsJsonObject)
            {
                return default;
            }

            JsonObject root = state["ExtensionState"].AsJsonObject;
            return root.ContainsKey(key) ? root[key] : default;
        }

        public JsonObject GetExtensionStateObject(string key)
        {
            JsonValue val = GetExtensionState(key);
            return val.IsJsonObject ? val.AsJsonObject : null;
        }

        public void SetExtensionState(string key, JsonValue value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            JsonObject root = EnsureExtensionStateRoot();
            root[key] = value;
        }

        public void RemoveExtensionState(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (state.ContainsKey("ExtensionState") && state["ExtensionState"].IsJsonObject)
            {
                state["ExtensionState"].AsJsonObject.Remove(key);
            }
        }

        public void ClearExtensionState(string prefix = null)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                state["ExtensionState"] = new JsonObject();
                return;
            }

            if (!state.ContainsKey("ExtensionState") || !state["ExtensionState"].IsJsonObject)
            {
                return;
            }

            JsonObject root = state["ExtensionState"].AsJsonObject;
            List<string> keysToRemove = new();

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
        }

        #endregion

        #region Migration

        /// <summary>
        /// Returns true if the SequenceIndex key has been explicitly stored
        /// (used for legacy migration from SessionNumber-only users).
        /// </summary>
        public bool HasExplicitSequenceIndex =>
            state.ContainsKey("SequenceIndex") && state["SequenceIndex"].IsInteger;

        /// <summary>
        /// Migrates legacy flat PlayerData keys into this track's nested state.
        /// Called when a single-track user transitions to the track system.
        /// Only migrates if this track has no existing state.
        /// </summary>
        public void MigrateFromFlatKeys()
        {
            // Only migrate if this track has no existing state
            if (state.Count > 0)
            {
                return;
            }

            MigrateFlatKey(ProtocolManager.DataKeys.SessionNumber, "SessionNumber");
            MigrateFlatKey(ProtocolManager.DataKeys.ElementNumber, "ElementNumber");
            MigrateFlatKey(ProtocolManager.DataKeys.SessionInProgress, "SessionInProgress");
            MigrateFlatKey(ProtocolManager.DataKeys.SequenceIndex, "SequenceIndex");
            MigrateFlatKey(ProtocolManager.DataKeys.LastEncounteredSequenceIndex, "LastEncounteredSequenceIndex");
            MigrateFlatKey(ProtocolManager.DataKeys.LockoutExpiration, "LockoutExpiration");
            MigrateFlatKey(ProtocolManager.DataKeys.LockoutHasBypassPassword, "LockoutHasBypassPassword");
            MigrateFlatKey(ProtocolManager.DataKeys.CurrentSequenceStartTime, "CurrentSequenceStartTime");
            MigrateFlatKey(ProtocolManager.DataKeys.SequenceTimes, "SequenceTimes");
            MigrateFlatKey(ProtocolManager.DataKeys.ExtensionState, "ExtensionState");
        }

        private void MigrateFlatKey(string playerDataKey, string trackStateKey)
        {
            JsonValue val = PlayerData.GetJsonValue(playerDataKey);
            if (!val.IsNull)
            {
                state[trackStateKey] = val;
            }
        }

        /// <summary>
        /// Clears all persisted state for this track.
        /// </summary>
        public void ClearState()
        {
            List<string> keys = new();
            foreach (var kvp in state)
            {
                keys.Add(kvp.Key);
            }

            foreach (string key in keys)
            {
                state.Remove(key);
            }
        }

        #endregion

        #region Static Helpers

        /// <summary>
        /// Reads aggregate track info from a UserData object without loading the protocol.
        /// Used by OnlineUserButton to display summary info for parallel-protocol users.
        /// </summary>
        public static (int totalSessionNumber, DateTime earliestLockout, bool hasBypassPassword)
            GetAggregateTrackInfo(UserData userData, List<string> trackKeys)
        {
            JsonValue tracksVal = userData.GetJsonValue(TracksDataKey);
            if (!tracksVal.IsJsonObject)
            {
                return (0, DateTime.Now, false);
            }

            JsonObject root = tracksVal.AsJsonObject;
            int totalSessions = 0;
            DateTime earliestLockout = DateTime.MaxValue;
            bool anyBypassPassword = false;

            foreach (string key in trackKeys)
            {
                if (!root.ContainsKey(key) || !root[key].IsJsonObject)
                {
                    continue;
                }

                JsonObject track = root[key].AsJsonObject;

                if (track.ContainsKey("SessionNumber") && track["SessionNumber"].IsInteger)
                {
                    totalSessions += track["SessionNumber"].AsInteger;
                }

                if (track.ContainsKey("LockoutExpiration"))
                {
                    DateTime exp = track["LockoutExpiration"].AsDateTime ?? DateTime.MinValue;
                    if (exp > DateTime.Now && exp < earliestLockout)
                    {
                        earliestLockout = exp;
                    }
                }

                if (track.ContainsKey("LockoutHasBypassPassword")
                    && track["LockoutHasBypassPassword"].IsBoolean
                    && track["LockoutHasBypassPassword"].AsBoolean)
                {
                    anyBypassPassword = true;
                }
            }

            if (earliestLockout == DateTime.MaxValue)
            {
                earliestLockout = DateTime.Now;
            }

            return (totalSessions, earliestLockout, anyBypassPassword);
        }

        #endregion
    }
}
