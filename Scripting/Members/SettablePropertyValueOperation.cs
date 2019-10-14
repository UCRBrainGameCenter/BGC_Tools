using System;

namespace BGC.Scripting
{
    public class SettablePropertyValueOperation<TInput, TResult> : IValue
    {
        private readonly IValueGetter value;
        private readonly Func<TInput, TResult> getOperation;
        private readonly Action<TInput, TResult> setOperation;

        public SettablePropertyValueOperation(
            IValueGetter value,
            Func<TInput, TResult> getOperation,
            Action<TInput, TResult> setOperation,
            Token source)
        {
            this.value = value;
            this.getOperation = getOperation;
            this.setOperation = setOperation;

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

            TResult result = getOperation(value.GetAs<TInput>(context));

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public void Set(RuntimeContext context, object newValue)
        {
            Type inputType = newValue.GetType();

            if (!typeof(TResult).AssignableFromType(inputType))
            {
                throw new ScriptRuntimeException($"Tried to set result of Indexing with type {typeof(TResult).Name} as type {inputType.Name}");
            }

            setOperation(
                value.GetAs<TInput>(context),
                (TResult)Convert.ChangeType(newValue, typeof(TResult)));
        }

        public void SetAs<T>(RuntimeContext context, T newValue)
        {
            Type inputType = typeof(T);

            if (!typeof(TResult).AssignableFromType(inputType))
            {
                throw new ScriptRuntimeException($"Tried to set result of Indexing with type {typeof(TResult).Name} as type {inputType.Name}");
            }

            setOperation(
                value.GetAs<TInput>(context),
                (TResult)Convert.ChangeType(newValue, typeof(TResult)));
        }

        public Type GetValueType() => typeof(TResult);

    }

}
