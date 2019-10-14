using System;

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

            if (assigneeType == typeof(string))
            {
                //Concatenation operator
            }
            else if (assigneeType == typeof(int) || assigneeType == typeof(double))
            {
                //Addition operator
                Type valueType = value.GetValueType();

                if (!(valueType == typeof(int) || valueType == typeof(double)))
                {
                    throw new ScriptParsingException(
                        source: source,
                        message: $"Value {value} for Operator {source} is not a numerical value: type {valueType.Name}");
                }

                if (assigneeType == typeof(int) && valueType == typeof(double))
                {
                    throw new ScriptParsingException(
                        source: source,
                        message: $"Unable to implicity cast Value {value} to type int for assignment to assignee {assignee}.");
                }
            }
            else
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Assignee {assignee} for Operator {source} is not a numerical value or string: type {assigneeType.Name}");
            }

            this.assignee = assignee;
            this.value = value;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            if (assigneeType == typeof(int))
            {
                assignee.SetAs(context, assignee.GetAs<int>(context) + value.GetAs<int>(context));
            }
            else if (assigneeType == typeof(double))
            {
                assignee.SetAs(context, assignee.GetAs<double>(context) + value.GetAs<double>(context));
            }
            else if (assigneeType == typeof(string))
            {
                assignee.SetAs(context, assignee.GetAs<string>(context) + GetStringValue(value, context));
            }
            else
            {
                throw new ScriptRuntimeException(
                    $"Incompatible types for operator {Operator.PlusEquals}: {assignee} of type {assigneeType.Name} and {value} of type {value.GetValueType().Name}");
            }

            return FlowState.Nominal;
        }

        private static string GetStringValue(IValueGetter arg, RuntimeContext context)
        {
            Type argType = arg.GetValueType();

            if (argType == typeof(string))
            {
                return arg.GetAs<string>(context);
            }
            else if (argType == typeof(double))
            {
                return arg.GetAs<double>(context).ToString();
            }
            else if (argType == typeof(int))
            {
                return arg.GetAs<int>(context).ToString();
            }
            else if (argType == typeof(bool))
            {
                return arg.GetAs<bool>(context).ToString();
            }

            throw new ScriptRuntimeException($"Unsupported type for Stringification: type {argType.Name}");
        }
    }
}
