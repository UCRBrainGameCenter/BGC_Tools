using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter and OnStateExit.
    /// The lambdas return strings which, if not null, are fired off as Triggers
    /// </summary>
    public class TriggeringLambdaState<TTriggerEnum> : TriggeringState<TTriggerEnum>
        where TTriggerEnum : struct, Enum
    {
        private readonly Func<TTriggerEnum?> onStateEnter;
        private readonly Func<TTriggerEnum?> onStateExit;

        public TriggeringLambdaState(
            string name,
            Func<TTriggerEnum?> onStateEnter = null,
            Func<TTriggerEnum?> onStateExit = null)
            : base(name)
        {
            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
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
    }
}
