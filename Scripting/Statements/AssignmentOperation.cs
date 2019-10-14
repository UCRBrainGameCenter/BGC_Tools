using System;

namespace BGC.Scripting
{
    public class AssignmentOperation : Statement
    {
        private readonly IValue assignee;
        private readonly IValueGetter value;

        private readonly Type assigneeType;
        private readonly Type valueType;

        public AssignmentOperation(
            IValue assignee,
            IValueGetter value,
            Token source)
        {
            this.assignee = assignee;
            this.value = value;

            assigneeType = assignee.GetValueType();
            valueType = value.GetValueType();

            if (!assigneeType.AssignableFromType(valueType))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Assignment operator has an incompatible type for {assigneeType.Name} variable: type {valueType.Name}");
            }
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            if (assigneeType == valueType)
            {
                assignee.Set(context, value.GetAs<object>(context));
            }
            else if (assigneeType.AssignableFromType(valueType))
            {
                assignee.Set(context, Convert.ChangeType(value.GetAs<object>(context), assigneeType));
            }
            else
            {
                throw new ScriptRuntimeException(
                    $"Unable to assign {assignee} of type {assigneeType.Name} the value {value} of type {value.GetValueType().Name}");
            }

            return FlowState.Nominal;
        }
    }
}
