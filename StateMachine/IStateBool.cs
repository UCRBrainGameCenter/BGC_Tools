using System;

namespace BGC.StateMachine
{
    public interface IStateDataBool<TBoolEnum>
        where TBoolEnum : Enum
    {
        bool GetBool(TBoolEnum key);
        void SetBool(TBoolEnum key, bool value);
    }
}