using System;
using System.Reflection;

namespace BGC.Scripting
{
    public class IndexerOperation : IValue, IValueGetter, IValueSetter
    {
        private readonly IValueGetter containerArg;
        private readonly IValueGetter indexArg;
        private readonly Type containerType;
        private readonly Type indexerType;
        private readonly Type getType;
        private readonly MethodInfo indexGetter;
        private readonly MethodInfo indexSetter;

        public IndexerOperation(
            IValueGetter containerArg,
            IValueGetter indexArg,
            Token source)
        {
            containerType = containerArg.GetValueType();

            indexerType = containerType.GetIndexingType()!;

            if (indexerType == null)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Indexer used on non-Indexable type: type {containerType.Name}");
            }

            getType = containerType.GetIndexingReturnType()!;

            if (!indexerType.AssignableOrConvertableFromType(indexArg.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Indexer value must be of type {indexerType.Name}: type {indexArg.GetValueType().Name}");
            }

            this.containerArg = containerArg;
            this.indexArg = indexArg;

            if (!containerType.IsArray)
            {
                indexGetter = containerType.GetMethod("get_Item", new Type[] { indexerType })!;
                indexSetter = containerType.GetMethod("set_Item", new Type[] { indexerType, getType! })!;

                if (indexGetter == null)
                {
                    throw new ScriptParsingException(
                        source: source,
                        message: $"Failed to get indexGetter for type: type {containerType.Name}");
                }

                if (indexSetter == null)
                {
                    throw new ScriptParsingException(
                        source: source,
                        message: $"Failed to get indexSetter for type: type {containerType.Name}");
                }
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(getType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Indexing with type {getType.Name} as type {returnType.Name}");
            }

            object value;

            if (!containerType.IsArray)
            {
                object index = indexArg.GetAs<object>(context);

                if (!indexerType.IsAssignableFrom(indexArg.GetValueType()))
                {
                    index = Convert.ChangeType(index, indexerType);
                }

                value = indexGetter!.Invoke(containerArg.GetAs<object>(context), new object[] { index! });

            }
            else
            {
                Array array = containerArg.GetAs<Array>(context)!;

                int index = indexArg.GetAs<int>(context);

                value = array.GetValue(index);
            }

            if (!returnType.IsAssignableFrom(getType))
            {
                return (T)Convert.ChangeType(value, returnType);
            }

            return (T)value;
        }

        public void Set(RuntimeContext context, object value)
        {
            Type valueType = value?.GetType() ?? typeof(object);

            if (!getType.AssignableOrConvertableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to set result of Indexing with type {getType.Name} as type {valueType.Name}");
            }

            if (!getType.IsAssignableFrom(valueType))
            {
                value = Convert.ChangeType(value, getType);
            }

            if (!containerType.IsArray)
            {
                object index = indexArg.GetAs<object>(context);

                if (!indexerType.IsAssignableFrom(indexArg.GetValueType()))
                {
                    index = Convert.ChangeType(index, indexerType);
                }

                indexSetter!.Invoke(containerArg.GetAs<object>(context), new object[] { index!, value! });
            }
            else
            {
                Array array = containerArg.GetAs<Array>(context)!;
                int index = indexArg.GetAs<int>(context);
                array.SetValue(value, index);
            }

        }

        public void SetAs<T>(RuntimeContext context, T value)
        {
            Type setType = typeof(T);

            if (!getType.AssignableOrConvertableFromType(setType))
            {
                throw new ScriptRuntimeException($"Tried to set result of Indexing with type {getType.Name} as type {setType.Name}");
            }

            object convertedValue = value;

            if (!getType.IsAssignableFrom(setType))
            {
                convertedValue = Convert.ChangeType(value, getType);
            }

            if (!containerType.IsArray)
            {
                object index = indexArg.GetAs<object>(context);

                if (!indexerType.IsAssignableFrom(indexArg.GetValueType()))
                {
                    index = Convert.ChangeType(index, indexerType);
                }

                indexSetter!.Invoke(containerArg.GetAs<object>(context), new object[] { index!, convertedValue! });
            }
            else
            {
                Array array = containerArg.GetAs<Array>(context)!;
                int index = indexArg.GetAs<int>(context);
                array.SetValue(convertedValue, index);
            }
        }

        public Type GetValueType() => getType;

    }
}