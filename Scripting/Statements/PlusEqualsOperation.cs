using System;
using System.Threading;

namespace BGC.Scripting
{
    public class PlusEqualsOperation : Statement
    {
        private readonly IValue assignee;
        private readonly IValueGetter value;
        private readonly Type assigneeType;

        public PlusEqualsOperation(
            IValue assignee,
            IValueGetter value,
            Token source)
        {
            assigneeType = assignee.GetValueType();

            if (assigneeType != typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Assignee {assignee} for Operator {source} is not a numerical value or string: type {assigneeType.Name}");
            }

            this.assignee = assignee;
            this.value = value;
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            if (assigneeType != typeof(string))
            {
                throw new ScriptRuntimeException($"Incompatible types for operator {Operator.PlusEquals}: {assignee} of type {assigneeType.Name} and {value} of type {value.GetValueType().Name}");
            }

            assignee.SetAs(context, assignee.GetAs<string>(context) + GetStringValue(value, context));

            return FlowState.Nominal;
        }

        private static string GetStringValue(IValueGetter arg, RuntimeContext context)
        {
            Type argType = arg.GetValueType();

            if (argType == typeof(string))
            {
                return arg.GetAs<string>(context)!;
            }

            return arg.GetAs<object>(context)!.ToString()!;
        }
    }
}