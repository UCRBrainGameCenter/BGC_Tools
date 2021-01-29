using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class StaticValueOperation<TResult> : IValueGetter
    {
        private readonly Func<TResult> operation;

        public StaticValueOperation(
            Func<TResult> operation)
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

            TResult result = operation();

            if (typeof(T).IsAssignableFrom(typeof(TResult)))
            {
                return (T)(object)result;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public Type GetValueType() => typeof(TResult);
    }
}
