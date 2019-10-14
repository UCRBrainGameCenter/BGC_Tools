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
            if (!(arg1.GetValueType() == typeof(double) || arg1.GetValueType() == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Left side of operator {operatorToken.operatorType} has incompatible type: {arg1.GetValueType().Name}");
            }

            if (!(arg2.GetValueType() == typeof(double) || arg2.GetValueType() == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Right side of operator {operatorToken.operatorType} has incompatible type: {arg2.GetValueType().Name}");
            }


            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                switch (operatorToken.operatorType)
                {
                    case Operator.IsGreaterThan: return new LiteralToken<bool>(operatorToken, litArg1.GetAs<double>() > litArg2.GetAs<double>());
                    case Operator.IsGreaterThanOrEqualTo: return new LiteralToken<bool>(operatorToken, litArg1.GetAs<double>() >= litArg2.GetAs<double>());
                    case Operator.IsLessThan: return new LiteralToken<bool>(operatorToken, litArg1.GetAs<double>() < litArg2.GetAs<double>());
                    case Operator.IsLessThanOrEqualTo: return new LiteralToken<bool>(operatorToken, litArg1.GetAs<double>() <= litArg2.GetAs<double>());

                    default: throw new ArgumentException($"Unexpected Operator {operatorToken.operatorType}");
                }
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
            if (!typeof(T).AssignableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying {operatorType} as type {typeof(T).Name}");
            }

            switch (operatorType)
            {
                case Operator.IsGreaterThan: return (T)(object)(arg1.GetAs<double>(context) > arg2.GetAs<double>(context));
                case Operator.IsGreaterThanOrEqualTo: return (T)(object)(arg1.GetAs<double>(context) >= arg2.GetAs<double>(context));
                case Operator.IsLessThan: return (T)(object)(arg1.GetAs<double>(context) < arg2.GetAs<double>(context));
                case Operator.IsLessThanOrEqualTo: return (T)(object)(arg1.GetAs<double>(context) <= arg2.GetAs<double>(context));

                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public Type GetValueType() => typeof(bool);
    }
}
