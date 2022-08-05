using System;

namespace BGC.Scripting
{
    public class ComparisonOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;

        public static IExpression CreateComparisonOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (!(arg1Type.IsExtendedPrimitive() || arg1Type.IsEnum))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Left side of operator {operatorToken.operatorType} has incompatible type: {arg1Type.Name}");
            }

            if (!(arg2Type.IsExtendedPrimitive() || arg2Type.IsEnum))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Right side of operator {operatorToken.operatorType} has incompatible type: {arg2Type.Name}");
            }

            //Checks that promotion is possible
            operatorToken.GetBinaryPromotedType(arg1Type, arg2Type);

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                return new LiteralToken<bool>(
                    operatorToken,
                    PerformOperator(litArg1.GetAs<object>(), litArg2.GetAs<object>(), operatorToken.operatorType));
            }

            return new ComparisonOperation(arg1, arg2, operatorToken.operatorType);
        }

        private ComparisonOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Operator operatorType)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.operatorType = operatorType;

            switch (operatorType)
            {
                case Operator.IsGreaterThan:
                case Operator.IsGreaterThanOrEqualTo:
                case Operator.IsLessThan:
                case Operator.IsLessThanOrEqualTo:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableOrConvertableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying {operatorType} as type {typeof(T).Name}");
            }

            return (T)(object)PerformOperator(arg1.GetAs<object>(context)!, arg2.GetAs<object>(context)!, operatorType);
        }

        public Type GetValueType() => typeof(bool);

        private static bool PerformOperator(dynamic arg1, dynamic arg2, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.IsGreaterThan: return arg1 > arg2;
                case Operator.IsGreaterThanOrEqualTo: return arg1 >= arg2;
                case Operator.IsLessThan: return arg1 < arg2;
                case Operator.IsLessThanOrEqualTo: return arg1 <= arg2;

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }
    }
}