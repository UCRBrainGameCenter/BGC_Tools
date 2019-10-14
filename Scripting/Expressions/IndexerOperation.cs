using System;
using System.Reflection;

namespace BGC.Scripting
{
    public class IndexerOperation : IValue, IValueGetter, IValueSetter
    {
        private readonly IValueGetter valueArg;
        private readonly IValueGetter indexArg;
        private readonly Type indexerType;
        private readonly Type getType;
        private readonly MethodInfo indexGetter;
        private readonly MethodInfo indexSetter;

        public IndexerOperation(
            IValueGetter valueArg,
            IValueGetter indexArg,
            Token source)
        {
            Type valueType = valueArg.GetValueType();

            indexerType = valueType.GetIndexingType();

            if (indexerType == null)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Indexer used on non-Indexable type: type {valueType.Name}");
            }

            getType = valueType.GetIndexingReturnType();

            if (!indexerType.AssignableFromType(indexArg.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Indexer value must be of type {indexerType.Name}: type {indexArg.GetValueType().Name}");
            }

            this.valueArg = valueArg;
            this.indexArg = indexArg;

            indexGetter = valueType.GetMethod("get_Item", new Type[] { indexerType });
            indexSetter = valueType.GetMethod("set_Item", new Type[] { indexerType, getType });

            if (indexGetter == null)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Failed to get indexGetter for type: type {valueType.Name}");
            }

            if (indexSetter == null)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Failed to get indexSetter for type: type {valueType.Name}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(getType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Indexing with type {getType.Name} as type {returnType.Name}");
            }

            object index = indexArg.GetAs<object>(context);

            if (!indexerType.IsAssignableFrom(indexArg.GetValueType()))
            {
                index = Convert.ChangeType(index, indexerType);
            }

            object value = indexGetter.Invoke(valueArg.GetAs<object>(context), new object[] { index });

            if (returnType.IsAssignableFrom(getType))
            {
                return (T)value;
            }

            return (T)Convert.ChangeType(value, returnType);
        }

        public void Set(RuntimeContext context, object value)
        {
            Type valueType = value.GetType();

            if (!getType.AssignableFromType(valueType))
            {
                throw new ScriptRuntimeException($"Tried to set result of Indexing with type {getType.Name} as type {valueType.Name}");
            }

            object index = indexArg.GetAs<object>(context);

            if (!indexerType.IsAssignableFrom(indexArg.GetValueType()))
            {
                index = Convert.ChangeType(index, indexerType);
            }

            if (!getType.IsAssignableFrom(valueType))
            {
                value = Convert.ChangeType(value, getType);
            }

            indexSetter.Invoke(valueArg.GetAs<object>(context), new object[] { index, value });
        }

        public void SetAs<T>(RuntimeContext context, T value)
        {
            Type setType = typeof(T);

            if (!getType.AssignableFromType(setType))
            {
                throw new ScriptRuntimeException($"Tried to set result of Indexing with type {getType.Name} as type {setType.Name}");
            }

            object index = indexArg.GetAs<object>(context);

            if (!indexerType.IsAssignableFrom(indexArg.GetValueType()))
            {
                index = Convert.ChangeType(index, indexerType);
            }

            if (getType.IsAssignableFrom(setType))
            {
                indexSetter.Invoke(valueArg.GetAs<object>(context), new object[] { index, value });
            }
            else
            {
                indexSetter.Invoke(valueArg.GetAs<object>(context), new object[] { index, Convert.ChangeType(value, getType) });
            }
        }

        public Type GetValueType() => getType;

    }
}
