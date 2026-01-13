using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightJson;

namespace BGC.Study
{
    public abstract class LockoutElement
    {
        private static int nextElementID = 1;

        public abstract string ElementType { get; }
        public readonly int id;

        public JsonObject envVals;

        //Deserialized lockoutElements are not added to the LockoutElement dictionary
        public LockoutElement(JsonObject data)
        {
            id = data[ProtocolKeys.LockoutElement.Id];
            if (nextElementID <= id)
            {
                nextElementID = id + 1;
            }

            if (data.ContainsKey(ProtocolKeys.LockoutElement.EnvironmentValues))
            {
                envVals = data[ProtocolKeys.LockoutElement.EnvironmentValues].AsJsonObject;
            }
            else
            {
                envVals = new JsonObject();
            }
        }

        //Explicitly created lockoutElements are added to the dictionary
        public LockoutElement()
        {
            id = nextElementID++;

            envVals = new JsonObject();

            ProtocolManager.lockoutElementDictionary.Add(id, this);
        }

        public JsonObject SerializeElement()
        {
            JsonObject lockoutElement = new JsonObject()
            {
                { ProtocolKeys.LockoutElement.Id, id },
                { ProtocolKeys.LockoutElement.Type, ElementType }
            };

            if (envVals.Count > 0)
            {
                lockoutElement.Add(ProtocolKeys.LockoutElement.EnvironmentValues, envVals);
            }

            _PopulateJSONObject(lockoutElement);

            return lockoutElement;
        }

        protected abstract void _PopulateJSONObject(JsonObject jsonObject);

        public abstract bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes);
        public virtual string GetBypassPassword() { return null; }
        public virtual void OnLockoutCompleted(DateTime encounteredTime, DateTime completedTime)
        {
        }

        public static void HardClear()
        {
            nextElementID = 1;
        }
    }
}
