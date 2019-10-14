using System;

namespace BGC.Scripting
{
    public class TernaryOperation : IValueGetter
    {
        private readonly IValueGetter condition;
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Type valueType = null;

        public TernaryOperation(
            IValueGetter condition,
            IValueGetter arg1,
            IValueGetter arg2,
            OperatorToken operatorToken)
        {
            Type arg1Type = arg1.GetValueType();
            Type arg2Type = arg2.GetValueType();

            if (condition.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Condition of Ternary Operator must be a boolean value: type {condition.GetValueType()}.");
            }

            if (arg1Type == arg2Type)
            {
                valueType = arg1Type;
            }
            else if ((arg1Type == typeof(int) || arg1Type == typeof(double)) &&
                     (arg2Type == typeof(int) || arg2Type == typeof(double)))
            {
                valueType = typeof(double);
            }
            else
            {
                throw new ScriptParsingException(
                    source: operatorToken,
                    message: $"Incompatible argments in Ternary operator: {arg1Type.Name} vs {arg2Type.Name}");
            }

            this.condition = condition;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to implicitly cast the results of {this} to type {returnType.Name} instead of argument type {valueType.Name}");
            }

            bool cond = condition.GetAs<bool>(context);

            return (T)Convert.ChangeType(cond ? arg1.GetAs<object>(context) : arg2.GetAs<object>(context), typeof(T));
        }

        public Type GetValueType() => valueType;
    }
}
