using System;
using System.Globalization;
using System.Reflection;

namespace BGC.Scripting
{
    public class ConstructObjectExpression : IValueGetter
    {
        private readonly Type objectType;
        private readonly InvocationArgument[] args;

        public ConstructObjectExpression(
            Type objectType,
            InvocationArgument[] args)
        {
            this.objectType = objectType;
            this.args = args;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(objectType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of object construction of type {objectType.Name} as type {returnType.Name}");
            }

            if (args.Length == 0)
            {
                return (T)Activator.CreateInstance(
                    type: objectType,
                    bindingAttr: BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                    binder: null,
                    args: null,
                    culture: CultureInfo.CurrentCulture);
            }
            else
            {
                object[] argumentValues = args.GetArgs(context);

                T value = (T)Activator.CreateInstance(
                    type: objectType,
                    bindingAttr: BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                    binder: null,
                    args: argumentValues,
                    culture: CultureInfo.CurrentCulture);

                args.HandlePostInvocation(argumentValues, context);

                return value;
            }
        }

        public Type GetValueType() => objectType;
    }
}