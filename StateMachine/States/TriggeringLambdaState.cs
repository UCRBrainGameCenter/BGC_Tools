using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter and OnStateExit.
    /// The lambdas return strings which, if not null, are fired off as Triggers
    /// </summary>
    public class TriggeringLambdaState<TBoolEnum, TTriggerEnum> : State<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        private readonly Func<TTriggerEnum> onStateEnter;
        private readonly Func<TTriggerEnum> onStateExit;

        public TriggeringLambdaState(
            string name,
            Func<TTriggerEnum> onStateEnter = null,
            Func<TTriggerEnum> onStateExit = null)
            : base(name)
        {
            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
        }

        protected override void OnStateEnter()
        {
            TTriggerEnum trigger = onStateEnter?.Invoke();
            ActivateTrigger(trigger);
        }

        protected override void OnStateExit()
        {
            TTriggerEnum trigger = onStateExit?.Invoke();
            ActivateTrigger(trigger);
        }
    }
}
