using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightJson;

namespace BGC.Study
{
    public class Protocol : IEnumerable<SessionID>
    {
        private static int nextProtocolID = 1;
        public readonly string key;

        public string name;
        public List<SessionID> sessions;
        public JsonObject envVals;

        private static class Keys
        {
            //Attributes
            public const string Name = "Name";
            public const string SessionIDs = "Sessions";

            //Dictionary
            public const string EnvironmentValues = "Env";
        }

        //Explicitly created Protocols are added to the Protocol dictionary
        [Obsolete("Transition to string-based Protocol IDs")]
        public Protocol()
        {
            key = (nextProtocolID++).ToString();
            sessions = new List<SessionID>();
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
            sessions = new List<SessionID>();
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

            sessions = new List<SessionID>();
            foreach (int sessionID in data[Keys.SessionIDs].AsJsonArray)
            {
                sessions.Add(sessionID);
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
            JsonArray jsonSessionIDs = new JsonArray();
            foreach (SessionID sessionID in sessions)
            {
                jsonSessionIDs.Add(sessionID.id);
            }

            JsonObject serializedData = new JsonObject()
            {
                { Keys.Name, name },
                { Keys.SessionIDs, jsonSessionIDs }
            };

            if (envVals.Count > 0)
            {
                serializedData.Add(Keys.EnvironmentValues, envVals);
            }

            return serializedData;
        }

        public int Count => sessions.Count;

        public Session this[int i] => sessions[i].Session;

        public void Add(Session session) => sessions.Add(session);

        public void AddRange(IEnumerable<Session> sessions)
        {
            foreach (Session session in sessions)
            {
                this.sessions.Add(session);
            }
        }

        public static void HardClear()
        {
            nextProtocolID = 1;
        }

        #region IEnumerator

        IEnumerator<SessionID> IEnumerable<SessionID>.GetEnumerator() => sessions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => sessions.GetEnumerator();

        #endregion IEnumerator
    }

    public class Session : IEnumerable<SessionElementID>
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

        #region IEnumerator

        IEnumerator<SessionElementID> IEnumerable<SessionElementID>.GetEnumerator() => sessionElements.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => sessionElements.GetEnumerator();

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

    public readonly struct SessionID
    {
        public readonly int id;
        public Session Session => ProtocolManager.sessionDictionary.ContainsKey(id) ?
            ProtocolManager.sessionDictionary[id] : null;

        public SessionID(int id)
        {
            this.id = id;
        }

        public static implicit operator SessionID(Session session) => new SessionID(session.id);
        public static implicit operator SessionID(int id) => new SessionID(id);
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
}
