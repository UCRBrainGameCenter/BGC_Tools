using BGC.Mathematics;
using System;

namespace BGC.Scripting
{
    public class ClampMathFunction : IValueGetter
    {
        private readonly IValueGetter value;
        private readonly IValueGetter lowerbound;
        private readonly IValueGetter upperbound;
        private readonly Type valueType;

        public ClampMathFunction(
            IValueGetter value,
            IValueGetter lowerbound,
            IValueGetter upperbound,
            Token source)
        {
            Type inputValueType = value.GetValueType();
            Type lowerboundType = lowerbound.GetValueType();
            Type upperboundType = upperbound.GetValueType();

            if (!(inputValueType == typeof(int) || inputValueType == typeof(double)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Tried to apply function Clamp to value {value} of type {inputValueType.Name}");
            }

            if (!(lowerboundType == typeof(int) || lowerboundType == typeof(double)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Tried to apply function Clamp with lowerbound value {lowerbound} of type {lowerboundType.Name}");
            }

            if (!(upperboundType == typeof(int) || upperboundType == typeof(double)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Tried to apply function Clamp with upperbound value {upperbound} of type {upperboundType.Name}");
            }

            if (inputValueType == typeof(int) &&
                lowerboundType == typeof(int) &&
                upperboundType == typeof(int))
            {
                valueType = inputValueType;
            }
            else
            {
                valueType = typeof(double);
            }

            this.value = value;
            this.lowerbound = lowerbound;
            this.upperbound = upperbound;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if(!returnType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying Clamp to {value} as type {returnType.Name}");
            }

            if (returnType == typeof(int))
            {
                return (T)(object)GeneralMath.Clamp(
                    value: value.GetAs<int>(context),
                    min: lowerbound.GetAs<int>(context),
                    max: upperbound.GetAs<int>(context));
            }

            return (T)(object)GeneralMath.Clamp(
                value: value.GetAs<double>(context),
                min: lowerbound.GetAs<double>(context),
                max: upperbound.GetAs<double>(context));
        }

        public Type GetValueType() => valueType;
    }
}
