using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// A bool condition checks for booleans, similar to the trigger condition,
    /// but will not consume the boolean once it has been used. Instead it keeps
    /// the value exactly as it was when a transition occurs.
    /// </summary>
    public class BoolCondition<TBoolEnum, TTriggerEnum> : TransitionCondition<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        /// <summary>
        /// Key to access required boolean in state machine
        /// </summary>
        private readonly TBoolEnum key;

        /// <summary>
        /// Expected boolean value for when this condition should call for a
        /// transition
        /// </summary>
        private readonly bool val;

        /// <summary>
        /// Build a boolean condition that checks the state machine boolean 
        /// dictionary and will call for a transition when the expected value
        /// is found
        /// </summary>
        public BoolCondition(TBoolEnum key, bool val)
        {
            this.key = key;
            this.val = val;
        }

        /// <summary>
        /// Not used
        /// </summary>
        public override void OnTransition()
        {
            // pass
        }

        /// <summary>
        /// Returns true when the correct value specified during construction 
        /// is seen
        /// </summary>
        public override bool ShouldTransition() => stateMachine.GetBool(key) == val;

        /// <summary>
        /// Not used
        /// </summary>
        protected override void StateMachineFunctionsSet()
        {
            // pass
        }
    }
}