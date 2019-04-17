using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter and OnStateExit.
    /// The lambdas return strings which, if not null, are fired off as Triggers
    /// </summary>
    public class TriggeringLambdaState : State
    {
        private readonly Func<string> onStateEnter;
        private readonly Func<string> onStateExit;

        public TriggeringLambdaState(
            string name,
            Func<string> onStateEnter = null,
            Func<string> onStateExit = null)
            : base(name)
        {
            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
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
    }
}
