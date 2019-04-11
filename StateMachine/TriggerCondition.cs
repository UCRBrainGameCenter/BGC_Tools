using System;

namespace BGC.StateMachine
{
    public class TriggerCondition : TransitionCondition
    {
        private string key;

        public TriggerCondition(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key),
                    message: "TriggerCondition cannot have a null key.");
            }
            else if (key.Equals(""))
            {
                throw new ArgumentException(
                    message: "Trigger condition cannot have a key that is an empty string.",
                    paramName: nameof(key));
            }

            this.key = key;
        }

        public override void OnTransition()
        {
            consumeTrigger(key);
        }

        public override bool ShouldTransition()
        {
            return getTrigger(key);
        }

        protected override void StateMachineFunctionsSet()
        {
            // pass
        }
    }
}