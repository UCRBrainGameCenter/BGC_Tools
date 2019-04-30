using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// An or conjunction is a set of transitions conditions where they are
    /// tested together with the logical or operator. If one transition
    /// condition returns true, then this entire conjunction will also return
    /// true.
    /// </summary>
    public class OrConjunction<TBoolEnum, TTriggerEnum> : TransitionCondition<TBoolEnum, TTriggerEnum> 
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        /// <summary>
        /// Set of required conditions for a transition to be called
        /// </summary>
        private readonly TransitionCondition<TBoolEnum, TTriggerEnum>[] conditions;

        /// <summary>
        /// Construct an or conjuction which operates as a set of boolean results
        /// with an or between each. This function sets the conditions and does
        /// error checking for null values.
        /// </summary>
        public OrConjunction(params TransitionCondition<TBoolEnum, TTriggerEnum>[] conditions)
        {
            if (conditions == null)
            {
                throw new ArgumentNullException(nameof(conditions),
                    message: "OrConjunction conditions canot be null.");
            }

            for (int i = 0; i < conditions.Length; ++i)
            {
                if (conditions[i] == null)
                {
                    throw new ArgumentNullException(nameof(conditions),
                        message: $"OrConjunction conditions element {i} is null and should not be.");
                }
            }

            this.conditions = conditions;
        }

        /// <summary>
        /// On Transition, all conditions are notified of the transition.
        /// </summary>
        public override void OnTransition()
        {
            for (int i = 0; i < conditions.Length; ++i)
            {
                if (conditions[i].ShouldTransition())
                {
                    conditions[i].OnTransition();
                    break;
                }
            }
        }

        /// <summary>
        /// Returns true as long as one state returns that a transition should
        /// happen
        /// </summary>
        public override bool ShouldTransition()
        {
            bool shouldTransition = false;
            for (int i = 0; i < conditions.Length; ++i)
            {
                if (conditions[i].ShouldTransition())
                {
                    shouldTransition = true;
                    break;
                }
            }

            return conditions.Length == 0 ? true : shouldTransition;
        }

        /// <summary>
        /// Calls this function on every state to give them their required
        /// functionality.
        /// </summary>
        protected override void StateMachineFunctionsSet()
        {
            for (int i = 0; i < conditions.Length; ++i)
            {
                conditions[i].SetStateMachineFunctions(stateMachine);
            }
        }
    }
}