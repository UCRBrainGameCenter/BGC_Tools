using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simple State with optional lambda arguments for OnStateEnter and OnStateExit.
    /// </summary>
    public sealed class LambdaState: State
    {
        private readonly Action onStateEnter;
        private readonly Action onStateExit;

        public LambdaState(
            string name,
            Action onStateEnter = null,
            Action onStateExit = null)
            : base(name)
        {
            this.onStateEnter = onStateEnter;
            this.onStateExit = onStateExit;
        }

        protected override void OnStateEnter() => onStateEnter?.Invoke();
        protected override void OnStateExit() => onStateExit?.Invoke();
    }
}
