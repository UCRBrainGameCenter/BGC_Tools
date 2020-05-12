using System;

namespace BGC.Scripting
{
    public class EqualityCompairsonOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;
        private readonly Type argType = null;

        public static IExpression CreateEqualityComparisonOperator(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type argType;
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (arg1Type == arg2Type)
            {
                argType = arg1Type;
            }
            else if (arg1Type.AssignableFromType(arg2Type))
            {
                argType = arg1Type;
            }
            else if (arg2Type.AssignableFromType(arg1Type))
            {
                argType = arg2Type;
            }
            else
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Incompatible Types for {operatorToken.operatorType} operator: {arg1Type.Name} and {arg2Type.Name}");
            }

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                object value1 = litArg1.GetAs<object>();
                object value2 = litArg2.GetAs<object>();

                if (argType != arg1Type)
                {
                    value1 = Convert.ChangeType(value1, argType);
                }

                if (argType != arg2Type)
                {
                    value2 = Convert.ChangeType(value2, argType);
                }

                switch (operatorToken.operatorType)
                {
                    case Operator.IsEqualTo: return new LiteralToken<bool>(operatorToken, value1.Equals(value2));
                    case Operator.IsNotEqualTo: return new LiteralToken<bool>(operatorToken, !value1.Equals(value2));

                    default: throw new ArgumentException($"Unexpected Operator: {operatorToken.operatorType}");
                }
            }

            return new EqualityCompairsonOperation(arg1, arg2, argType, operatorToken.operatorType);
        }


        private EqualityCompairsonOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Type argType,
            Operator operatorType)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
            this.argType = argType;
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
            if (!typeof(T).AssignableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException(
                    $"Return value of {operatorType} is a boolean, but it was accessed as {typeof(T).Name}");
            }

            if (argType == typeof(double) || argType == typeof(int))
            {
                switch (operatorType)
                {
                    case Operator.IsEqualTo: return (T)(object)(arg1.GetAs<double>(context) == arg2.GetAs<double>(context));
                    case Operator.IsNotEqualTo: return (T)(object)(arg1.GetAs<double>(context) != arg2.GetAs<double>(context));

                    default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }
            }
            else if (argType == typeof(bool))
            {
                switch (operatorType)
                {
                    case Operator.IsEqualTo: return (T)(object)(arg1.GetAs<bool>(context) == arg2.GetAs<bool>(context));
                    case Operator.IsNotEqualTo: return (T)(object)(arg1.GetAs<bool>(context) != arg2.GetAs<bool>(context));

                    default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }
            }
            else if (argType == typeof(string))
            {
                switch (operatorType)
                {
                    case Operator.IsEqualTo: return (T)(object)(arg1.GetAs<string>(context) == arg2.GetAs<string>(context));
                    case Operator.IsNotEqualTo: return (T)(object)(arg1.GetAs<string>(context) != arg2.GetAs<string>(context));

                    default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }
            }
            else
            {
                switch (operatorType)
                {
                    case Operator.IsEqualTo: return (T)(object)(arg1.GetAs<object>(context) == arg2.GetAs<object>(context));
                    case Operator.IsNotEqualTo: return (T)(object)(arg1.GetAs<object>(context) != arg2.GetAs<object>(context));

                    default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
                }
            }
        }

        public Type GetValueType() => typeof(bool);
    }
}
