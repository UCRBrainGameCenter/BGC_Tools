using System;

namespace BGC.StateMachine
{
    public interface IStateDataRetriever<TBoolEnum, TTriggerEnum> where TBoolEnum : Enum where TTriggerEnum : Enum
    {
        void ActivateTrigger(TTriggerEnum key);
        bool GetTrigger(TTriggerEnum key);

        bool GetBool(TBoolEnum key);
        void SetBool(TBoolEnum key, bool value);
    }
}