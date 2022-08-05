using System;
using System.Reflection;

namespace BGC.Scripting.Parsing
{
    public abstract class RegisteredGettablePropertyOperation : IValueGetter
    {
        private readonly PropertyInfo propertyInfo;
        private readonly Type propertyType;
        private readonly Token source;

        protected abstract object GetInstanceValue(RuntimeContext context);

        public RegisteredGettablePropertyOperation(
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

        public Type GetValueType() => propertyType;

        public override string ToString() => $"{GetType()}: From {source}.";
    }

    public class RegisteredGettableInstancePropertyOperation : RegisteredGettablePropertyOperation
    {
        private readonly IValueGetter value;

        public RegisteredGettableInstancePropertyOperation(
            IValueGetter value,
            PropertyInfo propertyInfo,
            Token source)
            : base(propertyInfo, source)
        {
            this.value = value;
        }

        protected override object GetInstanceValue(RuntimeContext context) => value.GetAs<object>(context);
    }

    public class RegisteredGettableStaticPropertyOperation : RegisteredGettablePropertyOperation
    {
        public RegisteredGettableStaticPropertyOperation(
            PropertyInfo propertyInfo,
            Token source)
            : base(propertyInfo, source)
        {
        }

        protected override object GetInstanceValue(RuntimeContext context) => null;
    }
}