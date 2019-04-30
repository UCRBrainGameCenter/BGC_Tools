using System;

using UnityEngine.Assertions;

namespace BGC.StateMachine
{
    /// <summary>
    /// Defines a class that must be implemented which is for defining when
    /// transitions should occur. It also allows for work to be done when a
    /// transition cocurs
    /// </summary>
    public abstract class TransitionCondition<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        protected ITransitionDataRetriever<TBoolEnum, TTriggerEnum> stateMachine;

        /// <summary>
        /// Sets the required functions for the transition that it has received
        /// from the state machine.
        /// </summary>
        public void SetStateMachineFunctions(ITransitionDataRetriever<TBoolEnum, TTriggerEnum> stateMachine)
        {
            Assert.IsNotNull(stateMachine);

            this.stateMachine = stateMachine;
            StateMachineFunctionsSet();
        }

        /// <summary>
        /// Called before the transition for anything useful to be done such as
        /// consumeing triggers
        /// </summary>
        public abstract void OnTransition();

        /// <summary>
        /// Returns whether or not the condition has been met and a transition 
        /// should occur
        /// </summary>
        /// <returns></returns>
        public abstract bool ShouldTransition();

        /// <summary>
        /// A transition receives information from the state machine on 
        /// construction, however, the transition condition will send the
        /// functions to all it contains
        /// </summary>
        protected abstract void StateMachineFunctionsSet();
    }
}