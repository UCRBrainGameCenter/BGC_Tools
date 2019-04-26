using System;
using UnityEngine;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter, OnStateExit, and Update.
    /// The lambdas return strings which, if not null, are fired off as Triggers
    /// </summary>
    public class TriggeringUpdatingLambdaState<TTriggerEnum> : TriggeringState<TTriggerEnum>
        where TTriggerEnum : struct, Enum
    {
        private readonly Func<TTriggerEnum?> onStateEnter;
        private readonly Func<TTriggerEnum?> onStateExit;
        private readonly Func<TTriggerEnum?> update;

        public TriggeringUpdatingLambdaState(
            string name,
            Func<TTriggerEnum?> onStateEnter = null,
            Func<TTriggerEnum?> onStateExit = null,
            Func<TTriggerEnum?> update = null)
            : base(name)
        {
            Debug.Assert(update != null);

            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
            this.update = update;
        }

        protected override void OnStateEnter()
        {
            TTriggerEnum? trigger = onStateEnter?.Invoke();

            if (trigger.HasValue)
            {
                ActivateTrigger(trigger.Value);
            }
        }

        protected override void OnStateExit()
        {
            TTriggerEnum? trigger = onStateExit?.Invoke();

            if (trigger.HasValue)
            {
                ActivateTrigger(trigger.Value);
            }
        }

        public override void Update()
        {
            TTriggerEnum? trigger = update();

            if(trigger.HasValue)
            {
                ActivateTrigger(trigger.Value);
            }
        }
    }
}
