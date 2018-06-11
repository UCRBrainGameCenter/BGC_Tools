using System.Collections.Generic;
using System;
using UnityEngine;

namespace BGC.StateMachine
{
    public class StateMachine
    {
        private enum TransitionType
        {
            Regular,
            Any,
            Max
        }

        #region State Machine Construction
        private bool verbose;
        private string currentState;
        private string entryState = null;
        private StateData stateData;
        private Dictionary<string, State> states;
        private Dictionary<string, List<Transition>> stateTransitions;
        private List<Transition> anyStateTransitions;

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
        /// <param name="state"></param>
        /// <returns>True if the state was added</returns>
        public void AddState(State state)
        {
            states.Add(state.Name, state);
            state.SetStateMachineFunctions(ActivateTrigger, GetTrigger, GetBool, SetBool);
            state.SetVerbose(verbose);
            stateTransitions.Add(state.Name, new List<Transition>());
        }

        /// <summary>
        /// Add the state to where the game runs
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public void AddEntryState(State state)
        {
            if (entryState != null)
            {
                throw new ArgumentException(
                    $"{state.Name} cannot be made the entry state because " +
                    $"{entryState} was already defined as the entry state.");
            }

            AddState(state);
            entryState = state.Name;
        }

        /// <summary>
        /// Add a transition to a state
        /// </summary>
        /// <param name="transition"></param>
        public void AddTransition(State fromState, State targetState, params TransitionCondition[] conditions)
        {
            if (fromState == null)
            {
                throw new ArgumentNullException("Cannot add a transition that has an empty state");
            }

            Transition transition = new Transition(targetState, conditions);
            stateTransitions[fromState.Name].Add(transition);
            transition.SetStateDataRetrievers(GetBool, GetTrigger, stateData.DeActivateTrigger);
        }

        /// <summary>
        /// Add a transition that can occur on any state
        /// </summary>
        /// <param name="transition"></param>
        public void AddAnyStateTransition(State targetState, params TransitionCondition[] conditions)
        {
            Transition transition = new Transition(targetState, conditions);
            transition.SetStateDataRetrievers(GetBool, GetTrigger, stateData.DeActivateTrigger);
            anyStateTransitions.Add(transition);
        }
        #endregion

        #region State Data Operations
        /// <summary>
        /// Add a boolean that can affect transitions
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if the boolean was added without error</returns>
        public void AddBool(string key, bool value)
        {
            stateData.AddBoolean(key, value);
        }

        /// <summary>
        /// Get a boolean that can affect transitions
        /// Warning: Should only be called from State!
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Value of the key. False is default if value is not found.</returns>
        public bool GetBool(string key)
        {
            return stateData.GetBoolean(key);
        }

        /// <summary>
        /// Add a trigger that can cause the state machine to go to the next state
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if the trigger was added</returns>
        public void AddTrigger(string key)
        {
            stateData.AddTrigger(key);
        }

        /// <summary>
        /// Activate a trigger to move the state machine forward
        /// Warning: Should only be called from State!
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True if the trigger was succesfully activated</returns>
        public void ActivateTrigger(string key)
        {
            stateData.ActivateTrigger(key);
            transition();
        }

        /// <summary>
        /// Set a boolean that can affect transitions
        /// Warning: Should only be called from State!
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>True if the boolean was set without error</returns>
        public void SetBool(string key, bool value)
        {
            stateData.SetBoolean(key, value);
            transition();
        }

        /// <summary>
        /// Gets a trigger value
        /// Warning: Should only be called from State!
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetTrigger(string key)
        {
            return stateData.GetTrigger(key);
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

            currentState = entryState;
            states[entryState].OnEnter();
        }

        public void Reset()
        {
            states[currentState].OnExit();
            currentState = entryState;
        }

        public string CurrentState()
        {
            return currentState;
        }

        private Transition shouldTransition()
        {
            for (int i = 0; i < anyStateTransitions.Count; ++i)
            {
                if (anyStateTransitions[i].ShouldTransition())
                {
                    return anyStateTransitions[i];
                }
            }

            for (int i = 0; i < stateTransitions[currentState].Count; ++i)
            {
                if (stateTransitions[currentState][i].ShouldTransition())
                {
                    return stateTransitions[currentState][i];
                }
            }

            return null;
        }

        private void transition()
        {
            Transition t = shouldTransition();

            if (t != null)
            {
                string activeState = currentState;
                currentState = t.TargetState;
                t.OnTransition();
                states[activeState].OnExit();
                states[currentState].OnEnter();

                // run transition again if we have transitioned so we can continue
                // through the statem achine until we reach a point to wait again
                transition();
            }
        }
        #endregion

        /// <summary>
        /// Log the string if verbose
        /// </summary>
        /// <param name="str"></param>
        private void log(string str)
        {
            if (verbose)
            {
                Debug.Log(str);
            }
        }
    }
}