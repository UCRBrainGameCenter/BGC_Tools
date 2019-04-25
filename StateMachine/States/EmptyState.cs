using System;

namespace BGC.StateMachine
{
    /// <summary>
    /// Simplest state possible - effectively featureless
    /// </summary>
    public sealed class EmptyState<TBoolEnum, TTriggerEnum> : State<TBoolEnum, TTriggerEnum> 
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        public EmptyState(string name) : base(name) { }
        protected override void OnStateEnter() { }
    }
}
