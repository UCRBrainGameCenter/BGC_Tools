using System;

namespace BGC.Scripting
{
    public class TernaryOperation : IValueGetter
    {
        private readonly IValueGetter condition;
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;
        private readonly Type valueType;

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
            else if (arg1Type.IsAssignableFrom(arg2Type))
            {
                valueType = arg1Type;
            }
            else if (arg2Type.IsAssignableFrom(arg1Type))
            {
                valueType = arg2Type;
            }
            else if (arg1Type.IsExtendedPrimitive() && arg2Type.IsExtendedPrimitive())
            {
                valueType = operatorToken.GetBinaryPromotedType(arg1Type, arg2Type);
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

            if (!returnType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to implicitly cast the results of {this} to type {returnType.Name} instead of argument type {valueType.Name}");
            }

            bool cond = condition.GetAs<bool>(context);

            if (!returnType.IsAssignableFrom(valueType))
            {
                return (T)Convert.ChangeType(cond ? arg1.GetAs<object>(context) : arg2.GetAs<object>(context), returnType);
            }

            return (T)(cond ? arg1.GetAs<object>(context) : arg2.GetAs<object>(context));
        }

        public Type GetValueType() => valueType;
    }
}