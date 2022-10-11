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

            this.assignee = assignee;
            this.value = value;
            operatorType = operatorToken.operatorType;

            if (assigneeType.IsPrimitive && valueType.IsPrimitive)
            {
                if (!assigneeType.AssignableOrConvertableFromType(valueType))
                {
                    throw new ScriptParsingException(
                        source: operatorToken,
                        message: $"Assignee {assignee} for Operator {operatorToken.operatorType} is not a numerical value: type {assigneeType.Name}");
                }

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
            else
            {
                string operatorName = operatorType switch
                {
                    Operator.PlusEquals => "op_Addition",
                    Operator.MinusEquals => "op_Subtraction",
                    Operator.TimesEquals => "op_Multiply",
                    Operator.DivideEquals => "op_Division",
                    Operator.ModuloEquals => "op_Modulus",
                    Operator.BitwiseLeftShiftEquals => "op_LeftShift",
                    Operator.BitwiseRightShiftEquals => "op_RightShift",
                    Operator.BitwiseXOrEquals => "op_ExclusiveOr",
                    Operator.AndEquals => "op_BitwiseAnd",
                    Operator.OrEquals => "op_BitwiseOr",
                    _ => null,
                };

                if (operatorName == null)
                {
                    throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }

                var (canInvoke, error) = assigneeType.CanInvokeStaticMethod(operatorName, assigneeType, valueType);
                if (!canInvoke)
                {
                    throw new ScriptParsingException(operatorToken, error);
                }
            }
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            switch (operatorType)
            {
                case Operator.PlusEquals: return ExecutePlusEquals(context);
                case Operator.MinusEquals: return ExecuteMinusEquals(context);
                case Operator.TimesEquals: return ExecuteTimesEquals(context);
                case Operator.DivideEquals: return ExecuteDivideEquals(context);
                case Operator.ModuloEquals: return ExecuteModuloEquals(context);
                case Operator.BitwiseLeftShiftEquals: return ExecuteBitwiseLeftShiftEquals(context);
                case Operator.BitwiseRightShiftEquals: return ExecuteBitwiseRightShiftEquals(context);
                case Operator.BitwiseXOrEquals: return ExecuteBitwiseXOrEquals(context);
                case Operator.AndEquals: return ExecuteAndEquals(context);
                case Operator.OrEquals: return ExecuteOrEquals(context);
                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public FlowState ExecutePlusEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeType.IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: prim1 += prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: prim1 += prim2; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: prim1 += prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 += prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: prim1 += prim2; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: prim1 += prim2; break;
                            case uint prim2: prim1 += prim2; break;
                            case long prim2: prim1 += prim2; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 += prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 += prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: prim1 += prim2; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 += prim2; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: prim1 += prim2; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: prim1 += prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 += prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 += prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 += prim2; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case decimal prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: prim1 += prim2; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: prim1 += prim2; break;
                            case uint prim2: prim1 += prim2; break;
                            case long prim2: prim1 += prim2; break;
                            case ulong prim2: prim1 += prim2; break;
                            case nint prim2: prim1 += prim2; break;
                            case nuint prim2: prim1 += prim2; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: prim1 += prim2; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case float prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: prim1 += prim2; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: prim1 += prim2; break;
                            case uint prim2: prim1 += prim2; break;
                            case long prim2: prim1 += prim2; break;
                            case ulong prim2: prim1 += prim2; break;
                            case nint prim2: prim1 += prim2; break;
                            case nuint prim2: prim1 += prim2; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 += prim2; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case double prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 += prim2; break;
                            case sbyte prim2: prim1 += prim2; break;
                            case short prim2: prim1 += prim2; break;
                            case ushort prim2: prim1 += prim2; break;
                            case int prim2: prim1 += prim2; break;
                            case uint prim2: prim1 += prim2; break;
                            case long prim2: prim1 += prim2; break;
                            case ulong prim2: prim1 += prim2; break;
                            case nint prim2: prim1 += prim2; break;
                            case nuint prim2: prim1 += prim2; break;
                            case char prim2: prim1 += prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 += prim2; break;
                            case double prim2: prim1 += prim2; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator += to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_Addition", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteMinusEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: prim1 -= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: prim1 -= prim2; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: prim1 -= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 -= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: prim1 -= prim2; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: prim1 -= prim2; break;
                            case uint prim2: prim1 -= prim2; break;
                            case long prim2: prim1 -= prim2; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 -= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 -= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: prim1 -= prim2; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 -= prim2; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: prim1 -= prim2; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: prim1 -= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 -= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 -= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 -= prim2; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case decimal prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: prim1 -= prim2; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: prim1 -= prim2; break;
                            case uint prim2: prim1 -= prim2; break;
                            case long prim2: prim1 -= prim2; break;
                            case ulong prim2: prim1 -= prim2; break;
                            case nint prim2: prim1 -= prim2; break;
                            case nuint prim2: prim1 -= prim2; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: prim1 -= prim2; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case float prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: prim1 -= prim2; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: prim1 -= prim2; break;
                            case uint prim2: prim1 -= prim2; break;
                            case long prim2: prim1 -= prim2; break;
                            case ulong prim2: prim1 -= prim2; break;
                            case nint prim2: prim1 -= prim2; break;
                            case nuint prim2: prim1 -= prim2; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 -= prim2; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case double prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 -= prim2; break;
                            case sbyte prim2: prim1 -= prim2; break;
                            case short prim2: prim1 -= prim2; break;
                            case ushort prim2: prim1 -= prim2; break;
                            case int prim2: prim1 -= prim2; break;
                            case uint prim2: prim1 -= prim2; break;
                            case long prim2: prim1 -= prim2; break;
                            case ulong prim2: prim1 -= prim2; break;
                            case nint prim2: prim1 -= prim2; break;
                            case nuint prim2: prim1 -= prim2; break;
                            case char prim2: prim1 -= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 -= prim2; break;
                            case double prim2: prim1 -= prim2; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator -= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_Subtraction", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteTimesEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: prim1 *= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: prim1 *= prim2; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: prim1 *= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 *= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: prim1 *= prim2; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: prim1 *= prim2; break;
                            case uint prim2: prim1 *= prim2; break;
                            case long prim2: prim1 *= prim2; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 *= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 *= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: prim1 *= prim2; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 *= prim2; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: prim1 *= prim2; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: prim1 *= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 *= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 *= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 *= prim2; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case decimal prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: prim1 *= prim2; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: prim1 *= prim2; break;
                            case uint prim2: prim1 *= prim2; break;
                            case long prim2: prim1 *= prim2; break;
                            case ulong prim2: prim1 *= prim2; break;
                            case nint prim2: prim1 *= prim2; break;
                            case nuint prim2: prim1 *= prim2; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: prim1 *= prim2; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case float prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: prim1 *= prim2; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: prim1 *= prim2; break;
                            case uint prim2: prim1 *= prim2; break;
                            case long prim2: prim1 *= prim2; break;
                            case ulong prim2: prim1 *= prim2; break;
                            case nint prim2: prim1 *= prim2; break;
                            case nuint prim2: prim1 *= prim2; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 *= prim2; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case double prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 *= prim2; break;
                            case sbyte prim2: prim1 *= prim2; break;
                            case short prim2: prim1 *= prim2; break;
                            case ushort prim2: prim1 *= prim2; break;
                            case int prim2: prim1 *= prim2; break;
                            case uint prim2: prim1 *= prim2; break;
                            case long prim2: prim1 *= prim2; break;
                            case ulong prim2: prim1 *= prim2; break;
                            case nint prim2: prim1 *= prim2; break;
                            case nuint prim2: prim1 *= prim2; break;
                            case char prim2: prim1 *= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 *= prim2; break;
                            case double prim2: prim1 *= prim2; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator *= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_Multiply", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteDivideEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: prim1 /= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: prim1 /= prim2; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: prim1 /= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 /= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: prim1 /= prim2; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: prim1 /= prim2; break;
                            case uint prim2: prim1 /= prim2; break;
                            case long prim2: prim1 /= prim2; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 /= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 /= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: prim1 /= prim2; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 /= prim2; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: prim1 /= prim2; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: prim1 /= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 /= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 /= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 /= prim2; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case decimal prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: prim1 /= prim2; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: prim1 /= prim2; break;
                            case uint prim2: prim1 /= prim2; break;
                            case long prim2: prim1 /= prim2; break;
                            case ulong prim2: prim1 /= prim2; break;
                            case nint prim2: prim1 /= prim2; break;
                            case nuint prim2: prim1 /= prim2; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: prim1 /= prim2; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case float prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: prim1 /= prim2; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: prim1 /= prim2; break;
                            case uint prim2: prim1 /= prim2; break;
                            case long prim2: prim1 /= prim2; break;
                            case ulong prim2: prim1 /= prim2; break;
                            case nint prim2: prim1 /= prim2; break;
                            case nuint prim2: prim1 /= prim2; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 /= prim2; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case double prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 /= prim2; break;
                            case sbyte prim2: prim1 /= prim2; break;
                            case short prim2: prim1 /= prim2; break;
                            case ushort prim2: prim1 /= prim2; break;
                            case int prim2: prim1 /= prim2; break;
                            case uint prim2: prim1 /= prim2; break;
                            case long prim2: prim1 /= prim2; break;
                            case ulong prim2: prim1 /= prim2; break;
                            case nint prim2: prim1 /= prim2; break;
                            case nuint prim2: prim1 /= prim2; break;
                            case char prim2: prim1 /= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 /= prim2; break;
                            case double prim2: prim1 /= prim2; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator /= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_Division", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteModuloEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: prim1 %= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: prim1 %= prim2; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: prim1 %= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 %= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: prim1 %= prim2; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: prim1 %= prim2; break;
                            case uint prim2: prim1 %= prim2; break;
                            case long prim2: prim1 %= prim2; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 %= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 %= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: prim1 %= prim2; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 %= prim2; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: prim1 %= prim2; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: prim1 %= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: prim1 %= prim2; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: prim1 %= prim2; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: prim1 %= prim2; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case decimal prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: prim1 %= prim2; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: prim1 %= prim2; break;
                            case uint prim2: prim1 %= prim2; break;
                            case long prim2: prim1 %= prim2; break;
                            case ulong prim2: prim1 %= prim2; break;
                            case nint prim2: prim1 %= prim2; break;
                            case nuint prim2: prim1 %= prim2; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: prim1 %= prim2; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case float prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: prim1 %= prim2; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: prim1 %= prim2; break;
                            case uint prim2: prim1 %= prim2; break;
                            case long prim2: prim1 %= prim2; break;
                            case ulong prim2: prim1 %= prim2; break;
                            case nint prim2: prim1 %= prim2; break;
                            case nuint prim2: prim1 %= prim2; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 %= prim2; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case double prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 %= prim2; break;
                            case sbyte prim2: prim1 %= prim2; break;
                            case short prim2: prim1 %= prim2; break;
                            case ushort prim2: prim1 %= prim2; break;
                            case int prim2: prim1 %= prim2; break;
                            case uint prim2: prim1 %= prim2; break;
                            case long prim2: prim1 %= prim2; break;
                            case ulong prim2: prim1 %= prim2; break;
                            case nint prim2: prim1 %= prim2; break;
                            case nuint prim2: prim1 %= prim2; break;
                            case char prim2: prim1 %= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: prim1 %= prim2; break;
                            case double prim2: prim1 %= prim2; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator %= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_Modulus", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteBitwiseLeftShiftEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 <<= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: prim1 <<= prim2; break;
                            case short prim2: prim1 <<= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 <<= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: prim1 <<= prim2; break;
                            case short prim2: prim1 <<= prim2; break;
                            case ushort prim2: prim1 <<= prim2; break;
                            case int prim2: prim1 <<= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 <<= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: prim1 <<= prim2; break;
                            case short prim2: prim1 <<= prim2; break;
                            case ushort prim2: prim1 <<= prim2; break;
                            case int prim2: prim1 <<= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 <<= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: prim1 <<= prim2; break;
                            case short prim2: prim1 <<= prim2; break;
                            case ushort prim2: prim1 <<= prim2; break;
                            case int prim2: prim1 <<= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 <<= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 <<= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 <<= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator <<= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_LeftShift", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteBitwiseRightShiftEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 >>= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: prim1 >>= prim2; break;
                            case short prim2: prim1 >>= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 >>= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: prim1 >>= prim2; break;
                            case short prim2: prim1 >>= prim2; break;
                            case ushort prim2: prim1 >>= prim2; break;
                            case int prim2: prim1 >>= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 >>= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: prim1 >>= prim2; break;
                            case short prim2: prim1 >>= prim2; break;
                            case ushort prim2: prim1 >>= prim2; break;
                            case int prim2: prim1 >>= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 >>= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: prim1 >>= prim2; break;
                            case short prim2: prim1 >>= prim2; break;
                            case ushort prim2: prim1 >>= prim2; break;
                            case int prim2: prim1 >>= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 >>= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 >>= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 >>= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator >>= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_RightShift", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteBitwiseXOrEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case bool prim1:
                        switch (modValue)
                        {
                            case bool prim2: prim1 ^= prim2; break;
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case byte prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 ^= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: prim1 ^= prim2; break;
                            case short prim2: prim1 ^= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 ^= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: prim1 ^= prim2; break;
                            case short prim2: prim1 ^= prim2; break;
                            case ushort prim2: prim1 ^= prim2; break;
                            case int prim2: prim1 ^= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 ^= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: prim1 ^= prim2; break;
                            case short prim2: prim1 ^= prim2; break;
                            case ushort prim2: prim1 ^= prim2; break;
                            case int prim2: prim1 ^= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 ^= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: prim1 ^= prim2; break;
                            case short prim2: prim1 ^= prim2; break;
                            case ushort prim2: prim1 ^= prim2; break;
                            case int prim2: prim1 ^= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 ^= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 ^= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 ^= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    // Floats and Doubles treat ^ as a power operator.
                    case float prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case sbyte prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case short prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case ushort prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case int prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case uint prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case long prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case ulong prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case nint prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case nuint prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                            case char prim2: prim1 = (float)Math.Pow(prim1, prim2); break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case double prim1:
                        switch (modValue)
                        {
                            case bool prim2: isConvErr = true; break;
                            case byte prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case sbyte prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case short prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case ushort prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case int prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case uint prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case long prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case ulong prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case nint prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case nuint prim2: prim1 = Math.Pow(prim1, prim2); break;
                            case char prim2: prim1 = Math.Pow(prim1, prim2); break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator ^= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_ExclusiveOr", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteAndEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case bool prim1:
                        switch (modValue)
                        {
                            case bool prim2: prim1 &= prim2; break;
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 &= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: prim1 &= prim2; break;
                            case short prim2: prim1 &= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 &= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: prim1 &= prim2; break;
                            case short prim2: prim1 &= prim2; break;
                            case ushort prim2: prim1 &= prim2; break;
                            case int prim2: prim1 &= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 &= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: prim1 &= prim2; break;
                            case short prim2: prim1 &= prim2; break;
                            case ushort prim2: prim1 &= prim2; break;
                            case int prim2: prim1 &= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 &= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: prim1 &= prim2; break;
                            case short prim2: prim1 &= prim2; break;
                            case ushort prim2: prim1 &= prim2; break;
                            case int prim2: prim1 &= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 &= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 &= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 &= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator &= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_BitwiseAnd", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }

        public FlowState ExecuteOrEquals(ScopeRuntimeContext context)
        {
            object assigneeValue = assignee.GetAs<object>(context)!;
            object modValue = value.GetAs<object>(context)!;

            if (assigneeValue.GetType().IsPrimitive && modValue.GetType().IsPrimitive)
            {
                bool isConvErr = false;
                switch (assigneeValue)
                {
                    case bool prim1:
                        switch (modValue)
                        {
                            case bool prim2: prim1 |= prim2; break;
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case byte prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case sbyte prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: prim1 |= prim2; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case short prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: prim1 |= prim2; break;
                            case short prim2: prim1 |= prim2; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: isConvErr = true; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ushort prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 |= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case int prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: prim1 |= prim2; break;
                            case short prim2: prim1 |= prim2; break;
                            case ushort prim2: prim1 |= prim2; break;
                            case int prim2: prim1 |= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case uint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 |= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case long prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: prim1 |= prim2; break;
                            case short prim2: prim1 |= prim2; break;
                            case ushort prim2: prim1 |= prim2; break;
                            case int prim2: prim1 |= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case ulong prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 |= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: prim1 |= prim2; break;
                            case short prim2: prim1 |= prim2; break;
                            case ushort prim2: prim1 |= prim2; break;
                            case int prim2: prim1 |= prim2; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case nuint prim1:
                        switch (modValue)
                        {
                            case byte prim2: prim1 |= prim2; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: prim1 |= prim2; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;
                    case char prim1:
                        switch (modValue)
                        {
                            case byte prim2: isConvErr = true; break;
                            case sbyte prim2: isConvErr = true; break;
                            case short prim2: isConvErr = true; break;
                            case ushort prim2: isConvErr = true; break;
                            case int prim2: isConvErr = true; break;
                            case uint prim2: isConvErr = true; break;
                            case long prim2: isConvErr = true; break;
                            case ulong prim2: isConvErr = true; break;
                            case nint prim2: isConvErr = true; break;
                            case nuint prim2: isConvErr = true; break;
                            case char prim2: prim1 |= prim2; break;
                            case decimal prim2: isConvErr = true; break;
                            case float prim2: isConvErr = true; break;
                            case double prim2: isConvErr = true; break;
                        }
                        if (!isConvErr)
                        {
                            assigneeValue = prim1;
                        }
                        break;

                    default:
                        isConvErr = true;
                        break;
                }

                if (isConvErr)
                {
                    throw new ArgumentException($"Cannot apply operator |= to types {assigneeValue.GetType().Name} and {modValue.GetType().Name}");
                }
            }
            else
            {
                object newAssigneeValue = assigneeValue.GetType().InvokeStaticMethod("op_BitwiseOr", assigneeValue, modValue);
                if (assigneeValue.GetType() != newAssigneeValue.GetType())
                {
                    throw new ArgumentException($"Result of {operatorType} had wrong type. Expected {assigneeValue.GetType().Name}, but got {newAssigneeValue.GetType().Name}");
                }
                assigneeValue = newAssigneeValue;
            }

            assignee.Set(context, assigneeValue);

            return FlowState.Nominal;
        }
    }
}