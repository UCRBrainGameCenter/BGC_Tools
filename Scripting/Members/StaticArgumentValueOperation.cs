using System;

namespace BGC.Scripting
{
    public class StaticArgumentValueOperation<TResult> : IValueGetter
    {
        private readonly Func<RuntimeContext, TResult> operation;

        public StaticArgumentValueOperation(
            Func<RuntimeContext, TResult> operation)
        {
            this.operation = operation;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(typeof(TResult)))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Indexing with type {typeof(TResult).Name} as type {returnType.Name}");
            }

            TResult result = operation(context);

            if (typeof(T).IsAssignableFrom(typeof(TResult)))
            {
                return (T)(object)result;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public Type GetValueType() => typeof(TResult);
    }
}
