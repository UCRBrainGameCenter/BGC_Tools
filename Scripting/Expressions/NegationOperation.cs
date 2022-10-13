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
                string operatorName = source.operatorType switch
                {
                    Operator.Negate => "op_UnaryNegation",
                    Operator.BitwiseComplement => "op_OnesComplement",
                    _ => null,
                };
                if (operatorName == null)
                {
                    throw new ArgumentException($"Unexpected Operator: {source.operatorType}");
                }

                var (canInvoke, error) = argType.CanInvokeStaticMethod(operatorName, argType);
                if (!canInvoke)
                {
                    throw new ScriptParsingException(source, error);
                }
            }
            else
            {
                Type promotedType = source.GetUnaryPromotedType(argType);

                if (arg is LiteralToken litArg)
                {
                    return new ConstantToken(source, PerformOperator(litArg.GetAs<object>(), source.operatorType), promotedType);
                }
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

        private static object PerformOperator(object arg, Operator operatorType)
        {
            switch (operatorType)
            {
                case Operator.Negate: return PerformNegate(arg);
                case Operator.BitwiseComplement: return PerformBitwiseComplement(arg);
                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }
        }

        private static object PerformNegate(object arg)
        {
            Type argType = arg.GetType();
            if (argType.IsPrimitive)
            {
                switch (arg)
                {
                    case byte prim: return -prim;
                    case sbyte prim: return -prim;
                    case short prim: return -prim;
                    case ushort prim: return -prim;
                    case int prim: return -prim;
                    case uint prim: return -prim;
                    case long prim: return -prim;
                    case nint prim: return -prim;
                    case char prim: return -prim;
                    case decimal prim: return -prim;
                    case float prim: return -prim;
                    case double prim: return -prim;
                }

                throw new ArgumentException($"Cannot apply unary operator - to type {argType.Name}");
            }
            else
            {
                return argType.InvokeStaticMethod("op_UnaryNegation", arg);
            }
        }

        private static object PerformBitwiseComplement(object arg)
        {
            Type argType = arg.GetType();
            if (argType.IsPrimitive)
            {
                switch (arg)
                {
                    case byte prim: return ~prim;
                    case sbyte prim: return ~prim;
                    case short prim: return ~prim;
                    case ushort prim: return ~prim;
                    case int prim: return ~prim;
                    case uint prim: return ~prim;
                    case long prim: return ~prim;
                    case ulong prim: return ~prim;
                    case nint prim: return ~prim;
                    case nuint prim: return ~prim;
                    case char prim: return ~prim;
                }

                throw new ArgumentException($"Cannot apply unary operator - to type {argType.Name}");
            }
            else
            {
                return argType.InvokeStaticMethod("op_OnesComplement", arg);
            }
        }
    }
}