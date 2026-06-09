using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using BGC.IO;
using BGC.Study;

namespace BGC.Users
{
    public abstract class ProfileData
    {
        public string UserName { get; }
        private string loggingName;
        public string LoggingName => loggingName;

        private JsonObject userData = new JsonObject();

        /// <summary>
        /// Direct reference to the flat root JsonObject (serialized under the
        /// "UserDicts" JSON key). Exposed for write-through and deep-copy callers
        /// (PlayerData, ProtocolTrack) that need to iterate or mutate the root
        /// outside the normal typed-key API. In-place mutations are preserved by
        /// the next <see cref="Serialize"/> call.
        ///
        /// NOTE: this is the raw-root escape hatch. The typed accessors below ROUTE
        /// per active protocol track (see <see cref="RoutingActive"/>); callers that
        /// must always reach flat root (the routing internals) use this field.
        /// </summary>
        public JsonObject UserDicts => userData;

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

        #region Per-Track Routing

        // The per-track routing lives here (rather than only in the PlayerData static
        // wrapper) so it is TRANSPARENT: both PlayerData.X and PlayerData.ProfileData.X
        // resolve identically, and existing callers route without being modified.
        //
        // NOTE on layering: BGC.Users and BGC.Study compile into the same assembly, and
        // PlayerData already references ProtocolManager/ProtocolTrack, so referencing them
        // here is not a new dependency. If these namespaces are ever split into separate
        // assemblies, replace the direct references with a resolver delegate registered by
        // the Study layer.

        /// <summary>
        /// Whether this profile's typed accessors route per active protocol track.
        /// Only the currently-logged-in user routes; <see cref="DefaultData"/>/
        /// <see cref="GlobalData"/> (IsDefault) and any non-current <see cref="UserData"/>
        /// instance (e.g. user-list summary reads) operate on flat root, unchanged.
        /// </summary>
        private bool RoutingActive => !IsDefault && ReferenceEquals(this, PlayerData.CurrentUserOrNull);

        /// <summary>
        /// Meta-keys ("ProtocolManager.*") always read/write flat root regardless of active
        /// track — these are the keys the routing itself reads (e.g. "ProtocolManager.Tracks"),
        /// so routing them would recurse.
        /// </summary>
        private static bool IsMetaKey(string key) =>
            key != null && key.StartsWith("ProtocolManager.", StringComparison.Ordinal);

        /// <summary>
        /// The active track's JsonObject under "ProtocolManager.Tracks", or null when no track
        /// is active or the entry is absent. Reads the raw <see cref="userData"/> field directly
        /// (never the routed accessors) to avoid recursion.
        /// </summary>
        private JsonObject ActiveTrackJsonOrNull()
        {
            string trackKey = ProtocolManager.ActiveTrackKey;
            if (string.IsNullOrEmpty(trackKey)) return null;
            if (!userData.ContainsKey(ProtocolTrack.TracksDataKey)) return null;

            JsonValue val = userData[ProtocolTrack.TracksDataKey];
            if (!val.IsJsonObject) return null;

            JsonObject tracksRoot = val.AsJsonObject;
            if (!tracksRoot.ContainsKey(trackKey) || !tracksRoot[trackKey].IsJsonObject) return null;

            return tracksRoot[trackKey].AsJsonObject;
        }

        /// <summary>
        /// Every track's JsonObject under "ProtocolManager.Tracks" (raw field read). Used by the
        /// no-active-track write-through path to fan a Set out to every track.
        /// </summary>
        private IEnumerable<JsonObject> EnumerateTrackJsonObjects()
        {
            if (!userData.ContainsKey(ProtocolTrack.TracksDataKey)) yield break;

            JsonValue val = userData[ProtocolTrack.TracksDataKey];
            if (!val.IsJsonObject) yield break;

            foreach (KeyValuePair<string, JsonValue> kvp in val.AsJsonObject)
            {
                if (kvp.Value.IsJsonObject) yield return kvp.Value.AsJsonObject;
            }
        }

        #endregion Per-Track Routing

        /// <summary> Clear all values and keys </summary>
        public virtual void Clear()
        {
            userData.Clear();
        }

        #region Routed Typed Accessors

