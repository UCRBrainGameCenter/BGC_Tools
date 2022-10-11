using System;
using System.Threading;

namespace BGC.Scripting
{
    public class UnaryValueOperation : IValueGetter, IExecutable
    {
        private readonly IValue arg;
        private readonly Operator operatorType;
        private readonly Type valueType;

        private readonly bool prefix;

        public UnaryValueOperation(
            IValue arg,
            OperatorToken operatorToken,
            bool prefix)
        {
            Type argType = arg.GetValueType();

            if (!argType.IsExtendedPrimitive() || argType == typeof(string))
            {
                string operatorName = operatorToken.operatorType switch
                {
                    Operator.Increment => "op_Increment",
                    Operator.Decrement => "op_Decrement",
                    _ => null,
                };
                if (operatorName == null)
                {
                    throw new ArgumentException($"Unexpected Operator: {operatorToken.operatorType}");
                }

                var (canInvoke, error) = argType.CanInvokeStaticMethod(operatorName, argType);
                if (!canInvoke)
                {
                    throw new ScriptParsingException(operatorToken, error);
                }
            }

            this.arg = arg;
            this.prefix = prefix;
            valueType = argType;
            operatorType = operatorToken.operatorType;
        }


        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying {operatorType} to {arg} of type {valueType.Name} as type {returnType.Name}");
            }

            object value;

            if (prefix)
            {
                Modify(context);
                value = arg.GetAs<object>(context)!;
            }
            else
            {
                //PostFix
                value = arg.GetAs<object>(context)!;
                Modify(context);
            }

            if (!returnType.IsAssignableFrom(valueType))
            {
                value = Convert.ChangeType(value, returnType);
            }

            return (T)value;
        }

        public FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            Modify(context);

            return FlowState.Nominal;
        }

        public Type GetValueType() => valueType;

        private void Modify(RuntimeContext context)
        {
            object value = arg.GetAs<object>(context)!;

            if (operatorType == Operator.Increment)
            {
                switch (value)
                {
                    case bool: throw new ArgumentException($"Operator {operatorType} cannot be applied to bool");
                    case byte prim: value = prim + 1; break;
                    case sbyte prim: value = prim + 1; break;
                    case short prim: value = prim + 1; break;
                    case ushort prim: value = prim + 1; break;
                    case int prim: value = prim + 1; break;
                    case uint prim: value = prim + 1; break;
                    case long prim: value = prim + 1; break;
                    case ulong prim: value = prim + 1; break;
                    case nint prim: value = prim + 1; break;
                    case nuint prim: value = prim + 1; break;
                    case char prim: value = prim + 1; break;
                    case decimal prim: value = prim + 1; break;
                    case float prim: value = prim + 1; break;
                    case double prim: value = prim + 1; break;
                    default:
                    {
                        value.GetType().InvokeStaticMethod("op_Increment", value);
                        break;
                    }
                }
            }
            else if (operatorType == Operator.Decrement)
            {
                switch (value)
                {
                    case bool: throw new ArgumentException($"Operator {operatorType} cannot be applied to bool");
                    case byte prim: value = prim - 1; break;
                    case sbyte prim: value = prim - 1; break;
                    case short prim: value = prim - 1; break;
                    case ushort prim: value = prim - 1; break;
                    case int prim: value = prim - 1; break;
                    case uint prim: value = prim - 1; break;
                    case long prim: value = prim - 1; break;
                    case ulong prim: value = prim - 1; break;
                    case nint prim: value = prim - 1; break;
                    case nuint prim: value = prim - 1; break;
                    case char prim: value = prim - 1; break;
                    case decimal prim: value = prim - 1; break;
                    case float prim: value = prim - 1; break;
                    case double prim: value = prim - 1; break;
                    default:
                    {
                        value.GetType().InvokeStaticMethod("op_Decrement", value);
                        break;
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Unexpected Operator {operatorType}");
            }

            arg.Set(context, value);
        }
    }
}