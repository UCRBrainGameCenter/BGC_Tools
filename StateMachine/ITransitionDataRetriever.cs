namespace BGC.StateMachine
{
    public interface ITransitionDataRetriever
    {
        bool GetBool(string key);
        bool GetTrigger(string key);
        void ConsumeTrigger(string key);
    }
}