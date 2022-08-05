using System;
using System.Reflection;

namespace BGC.Scripting
{
    public class CastOperation : IValueGetter
    {
        private readonly IValueGetter arg;
        private readonly Type valueType;

        public static IExpression CreateCastOperation(
            IValueGetter arg,
            CastingOperationToken castingOperationToken)
        {
            return new CastOperation(arg, castingOperationToken.type);
        }

        private CastOperation(
            IValueGetter arg,
            Type valueType)
        {
            this.arg = arg;
            this.valueType = valueType;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of applying ({valueType}) as type {returnType.Name}");
            }

            object value = arg.GetAs<object>(context)!;
            value = CastAs(value, valueType);

            if (!returnType.IsAssignableFrom(valueType))
            {
                value = Convert.ChangeType(value, returnType);
            }

            return (T)value!;
        }


        public static Tout Cast<Tout>(dynamic obj)
        {
            try
            {
                return (Tout)obj;
            }
            catch (Exception)
            {
                return default!;
            }
        }

        public static object CastAs(object obj, Type type)
        {
            MethodInfo methodInfo = typeof(CastOperation)
                .GetMethod(nameof(Cast), BindingFlags.Static | BindingFlags.Public)!
                .MakeGenericMethod(type);

            return methodInfo.Invoke(null, new object[] { obj });
        }

        public Type GetValueType() => valueType;
    }
}