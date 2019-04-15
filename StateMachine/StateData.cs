using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BGC.StateMachine
{
    /// <summary>
    /// Contains data for a state machine with descriptive functions to improve
    /// code clarity. Likely to be removed in the future.
    /// </summary>
    public class StateData
    {
        private readonly Dictionary<string, bool> booleans = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> triggers = new Dictionary<string, bool>();

        public void AddBoolean(string key, bool value)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            booleans.Add(key, value);
        }

        public void SetBoolean(string key, bool value)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            booleans[key] = value;
        }

        public bool GetBoolean(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            return booleans[key];
        }

        public void AddTrigger(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            triggers.Add(key, false);
        }

        public void ActivateTrigger(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            triggers[key] = true;
        }

        public void DeActivateTrigger(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            triggers[key] = false;
        }

        public bool GetTrigger(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            return triggers[key];
        }
    }
}