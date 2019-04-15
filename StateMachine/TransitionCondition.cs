using System;

using UnityEngine.Assertions;

namespace BGC.StateMachine
{
    /// <summary>
    /// Defines a class that must be implemented which is for defining when
    /// transitions should occur. It also allows for work to be done when a
    /// transition cocurs
    /// </summary>
    public abstract class TransitionCondition
    {
        protected Func<string, bool> getBool;
        protected Func<string, bool> getTrigger;
        protected Action<string> consumeTrigger;

        /// <summary>
        /// Sets the required functions for the transition that it has received
        /// from the state machine.
        /// </summary>
        /// <param name="getBool"></param>
        /// <param name="getTrigger"></param>
        /// <param name="consumeTrigger"></param>
        public void SetStateMachineFunctions(
            Func<string, bool> getBool, 
            Func<string, bool> getTrigger, 
            Action<string> consumeTrigger)
        {
            // These assert checks are here for safety despite us being all but
            // guaranteed that the functions will always be valid because we 
            // have received them from the state machine
            Assert.IsNotNull(consumeTrigger);
            Assert.IsNotNull(getTrigger);
            Assert.IsNotNull(getBool);

            this.consumeTrigger = consumeTrigger;
            this.getTrigger = getTrigger;
            this.getBool = getBool;

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