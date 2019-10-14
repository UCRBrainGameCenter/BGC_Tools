using System;

namespace BGC.Scripting
{
    public class NegationOperation : IValueGetter
    {
        private readonly IValueGetter arg;
        private readonly Type valueType;

        public static IExpression CreateNegationOperation(
            IValueGetter arg,
            Token source)
        {
            Type argType = arg.GetValueType();

            if (!(argType == typeof(double) || argType == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Cannot negate a non-numerical value {arg} of type {argType.Name}");
            }

            if (arg is LiteralToken litArg)
            {
                if (argType == typeof(int))
                {
                    return new LiteralToken<int>(source, -litArg.GetAs<int>());
                }
                else
                {
                    return new LiteralToken<double>(source, -litArg.GetAs<double>());
                }
            }

            return new NegationOperation(arg, argType);
        }

        private NegationOperation(
            IValueGetter arg,
            Type valueType)
        {
            this.arg = arg;
            this.valueType = valueType;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying negation to {arg} of type {valueType.Name} as type {returnType.Name}");
            }

            object value;

            if (valueType == typeof(int))
            {
                value = -arg.GetAs<int>(context);
            }
            else
            {
                value = -arg.GetAs<double>(context);
            }

            if (returnType.IsAssignableFrom(valueType))
            {
                return (T)value;
            }

            return (T)Convert.ChangeType(value, returnType);
        }

        public Type GetValueType() => valueType;
    }
}
