namespace BGC.StateMachine
{
    /// <summary>
    /// Simplest state possible - effectively featureless
    /// </summary>
    public sealed class EmptyState : State
    {
        public EmptyState(string name) : base(name) { }
        protected override void OnStateEnter() { }
    }
}
