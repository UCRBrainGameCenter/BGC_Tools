using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BGC.DataStructures.Generic;

namespace BGC.Scripting
{
    public class ConstructInitializedCollectionExpression : IValueGetter
    {
        private readonly Type objectType;
        private readonly IValueGetter[] args;
        private readonly IValueGetter[] items;

        private readonly MethodInfo addMethod;

        public ConstructInitializedCollectionExpression(
            Type objectType,
            IValueGetter[] args,
            IValueGetter[] items,
            Token source)
        {
            this.objectType = objectType;
            this.args = args;
            this.items = items;

            Type genericTypeDefinition = objectType.GetGenericTypeDefinition();

            if (typeof(IList<>).IsAssignableFrom(genericTypeDefinition) ||
                typeof(RingBuffer<>).IsAssignableFrom(genericTypeDefinition) ||
                objectType.GetInterfaces().Any(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDepletable<>)))
            {
                addMethod = objectType.GetMethod("Add");
            }
            else if (typeof(Stack<>).IsAssignableFrom(genericTypeDefinition))
            {
                addMethod = objectType.GetMethod("Push");
            }
            else if (typeof(Queue<>).IsAssignableFrom(genericTypeDefinition))
            {
                addMethod = objectType.GetMethod("Enqueue");
            }
            else
            {
                addMethod = objectType.GetMethod("Add");
                if (addMethod == null)
                {
                    throw new ScriptParsingException(
                        source: source,
                        message: $"Unable to find appropriate Add method for Container: {objectType.Name}");
                }
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(objectType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of object construction of type {objectType.Name} as type {returnType.Name}");
            }

            T newCollection;

            if (args.Length == 0)
            {
                newCollection = (T)Activator.CreateInstance(
                    type: objectType,
                    bindingAttr: BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                    binder: null,
                    args: null,
                    culture: CultureInfo.CurrentCulture);
            }
            else
            {
                newCollection = (T)Activator.CreateInstance(
                    type: objectType,
                    bindingAttr: BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding,
                    binder: null,
                    args: args.Select(x => x.GetAs<object>(context)).ToArray(),
                    culture: CultureInfo.CurrentCulture);
            }

            object[] item = new object[1];
            foreach(IValueGetter value in items)
            {
                item[0] = value.GetAs<object>(context);
                addMethod.Invoke(newCollection, item);
            }

            return newCollection;
        }

        public Type GetValueType() => objectType;
    }
}
