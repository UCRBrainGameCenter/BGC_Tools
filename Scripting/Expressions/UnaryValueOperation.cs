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
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Cannot perform numerical operation {operatorType} on nonNumerical value {arg} of type {argType.Name}");
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
            dynamic value = arg.GetAs<object>(context)!;

            switch (operatorType)
            {
                case Operator.Increment:
                    value++;
                    break;

                case Operator.Decrement:
                    value--;
                    break;

                default: throw new ArgumentException($"Unexpected Operator {operatorType}");
            }

            arg.Set(context, value);
        }
    }
}