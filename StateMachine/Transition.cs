using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// This defines a transition between two states with the required 
    /// transition conditions for the transition to occur
    /// </summary>
    public class Transition<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        private readonly TransitionCondition<TBoolEnum, TTriggerEnum>[] transitionConditions;

        protected ITransitionDataRetriever<TBoolEnum, TTriggerEnum> stateMachine;

        /// <summary>
        /// Get name of the state this transition goes to
        /// </summary>
        public readonly State TargetState;

        /// <summary>
        /// Construct abstract transtion to define path
        /// </summary>
        public Transition(
            State targetState,
            params TransitionCondition<TBoolEnum, TTriggerEnum>[] transitionConditions)
        {
            TargetState = targetState ?? throw new ArgumentNullException(
                paramName: nameof(targetState),
                message: "Transition target state cannot be null.");

            this.transitionConditions = transitionConditions ?? throw new ArgumentNullException(
                paramName: nameof(transitionConditions),
                message: "Transition conditions cannot be null.");

            for (int i = 0; i < transitionConditions.Length; ++i)
            {
                if (transitionConditions[i] == null)
                {
                    throw new ArgumentNullException(
                        paramName: nameof(transitionConditions),
                        message: $"Transition conditions element {i} is null and should not be.");
                }
            }
        }

        /// <summary>
        /// Add state machine data which is required for checking info
        /// </summary>
        public void SetStateDataRetrievers(ITransitionDataRetriever<TBoolEnum, TTriggerEnum> stateMachine)
        {
            this.stateMachine = stateMachine ?? throw new ArgumentNullException(
                paramName: nameof(stateMachine),
                message: "stateMachine argument cannot be null.");

            for (int i = 0; i < transitionConditions.Length; ++i)
            {
                transitionConditions[i].SetStateMachineFunctions(stateMachine);
            }
        }

        /// <summary>
        /// Test whether or not this transition should occur
        /// </summary>
        /// <returns>True if a transition should occur</returns>
        public bool ShouldTransition()
        {
            bool shouldTransfer = true;
            for (int i = 0; i < transitionConditions.Length; ++i)
            {
                shouldTransfer &= transitionConditions[i].ShouldTransition();

                if (shouldTransfer == false)
                {
                    break;
                }
            }

            return shouldTransfer;
        }

        /// <summary>
        /// Right before a transition this function is called so the 
        /// transition and clean anything it has done up. For example:
        /// deactivating any triggers it has used is required
        /// </summary>
        public void OnTransition()
        {
            for (int i = 0; i < transitionConditions.Length; ++i)
            {
                transitionConditions[i].OnTransition();
            }
        }
    }
}