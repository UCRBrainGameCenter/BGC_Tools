using System;

namespace BGC.StateMachine
{
    class OrConjunction : TransitionCondition
    {
        private TransitionCondition[] conditions;

        public OrConjunction(params TransitionCondition[] conditions)
        {
            if (conditions == null)
            {
                throw new ArgumentNullException("OrConjunction conditions canot be null.");
            }

            for (int i = 0; i < conditions.Length; ++i)
            {
                if (conditions[i] == null)
                {
                    throw new ArgumentNullException("OrConjunction conditions element " + i + " is null and should not be.");
                }
            }

            this.conditions = conditions;
        }

        public override void OnTransition()
        {
            for (int i = 0; i < conditions.Length; ++i)
            {
                if (conditions[i].ShouldTransition())
                {
                    conditions[i].OnTransition();
                    break;
                }
            }
        }

        public override bool ShouldTransition()
        {
            bool shouldTransition = false;
            for (int i = 0; i < conditions.Length; ++i)
            {
                if (conditions[i].ShouldTransition())
                {
                    shouldTransition = true;
                    break;
                }
            }

            return conditions.Length == 0 ? true : shouldTransition;
        }

        protected override void StateMachineFunctionsSet()
        {
            for (int i = 0; i < conditions.Length; ++i)
            {
                conditions[i].SetStateMachineFunctions(getBool, getTrigger, consumeTrigger);
            }
        }
    }
}