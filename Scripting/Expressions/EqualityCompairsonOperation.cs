using System;

namespace BGC.Scripting
{
    public class EqualityCompairsonOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;

        public static IExpression CreateEqualityComparisonOperator(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (arg1Type != arg2Type &&
                !arg1Type.AssignableOrConvertableFromType(arg2Type) &&
                !arg2Type.AssignableOrConvertableFromType(arg1Type))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Incompatible Types for {operatorToken.operatorType} operator: {arg1Type.Name} and {arg2Type.Name}");
            }

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                return new LiteralToken<bool>(
                    operatorToken,
                    PerformOperator(litArg1.GetAs<object>(), litArg2.GetAs<object>(), operatorToken.operatorType));
            }

            return new EqualityCompairsonOperation(arg1, arg2, operatorToken.operatorType);
        }


        private EqualityCompairsonOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Operator operatorType)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.operatorType = operatorType;

            switch (operatorType)
            {
                case Operator.IsEqualTo:
                case Operator.IsNotEqualTo:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableOrConvertableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException(
                    $"Return value of {operatorType} is a boolean, but it was accessed as {typeof(T).Name}");
            }

            return (T)(object)PerformOperator(arg1.GetAs<object>(context)!, arg2.GetAs<object>(context)!, operatorType);
        }

        public Type GetValueType() => typeof(bool);

        private static bool PerformOperator(dynamic arg1, dynamic arg2, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.IsEqualTo: return arg1 == arg2;
                case Operator.IsNotEqualTo: return arg1 != arg2;

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }
    }
}