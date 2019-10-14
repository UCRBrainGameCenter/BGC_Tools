using System;

namespace BGC.Scripting
{
    public class GettablePropertyValueOperation<TInput, TResult> : IValueGetter
    {
        private readonly IValueGetter value;
        private readonly Func<TInput, TResult> operation;

        public GettablePropertyValueOperation(
            IValueGetter value,
            Func<TInput, TResult> operation,
            Token source)
        {
            this.value = value;
            this.operation = operation;

            if (!typeof(TInput).AssignableFromType(value.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Incorrect value type.  Expected: {typeof(TInput).Name}.  Received: {value.GetValueType().Name}. ");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(typeof(TResult)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Indexing with type {typeof(TResult).Name} as type {returnType.Name}");
            }

            TResult result = operation(value.GetAs<TInput>(context));

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public Type GetValueType() => typeof(TResult);
    }

}
