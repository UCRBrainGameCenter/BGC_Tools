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
        /// <summary>
        /// Enumeration defining the two transition types. Regular is a 
        /// transition that has been defined between to states. Any is a
        /// transition that can happy between any state to the target.
        /// </summary>
        private enum TransitionType
        {
            Regular,
            Any,
            Max
        }

        private readonly Dictionary<string, List<Transition>> stateTransitions;
        private readonly List<Transition> anyStateTransitions;
        private readonly Dictionary<string, State> states;
        private readonly StateData stateData;
        private readonly bool verbose;

        private string entryState = null;

        /// <summary>
        /// Get the current state name that the state machine is in
        /// </summary>
        public string CurrentState
        {
            get;
            private set;
        }

        #region State Machine Construction
        /// <summary>
        /// Build a state machine with the option for it to be verbose in it's 
        /// state transitions or not
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
        /// <returns>True if the state was added</returns>
        public void AddState(State state)
        {
            states.Add(state.Name, state);
            state.SetStateMachineFunctions(this);
            state.SetVerbose(verbose);
            stateTransitions.Add(state.Name, new List<Transition>());
        }

        /// <summary>
        /// Add the state to where the game runs
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
        /// Add a transition to a state
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
        /// Add a transition that can occur on any state
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
        /// <returns>True if the boolean was added without error</returns>
        public void AddBool(string key, bool value)
        {
            stateData.AddBoolean(key, value);
        }

        /// <summary>
        /// Add a trigger that can cause the state machine to go to the next state
        /// </summary>
        /// <returns>True if the trigger was added</returns>
        public void AddTrigger(string key)
        {
            stateData.AddTrigger(key);
        }

        /// <summary>
        /// Activate a trigger to move the state machine forward
        /// Warning: Should only be called from State!
        /// </summary>
        /// <returns>True if the trigger was succesfully activated</returns>
        public void ActivateTrigger(string key)
        {
            stateData.ActivateTrigger(key);
            Transition();
        }

        /// <summary>
        /// Set a boolean that can affect transitions
        /// Warning: Should only be called from State!
        /// </summary>
        /// <returns>True if the boolean was set without error</returns>
        public void SetBool(string key, bool value)
        {
            stateData.SetBoolean(key, value);
            Transition();
        }

        /// <summary>
        /// Call update function on the currently active state.
        /// </summary>
        public void Update()
        {
            states[CurrentState].Update();
        }
        #endregion

        #region Running State Machine
        /// <summary>
        /// Run the first state of the state machine
        /// </summary>
        /// <returns>True if the state machine started</returns>
        public void Start()
        {
            if (entryState == null)
            {
                throw new InvalidOperationException("State machine must have an entry state defined to start.");
            }

            CurrentState = entryState;
            states[entryState].OnEnter();
        }

        /// <summary>
        /// Reset the state machine by exiting the current state and setting the
        /// state to the entry state. if restartStateMachine is set to true than
        /// entry state will be entered and the machine will have effectively
        /// restarted
        /// </summary>
        public void Reset(bool restartStateMachine = false)
        {
            states[CurrentState].OnExit();

            if (restartStateMachine)
            {
                Start();
            }
            else
            {
                CurrentState = entryState;
            }
        }

        /// <summary>
        /// Test to see if there is a valid transition and if there is then
        /// run that ransition.
        /// </summary>
        private void Transition()
        {
            Transition transition = null;

            //Check AnyState Transitions
            for (int i = 0; i < anyStateTransitions.Count; ++i)
            {
                if (anyStateTransitions[i].ShouldTransition())
                {
                    transition = anyStateTransitions[i];
                    break;
                }
            }

            //Check State Transitions (only if there's no valid AnyState Transition)
            if (transition == null)
            {
                for (int i = 0; i < stateTransitions[CurrentState].Count; ++i)
                {
                    if (stateTransitions[CurrentState][i].ShouldTransition())
                    {
                        transition = stateTransitions[CurrentState][i];
                        break;
                    }
                }
            }

            if (transition != null)
            {
                string activeState = CurrentState;
                CurrentState = transition.TargetState;
                transition.OnTransition();
                states[activeState].OnExit();
                states[CurrentState].OnEnter();

                // run transition again if we have transitioned so we can continue
                // through the statem achine until we reach a point to wait again
                Transition();
            }
        }
        #endregion

        #region IStateDataRetriever
        void IStateDataRetriever.ActivateTrigger(string key) => ActivateTrigger(key);
        bool IStateDataRetriever.GetTrigger(string key) => stateData.GetTrigger(key);
        bool IStateDataRetriever.GetBool(string key) => stateData.GetBoolean(key);
        void IStateDataRetriever.SetBool(string key, bool value) => SetBool(key, value);
        #endregion IStateDataRetriever

        #region ITransitionDataRetriever
        bool ITransitionDataRetriever.GetBool(string key) => stateData.GetBoolean(key);
        bool ITransitionDataRetriever.GetTrigger(string key) => stateData.GetTrigger(key);
        void ITransitionDataRetriever.ConsumeTrigger(string key) => stateData.DeActivateTrigger(key);
        #endregion ITransitionDataRetriever

        /// <summary>
        /// Log the string if verbose
        /// </summary>
        /// <param name="str"></param>
        private void Log(string str)
        {
            if (verbose)
            {
                Debug.Log(str);
            }
        }
    }
}