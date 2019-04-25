using System;
using System.Collections.Generic;

namespace BGC.StateMachine
{
    /// <summary>
    /// Contains data for a state machine with descriptive functions to improve code clarity.
    /// </summary>
    public class StateData<BoolT, TriggerT>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        private readonly Dictionary<BoolT, bool> initialBooleans = new Dictionary<BoolT, bool>();
        private readonly Dictionary<BoolT, bool> booleans = new Dictionary<BoolT, bool>();
        private readonly HashSet<TriggerT> triggers = new HashSet<TriggerT>();

        /// <summary>
        ///  initialize data with original booleans 
        /// </summary>
        public void Initialize()
        {
            Clear();

            foreach (KeyValuePair<BoolT, bool> boolRow in initialBooleans)
            {
                booleans.Add(boolRow.Key, boolRow.Value);
            }
        }

        public void Clear()
        {
            booleans.Clear();
            triggers.Clear();
        }

        public void AddBoolean(BoolT key, bool initialValue)
        {
            initialBooleans.Add(key, initialValue);
        }

        public void SetBoolean(BoolT key, bool value)
        {
            booleans[key] = value;
        }

        public bool GetBoolean(BoolT key)
        {
            return booleans[key];
        }

        public void ActivateTrigger(TriggerT key)
        {
            triggers.Add(key);
        }

        public void DeActivateTrigger(TriggerT key)
        {
            triggers.Remove(key);
        }

        public bool GetTrigger(TriggerT key)
        {
            return triggers.Contains(key);
        }

        internal bool GetTrigger<TTriggerEnum>(TTriggerEnum key)
        {
            throw new NotImplementedException();
        }
    }
}