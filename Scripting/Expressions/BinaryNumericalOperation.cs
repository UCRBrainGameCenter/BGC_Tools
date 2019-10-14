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
            Type valueType;

            if (!(arg1Type == typeof(double) || arg1Type == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Left side of operator {operatorToken.operatorType} not of expected type int or bool: {arg1.GetValueType().Name}");
            }

            if (!(arg2Type == typeof(double) || arg2Type == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Right side of operator {operatorToken.operatorType} not of expected type int or bool: {arg2.GetValueType().Name}");
            }

            if (arg1Type == arg2Type)
            {
                valueType = arg1Type;
            }
            else
            {
                valueType = typeof(double);
            }

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                if (valueType == typeof(int))
                {
                    return new LiteralToken<int>(
                        operatorToken,
                        IntOperator<int>(litArg1.GetAs<int>(), litArg2.GetAs<int>(), operatorToken.operatorType, valueType));
                }
                else
                {
                    return new LiteralToken<double>(
                        operatorToken,
                        DoubleOperator<double>(litArg1.GetAs<double>(), litArg2.GetAs<double>(), operatorToken.operatorType, valueType));
                }
            }

            return new BinaryNumericalOperation(arg1, arg2, valueType, operatorToken.operatorType);
        }

        private BinaryNumericalOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Type valueType,
            Operator operatorType)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.valueType = valueType;
            this.operatorType = operatorType;

            switch (this.operatorType)
            {
                case Operator.Plus:
                case Operator.Minus:
                case Operator.Times:
                case Operator.Divide:
                case Operator.Power:
                case Operator.Modulo:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Operator {this.operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (valueType == typeof(int))
            {
                return IntOperator<T>(arg1.GetAs<int>(context), arg2.GetAs<int>(context), operatorType, valueType);
            }

            return DoubleOperator<T>(arg1.GetAs<double>(context), arg2.GetAs<double>(context), operatorType, valueType);
        }

        private static T IntOperator<T>(int arg1, int arg2, Operator operatorType, Type valueType)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to implicitly cast the results of {operatorType} to type {returnType}");
            }

            //Integer type
            switch (operatorType)
            {
                case Operator.Plus: return (T)Convert.ChangeType(arg1 + arg2, returnType);
                case Operator.Minus: return (T)Convert.ChangeType(arg1 - arg2, returnType);
                case Operator.Times: return (T)Convert.ChangeType(arg1 * arg2, returnType);
                case Operator.Divide: return (T)Convert.ChangeType(arg1 / arg2, returnType);
                case Operator.Power: return (T)Convert.ChangeType((int)Math.Pow(arg1, arg2), returnType);
                case Operator.Modulo: return (T)Convert.ChangeType(arg1 % arg2, returnType);

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        private static T DoubleOperator<T>(double arg1, double arg2, Operator operatorType, Type valueType)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to implicitly cast the results of {operatorType} to type {returnType}");
            }

            //Integer type
            switch (operatorType)
            {
                case Operator.Plus: return (T)Convert.ChangeType(arg1 + arg2, returnType);
                case Operator.Minus: return (T)Convert.ChangeType(arg1 - arg2, returnType);
                case Operator.Times: return (T)Convert.ChangeType(arg1 * arg2, returnType);
                case Operator.Divide: return (T)Convert.ChangeType(arg1 / arg2, returnType);
                case Operator.Power: return (T)Convert.ChangeType(Math.Pow(arg1, arg2), returnType);
                case Operator.Modulo: return (T)Convert.ChangeType(arg1 % arg2, returnType);

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public Type GetValueType() => valueType;
    }
}
