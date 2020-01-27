using System;

namespace BGC.Scripting
{
    public class CastOperation : IValueGetter
    {
        private readonly IValueGetter arg;
        private readonly Operator operatorType;
        private readonly Type valueType;

        public static IExpression CreateCastOperation(
            IValueGetter arg,
            OperatorToken operatorToken)
        {
            Type argType = arg.GetValueType();

            if (!(argType == typeof(double) || argType == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Argument of operator {operatorToken.operatorType} is not numerical: type {argType.Name}.");
            }

            if (arg is LiteralToken litArg)
            {
                switch (operatorToken.operatorType)
                {
                    case Operator.CastDouble: return new LiteralToken<double>(operatorToken, litArg.GetAs<double>());
                    case Operator.CastInteger: return new LiteralToken<int>(operatorToken, (int)litArg.GetAs<double>());

                    default: throw new ArgumentException($"Unexpected Operator: {operatorToken.operatorType}");
                }
            }

            return new CastOperation(arg, operatorToken.operatorType);
        }

        private CastOperation(
            IValueGetter arg,
            Operator operatorType)
        {
            this.arg = arg;
            this.operatorType = operatorType;

            switch (this.operatorType)
            {
                case Operator.CastDouble:
                    valueType = typeof(double);
                    break;

                case Operator.CastInteger:
                    valueType = typeof(int);
                    break;

                default: throw new ArgumentException($"Unexpected Operator: {this.operatorType}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying {operatorType} as type {returnType.Name}");
            }

            if (returnType == typeof(int))
            {
                return (T)(object)(int)arg.GetAs<double>(context);
            }

            switch (operatorType)
            {
                case Operator.CastDouble: return (T)(object)arg.GetAs<double>(context);
                case Operator.CastInteger: return (T)(object)(int)Math.Floor(arg.GetAs<double>(context));

                default: throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        public Type GetValueType() => valueType;
    }
}
