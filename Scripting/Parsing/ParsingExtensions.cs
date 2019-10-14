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

        public static void AssertAndSkip(this IEnumerator<Token> tokens, Separator separator, bool checkEOF = true)
        {
            if (tokens.Current is SeparatorToken sep && sep.separator == separator)
            {
                tokens.CautiousAdvance(checkEOF);
                return;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {separator} but found: {tokens.Current}");
        }

        public static void AssertAndSkip(this IEnumerator<Token> tokens, Keyword keyword, bool checkEOF = true)
        {
            if (tokens.Current is KeywordToken keyw && keyw.keyword == keyword)
            {
                tokens.CautiousAdvance(checkEOF);
                return;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {keyword} but found: {tokens.Current}");
        }

        public static void AssertAndSkip(this IEnumerator<Token> tokens, Operator operatorType, bool checkEOF = true)
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

        public static T GetTokenWithoutSkipping<T>(this IEnumerator<Token> tokens, bool checkEOF = true)
        {
            if (tokens.Current is T token)
            {
                return token;
            }

            throw new ScriptParsingException(tokens.Current, $"Expected {typeof(T).Name} but found: {tokens.Current}");
        }

        public static bool TestWithoutSkipping(this IEnumerator<Token> tokens, Separator separator)
        {
            if (tokens.Current is SeparatorToken sep && sep.separator == separator)
            {
                return true;
            }

            return false;
        }

        public static bool TestWithoutSkipping(this IEnumerator<Token> tokens, Keyword keyword)
        {
            if (tokens.Current is KeywordToken keyTok && keyTok.keyword == keyword)
            {
                return true;
            }

            return false;
        }

        public static bool TestWithoutSkipping(this IEnumerator<Token> tokens, Operator operatorType)
        {
            if (tokens.Current is OperatorToken opTok && opTok.operatorType == operatorType)
            {
                return true;
            }

            return false;
        }

        public static bool TestAndConditionallySkip(this IEnumerator<Token> tokens, Separator separator, bool checkEOF = true)
        {
            if (tokens.Current is SeparatorToken sep && sep.separator == separator)
            {
                tokens.CautiousAdvance(checkEOF);
                return true;
            }

            return false;
        }

        public static bool TestAndConditionallySkip(this IEnumerator<Token> tokens, Operator operatorType, bool checkEOF = true)
        {
            if (tokens.Current is OperatorToken opTok && opTok.operatorType == operatorType)
            {
                tokens.CautiousAdvance(checkEOF);
                return true;
            }

            return false;
        }

        public static bool TestAndConditionallySkip(this IEnumerator<Token> tokens, Keyword keyword, bool checkEOF = true)
        {
            if (tokens.Current is KeywordToken keyTok && keyTok.keyword == keyword)
            {
                tokens.CautiousAdvance(checkEOF);
                return true;
            }

            return false;
        }

        public static Type ReadType(this IEnumerator<Token> tokens)
        {
            KeywordToken typeToken = tokens.GetTokenAndAdvance<KeywordToken>();

            if (!typeToken.keyword.IsTypeKeyword())
            {
                throw new ScriptParsingException(
                    source: typeToken,
                    message: $"Expected a Type keyword, but instead found: {typeToken.keyword}");
            }

            if (typeToken.keyword.IsGenericType())
            {
                return typeToken.keyword.GetValueType().MakeGenericType(tokens.ReadTypeArguments());
            }

            return typeToken.keyword.GetValueType();
        }


        public static Type[] ReadTypeArguments(this IEnumerator<Token> tokens)
        {
            tokens.AssertAndSkip(Operator.IsLessThan);

            if (tokens.Current is OperatorToken closeToken &&
                closeToken.operatorType == Operator.IsLessThan)
            {
                throw new ScriptParsingException(
                    source: tokens.Current,
                    message: "Generic types must be specified");
            }

            List<Type> types = new List<Type>();
            do
            {
                types.Add(tokens.ReadType());
            }
            while (tokens.TestAndConditionallySkip(Separator.Comma));

            tokens.AssertAndSkip(Operator.IsGreaterThan);

            return types.ToArray();
        }


        public static bool AssignableFromType(this Type valueType, Type otherValueType)
        {
            if (valueType == otherValueType)
            {
                return true;
            }

            if (valueType == typeof(double) && otherValueType == typeof(int))
            {
                return true;
            }

            if (valueType.IsAssignableFrom(otherValueType))
            {
                return true;
            }

            if (otherValueType == typeof(NullLiteralToken) && !valueType.IsValueType)
            {
                return true;
            }

            return false;
        }

        public static Type GetIndexingType(this Type valueType)
        {
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

        public static Type GetInitializerItemType(this Type valueType)
        {
            if (!valueType.IsInitializerSupportedCollection())
            {
                return null;
            }

            return valueType.GetGenericArguments()[0];
        }
    }
}
