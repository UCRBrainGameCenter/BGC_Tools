using System;
using System.Reflection;

namespace BGC.Scripting.Parsing
{
    public abstract class RegisteredSettableFieldOperation : IValue
    {
        private readonly FieldInfo fieldInfo;
        private readonly Type fieldType;
        private readonly Token source;

        protected abstract object GetInstanceValue(RuntimeContext context);

        public RegisteredSettableFieldOperation(
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

        public void Set(RuntimeContext context, object newValue)
        {
            Type inputType = newValue?.GetType() ?? typeof(object);

            if (!fieldType.AssignableOrConvertableFromType(inputType))
            {
                throw new ScriptRuntimeException($"Tried to set Field with {fieldInfo.Name} type {fieldType.Name} as type {inputType.Name}");
            }

            object convertedValue = newValue;

            if (!fieldType.IsAssignableFrom(inputType))
            {
                convertedValue = Convert.ChangeType(convertedValue, fieldType);
            }

            fieldInfo.SetValue(GetInstanceValue(context), convertedValue);
        }

        public void SetAs<T>(RuntimeContext context, T newValue)
        {
            Type inputType = typeof(T);

            if (!fieldType.AssignableOrConvertableFromType(inputType))
            {
                throw new ScriptRuntimeException($"Tried to set Field {fieldInfo.Name} with type {fieldType.Name} as type {inputType.Name}");
            }

            object convertedValue = newValue;

            if (!fieldType.IsAssignableFrom(inputType))
            {
                convertedValue = Convert.ChangeType(convertedValue, fieldType);
            }

            fieldInfo.SetValue(GetInstanceValue(context), convertedValue);
        }

        public Type GetValueType() => fieldType;
        public override string ToString() => $"{GetType()}: From {source}.";
    }

    public class RegisteredSettableInstanceFieldOperation : RegisteredSettableFieldOperation
    {
        private readonly IValueGetter value;

        public RegisteredSettableInstanceFieldOperation(
            IValueGetter value,
            FieldInfo fieldInfo,
            Token source)
            : base(fieldInfo, source)
        {
            this.value = value;
        }

        protected override object GetInstanceValue(RuntimeContext context) => value.GetAs<object>(context);
    }

    public class RegisteredSettableStaticFieldOperation : RegisteredSettableFieldOperation
    {
        public RegisteredSettableStaticFieldOperation(
            FieldInfo fieldInfo,
            Token source)
            : base(fieldInfo, source)
        {
        }

        protected override object GetInstanceValue(RuntimeContext context) => null;
    }
}