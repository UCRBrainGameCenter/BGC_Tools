using System;

namespace BGC.StateMachine
{
    public interface ITransitionDataRetriever<TBoolEnum, TTriggerEnum>
        where TBoolEnum : Enum
        where TTriggerEnum : Enum
    {
        bool GetBool(TBoolEnum key);
        bool GetTrigger(TTriggerEnum key);
        void ConsumeTrigger(TTriggerEnum key);
    }
}