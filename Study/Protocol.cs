using LightJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BGC.Study
{
    public class Protocol : IEnumerable<SequenceElement>
    {
        private static int nextProtocolID = 1;
        public readonly string key;

        public string name;
        public List<SequenceElement> sequences;
        public JsonObject envVals;

        public List<Session> Sessions => sequences
            .Where(seq => seq.type == SequenceType.Session)
            .Select(seq => seq.Session)
            .Where(session => session != null)
            .ToList();

        public List<Lockout> Lockouts => sequences
            .Where(seq => seq.type == SequenceType.Lockout)
            .Select(seq => seq.Lockout)
            .Where(lockout => lockout != null)
            .ToList();

        public int SessionCount => sequences.Count(seq => seq.type == SequenceType.Session);
        public int LockoutCount => sequences.Count(seq => seq.type == SequenceType.Lockout);

        private static class Keys
        {
            //Attributes
            public const string Name = "Name";
            public const string SessionIDs = "Sessions";
            public const string Sequence = "Sequence";

            //Dictionary
            public const string EnvironmentValues = "Env";
        }

        //Explicitly created Protocols are added to the Protocol dictionary
        [Obsolete("Transition to string-based Protocol IDs")]
        public Protocol()
        {
            key = (nextProtocolID++).ToString();
            sequences = new List<SequenceElement>();
            envVals = new JsonObject();

            ProtocolManager.protocolDictionary.Add(key, this);
        }

        //Explicitly created Protocols are added to the Protocol dictionary
        [Obsolete("Transition to string-based Protocol IDs")]
        public Protocol(string name)
            : this()
        {
            this.name = name;
        }

        //Explicitly created Protocols are added to the Protocol dictionary
        public Protocol(string name, string key)
        {
            this.name = name;
            this.key = key;
            sequences = new List<SequenceElement>();
            envVals = new JsonObject();

            ProtocolManager.protocolDictionary.Add(key, this);
        }

        //Deserialized Protocols are not added to the Protocol dictionary
        [Obsolete("Transition to string-based Protocol IDs")]
        public Protocol(int id)
        {
            key = id.ToString();
            if (nextProtocolID <= id)
            {
                nextProtocolID = id + 1;
            }

            //Should be assigned by constructing caller
            envVals = null;
        }

        //Deserializing constructor - not added to the Protocol dictionary
        public Protocol(JsonObject data, string key)
        {
            name = data[Keys.Name];
            this.key = key;

            sequences = new List<SequenceElement>();
            if (data.ContainsKey(Keys.SessionIDs))
            {
                foreach (int sessionID in data[Keys.SessionIDs].AsJsonArray)
                {
                    sequences.Add(new SequenceElement(sessionID, SequenceType.Session));
                }
            }
            else if (data.ContainsKey(Keys.Sequence))
            {
                foreach (JsonObject sequenceElement in data[Keys.Sequence].AsJsonArray)
                {
                    int id = sequenceElement["Id"];
                    string type = sequenceElement["Type"];
                    sequences.Add(new SequenceElement(id, type));
                }
            }

            if (data.ContainsKey(Keys.EnvironmentValues))
            {
                envVals = data[Keys.EnvironmentValues].AsJsonObject;
            }
            else
            {
                envVals = new JsonObject();
            }
        }

        public JsonObject ToJson()
        {
            // Prefer serializing as sessions unless lockouts are present
            if (sequences.TrueForAll(seq => seq.type == SequenceType.Session))
            {
                JsonArray sessionIDs = new();
                foreach (SequenceElement sequence in sequences)
                {
                    sessionIDs.Add(sequence.id);
                }

                JsonObject serializedData = new()
                {
                    { Keys.Name, name },
                    { Keys.SessionIDs, sessionIDs }
                };

                if (envVals.Count > 0)
                {
                    serializedData.Add(Keys.EnvironmentValues, envVals);
                }

                return serializedData;
            }
            else
            {
                JsonArray jsonSequences = new JsonArray();
                foreach (SequenceElement sequence in sequences)
                {
                    JsonObject sequenceObject = new()
                    {
                        { ProtocolKeys.SequenceElement.Type, SequenceElement.SequenceTypeNames[sequence.type] },
                        { ProtocolKeys.SequenceElement.Id, sequence.id }
                    };
                    jsonSequences.Add(sequenceObject);
                }

                JsonObject serializedData = new()
                {
                    { Keys.Name, name },
                    { Keys.Sequence, jsonSequences }
                };

                if (envVals.Count > 0)
                {
                    serializedData.Add(Keys.EnvironmentValues, envVals);
                }
                return serializedData;
            }
        }

        public int Count => sequences.Count;

        public SequenceElement this[int i] => sequences[i];

        public void Add(SequenceElement sequence) => sequences.Add(sequence);

        public void AddRange(IEnumerable<SequenceElement> sequenceElements)
        {
            foreach (SequenceElement sequence in sequenceElements)
            {
                sequences.Add(sequence);
            }
        }

        public static void HardClear()
        {
            nextProtocolID = 1;
        }

        #region IEnumerator

        IEnumerator<SequenceElement> IEnumerable<SequenceElement>.GetEnumerator() => sequences.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => sequences.GetEnumerator();

        #endregion IEnumerator
    }

    public class Session : IEnumerable<SessionElementID>, IProtocolSequenceMember
    {
        private static int nextSessionID = 1;
        public readonly int id;

        public List<SessionElementID> sessionElements;
        public JsonObject envVals;

        public static class Keys
        {
            //Attributes
            public const string Id = "Id";
            public const string SessionElementIDs = "Elements";

            //Dictionary
            public const string EnvironmentValues = "Env";
            public const string PasswordId = "Password";
            public const string LockoutMinutes = "Lockout";
        }

        //Explicitly created sessions are added to the Session dictionary
        public Session()
        {
            id = nextSessionID++;
            sessionElements = new List<SessionElementID>();
            envVals = new JsonObject();

            ProtocolManager.sessionDictionary.Add(id, this);
        }

        /// <summary>
        /// Deserialization Constructor
        /// Deserialized Sessions are not added to the Session dictionary
        /// </summary>
        public Session(JsonObject sessionData)
        {
            id = sessionData[Keys.Id];

            if (nextSessionID <= id)
            {
                nextSessionID = id + 1;
            }

            sessionElements = new List<SessionElementID>();
            foreach (int sessionElementID in sessionData[Keys.SessionElementIDs].AsJsonArray)
            {
                sessionElements.Add(sessionElementID);
            }

            if (sessionData.ContainsKey(Keys.EnvironmentValues))
            {
                envVals = sessionData[Keys.EnvironmentValues].AsJsonObject;
            }
            else
            {
                envVals = new JsonObject();
            }
        }

        public int Count => sessionElements.Count;
        public SessionElement this[int i] => sessionElements[i].Element;

        public int ID => id;
        public SequenceType Type => SequenceType.Session;

        public void Add(SessionElement element)
        {
            //It was supremely convenient for constructing tasks to allow for (and block)
            //  null session elements
            if (element != null)
            {
                sessionElements.Add(element);
            }
        }

        public void AddRange(IEnumerable<SessionElement> elements)
        {
            foreach (SessionElement element in elements)
            {
                //It was supremely convenient for constructing tasks to allow for (and block)
                //  null session elements
                if (element != null)
                {
                    sessionElements.Add(element);
                }
            }
        }

        public JsonObject SerializeSession()
        {
            JsonArray jsonElementsIDs = new JsonArray();
            foreach (SessionElementID elementID in sessionElements)
            {
                jsonElementsIDs.Add(elementID.id);
            }

            JsonObject newSession = new JsonObject()
            {
                { Keys.Id, id },
                { Keys.SessionElementIDs, jsonElementsIDs }
            };

            if (envVals.Count > 0)
            {
                newSession.Add(Keys.EnvironmentValues, envVals);
            }

            return newSession;
        }

        public Session SetPassword(string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
            {
                envVals[Keys.PasswordId] = password;
            }
            else
            {
                envVals.Remove(Keys.PasswordId);
            }
            return this;
        }

        public string GetPassword()
        {
            if (envVals.ContainsKey(Keys.PasswordId) && envVals[Keys.PasswordId].IsString)
            {
                string password = envVals[Keys.PasswordId].AsString;
                if (!string.IsNullOrWhiteSpace(password))
                {
                    return password;
                }
            }
            return null;
        }

        public Session SetLockoutMinutes(int lockoutMinutes)
        {
            if (lockoutMinutes > 0)
            {
                envVals[Keys.LockoutMinutes] = lockoutMinutes;
            }
            else
            {
                envVals.Remove(Keys.LockoutMinutes);
            }
            return this;
        }

        public int GetLockoutMinutes()
        {
            if (envVals.ContainsKey(Keys.LockoutMinutes))
            {
                int lockoutMinutes = envVals[Keys.LockoutMinutes].AsInteger;
                if (lockoutMinutes > 0)
                {
                    return lockoutMinutes;
                }
            }
            return 0;
        }

        public static void HardClear()
        {
            nextSessionID = 1;
        }

        public ProtocolStatus CheckStatus()
        {
            string password = GetPassword();
            if (!string.IsNullOrEmpty(password))
            {
                return ProtocolStatus.Locked;
            }

            return ProtocolStatus.SessionReady;
        }

        public void OnEncountered()
        {
            ProtocolManager.CurrentSequenceStartTime = DateTime.Now;
        }

        public void OnCompleted()
        {
            ProtocolManager.AddSequenceTime(
                new SequenceTime(SequenceType.Session, id, ProtocolManager.CurrentSequenceStartTime, DateTime.Now));
        }

        #region IEnumerator

        IEnumerator<SessionElementID> IEnumerable<SessionElementID>.GetEnumerator() => sessionElements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => sessionElements.GetEnumerator();

        #endregion IEnumerator
    }

    public class Lockout : IEnumerable<LockoutElementID>, IProtocolSequenceMember
    {
        private static int nextLockoutID = 1;
        public readonly int id;

        public List<LockoutElementID> lockoutElements;
        public JsonObject envVals;

        public static class Keys
        {
            // Attributes
            public const string Id = "Id";
            public const string LockoutElementIDs = "Elements";

            // Dictionary
            public const string EnvironmentValues = "Env";
        }

        // Explicitly created lockouts are added to the Lockout dictionary
        public Lockout()
        {
            id = nextLockoutID++;
            lockoutElements = new List<LockoutElementID>();
            envVals = new JsonObject();

            ProtocolManager.lockoutDictionary.Add(id, this);
        }

        /// <summary>
        /// Deserialization Constructor
        /// Deserialized Lockouts are not added to the Lockout dictionary
        /// </summary>
        public Lockout(JsonObject lockoutData)
        {
            id = lockoutData[Keys.Id];

            if (nextLockoutID <= id)
            {
                nextLockoutID = id + 1;
            }

            lockoutElements = new List<LockoutElementID>();
            foreach (int lockoutElementID in lockoutData[Keys.LockoutElementIDs].AsJsonArray)
            {
                lockoutElements.Add(lockoutElementID);
            }

            if (lockoutData.ContainsKey(Keys.EnvironmentValues))
            {
                envVals = lockoutData[Keys.EnvironmentValues].AsJsonObject;
            }
            else
            {
                envVals = new JsonObject();
            }
        }

        public int Count => lockoutElements.Count;
        public LockoutElement this[int i] => lockoutElements[i].Element;

        public int ID => id;
        public SequenceType Type => SequenceType.Lockout;

        public void Add(LockoutElement element)
        {
            if (element != null)
            {
                lockoutElements.Add(element);
            }
        }

        public void AddRange(IEnumerable<LockoutElement> elements)
        {
            foreach (LockoutElement element in elements)
            {
                if (element != null)
                {
                    lockoutElements.Add(element);
                }
            }
        }

        public JsonObject SerializeLockout()
        {
            JsonArray jsonElementsIDs = new JsonArray();
            foreach (LockoutElementID elementID in lockoutElements)
            {
                jsonElementsIDs.Add(elementID.id);
            }

            JsonObject newLockout = new JsonObject()
            {
                { Keys.Id, id },
                { Keys.LockoutElementIDs, jsonElementsIDs }
            };

            if (envVals.Count > 0)
            {
                newLockout.Add(Keys.EnvironmentValues, envVals);
            }

            return newLockout;
        }

        public static void HardClear()
        {
            nextLockoutID = 1;
        }

        public ProtocolStatus CheckStatus()
        {
            foreach (LockoutElementID elementId in lockoutElements)
            {
                LockoutElement element = elementId.Element;
                if (element == null)
                {
                    Debug.LogError($"Lockout element is null for ID: {elementId.id}");
                    continue;
                }

                if (element.CheckLockout(DateTime.Now, ProtocolManager.SequenceTimes))
                {
                    ProtocolManager.currentLockout = this;
                    return ProtocolStatus.Locked;
                }
            }

            return ProtocolStatus.StepCompleted;
        }

        public void OnEncountered()
        {
            ProtocolManager.CurrentSequenceStartTime = DateTime.Now;
        }

        public void OnCompleted()
        {
            DateTime encounteredTime = ProtocolManager.CurrentSequenceStartTime;
            DateTime completedTime = DateTime.Now;

            ProtocolManager.AddSequenceTime(
                new SequenceTime(SequenceType.Lockout, id, encounteredTime, completedTime));

            foreach (LockoutElementID elementId in lockoutElements)
            {
                LockoutElement element = elementId.Element;
                element?.OnLockoutCompleted(encounteredTime, completedTime);
            }
        }

        #region IEnumerator

        IEnumerator<LockoutElementID> IEnumerable<LockoutElementID>.GetEnumerator() => lockoutElements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => lockoutElements.GetEnumerator();

        #endregion IEnumerator
    }

    public readonly struct ProtocolID
    {
        public readonly string id;
        public Protocol Protocol => ProtocolManager.protocolDictionary.ContainsKey(id) ?
            ProtocolManager.protocolDictionary[id] : null;

        [Obsolete("Transition to string-based protocol IDs")]
        public ProtocolID(int id)
        {
            this.id = id.ToString();
        }

        public ProtocolID(string id)
        {
            this.id = id;
        }

        public static implicit operator ProtocolID(Protocol protocol) => new ProtocolID(protocol.key);
        [Obsolete("Transition to string-based protocol IDs")]
        public static implicit operator ProtocolID(int id) => new ProtocolID(id);
        public static implicit operator ProtocolID(string id) => new ProtocolID(id);
    }

    public enum SequenceType
    {
        Session,
        Lockout
    }

    public readonly struct SequenceElement
    {
        public static readonly ReadOnlyDictionary<SequenceType, string> SequenceTypeNames =
            new(new Dictionary<SequenceType, string>()
            {
                { SequenceType.Session, "Session" },
                { SequenceType.Lockout, "Lockout" }
            });

        public static readonly ReadOnlyDictionary<string, SequenceType> SequenceTypeFromNames =
            new(new Dictionary<string, SequenceType>()
            {
                { "Session", SequenceType.Session },
                { "Lockout", SequenceType.Lockout }
            });

        public readonly int id;
        public readonly SequenceType type;

        public Session Session => type == SequenceType.Session && 
            ProtocolManager.sessionDictionary.ContainsKey(id) ?
            ProtocolManager.sessionDictionary[id] : null;

        public Lockout Lockout => type == SequenceType.Lockout && 
            ProtocolManager.lockoutDictionary.ContainsKey(id) ?
            ProtocolManager.lockoutDictionary[id] : null;

        public SequenceElement(int id, SequenceType type)
        {
            this.id = id;
            this.type = type;
        }

        public SequenceElement(int id, string type)
        {
            this.id = id;

            if (SequenceTypeFromNames.ContainsKey(type))
            {
                this.type = SequenceTypeFromNames[type];
            }
            else
            {
                throw new ArgumentException($"Invalid sequence type name: {type}");
            }
        }

        public static implicit operator SequenceElement(Session session) =>
            new SequenceElement(session.id, SequenceType.Session);

        public static implicit operator SequenceElement(Lockout lockout) =>
            new SequenceElement(lockout.id, SequenceType.Lockout);
    }

    public readonly struct SessionElementID
    {
        public readonly int id;
        public SessionElement Element => ProtocolManager.sessionElementDictionary.ContainsKey(id) ?
            ProtocolManager.sessionElementDictionary[id] : null;

        public SessionElementID(int id)
        {
            this.id = id;
        }

        public static implicit operator SessionElementID(SessionElement element) => new SessionElementID(element.id);
        public static implicit operator SessionElementID(int id) => new SessionElementID(id);
    }

    public readonly struct LockoutElementID
    {
        public readonly int id;
        public LockoutElement Element => ProtocolManager.lockoutElementDictionary.ContainsKey(id) ?
            ProtocolManager.lockoutElementDictionary[id] : null;
        public LockoutElementID(int id)
        {
            this.id = id;
        }
        public static implicit operator LockoutElementID(LockoutElement element) => new LockoutElementID(element.id);
        public static implicit operator LockoutElementID(int id) => new LockoutElementID(id);
    }
}
