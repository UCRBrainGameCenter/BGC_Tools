using System;

namespace BGC.StateMachine
{
    public abstract class TriggeringState<TTriggerEnum> : State
        where TTriggerEnum : Enum 
    {
        private IStateTrigger<TTriggerEnum> stateTriggers;

        /// <summary>
        /// Create a coordinating state with the default name
        /// </summary>
        public TriggeringState() : base()
        {
        }

        /// <summary>
        /// Create a coordinate state with a custom name rather than the default
        /// </summary>
        public TriggeringState(string name) : base(name)
        {
        }

        /// <summary>
        /// Receive state machine related functions that give states required behaviour
        /// </summary>
        public void SetStateMachineFunctions(IStateTrigger<TTriggerEnum> triggers)
        {
            stateTriggers = triggers ?? throw new ArgumentNullException(
                paramName: nameof(triggers),
                message: "stateMachine cannot be null.");
        }

        /// <summary>
        /// Activate a trigger in the state machine this state is a part of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected void ActivateTrigger(TTriggerEnum key) => stateTriggers.ActivateTrigger(key);
    }
}