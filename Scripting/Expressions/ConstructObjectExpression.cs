using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace BGC.Scripting
{
    public class ConstructObjectExpression : IValueGetter
    {
        private readonly Type objectType;
        private readonly IValueGetter[] args;

        public ConstructObjectExpression(
            Type objectType,
            IValueGetter[] args)
        {
            this.objectType = objectType;
            this.args = args;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(objectType))
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
                return (T)Activator.CreateInstance(
                    type: objectType,
                    bindingAttr: BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                    binder: null,
                    args: args.Select(x => x.GetAs<object>(context)).ToArray(),
                    culture: CultureInfo.CurrentCulture);
            }
        }

        public Type GetValueType() => objectType;
    }
}
