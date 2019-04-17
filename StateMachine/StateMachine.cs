using System.Collections.Generic;
using System;
using UnityEngine;

namespace BGC.StateMachine
{
    /// <summary>
    /// Implements a state machine that can be constructed programatically that
    /// is similar to the Unity animation state machine but not frame capped.
    /// 
    /// To use the update function, put the Update call in a MonoBehavior class
    /// Update call.
    /// </summary>
    public class StateMachine : IStateDataRetriever, ITransitionDataRetriever
    {
        private readonly Dictionary<string, List<Transition>> stateTransitions;
        private readonly List<Transition> anyStateTransitions;
        private readonly Dictionary<string, State> states;
        private readonly StateData stateData;
        private readonly bool verbose;

        private string entryState = null;

        private bool running = false;
        private bool dirtyTransitionState = false;
        private bool blockTransitions = false;

        /// <summary>
        /// Get the name of the current State that the StateMachine is in
        /// </summary>
        public string CurrentState { get; private set; }

        #region State Machine Construction
        /// <summary>
        /// Construct a StateMachine.
        /// Verbose dumps state transition information to UnityEngine.Log
        /// </summary>
        public StateMachine(bool verbose = false)
        {
            this.verbose = verbose;
            stateData = new StateData();
            states = new Dictionary<string, State>();
            stateTransitions = new Dictionary<string, List<Transition>>();
            anyStateTransitions = new List<Transition>();
        }

        /// <summary>
        /// Add a state to the state machine
        /// </summary>
        public void AddState(State state)
        {
            states.Add(state.Name, state);
            state.SetStateMachineFunctions(this);
            state.SetVerbose(verbose);
            stateTransitions.Add(state.Name, new List<Transition>());
        }

        /// <summary>
        /// Add a state to the StateMachine and sets it as the initial State
        /// </summary>
        public void AddEntryState(State state)
        {
            if (entryState != null)
            {
                throw new ArgumentException(
                    message: $"{state.Name} cannot be made the entry state because " +
                        $"{entryState} was already defined as the entry state.",
                    paramName: nameof(entryState));
            }

            AddState(state);
            entryState = state.Name;
        }

        /// <summary>
        /// Add a Standard Transition between two States
        /// </summary>
        public void AddTransition(
            State fromState,
            State targetState,
            params TransitionCondition[] conditions)
        {
            if (fromState == null)
            {
                throw new ArgumentNullException(nameof(fromState),
                    message: "Cannot add a transition that has an empty state");
            }

            Transition transition = new Transition(targetState, conditions);
            stateTransitions[fromState.Name].Add(transition);
            transition.SetStateDataRetrievers(this);
        }

        /// <summary>
        /// Add a Transition that can occur from any state
        /// </summary>
        public void AddAnyStateTransition(State targetState, params TransitionCondition[] conditions)
        {
            Transition transition = new Transition(targetState, conditions);
            transition.SetStateDataRetrievers(this);
            anyStateTransitions.Add(transition);
        }
        #endregion

        #region State Data Operations
        /// <summary>
        /// Add a boolean that can affect transitions
        /// </summary>
        public void AddBool(string key, bool initialValue) => stateData.AddBoolean(key, initialValue);

        /// <summary>
        /// Add a trigger that can cause the state machine to go to the next state
        /// </summary>
        [Obsolete("Triggers no longer need to be added to the stateData.  This function does nothing.")]
        public void AddTrigger(string key) { }

        /// <summary>
        /// Activate a trigger to move the state machine forward.
        /// Initiate a transition check if transitions aren't blocked.
        /// </summary>
        public void ActivateTriggerImmediate(string key)
        {
            Debug.Assert(running,
                "Activating Triggers when the StateMachine is not running will accomplish nothing");

            stateData.ActivateTrigger(key);

            if (!blockTransitions)
            {
                ExecuteTransitions();
            }
            else
            {
                dirtyTransitionState = true;
            }
        }

        /// <summary>
        /// Activate a trigger to move the state machine forward.
        /// Relies upon the eventual calling of either an Update or an Immediate call.
        /// </summary>
        public void ActivateTriggerDeferred(string key)
        {
            Debug.Assert(running,
                "Activating Triggers when the StateMachine is not running will accomplish nothing");

            stateData.ActivateTrigger(key);
            dirtyTransitionState = true;
        }

        /// <summary>
        /// Activate a trigger to move the state machine forward
        /// </summary>
        [Obsolete("Indicate the immediacy of the ActivateTrigger call")]
        public void ActivateTrigger(string key) => ActivateTriggerImmediate(key);

        /// <summary>
        /// Set a boolean that can affect transitions.
        /// Initiate a transition check if transitions aren't blocked.
        /// </summary>
        public void SetBoolImmediate(string key, bool value)
        {
            Debug.Assert(running, 
                "Setting boolean values when the StateMachine is not running will accomplish nothing");

            stateData.SetBoolean(key, value);

            if (!blockTransitions)
            {
                ExecuteTransitions();
            }
            else
            {
                dirtyTransitionState = true;
            }
        }

