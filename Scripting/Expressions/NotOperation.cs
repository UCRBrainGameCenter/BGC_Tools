using System;

namespace BGC.Scripting
{
    public class NotOperation : IValueGetter
    {
        private readonly IValueGetter arg;

        public static IExpression CreateNotOperation(
            IValueGetter arg,
            OperatorToken operatorToken)
        {
            if (arg.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Argument of Operator {operatorToken} is not boolean: type {arg.GetValueType().Name}.");
            }

            if (arg is LiteralToken litArg)
            {
                return new LiteralToken<bool>(operatorToken, !litArg.GetAs<bool>());
            }

            return new NotOperation(arg);
        }

        public NotOperation(IValueGetter arg)
        {
            this.arg = arg;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            //Check Value Type
            if (!typeof(T).AssignableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Not operator at type {typeof(T).Name}");
            }

            return (T)(object)!arg.GetAs<bool>(context);
        }


        public Type GetValueType() => typeof(bool);
    }
}
