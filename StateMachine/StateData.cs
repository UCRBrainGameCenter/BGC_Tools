using System.Collections.Generic;
using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// Contains data for a state machine with descriptive functions to improve code clarity.
    /// </summary>
    public class StateData<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        private readonly Dictionary<TBoolEnum, bool> initialBooleans = new Dictionary<TBoolEnum, bool>();
        private readonly Dictionary<TBoolEnum, bool> booleans = new Dictionary<TBoolEnum, bool>();
        private readonly HashSet<TTriggerEnum> triggers = new HashSet<TTriggerEnum>();

        /// <summary>
        ///  initialize data with original booleans 
        /// </summary>
        public void Initialize()
        {
            Clear();

            foreach (KeyValuePair<TBoolEnum, bool> boolRow in initialBooleans)
            {
                booleans.Add(boolRow.Key, boolRow.Value);
            }
        }

        public void Clear()
        {
            booleans.Clear();
            triggers.Clear();
        }

        public void AddBoolean(TBoolEnum key, bool initialValue)
        {
            initialBooleans.Add(key, initialValue);
        }

        public void SetBoolean(TBoolEnum key, bool value)
        {
            booleans[key] = value;
        }

        public bool GetBoolean(TBoolEnum key)
        {
            return booleans[key];
        }

        public void ActivateTrigger(TTriggerEnum key)
        {
            triggers.Add(key);
        }

        public void DeActivateTrigger(TTriggerEnum key)
        {
            triggers.Remove(key);
        }

        public bool GetTrigger(TTriggerEnum key)
        {
            return triggers.Contains(key);
        }
    }
}