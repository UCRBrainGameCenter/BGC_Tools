using System.Collections.Generic;

namespace BGC.StateMachine
{
    public class StateData
    {
        private Dictionary<string, bool> booleans = new Dictionary<string, bool>();
        private Dictionary<string, bool> triggers = new Dictionary<string, bool>();

        public void AddBoolean(string key, bool value)
        {
            booleans.Add(key, value);
        }

        public void SetBoolean(string key, bool value)
        {
            booleans[key] = value;
        }

        public bool GetBoolean(string key)
        {
            return booleans[key];
        }

        public void AddTrigger(string key)
        {
            triggers.Add(key, false);
        }

        public void ActivateTrigger(string key)
        {
            triggers[key] = true;
        }

        public void DeActivateTrigger(string key)
        {
            triggers[key] = false;
        }

        public bool GetTrigger(string key)
        {
            return triggers[key];
        }
    }
}