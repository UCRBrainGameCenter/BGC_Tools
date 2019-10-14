using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class UnaryValueOperation : IValueGetter, IExecutable
    {
        private readonly IValue arg;
        private readonly Operator operatorType;
        private readonly Type valueType;

        private readonly bool increment;
        private readonly bool prefix;

        public UnaryValueOperation(
            IValue arg,
            OperatorToken operatorToken,
            bool prefix)
        {
            Type argType = arg.GetValueType();

            if (!(argType == typeof(double) || argType == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Cannot perform numerical operation {operatorType} on nonNumerical value {arg} of type {argType.Name}");
            }

            this.arg = arg;
            this.prefix = prefix;
            valueType = argType;
            operatorType = operatorToken.operatorType;

            switch (operatorType)
            {
                case Operator.Increment:
                    increment = true;
                    break;

                case Operator.Decrement:
                    increment = false;
                    break;

                default:
                    throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }

        private void Modify(RuntimeContext context)
        {
            int diff = increment ? 1 : -1;

            if (valueType == typeof(int))
            {
                //Modify as Integer
                arg.SetAs(context, arg.GetAs<int>(context) + diff);
            }
            else
            {
                //Modify as Double
                arg.SetAs(context, arg.GetAs<double>(context) + diff);
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!(returnType == typeof(int) || returnType == typeof(double)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying {operatorType} to {arg} of type {valueType.Name} as type {returnType.Name}");
            }

            if (returnType == typeof(int) && valueType != typeof(int))
            {
                throw new ScriptRuntimeException($"Tried to implicitly cast the results of {this} to type {returnType.Name}");
            }

            if (returnType == typeof(int))
            {
                if (prefix)
                {
                    Modify(context);
                    return (T)(object)arg.GetAs<int>(context);
                }
                else
                {
                    //PostFix
                    int outputValue = arg.GetAs<int>(context);
                    Modify(context);
                    return (T)(object)outputValue;
                }
            }

            if (prefix)
            {
                Modify(context);
                return (T)(object)arg.GetAs<double>(context);
            }
            else
            {
                //PostFix
                double outputValue = arg.GetAs<double>(context);
                Modify(context);
                return (T)(object)outputValue;
            }
        }

        public FlowState Execute(ScopeRuntimeContext context)
        {
            Modify(context);

            return FlowState.Nominal;
        }

        public Type GetValueType() => valueType;
    }
}
