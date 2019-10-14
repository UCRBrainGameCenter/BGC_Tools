using System;

namespace BGC.Scripting
{
    public class BooleanInPlaceOperation : Statement
    {
        private readonly IValue assignee;
        private readonly IValueGetter value;
        private readonly Operator operatorType;

        public BooleanInPlaceOperation(
            IValue assignee,
            IValueGetter value,
            Operator operatorType,
            Token source)
        {
            if (assignee.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Left Argument {assignee} of Operator {source} is not a bool: type {assignee.GetValueType().Name}");
            }

            if (value.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Right Argument {value} of Operator {source} is not a bool.");
            }

            this.assignee = assignee;
            this.value = value;
            this.operatorType = operatorType;

            switch (operatorType)
            {
                case Operator.AndEquals:
                case Operator.OrEquals:
                    //Acceptable
                    break;

                default:
                    throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            switch (operatorType)
            {
                case Operator.AndEquals:
                    if (!assignee.GetAs<bool>(context))
                    {
                        //Short Circuiting
                        //No need to set anything
                        break;
                    }
                    assignee.SetAs(context, value.GetAs<bool>(context));
                    break;

                case Operator.OrEquals:
                    if (assignee.GetAs<bool>(context))
                    {
                        //Short Circuiting
                        //No need to set anything
                        break;
                    }
                    assignee.SetAs(context, value.GetAs<bool>(context));
                    break;

                default:
                    throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }

            return FlowState.Nominal;
        }
    }
}
