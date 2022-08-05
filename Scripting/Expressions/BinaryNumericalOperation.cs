using System;

namespace BGC.Scripting
{
    public class BinaryNumericalOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;
        private readonly Type valueType;

        public static IExpression CreateBinaryNumericalOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (!arg1Type.IsExtendedPrimitive())
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Left side of operator {operatorToken.operatorType} not of expected primitive type: {arg1.GetValueType().Name}");
            }

            if (!arg2Type.IsExtendedPrimitive())
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Right side of operator {operatorToken.operatorType} not of expected primitive type: {arg2.GetValueType().Name}");
            }

            Type promotedType = operatorToken.GetBinaryPromotedType(arg1Type, arg2Type);

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                return new ConstantToken(
                    operatorToken,
                    PerformOperator(litArg1.GetAs<object>(), litArg2.GetAs<object>(), operatorToken.operatorType),
                    promotedType);
            }

            return new BinaryNumericalOperation(arg1, arg2, promotedType, operatorToken);
        }

        private BinaryNumericalOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Type valueType,
            OperatorToken operatorToken)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.valueType = valueType;
            operatorType = operatorToken.operatorType;

            switch (operatorType)
            {
                case Operator.Plus:
                case Operator.Minus:
                case Operator.Times:
                case Operator.Divide:
                case Operator.Modulo:

                case Operator.BitwiseAnd:
                case Operator.BitwiseOr:
                case Operator.BitwiseXOr:
                    //Acceptable
                    break;

                case Operator.BitwiseLeftShift:
                case Operator.BitwiseRightShift:
                    if (!arg2.GetValueType().IsIntegralType())
                    {
                        throw new ScriptParsingException(operatorToken, $"Operator {operatorType} requires the second argument be an integral type. Received {arg2.GetValueType()}.");
                    }
                    break;


                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException(
                    $"Return value of {operatorType} is {valueType}, but it was accessed as {returnType.Name}");
            }

            object value = PerformOperator(arg1.GetAs<object>(context)!, arg2.GetAs<object>(context)!, operatorType);

            if (!returnType.IsAssignableFrom(value.GetType()))
            {
                value = Convert.ChangeType(value, returnType);
            }

            return (T)value;
        }

        public Type GetValueType() => valueType;

        private static object PerformOperator(dynamic arg1, dynamic arg2, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.Plus: return arg1 + arg2;
                case Operator.Minus: return arg1 - arg2;
                case Operator.Times: return arg1 * arg2;
                case Operator.Divide: return arg1 / arg2;
                case Operator.Modulo: return arg1 % arg2;

                case Operator.BitwiseAnd: return arg1 & arg2;
                case Operator.BitwiseOr: return arg1 | arg2;
                case Operator.BitwiseXOr: return arg1 ^ arg2;

                case Operator.BitwiseLeftShift: return arg1 << arg2;
                case Operator.BitwiseRightShift: return arg1 >> arg2;

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }
    }
}