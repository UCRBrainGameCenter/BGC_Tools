using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BGC.StateMachine
{
    /// <summary>
    /// Contains data for a state machine with descriptive functions to improve code clarity.
    /// </summary>
    public class StateData
    {
        private readonly Dictionary<string, bool> initialBooleans = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> booleans = new Dictionary<string, bool>();
        private readonly HashSet<string> triggers = new HashSet<string>();

        public void Initialize()
        {
            Clear();

            foreach (var kvp in initialBooleans)
            {
                booleans.Add(kvp.Key, kvp.Value);
            }
        }

        public void Clear()
        {
            booleans.Clear();
            triggers.Clear();
        }

        public void AddBoolean(string key, bool initialValue)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            initialBooleans.Add(key, initialValue);
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

        public void ActivateTrigger(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            triggers.Add(key);
        }

        public void DeActivateTrigger(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            triggers.Remove(key);
        }

        public bool GetTrigger(string key)
        {
            Assert.IsFalse(string.IsNullOrEmpty(key));
            return triggers.Contains(key);
        }
    }
}