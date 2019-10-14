using System;

namespace BGC.Scripting
{
    public class CastingMemberValueOperation : IValueGetter
    {
        private readonly IValueGetter value;
        private readonly Type outputType;
        private readonly Func<object, object> operation;

        public CastingMemberValueOperation(
            IValueGetter value,
            Type outputType,
            Func<object, object> operation)
        {
            this.value = value;
            this.outputType = outputType;
            this.operation = operation;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(outputType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Indexing with type {outputType.Name} as type {returnType.Name}");
            }

            object result = operation(value.GetAs<object>(context));

            if (typeof(T).IsAssignableFrom(outputType))
            {
                return (T)result;
            }

            return (T)Convert.ChangeType(result, typeof(T));
        }

        public Type GetValueType() => outputType;
    }

}
