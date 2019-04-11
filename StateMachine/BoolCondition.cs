using System;

namespace BGC.StateMachine
{
    public class BoolCondition : TransitionCondition
    {
        private string key;
        private bool val;

        public BoolCondition(string key, bool val)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key),
                    message: "bool transition cannot receive null key");
            }
            else if (key.Equals(""))
            {
                throw new ArgumentException(
                    message: "bool transition key cannot be an empty string.",
                    paramName: nameof(key));
            }

            this.key = key;
            this.val = val;
        }

        public override void OnTransition()
        {
            // pass
        }

        public override bool ShouldTransition()
        {
            return getBool(key) == val;
        }

        protected override void StateMachineFunctionsSet()
        {
            // pass
        }
    }
}