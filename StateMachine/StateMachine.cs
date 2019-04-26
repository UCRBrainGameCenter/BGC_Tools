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
    public class StateMachine<TBoolEnum, TTriggerEnum> :
        IStateDataBool<TBoolEnum>,
        IStateTrigger<TTriggerEnum>,
        ITransitionDataRetriever<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum 
    {
        private readonly Dictionary<State, List<Transition<TBoolEnum, TTriggerEnum>>> stateTransitions;
        private readonly List<Transition<TBoolEnum, TTriggerEnum>> anyStateTransitions;
        private readonly StateData<TBoolEnum, TTriggerEnum> stateData;
        private readonly bool verbose;

        private State entryState = null;

        private bool running = false;
        private bool dirtyTransitionState = false;
        private bool blockTransitions = false;

        /// <summary>
        /// Get the name of the current State that the StateMachine is in
        /// </summary>
        public State CurrentState { get; private set; }

        #region State Machine Construction
        /// <summary>
        /// Construct a StateMachine.
        /// Verbose dumps state transition information to UnityEngine.Log
        /// </summary>
        public StateMachine(bool verbose = false)
        {
            this.verbose = verbose;
            stateData = new StateData<TBoolEnum, TTriggerEnum>();
            stateTransitions = new Dictionary<State, List<Transition<TBoolEnum, TTriggerEnum>>>();
            anyStateTransitions = new List<Transition<TBoolEnum, TTriggerEnum>>();
        }

        /// <summary>
        /// Add a state to the state machine
        /// </summary>
        public void AddState(State state)
        {
            if (state is CoordinatingState<TBoolEnum, TTriggerEnum> coordinatingState)
            {
                AddState(coordinatingState);
            }
            else if (state is TriggeringState<TTriggerEnum> triggeringState)
            {
                AddState(triggeringState);
            }
            else
            {
                state.SetVerbose(verbose);
                stateTransitions.Add(state, new List<Transition<TBoolEnum, TTriggerEnum>>());
            }
        }

        public void AddState(TriggeringState<TTriggerEnum> state)
        {
            state.SetStateMachineFunctions(this);
            state.SetVerbose(verbose);
            stateTransitions.Add(state, new List<Transition<TBoolEnum, TTriggerEnum>>());
        }

        /// <summary>
        /// Add a coordinating state to the state machine
        /// </summary>
        /// <param name="state"></param>
        public void AddState(CoordinatingState<TBoolEnum, TTriggerEnum> state)
        {
            state.SetStateMachineFunctions(this, this);
            state.SetVerbose(verbose);
            stateTransitions.Add(state, new List<Transition<TBoolEnum, TTriggerEnum>>());
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
            entryState = state;
        }

        /// <summary>
        /// Add a coordinate state to the state machine that is the initial state
        /// </summary>
        /// <param name="state"></param>
        public void AddEntryState(CoordinatingState<TBoolEnum, TTriggerEnum> state)
        {
            if (entryState != null)
            {
                throw new ArgumentException(
                    message: $"{state.Name} cannot be made the entry state because " +
                        $"{entryState} was already defined as the entry state.",
                    paramName: nameof(entryState));
            }

            AddState(state);
            entryState = state;
        }

        /// <summary>
        /// Add a Standard Transition between two States
        /// </summary>
        public void AddTransition(
            State fromState,
            State targetState,
            params TransitionCondition<TBoolEnum, TTriggerEnum>[] conditions)
        {
            if (fromState == null)
            {
                throw new ArgumentNullException(nameof(fromState),
                    message: "Cannot add a transition that has an empty state");
            }

            Transition<TBoolEnum, TTriggerEnum> transition = new Transition<TBoolEnum, TTriggerEnum>(targetState, conditions);
            stateTransitions[fromState].Add(transition);
            transition.SetStateDataRetrievers(this);
        }

        /// <summary>
        /// Add a Transition that can occur from any state
        /// </summary>
        public void AddAnyStateTransition(
            State targetState, 
            params TransitionCondition<TBoolEnum, TTriggerEnum>[] conditions)
        {
            Transition<TBoolEnum, TTriggerEnum> transition = new Transition<TBoolEnum, TTriggerEnum>(targetState, conditions);
            transition.SetStateDataRetrievers(this);
            anyStateTransitions.Add(transition);
        }
        #endregion

        #region State Data Operations
        /// <summary>
        /// Add a boolean that can affect transitions
        /// </summary>
        public void AddBool(TBoolEnum key, bool initialValue) => stateData.AddBoolean(key, initialValue);

        /// <summary>
        /// Activate a trigger to move the state machine forward.
        /// Initiate a transition check if transitions aren't blocked.
        /// </summary>
        public void ActivateTriggerImmediate(TTriggerEnum key)
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
        public void ActivateTriggerDeferred(TTriggerEnum key)
        {
            Debug.Assert(running,
                "Activating Triggers when the StateMachine is not running will accomplish nothing");

            stateData.ActivateTrigger(key);
            dirtyTransitionState = true;
        }

        /// <summary>
        /// Set a boolean that can affect transitions.
        /// Initiate a transition check if transitions aren't blocked.
        /// </summary>
        public void SetBoolImmediate(TBoolEnum key, bool value)
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
        public void SetBoolDeferred(TBoolEnum key, bool value)
        {
            Debug.Assert(running,
                "Setting boolean values when the StateMachine is not running will accomplish nothing");

            stateData.SetBoolean(key, value);
            dirtyTransitionState = true;
        }

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
                CurrentState.Update();
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
                entryState.OnEnter();
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
                CurrentState.OnExit();
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
        private Transition<TBoolEnum, TTriggerEnum> CheckTransitions()
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

            Transition<TBoolEnum, TTriggerEnum> transition;

            while ((transition = CheckTransitions()) != null)
            {
                transition.OnTransition();
                CurrentState.OnExit();
                CurrentState = transition.TargetState;
                CurrentState.OnEnter();
            }

            blockTransitions = false;
        }
        #endregion

        #region IStateTrigger
        void IStateTrigger<TTriggerEnum>.ActivateTrigger(TTriggerEnum key) => ActivateTriggerDeferred(key);
        bool IStateTrigger<TTriggerEnum>.GetTrigger(TTriggerEnum key) => stateData.GetTrigger(key);
        #endregion IStateTrigger

        #region IStateDataBool
        void IStateDataBool<TBoolEnum>.SetBool(TBoolEnum key, bool value) => SetBoolDeferred(key, value);
        bool IStateDataBool<TBoolEnum>.GetBool(TBoolEnum key) => stateData.GetBoolean(key);
        #endregion IStateDataBool

        #region ITransitionDataRetriever
        bool ITransitionDataRetriever<TBoolEnum, TTriggerEnum>.GetBool(TBoolEnum key) => stateData.GetBoolean(key);
        bool ITransitionDataRetriever<TBoolEnum, TTriggerEnum>.GetTrigger(TTriggerEnum key) => stateData.GetTrigger(key);
        void ITransitionDataRetriever<TBoolEnum, TTriggerEnum>.ConsumeTrigger(TTriggerEnum key) => stateData.DeActivateTrigger(key);
        #endregion ITransitionDataRetriever

        #region Factory Methods
        public TriggerCondition<TBoolEnum, TTriggerEnum> CreateTriggerCondition(TTriggerEnum key)
        {
            return new TriggerCondition<TBoolEnum, TTriggerEnum>(key);
        }

        public BoolCondition<TBoolEnum, TTriggerEnum> CreateBoolCondition(TBoolEnum key, bool value)
        {
            return new BoolCondition<TBoolEnum, TTriggerEnum>(key, value);
        }

        public OrConjunction<TBoolEnum, TTriggerEnum> CreateOrConjunction(params TransitionCondition<TBoolEnum, TTriggerEnum>[] conditions)
        {
            return new OrConjunction<TBoolEnum, TTriggerEnum>(conditions);
        }
        #endregion Factory Methods

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