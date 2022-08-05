using System;

namespace BGC.Scripting
{
    public class UnaryOperation : IValueGetter
    {
        private readonly IValueGetter arg;
        private readonly Type valueType;
        private readonly Operator operatorType;

        public static IExpression CreateUnaryOperation(
            IValueGetter arg,
            OperatorToken source)
        {
            Type argType = arg.GetValueType();

            if (!argType.IsExtendedPrimitive() || argType == typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Cannot negate a non-numerical value {arg} of type {argType.Name}");
            }

            Type promotedType = source.GetUnaryPromotedType(argType);

            if (arg is LiteralToken litArg)
            {
                return new ConstantToken(source, PerformOperator(litArg.GetAs<object>(), source.operatorType), promotedType);
            }

            return new UnaryOperation(arg, argType, source.operatorType);
        }

        private UnaryOperation(
            IValueGetter arg,
            Type valueType,
            Operator operatorType)
        {
            this.arg = arg;
            this.valueType = valueType;
            this.operatorType = operatorType;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying negation to {arg} of type {valueType.Name} as type {returnType.Name}");
            }

            object value = PerformOperator(arg.GetAs<object>(context)!, operatorType);

            if (!returnType.IsAssignableFrom(valueType))
            {
                return (T)Convert.ChangeType(value, returnType);
            }

            return (T)value;
        }

        public Type GetValueType() => valueType;

        private static object PerformOperator(dynamic arg, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.Negate: return -arg;
                case Operator.BitwiseComplement: return ~arg;

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }
    }
}