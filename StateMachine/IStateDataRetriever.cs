namespace BGC.StateMachine
{
    public interface IStateDataRetriever
    {
        void ActivateTrigger(string key);
        bool GetTrigger(string key);
        bool GetBool(string key);
        void SetBool(string key, bool value);
    }
}