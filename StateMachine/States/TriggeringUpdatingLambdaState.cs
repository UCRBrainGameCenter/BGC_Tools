using System;
using UnityEngine;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter, OnStateExit, and Update.
    /// The lambdas return strings which, if not null, are fired off as Triggers
    /// </summary>
    public class TriggeringUpdatingLambdaState : State
    {
        private readonly Func<string> onStateEnter;
        private readonly Func<string> onStateExit;
        private readonly Func<string> update;

        public TriggeringUpdatingLambdaState(
            string name,
            Func<string> onStateEnter = null,
            Func<string> onStateExit = null,
            Func<string> update = null)
            : base(name)
        {
            Debug.Assert(update != null);

            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
            this.update = update;
        }

        protected override void OnStateEnter()
        {
            string trigger = onStateEnter?.Invoke();

            if (!string.IsNullOrEmpty(trigger))
            {
                ActivateTrigger(trigger);
            }
        }

        protected override void OnStateExit()
        {
            string trigger = onStateExit?.Invoke();

            if (!string.IsNullOrEmpty(trigger))
            {
                ActivateTrigger(trigger);
            }
        }

        public override void Update()
        {
            string trigger = update();

            if (!string.IsNullOrEmpty(trigger))
            {
                ActivateTrigger(trigger);
            }
        }
    }
}
