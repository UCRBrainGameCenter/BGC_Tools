using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// A trigger condition, similar to the bool condition, will transition
    /// while a trigger is active. Once consumed, it will not transition. On a
    /// transition, it will consume the trigger it is associated with.
    /// </summary>
    public class TriggerCondition : TransitionCondition
    {
        /// <summary>
        /// Key to check state data triggers
        /// </summary>
        private readonly string key;

        /// <summary>
        /// Construct a trigger condition with the key that will be checked in
        /// the triggers dictionary.
        /// </summary>
        /// <param name="key"></param>
        public TriggerCondition(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(
                    paramName: nameof(key),
                    message: "Trigger condition cannot have a null or empty key");
            }

            this.key = key;
        }

        /// <summary>
        /// On transition, a trigger will always be consumed
        /// </summary>
        public override void OnTransition()
        {
            consumeTrigger(key);
        }

        /// <summary>
        /// If the required trigger has been activated this will return true
        /// until it has been consumed
        /// </summary>
        /// <returns></returns>
        public override bool ShouldTransition()
        {
            return getTrigger(key);
        }

        protected override void StateMachineFunctionsSet()
        {
            // pass
        }
    }
}