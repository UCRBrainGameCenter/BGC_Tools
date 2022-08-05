using System;

namespace BGC.Scripting
{
    public class ConstructArrayExpression : IValueGetter
    {
        private readonly Type elementType;
        private readonly Type arrayType;
        private readonly IValueGetter arg;
        private readonly IValueGetter[] initializer;

        public ConstructArrayExpression(
            Type arrayType,
            IValueGetter arg,
            IValueGetter[] initializer,
            Token token)
        {
            this.arrayType = arrayType;
            elementType = arrayType.GetElementType()!;

            this.arg = arg;
            this.initializer = initializer;

            if (arg is null && initializer is null)
            {
                throw new ScriptParsingException(
                    source: token,
                    message: $"Array construction requires either an element number or an initializer");
            }

            if (initializer is not null)
            {
                for (int i = 0; i < initializer.Length; i++)
                {
                    if (!elementType.AssignableOrConvertableFromType(initializer[i].GetValueType()))
                    {
                        throw new ScriptParsingException(
                            source: token,
                            message: $"Item {i} of type {initializer[i].GetValueType()} in initializer list of array of type {elementType} is incompatible.");

                    }
                }
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(arrayType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of object construction of type {arrayType.Name} as type {returnType.Name}");
            }

            int count;

            if (arg is not null)
            {
                count = arg.GetAs<int>(context);

                if (initializer is not null && initializer.Length > count)
                {
                    throw new ScriptRuntimeException(
                        $"Constructed an array with a count of {count} but an initializer list of {initializer.Length} elements");
                }
            }
            else
            {
                count = initializer!.Length;
            }

            Array array = Array.CreateInstance(elementType, count);

            if (initializer is not null)
            {
                for (int i = 0; i < initializer.Length; i++)
                {
                    object value = initializer[i].GetAs<object>(context);
                    if (!elementType.IsAssignableFrom(initializer[i].GetValueType()))
                    {
                        value = Convert.ChangeType(value, elementType);
                    }

                    array.SetValue(value, i);
                }
            }

            return (T)(object)array;
        }

        public Type GetValueType() => arrayType;
    }
}