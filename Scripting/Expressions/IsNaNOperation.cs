using System;

namespace BGC.Scripting
{
    public class IsNaNMathFunction : IValueGetter
    {
        private readonly IValueGetter arg;

        public IsNaNMathFunction(
            IValueGetter arg,
            Token source)
        {
            if (!(arg.GetValueType() == typeof(double) || arg.GetValueType() == typeof(int)))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Argument of IsNaN is not numerical: type {arg.GetValueType().Name}");
            }

            this.arg = arg;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableFromType(typeof(bool)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of IsNaN function at type {typeof(T).Name}");
            }

            return (T)(object)double.IsNaN(arg.GetAs<double>(context));
        }

        public Type GetValueType() => typeof(bool);
    }
}
