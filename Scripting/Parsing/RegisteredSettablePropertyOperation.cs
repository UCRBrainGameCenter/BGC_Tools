using System;
using System.Reflection;

namespace BGC.Scripting.Parsing
{
    public abstract class RegisteredSettablePropertyOperation : IValue
    {
        private readonly PropertyInfo propertyInfo;
        private readonly Type propertyType;
        private readonly Token source;

        protected abstract object GetInstanceValue(RuntimeContext context);

        public RegisteredSettablePropertyOperation(
            PropertyInfo propertyInfo,
            Token source)
        {
            this.propertyInfo = propertyInfo;
            this.source = source;
            propertyType = propertyInfo.PropertyType;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(propertyType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve Property {propertyInfo.Name} with type {propertyType.Name} as type {returnType.Name}");
            }

            object result = propertyInfo.GetValue(GetInstanceValue(context));

            if (!returnType.IsAssignableFrom(propertyType))
            {
                return (T)Convert.ChangeType(result, returnType);
            }

            return (T)result;
        }

        public void Set(RuntimeContext context, object newValue)
        {
            Type inputType = newValue?.GetType() ?? typeof(object);

            if (!propertyType.AssignableOrConvertableFromType(inputType))
            {
                throw new ScriptRuntimeException($"Tried to set Property {propertyInfo.Name} with type {propertyType.Name} as type {inputType.Name}");
            }

            object convertedValue = newValue;

            if (!propertyType.IsAssignableFrom(inputType))
            {
                convertedValue = Convert.ChangeType(convertedValue, propertyType);
            }

            propertyInfo.SetValue(GetInstanceValue(context), convertedValue);
        }

        public void SetAs<T>(RuntimeContext context, T newValue)
        {
            Type inputType = typeof(T);

            if (!propertyType.AssignableOrConvertableFromType(inputType))
            {
                throw new ScriptRuntimeException($"Tried to set Property {propertyInfo.Name} with type {propertyType.Name} as type {inputType.Name}");
            }

            object convertedValue = newValue;

            if (!propertyType.IsAssignableFrom(inputType))
            {
                convertedValue = Convert.ChangeType(convertedValue, propertyType);
            }

            propertyInfo.SetValue(GetInstanceValue(context), convertedValue);
        }

        public Type GetValueType() => propertyType;
        public override string ToString() => $"{GetType()}: From {source}.";
    }

    public class RegisteredSettableInstancePropertyOperation : RegisteredSettablePropertyOperation
    {
        private readonly IValueGetter value;

        public RegisteredSettableInstancePropertyOperation(
            IValueGetter value,
            PropertyInfo propertyInfo,
            Token source)
            : base(propertyInfo, source)
        {
            this.value = value;
        }

        protected override object GetInstanceValue(RuntimeContext context) => value.GetAs<object>(context);
    }

    public class RegisteredSettableStaticPropertyOperation : RegisteredSettablePropertyOperation
    {
        public RegisteredSettableStaticPropertyOperation(
            PropertyInfo propertyInfo,
            Token source)
            : base(propertyInfo, source)
        {
        }

        protected override object GetInstanceValue(RuntimeContext context) => null;
    }
}