using System;

namespace BGC.StateMachine
{
    public abstract class CoordinatingState<TBoolEnum, TTriggerEnum> : State
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        private IStateTrigger<TTriggerEnum> stateTriggers;
        private IStateDataBool<TBoolEnum> stateBooleans;

        /// <summary>
        /// Create a coordinating state with the default name
        /// </summary>
        public CoordinatingState() : base()
        {
        }

        /// <summary>
        /// Create a coordinate state with a custom name rather than the default
        /// </summary>
        public CoordinatingState(string name) : base(name)
        {
        }

        /// <summary>
        /// Receive state machine related functions that give states required behaviour
        /// </summary>
        public void SetStateMachineFunctions(IStateDataBool<TBoolEnum> booleans, IStateTrigger<TTriggerEnum> triggers)
        {
            stateTriggers = triggers ?? throw new ArgumentNullException(
                paramName: nameof(triggers),
                message: "stateMachine cannot be null.");

            stateBooleans = booleans ?? throw new ArgumentNullException(
                paramName: nameof(booleans),
                message: "stateMachine cannot be null.");
        }

        /// <summary>
        /// Activate a trigger in the state machine this state is a part of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected void ActivateTrigger(TTriggerEnum key) => stateTriggers.ActivateTrigger(key);

        /// <summary>
        /// Set a bool in the state machine this state is a part of
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected void SetBool(TBoolEnum key, bool val) => stateBooleans.SetBool(key, val);

        /// <summary>
        /// Get a bool from the state machine this state is a part of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool GetBool(TBoolEnum key) => stateBooleans.GetBool(key);
    }
}
