using System;

namespace BGC.StateMachine
{
    public interface IStateTrigger<TTriggerEnum>
        where TTriggerEnum : Enum
    {
        void ActivateTrigger(TTriggerEnum key);
        bool GetTrigger(TTriggerEnum key);
    }
}