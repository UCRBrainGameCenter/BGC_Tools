using System;
using UnityEngine;

namespace BGC.StateMachine
{
    public abstract class State
    {
        protected abstract string DefaultName { get; }
        private string name;
        private bool verbose = false;

        // these functions are called in implementations to stop the user from 
        // accidentally modifying them
        private Action<string> activateTrigger;
        private Action<string, bool> setBool;
        private Func<string, bool> getBool;
        private Func<string, bool> getTrigger;

        /// <summary>
        /// Name of the state. This will either be user defined or the default state
        /// name depending on the constructor used
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Create a state with the default name
        /// </summary>
        public State()
        {
            name = DefaultName;
        }

        /// <summary>
        /// Create a state with a custom name rather than the default
        /// </summary>
        /// <param name="name"></param>
        public State(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Called when the state is entered
        /// </summary>
        public abstract void OnEnter();

        /// <summary>
        /// Called when the state is exited before the next state is entered
        /// </summary>
        public abstract void OnExit();

        /// <summary>
        /// Receive state machine related functions that give states required behaviour
        /// </summary>
        /// <param name="activateTrigger"></param>
        /// <param name="getTrigger"></param>
        /// <param name="getBool"></param>
        /// <param name="setBool"></param>
        public void SetStateMachineFunctions(
            Action<string> activateTrigger,
            Func<string, bool> getTrigger,
            Func<string, bool> getBool,
            Action<string, bool> setBool)
        {
            this.activateTrigger = activateTrigger;
            this.getTrigger = getTrigger;
            this.setBool = setBool;
            this.getBool = getBool;

            if (this.activateTrigger == null)
            {
                throw new ArgumentNullException("activateTrigger function cannot be null.");
            }

            if (this.getTrigger == null)
            {
                throw new ArgumentNullException("getTrigger function cannot be null");
            }

            if (this.setBool == null)
            {
                throw new ArgumentNullException("setBool function cannot be null");
            }

            if (this.getBool == null)
            {
                throw new ArgumentNullException("getBool function cannot be null");
            }
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
        /// Activate a trigger in the state machine this state is apart of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected void ActivateTrigger(string key)
        {
            activateTrigger(key);
        }

        /// <summary>
        /// Set a bool in the state machine this state is apart of
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        protected void SetBool(string key, bool val)
        {
            setBool(key, val);
        }

        /// <summary>
        /// Get a bool from the state machine this state is apart of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool GetBool(string key)
        {
            return getBool(key);
        }

        /// <summary>
        /// Get a trigger from the state machine this state is apart of
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected bool GetTrigger(string key)
        {
            return getTrigger(key);
        }

        /// <summary>
        /// Call when a function a state has been entered
        /// </summary>
        protected void LogOnEnter()
        {
            if (verbose)
            {
                Debug.Log(Name + " entered.");
            }
        }

        /// <summary>
        /// Call when a statre has ben exit
        /// </summary>
        protected void LogOnExit()
        {
            if (verbose)
            {
                Debug.Log(Name + " left.");
            }
        }
    }
}