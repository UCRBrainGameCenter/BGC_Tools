using System;
using System.Reflection;

namespace BGC.Scripting.Parsing
{
    public abstract class RegisteredGettableFieldOperation : IValueGetter
    {
        private readonly FieldInfo fieldInfo;
        private readonly Type fieldType;
        private readonly Token source;

        protected abstract object GetInstanceValue(RuntimeContext context);

        public RegisteredGettableFieldOperation(
            FieldInfo fieldInfo,
            Token source)
        {
            this.fieldInfo = fieldInfo;
            this.source = source;
            fieldType = fieldInfo.FieldType;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(fieldType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve Field {fieldInfo.Name} with type {fieldType.Name} as type {returnType.Name}");
            }

            object result = fieldInfo.GetValue(GetInstanceValue(context));

            if (!returnType.IsAssignableFrom(fieldType))
            {
                return (T)Convert.ChangeType(result, returnType);
            }

            return (T)result;
        }

        public Type GetValueType() => fieldType;

        public override string ToString() => $"{GetType()}: From {source}.";
    }

    public class RegisteredGettableInstanceFieldOperation : RegisteredGettableFieldOperation
    {
        private readonly IValueGetter value;

        public RegisteredGettableInstanceFieldOperation(
            IValueGetter value,
            FieldInfo fieldInfo,
            Token source)
            : base(fieldInfo, source)
        {
            this.value = value;
        }

        protected override object GetInstanceValue(RuntimeContext context) => value.GetAs<object>(context);
    }

    public class RegisteredGettableStaticFieldOperation : RegisteredGettableFieldOperation
    {
        public RegisteredGettableStaticFieldOperation(
            FieldInfo fieldInfo,
            Token source)
            : base(fieldInfo, source)
        {
        }

        protected override object GetInstanceValue(RuntimeContext context) => null;
    }
}