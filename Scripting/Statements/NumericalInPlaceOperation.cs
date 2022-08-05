using System;
using System.Threading;

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
            OperatorToken operatorToken)
        {
            assigneeType = assignee.GetValueType();
            Type valueType = value.GetValueType();

            if (!assigneeType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Assignee {assignee} for Operator {operatorToken.operatorType} is not a numerical value: type {assigneeType.Name}");
            }

            this.assignee = assignee;
            this.value = value;
            operatorType = operatorToken.operatorType;

            switch (operatorType)
            {
                case Operator.PlusEquals:
                case Operator.MinusEquals:
                case Operator.TimesEquals:
                case Operator.DivideEquals:
                case Operator.ModuloEquals:
                    //Acceptable
                    break;

                case Operator.BitwiseLeftShiftEquals:
                case Operator.BitwiseRightShiftEquals:
                case Operator.BitwiseXOrEquals:
                case Operator.AndEquals:
                case Operator.OrEquals:
                    if (!value.GetValueType().IsIntegralType())
                    {
                        throw new ScriptParsingException(operatorToken, $"Operator {operatorType} requires the argument be an integral type. Received {value.GetValueType()}.");
                    }
                    break;


                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            dynamic assigneeValue = assignee.GetAs<object>(context)!;
            dynamic modVaue = value.GetAs<object>(context)!;

            switch (operatorType)
            {
                case Operator.PlusEquals:
                    assigneeValue += modVaue;
                    break;

                case Operator.MinusEquals:
                    assigneeValue -= modVaue;
                    break;

                case Operator.TimesEquals:
                    assigneeValue *= modVaue;
                    break;

                case Operator.DivideEquals:
                    assigneeValue /= modVaue;
                    break;

                case Operator.ModuloEquals:
                    assigneeValue %= modVaue;
                    break;

                case Operator.BitwiseLeftShiftEquals:
                    assigneeValue <<= modVaue;
                    break;

                case Operator.BitwiseRightShiftEquals:
                    assigneeValue >>= modVaue;
                    break;

                case Operator.BitwiseXOrEquals:
                    assigneeValue ^= modVaue;
                    break;

                case Operator.AndEquals:
                    assigneeValue &= modVaue;
                    break;

                case Operator.OrEquals:
                    assigneeValue |= modVaue;
                    break;

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }
    }
}