using System;
using UnityEngine;

namespace BGC.StateMachine
{
    /// <summary>
    /// This class is implemented by states that are use din game. It comes with
    /// access to state machine related functions once it has been added to the
    /// machine. Users must implement OnEnter and OnExit. Users can override
    /// Update if they have a function they want to call on a frame by frame
    /// basis.
    /// </summary>
    public abstract class State<TBoolEnum, TTriggerEnum> where TBoolEnum : Enum where TTriggerEnum : Enum
    {
        protected virtual string DefaultName => "State";
        private bool verbose = false;

        private IStateDataRetriever<TBoolEnum, TTriggerEnum> stateMachine;

        /// <summary>
        /// Name of the state. This will either be user defined or the default state
        /// name depending on the constructor used
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Create a state with the default name
        /// </summary>
        public State()
        {
            Debug.Assert(!string.IsNullOrEmpty(DefaultName));
            Name = DefaultName;
        }

        /// <summary>
        /// Create a state with a custom name rather than the default
        /// </summary>
        public State(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Name = name;
        }

        /// <summary>
        /// Called when the state is entered
        /// </summary>
        public void OnEnter()
        {
            if (verbose)
            {
                Debug.Log($"{Name} entered.");
            }

            OnStateEnter();
        }

        /// <summary>
        /// Called when the state is exited, before the next state is entered
        /// </summary>
        public void OnExit()
        {
            if (verbose)
            {
                Debug.Log($"{Name} left.");
            }

            OnStateExit();
        }

        /// <summary>
        /// Called when the state is entered
        /// </summary>
        protected abstract void OnStateEnter();

        /// <summary>
        /// Called when the state is exited before the next state is entered
        /// </summary>
        protected virtual void OnStateExit() { }

        /// <summary>
        /// This can be called every frame or whenever for complex states that
        /// have behavior on a frame by X basis.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Receive state machine related functions that give states required behaviour
        /// </summary>
        public void SetStateMachineFunctions(IStateDataRetriever<TBoolEnum, TTriggerEnum> stateMachine)
        {
            this.stateMachine = stateMachine ?? throw new ArgumentNullException(
                paramName: nameof(stateMachine),
                message: "stateMachine cannot be null.");
        }

        /// <summary>
        /// Set whether the state machine is verbose or not
        /// </summary>
        /// <param name="isVerbose"></param>
        public void SetVerbose(bool isVerbose)
        {
            verbose = isVerbose;
        }

        /// <summary>
        /// Activate a trigger in the state machine this state is a part of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected void ActivateTrigger(TTriggerEnum key) => stateMachine.ActivateTrigger(key);

        /// <summary>
        /// Set a bool in the state machine this state is a part of
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected void SetBool(TBoolEnum key, bool val) => stateMachine.SetBool(key, val);

        /// <summary>
        /// Get a bool from the state machine this state is a part of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool GetBool(TBoolEnum key) => stateMachine.GetBool(key);
    }
}