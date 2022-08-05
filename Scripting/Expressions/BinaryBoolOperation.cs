using System;

namespace BGC.Scripting
{
    public class BinaryBoolOperation : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Operator operatorType;

        public static IExpression CreateBinaryBoolOperator(
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            if (arg1.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Left side of operator {operatorToken.operatorType} not of expected type bool: {arg1.GetValueType().Name}");
            }

            if (arg2.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Right side of operator {operatorToken.operatorType} not of expected type bool: {arg2.GetValueType().Name}");
            }

            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                switch (operatorToken.operatorType)
                {
                    case Operator.And: return new LiteralToken<bool>(operatorToken, litArg1.GetAs<bool>() && litArg2.GetAs<bool>());
                    case Operator.Or: return new LiteralToken<bool>(operatorToken, litArg1.GetAs<bool>() || litArg2.GetAs<bool>());

                    default: throw new ArgumentException($"Unexpected Operator {operatorToken.operatorType}");
                }
            }

            return new BinaryBoolOperation(arg1, arg2, operatorToken.operatorType);
        }

        private BinaryBoolOperation(
            IValueGetter arg1,
            IValueGetter arg2,
            Operator operatorType)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;

            this.operatorType = operatorType;

            switch (operatorType)
            {
                case Operator.And:
                case Operator.Or:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableOrConvertableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying {operatorType} as type {typeof(T).Name}");
            }

            switch (operatorType)
            {
                case Operator.And: return (T)(object)(arg1.GetAs<bool>(context) && arg2.GetAs<bool>(context));
                case Operator.Or: return (T)(object)(arg1.GetAs<bool>(context) || arg2.GetAs<bool>(context));

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        public Type GetValueType() => typeof(bool);
    }
}