using System;
using UnityEngine;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter, OnStateExit, and Update.
    /// </summary>
    public sealed class UpdatingLambdaState<TBoolEnum, TTriggerEnum> : State
    {
        private readonly Action onStateEnter;
        private readonly Action onStateExit;
        private readonly Action update;

        public UpdatingLambdaState(
            string name,
            Action onStateEnter = null,
            Action onStateExit = null,
            Action update = null)
            : base(name)
        {
            Debug.Assert(update != null);

            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
            this.update = update;
        }

        protected override void OnStateEnter() => onStateEnter?.Invoke();
        protected override void OnStateExit() => onStateExit?.Invoke();
        public override void Update() => update();
    }
}