        // Routing per accessor:
        //  - !RoutingActive or meta-key -> raw root (today's behavior).
        //  - active track exists        -> the track is the target.
        //  - no active track            -> writes fan out to root + every track (write-through);
        //                                  reads use root.
        // Reads use the track only when the track CONTAINS the key (existence-based ownership, so
        // no false miss); otherwise they fall back to root (legacy read-through) so state written
        // to root before it was track-routed still resolves and migrates into the track on the next
        // write. The type check is applied within whichever object is read (root fallback is
        // therefore type-checked for the typed getters), never as part of the track-vs-root choice.

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetInt(string key, int value)
        {
            if (!RoutingActive || IsMetaKey(key)) { RawSetInt(userData, key, value); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null) { RawSetInt(active, key, value); return; }
            RawSetInt(userData, key, value);
            foreach (JsonObject track in EnumerateTrackJsonObjects()) RawSetInt(track, key, value);
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetBool(string key, bool value)
        {
            if (!RoutingActive || IsMetaKey(key)) { RawSetBool(userData, key, value); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null) { RawSetBool(active, key, value); return; }
            RawSetBool(userData, key, value);
            foreach (JsonObject track in EnumerateTrackJsonObjects()) RawSetBool(track, key, value);
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetString(string key, string value)
        {
            if (!RoutingActive || IsMetaKey(key)) { RawSetString(userData, key, value); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null) { RawSetString(active, key, value); return; }
            RawSetString(userData, key, value);
            foreach (JsonObject track in EnumerateTrackJsonObjects()) RawSetString(track, key, value);
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetFloat(string key, float value)
        {
            if (!RoutingActive || IsMetaKey(key)) { RawSetFloat(userData, key, value); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null) { RawSetFloat(active, key, value); return; }
            RawSetFloat(userData, key, value);
            foreach (JsonObject track in EnumerateTrackJsonObjects()) RawSetFloat(track, key, value);
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetDouble(string key, double value)
        {
            if (!RoutingActive || IsMetaKey(key)) { RawSetDouble(userData, key, value); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null) { RawSetDouble(active, key, value); return; }
            RawSetDouble(userData, key, value);
            foreach (JsonObject track in EnumerateTrackJsonObjects()) RawSetDouble(track, key, value);
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetJsonValue(string key, JsonValue value)
        {
            if (!RoutingActive || IsMetaKey(key)) { RawSetJsonValue(userData, key, value); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null)
            {
                RawSetJsonValue(active, key, value);
                return;
            }

            RawSetJsonValue(userData, key, value);
            // Deep-clone per track so tracks can independently mutate JsonObject/Array values.
            foreach (JsonObject track in EnumerateTrackJsonObjects())
            {
                RawSetJsonValue(track, key, ProtocolTrack.DeepClone(value));
            }
        }

        /// <summary> Set <paramref name="value"/> at indicated <paramref name="key"/> </summary>
        public void SetJsonArray(string key, JsonArray value)
        {
            if (!RoutingActive || IsMetaKey(key)) { RawSetJsonArray(userData, key, value); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null) { RawSetJsonArray(active, key, value); return; }
            RawSetJsonArray(userData, key, value);
            foreach (JsonObject track in EnumerateTrackJsonObjects())
            {
                RawSetJsonArray(track, key, ProtocolTrack.DeepClone(value).AsJsonArray);
            }
        }

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public int GetInt(string key, int defaultReturn = 0) => RawGetInt(ReadSource(key), key, defaultReturn);

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public bool GetBool(string key, bool defaultReturn = false) => RawGetBool(ReadSource(key), key, defaultReturn);

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public float GetFloat(string key, float defaultReturn = 0f) => RawGetFloat(ReadSource(key), key, defaultReturn);

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public double GetDouble(string key, double defaultReturn = 0.0) => RawGetDouble(ReadSource(key), key, defaultReturn);

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public string GetString(string key, string defaultReturn = "") => RawGetString(ReadSource(key), key, defaultReturn);

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public JsonValue GetJsonValue(string key, JsonValue defaultReturn = default(JsonValue)) => RawGetJsonValue(ReadSource(key), key, defaultReturn);

        /// <summary> Get value associated with indicated <paramref name="key"/> </summary>
        /// <param name="defaultReturn">The value to return if the key is not present in dictionary</param>
        public JsonArray GetJsonArray(string key, JsonArray defaultReturn = default(JsonArray)) => RawGetJsonArray(ReadSource(key), key, defaultReturn);

        /// <summary> Get if any value is associated with indicated <paramref name="key"/> </summary>
        public bool HasKey(string key)
        {
            if (!RoutingActive || IsMetaKey(key)) return userData.ContainsKey(key);
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null && active.ContainsKey(key)) return true;
            return userData.ContainsKey(key);
        }

        /// <summary> Remove any value is associated with indicated <paramref name="key"/> </summary>
        public void RemoveKey(string key)
        {
            if (!RoutingActive || IsMetaKey(key)) { userData.Remove(key); return; }
            JsonObject active = ActiveTrackJsonOrNull();
            if (active != null) { active.Remove(key); return; }
            userData.Remove(key);
            foreach (JsonObject track in EnumerateTrackJsonObjects()) track.Remove(key);
        }

        /// <summary>
        /// Resolves the JsonObject a read should consult for <paramref name="key"/>: the active
        /// track when it OWNS the key (existence-based, so the track never suffers a false miss),
        /// otherwise flat root (covers no-routing, meta-keys, no-active-track, and the legacy
        /// read-through for keys the active track doesn't have yet).
        /// </summary>
        private JsonObject ReadSource(string key)
        {
            if (!RoutingActive || IsMetaKey(key)) return userData;
            JsonObject active = ActiveTrackJsonOrNull();
            return (active != null && active.ContainsKey(key)) ? active : userData;
        }

        #endregion Routed Typed Accessors

        #region Raw root accessors (no routing)

        // These operate on an explicit target JsonObject and contain the type-mismatch warning
        // logic. Both the unrouted path (target = userData) and the routed path (target = a track)
        // reuse them, so warnings and type semantics are identical regardless of destination.

        private static void RawSetInt(JsonObject target, string key, int value)
        {
            if (!target.ContainsKey(key)) { target.Add(key, value); return; }
            if (!target[key].IsInteger && !target[key].IsNull)
                Debug.LogError($"PlayerData \"{key}\" Datatype changed from {target[key].Type} to Int");
            target[key] = value;
        }

        private static void RawSetBool(JsonObject target, string key, bool value)
        {
            if (!target.ContainsKey(key)) { target.Add(key, value); return; }
            if (!target[key].IsBoolean && !target[key].IsNull)
            {
                if (target[key].IsInteger)
                    Debug.LogWarning($"PlayerData \"{key}\" Datatype changed from {target[key].Type} to Bool");
                else
                    Debug.LogError($"PlayerData \"{key}\" Datatype changed from {target[key].Type} to Bool");
            }
            target[key] = value;
        }

        private static void RawSetString(JsonObject target, string key, string value)
        {
            if (!target.ContainsKey(key)) { target.Add(key, value); return; }
            if (!target[key].IsString && !target[key].IsNull)
                Debug.LogError($"PlayerData \"{key}\" Datatype changed from {target[key].Type} to String");
            target[key] = value;
        }

        private static void RawSetFloat(JsonObject target, string key, float value)
        {
            if (!target.ContainsKey(key)) { target.Add(key, value); return; }
            if (!target[key].IsNumber && !target[key].IsNull)
                Debug.LogError($"PlayerData \"{key}\" Datatype changed from {target[key].Type} to Number");
            target[key] = value;
        }

        private static void RawSetDouble(JsonObject target, string key, double value)
        {
            if (!target.ContainsKey(key)) { target.Add(key, value); return; }
            if (!target[key].IsNumber && !target[key].IsNull)
                Debug.LogError($"PlayerData \"{key}\" Datatype changed from {target[key].Type} to Number");
            target[key] = value;
        }

        private static void RawSetJsonValue(JsonObject target, string key, JsonValue value)
        {
            if (!target.ContainsKey(key)) target.Add(key, value);
            else target[key] = value;
        }

        private static void RawSetJsonArray(JsonObject target, string key, JsonArray value)
        {
            if (!target.ContainsKey(key)) target.Add(key, value);
            else target[key] = value;
        }

        private static int RawGetInt(JsonObject src, string key, int defaultReturn) =>
            src.ContainsKey(key) && src[key].IsInteger ? src[key].AsInteger : defaultReturn;

        private static bool RawGetBool(JsonObject src, string key, bool defaultReturn)
        {
            if (src.ContainsKey(key))
            {
                if (src[key].IsBoolean) return src[key].AsBoolean;
                if (src[key].IsInteger) return src[key].AsInteger != 0;
            }
            return defaultReturn;
        }

        private static float RawGetFloat(JsonObject src, string key, float defaultReturn) =>
            src.ContainsKey(key) && src[key].IsNumber ? (float)src[key].AsNumber : defaultReturn;

        private static double RawGetDouble(JsonObject src, string key, double defaultReturn) =>
            src.ContainsKey(key) && src[key].IsNumber ? src[key].AsNumber : defaultReturn;

        private static string RawGetString(JsonObject src, string key, string defaultReturn) =>
            src.ContainsKey(key) && src[key].IsString ? src[key].AsString : defaultReturn;

        private static JsonValue RawGetJsonValue(JsonObject src, string key, JsonValue defaultReturn) =>
            src.ContainsKey(key) ? src[key] : defaultReturn;

        private static JsonArray RawGetJsonArray(JsonObject src, string key, JsonArray defaultReturn) =>
            src.ContainsKey(key) && src[key].IsJsonArray ? src[key].AsJsonArray : defaultReturn;

        #endregion Raw root accessors

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
