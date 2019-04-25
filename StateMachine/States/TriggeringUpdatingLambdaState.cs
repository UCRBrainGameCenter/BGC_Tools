using System;
using UnityEngine;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter, OnStateExit, and Update.
    /// The lambdas return strings which, if not null, are fired off as Triggers
    /// </summary>
    public class TriggeringUpdatingLambdaState<TBoolEnum, TTriggerEnum> : State<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        private readonly Func<TTriggerEnum> onStateEnter;
        private readonly Func<TTriggerEnum> onStateExit;
        private readonly Func<TTriggerEnum> update;

        public TriggeringUpdatingLambdaState(
            string name,
            Func<TTriggerEnum> onStateEnter = null,
            Func<TTriggerEnum> onStateExit = null,
            Func<TTriggerEnum> update = null)
            : base(name)
        {
            Debug.Assert(update != null);

            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
            this.update = update;
        }

        protected override void OnStateEnter()
        {
            if (onStateEnter != null)
            {
                TTriggerEnum trigger = onStateEnter.Invoke();
                ActivateTrigger(trigger);
            }
        }

        protected override void OnStateExit()
        {
            if (onStateExit != null)
            {
                TTriggerEnum trigger = onStateExit.Invoke();
                ActivateTrigger(trigger);
            }
        }

        public override void Update()
        {
            TTriggerEnum trigger = update();
            ActivateTrigger(trigger);
        }
    }
}
