using System;
using System.Collections.Generic;
using BGC.DataStructures.Generic;

namespace BGC.Scripting
{
    public static class TokenExtensions
    {
        public static Type GetValueType(this Keyword keyword)
        {
            switch (keyword)
            {
                case Keyword.Void: return typeof(void);

                case Keyword.Bool: return typeof(bool);
                case Keyword.Double: return typeof(double);
                case Keyword.Integer: return typeof(int);
                case Keyword.String: return typeof(string);

                case Keyword.List: return typeof(List<>);
                case Keyword.Queue: return typeof(Queue<>);
                case Keyword.Stack: return typeof(Stack<>);
                case Keyword.DepletableBag: return typeof(DepletableBag<>);
                case Keyword.DepletableList: return typeof(DepletableList<>);
                case Keyword.RingBuffer: return typeof(RingBuffer<>);
                case Keyword.Dictionary: return typeof(Dictionary<,>);
                case Keyword.HashSet: return typeof(HashSet<>);

                case Keyword.Random: return typeof(Random);

                default:
                    throw new ArgumentException($"Unexpected Keyword: {keyword}");
            }
        }

        public static bool IsTypeKeyword(this Keyword keyword)
        {
            switch (keyword)
            {
                case Keyword.Void:
                    return true;

                case Keyword.Bool:
                case Keyword.Double:
                case Keyword.Integer:
                case Keyword.String:
                    return true;

                case Keyword.List:
                case Keyword.Queue:
                case Keyword.Stack:
                case Keyword.DepletableBag:
                case Keyword.DepletableList:
                case Keyword.RingBuffer:
                case Keyword.Dictionary:
                case Keyword.HashSet:
                    return true;

                case Keyword.Random:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsGenericType(this Keyword keyword)
        {
            switch (keyword)
            {
                case Keyword.List:
                case Keyword.Queue:
                case Keyword.Stack:
                case Keyword.DepletableBag:
                case Keyword.DepletableList:
                case Keyword.RingBuffer:
                case Keyword.Dictionary:
                case Keyword.HashSet:
                    return true;

                default:
                    return false;
            }
        }

        public static object GetDefaultValue(this Type valueType)
        {
            if (valueType.IsValueType)
            {
                return Activator.CreateInstance(valueType);
            }

            return null;
        }
    }
}
