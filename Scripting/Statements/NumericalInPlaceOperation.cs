using System;

namespace BGC.Scripting
{
    public class NumericalInPlaceOperation : Statement
    {
        private readonly IValue assignee;
        private readonly IValueGetter value;
        private readonly Operator operatorType;
        private readonly Type assigneeType;

        public NumericalInPlaceOperation(
            IValue assignee,
            IValueGetter value,
            Operator operatorType,
            Token source)
        {
            assigneeType = assignee.GetValueType();
            Type valueType = value.GetValueType();

            if (!(assigneeType == typeof(int) || assigneeType == typeof(double)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Assignee {assignee} for Operator {source} is not a numerical value: type {assigneeType.Name}");
            }

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

            this.assignee = assignee;
            this.value = value;
            this.operatorType = operatorType;

            switch (operatorType)
            {
                case Operator.PlusEquals:
                case Operator.MinusEquals:
                case Operator.TimesEquals:
                case Operator.DivideEquals:
                case Operator.PowerEquals:
                case Operator.ModuloEquals:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            if (assigneeType == typeof(int))
            {
                switch (operatorType)
                {
                    case Operator.PlusEquals:
                        assignee.SetAs(context, assignee.GetAs<int>(context) + value.GetAs<int>(context));
                        break;

                    case Operator.MinusEquals:
                        assignee.SetAs(context, assignee.GetAs<int>(context) - value.GetAs<int>(context));
                        break;

                    case Operator.TimesEquals:
                        assignee.SetAs(context, assignee.GetAs<int>(context) * value.GetAs<int>(context));
                        break;

                    case Operator.DivideEquals:
                        assignee.SetAs(context, assignee.GetAs<int>(context) / value.GetAs<int>(context));
                        break;

                    case Operator.PowerEquals:
                        assignee.SetAs(context, (int)Math.Pow(assignee.GetAs<int>(context), value.GetAs<int>(context)));
                        break;

                    case Operator.ModuloEquals:
                        assignee.SetAs(context, assignee.GetAs<int>(context) % value.GetAs<int>(context));
                        break;

                    default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }
            }
            else if (assigneeType == typeof(double))
            {
                switch (operatorType)
                {
                    case Operator.PlusEquals:
                        assignee.SetAs(context, assignee.GetAs<double>(context) + value.GetAs<double>(context));
                        break;

                    case Operator.MinusEquals:
                        assignee.SetAs(context, assignee.GetAs<double>(context) - value.GetAs<double>(context));
                        break;

                    case Operator.TimesEquals:
                        assignee.SetAs(context, assignee.GetAs<double>(context) * value.GetAs<double>(context));
                        break;

                    case Operator.DivideEquals:
                        assignee.SetAs(context, assignee.GetAs<double>(context) / value.GetAs<double>(context));
                        break;

                    case Operator.PowerEquals:
                        assignee.SetAs(context, Math.Pow(assignee.GetAs<double>(context), value.GetAs<double>(context)));
                        break;

                    case Operator.ModuloEquals:
                        assignee.SetAs(context, assignee.GetAs<double>(context) % value.GetAs<double>(context));
                        break;

                    default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }
            }
            else
            {
                throw new ScriptRuntimeException(
                    $"Incompatible types for operator {operatorType}: {assignee} of type {assigneeType.Name} and {value} of type {value.GetValueType().Name}");
            }

            return FlowState.Nominal;
        }
    }
}
