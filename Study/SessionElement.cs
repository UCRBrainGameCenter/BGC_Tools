using System;
using System.Threading.Tasks;
using LightJson;

namespace BGC.Study
{
    public abstract class SessionElement
    {
        private static int nextElementID = 1;

        public abstract string ElementType { get; }
        public readonly int id;

        public JsonObject envVals;

        //Deserialized sessionElements are not added to the SessionElement dictionary
        public SessionElement(JsonObject data)
        {
            id = data[ProtocolKeys.SessionElement.Id];
            if (nextElementID <= id)
            {
                nextElementID = id + 1;
            }

            if (data.ContainsKey(ProtocolKeys.SessionElement.EnvironmentValues))
            {
                envVals = data[ProtocolKeys.SessionElement.EnvironmentValues].AsJsonObject;
            }
            else
            {
                envVals = new JsonObject();
            }
        }

        //Explicitly created sessionElements are added to the dictionary
        public SessionElement()
        {
            id = nextElementID++;

            envVals = new JsonObject();

            ProtocolManager.sessionElementDictionary.Add(id, this);
        }

        public JsonObject SerializeElement()
        {
            JsonObject sessionElement = new JsonObject()
            {
                { ProtocolKeys.SessionElement.Id, id },
                { ProtocolKeys.SessionElement.ElementType, ElementType }
            };

            if (envVals.Count > 0)
            {
                sessionElement.Add(ProtocolKeys.SessionElement.EnvironmentValues, envVals);
            }

            _PopulateJSONObject(sessionElement);

            return sessionElement;
        }

        protected abstract void _PopulateJSONObject(JsonObject jsonObject);

        public abstract Task ExecuteElement(bool resuming = false);
        public virtual void CleanupElement() { }

        public static void HardClear()
        {
            nextElementID = 1;
        }
    }
}
