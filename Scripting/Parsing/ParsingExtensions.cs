using System;
using System.Collections.Generic;
using BGC.DataStructures.Generic;

namespace BGC.Scripting
{
    public static class ParsingExtensions
    {
        public static void CautiousAdvance(this IEnumerator<Token> tokens, bool checkEOF = true)
        {
            Token temp = tokens.Current;
            if (!tokens.MoveNext())
            {
                throw new ScriptParsingException(temp, $"Script ended unexpectedly: {temp}");
            }

            if (checkEOF && tokens.Current is EOFToken)
            {
                throw new ScriptParsingException(tokens.Current, $"Script ended unexpectedly: {tokens.Current}");
            }
        }

        public static void AssertAndAdvance(this IEnumerator<Token> tokens, Separator separator, bool checkEOF = true)
        {
            if (tokens.Current is SeparatorToken sep && sep.separator == separator)
            {
                tokens.CautiousAdvance(checkEOF);
                return;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {separator} but found: {tokens.Current}");
        }

        public static void AssertAndAdvance(this IEnumerator<Token> tokens, Keyword keyword, bool checkEOF = true)
        {
            if (tokens.Current is KeywordToken keyw && keyw.keyword == keyword)
            {
                tokens.CautiousAdvance(checkEOF);
                return;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {keyword} but found: {tokens.Current}");
        }

        public static void AssertAndAdvance(this IEnumerator<Token> tokens, Operator operatorType, bool checkEOF = true)
        {
            if (tokens.Current is OperatorToken opTok && opTok.operatorType == operatorType)
            {
                tokens.CautiousAdvance(checkEOF);
                return;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {operatorType} but found: {tokens.Current}");
        }

        public static T GetTokenAndAdvance<T>(this IEnumerator<Token> tokens, bool checkEOF = true)
        {
            if (tokens.Current is T token)
            {
                tokens.CautiousAdvance(checkEOF);
                return token;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {typeof(T).Name} but found: {tokens.Current}");
        }

        public static IdentifierToken GetIdentifierAndAdvance(this IEnumerator<Token> tokens, bool checkEOF = true)
        {
            if (tokens.Current is IdentifierToken idToken)
            {
                tokens.CautiousAdvance(checkEOF);
                return idToken;
            }

            if (tokens.Current is TypeToken typeToken)
            {
                tokens.CautiousAdvance(checkEOF);

                IdentifierToken newIdentifierToken = new IdentifierToken(typeToken, typeToken.alias);
                if (typeToken.genericArguments is not null)
                {
                    newIdentifierToken.ApplyGenericArguments(typeToken.genericArguments);
                }

                return newIdentifierToken;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected IdentifierToken or TypeToken but found: {tokens.Current}");
        }

        public static T GetTokenWithoutAdvancing<T>(this IEnumerator<Token> tokens)
        {
            if (tokens.Current is T token)
            {
                return token;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {typeof(T).Name} but found: {tokens.Current}");
        }

        public static bool TestWithoutAdvancing(this IEnumerator<Token> tokens, Separator separator)
        {
            if (tokens.Current is SeparatorToken sep && sep.separator == separator)
            {
                return true;
            }

            return false;
        }

        public static bool TestWithoutAdvancing(this IEnumerator<Token> tokens, Keyword keyword)
        {
            if (tokens.Current is KeywordToken keyTok && keyTok.keyword == keyword)
            {
                return true;
            }

            return false;
        }

        public static bool TestWithoutAdvancing(this IEnumerator<Token> tokens, Operator operatorType)
        {
            if (tokens.Current is OperatorToken opTok && opTok.operatorType == operatorType)
            {
                return true;
            }

            return false;
        }

        public static bool TestAndConditionallyAdvance(this IEnumerator<Token> tokens, Separator separator, bool checkEOF = true)
        {
            if (tokens.Current is SeparatorToken sep && sep.separator == separator)
            {
                tokens.CautiousAdvance(checkEOF);
                return true;
            }

            return false;
        }

        public static bool TestAndConditionallyAdvance(this IEnumerator<Token> tokens, Operator operatorType, bool checkEOF = true)
        {
            if (tokens.Current is OperatorToken opTok && opTok.operatorType == operatorType)
            {
                tokens.CautiousAdvance(checkEOF);
                return true;
            }

            return false;
        }

        public static bool TestAndConditionallyAdvance(this IEnumerator<Token> tokens, Keyword keyword, bool checkEOF = true)
        {
            if (tokens.Current is KeywordToken keyTok && keyTok.keyword == keyword)
            {
                tokens.CautiousAdvance(checkEOF);
                return true;
            }

            return false;
        }

        public static ArgumentType GetArgumentType(this IEnumerator<Token> tokens, bool checkEOF = true)
        {
            if (tokens.Current is KeywordToken keyTok)
            {
                switch (keyTok.keyword)
                {
                    case Keyword.In:
                        tokens.CautiousAdvance(checkEOF);
                        return ArgumentType.In;

                    case Keyword.Out:
                        tokens.CautiousAdvance(checkEOF);
                        return ArgumentType.Out;

                    case Keyword.Ref:
                        tokens.CautiousAdvance(checkEOF);
                        return ArgumentType.Ref;

                    case Keyword.Params:
                        tokens.CautiousAdvance(checkEOF);
                        return ArgumentType.Params;

                    default:
                        //Could be valid (like new)
                        break;
                }
            }

            return ArgumentType.Standard;
        }

        public static Type ReadTypeAndAdvance(this IEnumerator<Token> tokens, bool checkEOF = true)
        {
            Type type = null;

            if (tokens.Current is TypeToken typeToken)
            {
                type = typeToken.BuildType();
                tokens.CautiousAdvance(checkEOF);
            }

            if (type is null)
            {
                throw new ScriptParsingException(
                    source: tokens.Current,
                    message: $"Expected a Type keyword, but instead found: {tokens.Current}");
            }

            return type;
        }


        public static bool AssignableOrConvertableFromType(this Type valueType, Type otherValueType)
        {
            if (valueType == otherValueType)
            {
                return true;
            }

            if (valueType.IsAssignableFrom(otherValueType))
            {
                return true;
            }

            if (valueType.IsExtendedPrimitive() && otherValueType.IsExtendedPrimitive())
            {
                return ConvertableFromTypePrimitive(valueType, otherValueType);
            }

            if (otherValueType == typeof(NullLiteralToken) && !valueType.IsValueType)
            {
                return true;
            }

            return false;
        }

        private class PrimitiveFeatures
        {
            public readonly int Rank;
            public readonly bool Signed;

            public PrimitiveFeatures(int rank, bool signed)
            {
                Rank = rank;
                Signed = signed;
            }
        }

        private static readonly Dictionary<Type, PrimitiveFeatures> primitiveLookup = new Dictionary<Type, PrimitiveFeatures>();
        private static readonly Dictionary<(Type, Type), bool> primitivePairingOverrideLookup = new Dictionary<(Type, Type), bool>();

        private static bool ConvertableFromTypePrimitive(
            Type type1,
            Type type2)
        {
            if (type1 == type2)
            {
                return true;
            }

            //Check if looks need to be constructed 
            if (primitiveLookup.Count == 0)
            {
                //Every signed value type can hold things below its rank, but not equal to (since those are Different types of the same rank)
                //Unsigned values can only hold unsigned values below its rank
                primitiveLookup.Add(typeof(byte), new PrimitiveFeatures(1, false));
                primitiveLookup.Add(typeof(sbyte), new PrimitiveFeatures(1, true));

                primitiveLookup.Add(typeof(short), new PrimitiveFeatures(2, true));
                primitiveLookup.Add(typeof(ushort), new PrimitiveFeatures(2, false));
                primitiveLookup.Add(typeof(char), new PrimitiveFeatures(2, false));

                primitiveLookup.Add(typeof(int), new PrimitiveFeatures(4, true));
                primitiveLookup.Add(typeof(uint), new PrimitiveFeatures(4, false));

                primitiveLookup.Add(typeof(long), new PrimitiveFeatures(8, true));
                primitiveLookup.Add(typeof(ulong), new PrimitiveFeatures(8, false));

                //Setting decimal at 9 like Float, since you can't assign either to one another.
                primitiveLookup.Add(typeof(decimal), new PrimitiveFeatures(9, true));
                primitiveLookup.Add(typeof(float), new PrimitiveFeatures(9, true));
                primitiveLookup.Add(typeof(double), new PrimitiveFeatures(10, true));


                //Overrides of Type-Type pairings
                //The override takes care of blocking double = decimal
                primitivePairingOverrideLookup.Add((typeof(double), typeof(decimal)), false);

                //ushorts can hold Chars, but shorts cannot
                primitivePairingOverrideLookup.Add((typeof(ushort), typeof(char)), true);
            }

            if (type1 == typeof(string) || type2 == typeof(string))
            {
                //strings won't be convertable 
                return false;
            }

            if (type1 == typeof(bool) || type2 == typeof(bool))
            {
                //bools won't be convertable 
                return false;
            }

            if (type1 == typeof(char))
            {
                //No type can be assigned Into char directly
                return false;
            }

            if (primitivePairingOverrideLookup.TryGetValue((type1, type2), out bool overrideValue))
            {
                return overrideValue;
            }

            if (!primitiveLookup.TryGetValue(type1, out PrimitiveFeatures type1Features))
            {
                throw new Exception($"Primitive type not defined in Rank Lookup: {type1.Name}");
            }

            if (!primitiveLookup.TryGetValue(type2, out PrimitiveFeatures type2Features))
            {
                throw new Exception($"Primitive type not defined in Rank Lookup: {type2.Name}");
            }

            if (!type1Features.Signed && type2Features.Signed)
            {
                //Can't assign a signed value to an unsigned one
                return false;
            }

            //Remaining datatypes can hold values of lower rank
            return type1Features.Rank > type2Features.Rank;
        }

        public static Type GetIndexingType(this Type valueType)
        {
            if (valueType.IsArray || valueType == typeof(string))
            {
                return typeof(int);
            }

            if (!valueType.IsGenericType)
            {
                return null;
            }

            Type genericTypeDefinition = valueType.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(List<>) ||
                genericTypeDefinition == typeof(RingBuffer<>))
            {
                return typeof(int);
            }
            else if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                return valueType.GetGenericArguments()[0];
            }


            return null;
        }

        public static Type GetIndexingReturnType(this Type valueType)
        {
            if (valueType.IsArray)
            {
                return valueType.GetElementType();
            }

            if (valueType == typeof(string))
            {
                return typeof(char);
            }

            if (!valueType.IsGenericType)
            {
                return null;
            }

            Type genericTypeDefinition = valueType.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(List<>) ||
                genericTypeDefinition == typeof(RingBuffer<>))
            {
                return valueType.GetGenericArguments()[0];
            }
            else if (genericTypeDefinition == typeof(Dictionary<,>))
            {
                return valueType.GetGenericArguments()[1];
            }

            return null;
        }

        public static bool IsInitializerSupportedCollection(this Type valueType)
        {
            if (valueType.IsArray)
            {
                return true;
            }

            if (!valueType.IsGenericType)
            {
                return false;
            }

            Type genericTypeDefinition = valueType.GetGenericTypeDefinition();

            if (valueType.IsGenericType &&
                (genericTypeDefinition == typeof(List<>) ||
                 genericTypeDefinition == typeof(Queue<>) ||
                 genericTypeDefinition == typeof(Stack<>) ||
                 genericTypeDefinition == typeof(DepletableBag<>) ||
                 genericTypeDefinition == typeof(DepletableList<>) ||
                 genericTypeDefinition == typeof(HashSet<>) ||
                 genericTypeDefinition == typeof(RingBuffer<>)))
            {
                return true;
            }

            return false;
        }

        public static bool IsExtendedPrimitive(this Type valueType) =>
            valueType.IsPrimitive || valueType == typeof(decimal) || valueType == typeof(string);

        public static bool IsSmallIntegralType(this Type valueType) =>
            valueType == typeof(byte) || valueType == typeof(sbyte) || valueType == typeof(short) || valueType == typeof(ushort);

        public static bool IsIntegralType(this Type valueType) =>
            valueType == typeof(byte) || valueType == typeof(sbyte) || valueType == typeof(short) || valueType == typeof(ushort) ||
            valueType == typeof(int) || valueType == typeof(uint) || valueType == typeof(long) || valueType == typeof(ulong);

        public static bool IsLiteralInRange(this LiteralToken literalToken, Type valueType)
        {
            int value = literalToken.GetAs<int>();

            if (valueType == typeof(byte))
            {
                return value >= byte.MinValue && value <= byte.MaxValue;
            }

            if (valueType == typeof(sbyte))
            {
                return value >= sbyte.MinValue && value <= sbyte.MaxValue;
            }

            if (valueType == typeof(short))
            {
                return value >= short.MinValue && value <= short.MaxValue;
            }

            if (valueType == typeof(ushort))
            {
                return value >= ushort.MinValue && value <= ushort.MaxValue;
            }

            return false;
        }

        public static Type GetInitializerItemType(this Type valueType)
        {
            if (valueType.IsArray)
            {
                return valueType.GetElementType();
            }


            if (!valueType.IsInitializerSupportedCollection())
            {
                return null;
            }

            return valueType.GetGenericArguments()[0];
        }

        public static Type GetUnaryPromotedType(
            this OperatorToken source,
            Type type)
        {
            if (type == typeof(uint))
            {
                return typeof(long);
            }

            if (type == typeof(int) || type == typeof(long))
            {
                return type;
            }

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                if (source.operatorType == Operator.BitwiseComplement)
                {
                    throw new ScriptParsingException(source, $"Cannot perform unary operation {source.operatorType} on a value of type {type}");
                }

                return type;
            }

            if (type == typeof(sbyte) || type == typeof(byte) || type == typeof(short) || type == typeof(ushort) || type == typeof(char))
            {
                return typeof(int);
            }

            //ulong
            throw new ScriptParsingException(source, $"Cannot perform unary operation {source.operatorType} on a value of type {type}");
        }

        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions#1147-numeric-promotions
        /// </summary>
        public static Type GetBinaryPromotedType(
            this OperatorToken source,
            Type type1,
            Type type2)
        {
            //decimal
            if (type1 == typeof(decimal))
            {
                if (type2 == typeof(float) || type2 == typeof(double))
                {
                    throw new ScriptParsingException(source, $"Cannot perform a binary operation on values of types {type1} and {type2}");
                }

                return typeof(decimal);
            }

            if (type2 == typeof(decimal))
            {
                if (type1 == typeof(float) || type1 == typeof(double))
                {
                    throw new ScriptParsingException(source, $"Cannot perform a binary operation on values of types {type1} and {type2}");
                }

                return typeof(decimal);
            }

            //double
            if (type1 == typeof(double) || type2 == typeof(double))
            {
                return typeof(double);
            }

            //float
            if (type1 == typeof(float) || type2 == typeof(float))
            {
                return typeof(float);
            }

            //ulong
            if (type1 == typeof(ulong))
            {
                if (type2 == typeof(sbyte) || type2 == typeof(short) || type2 == typeof(int) || type2 == typeof(long))
                {
                    throw new ScriptParsingException(source, $"Cannot perform a binary operation on values of types {type1} and {type2}");
                }

                return typeof(ulong);
            }

            if (type2 == typeof(ulong))
            {
                if (type1 == typeof(sbyte) || type1 == typeof(short) || type1 == typeof(int) || type1 == typeof(long))
                {
                    throw new ScriptParsingException(source, $"Cannot perform a binary operation on values of types {type1} and {type2}");
                }

                return typeof(ulong);
            }

            //long
            if (type1 == typeof(long) || type2 == typeof(long))
            {
                return typeof(long);
            }

            //uint
            if (type1 == typeof(uint))
            {
                if (type2 == typeof(sbyte) || type2 == typeof(short) || type2 == typeof(int))
                {
                    return typeof(long);
                }

                return typeof(uint);
            }

            if (type2 == typeof(uint))
            {
                if (type1 == typeof(sbyte) || type1 == typeof(short) || type1 == typeof(int))
                {
                    return typeof(long);
                }

                return typeof(uint);
            }

            return typeof(int);
        }

        public static bool IsAssignableTo(this Type type, Type targetType)
        {
            if (targetType is null)
            {
                return false;
            }

            return targetType.IsAssignableFrom(type);
        }
    }
}