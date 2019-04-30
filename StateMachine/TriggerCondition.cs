using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// A trigger condition, similar to the bool condition, will transition
    /// while a trigger is active. Once consumed, it will not transition. On a
    /// transition, it will consume the trigger it is associated with.
    /// </summary>
    public class TriggerCondition<TBoolEnum, TTriggerEnum> : 
        TransitionCondition<TBoolEnum, TTriggerEnum> 
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        /// <summary>
        /// Key to check state data triggers
        /// </summary>
        private readonly TTriggerEnum key;

        /// <summary>
        /// Construct a trigger condition with the key that will be checked in
        /// the triggers dictionary.
        /// </summary>
        public TriggerCondition(TTriggerEnum key)
        {
            this.key = key;
        }

        /// <summary>
        /// On transition, a trigger will always be consumed
        /// </summary>
        public override void OnTransition() => stateMachine.ConsumeTrigger(key);

        /// <summary>
        /// If the required trigger has been activated this will return true
        /// until it has been consumed
        /// </summary>
        public override bool ShouldTransition() => stateMachine.GetTrigger(key);

        protected override void StateMachineFunctionsSet()
        {
            // pass
        }
    }
}