        /// <summary>
        /// Set a boolean that can affect transitions.
        /// Relies upon the eventual calling of either an Update or an Immediate call.
        /// </summary>
        public void SetBoolDeferred(string key, bool value)
        {
            Debug.Assert(running,
                "Setting boolean values when the StateMachine is not running will accomplish nothing");

            stateData.SetBoolean(key, value);
            dirtyTransitionState = true;
        }

        /// <summary>
        /// Set a boolean that can affect transitions
        /// </summary>
        [Obsolete("Indicate the immediacy of the SetBool call")]
        public void SetBool(string key, bool value) => SetBoolImmediate(key, value);

        /// <summary>
        /// Call update function on the currently active state.
        /// </summary>
        public void Update()
        {
            if (!running)
            {
                return;
            }

            //Pre-Update check for Transitions
            if (dirtyTransitionState)
            {
                ExecuteTransitions();
            }

            //Block mid-update transitions
            {
                blockTransitions = true;
                //Update current state
                states[CurrentState].Update();
                blockTransitions = false;
            }

            //Post-Update check for Transitions
            if (dirtyTransitionState)
            {
                ExecuteTransitions();
            }
        }
        #endregion

        #region Running State Machine
        /// <summary>
        /// Run the first state of the state machine
        /// </summary>
        public void Start()
        {
            if (entryState == null)
            {
                throw new InvalidOperationException("State machine must have an entry state defined to start.");
            }

            CurrentState = entryState;
            running = true;
            dirtyTransitionState = false;
            stateData.Initialize();

            //Block transitions during OnEnter
            {
                blockTransitions = true;
                states[entryState].OnEnter();
                blockTransitions = false;
            }

            //Execute any transitions
            ExecuteTransitions();
        }

        /// <summary>
        /// Reset the state machine by exiting the current state and setting the
        /// state to the entry state. 
        /// If restartStateMachine is set to true then entry state will be entered and the
        /// StateMachine will have effectively restarted
        /// </summary>
        public void Reset(bool restartStateMachine = false)
        {
            if (running)
            {
                //Prevent escape of current state
                blockTransitions = true;
                states[CurrentState].OnExit();
                blockTransitions = false;
            }

            CurrentState = null;
            running = false;
            dirtyTransitionState = false;
            stateData.Clear();

            if (restartStateMachine)
            {
                Start();
            }
        }

        /// <summary>
        /// Test to see if there is a valid transitions and return it, or null.
        /// Clears the dirtyTransitionState flag.
        /// </summary>
        private Transition CheckTransitions()
        {
            dirtyTransitionState = false;

            //Check AnyState Transitions First
            for (int i = 0; i < anyStateTransitions.Count; ++i)
            {
                if (anyStateTransitions[i].ShouldTransition())
                {
                    return anyStateTransitions[i];
                }
            }

            //Check State Transitions Next
            for (int i = 0; i < stateTransitions[CurrentState].Count; ++i)
            {
                if (stateTransitions[CurrentState][i].ShouldTransition())
                {
                    return stateTransitions[CurrentState][i];
                }
            }

            //No valid Transition found
            return null;
        }

        /// <summary>
        /// Run transitions as long as we have a valid one to perform next.
        /// Blocks immediate transitions from occuring.
        /// </summary>
        private void ExecuteTransitions()
        {
            blockTransitions = true;

            Transition transition;

            while ((transition = CheckTransitions()) != null)
            {
                transition.OnTransition();
                states[CurrentState].OnExit();
                CurrentState = transition.TargetState;
                states[CurrentState].OnEnter();
            }

            blockTransitions = false;
        }
        #endregion

        #region IStateDataRetriever
        void IStateDataRetriever.ActivateTrigger(string key) => ActivateTriggerDeferred(key);
        bool IStateDataRetriever.GetTrigger(string key) => stateData.GetTrigger(key);
        bool IStateDataRetriever.GetBool(string key) => stateData.GetBoolean(key);
        void IStateDataRetriever.SetBool(string key, bool value) => SetBoolDeferred(key, value);
        #endregion IStateDataRetriever

        #region ITransitionDataRetriever
        bool ITransitionDataRetriever.GetBool(string key) => stateData.GetBoolean(key);
        bool ITransitionDataRetriever.GetTrigger(string key) => stateData.GetTrigger(key);
        void ITransitionDataRetriever.ConsumeTrigger(string key) => stateData.DeActivateTrigger(key);
        #endregion ITransitionDataRetriever

        /// <summary>
        /// Log the string if verbose
        /// </summary>
        private void Log(string str)
        {
            if (verbose)
            {
                Debug.Log(str);
            }
        }
    }
}