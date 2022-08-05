using System;
using System.Collections.Generic;
using System.Linq;

using BGC.DataStructures.Generic;

namespace BGC.Scripting
{
    public static class ScriptParser
    {
        /// <summary>
        /// Parses the script string and ensures the expected functions are present.
        /// </summary>
        /// <exception cref="ScriptParsingException"></exception>
        public static Script LexAndParseScript(
            string script,
            params FunctionSignature[] expectedFunctions)
        {
            Script scriptObject;
            using (GeneralScriptReader reader = new GeneralScriptReader(script))
            {
                IEnumerator<Token> tokens = reader
                    .GetTokens()
                    .ExpandInterpolatedStrings()
                    .DropComments()
                    .HandleArrays()
                    .HandleElseIf()
                    .HandleAmbiguousMinus()
                    .CheckParens()
                    .HandleGenericTypeArguments()
                    .HandleCasting()
                    .GetEnumerator();

                tokens.MoveNext();

                scriptObject = new Script(script, tokens, expectedFunctions);
            }

            return scriptObject;
        }

        private static IEnumerable<Token> ExpandInterpolatedStrings(this IEnumerable<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                if (token is InterpolatedString interpolatedString)
                {
                    foreach (Token rewrittenToken in interpolatedString.RewriteToken().ExpandInterpolatedStrings())
                    {
                        yield return rewrittenToken;
                    }
                }
                else
                {
                    yield return token;
                }
            }
        }

        //Check that all parens are matched
        private static IEnumerable<Token> CheckParens(this IEnumerable<Token> tokens)
        {
            //var temp = tokens.ToList();
            Stack<Separator> invocationStack = new Stack<Separator>();

            foreach (Token token in tokens)
            {
                if (token is SeparatorToken separator)
                {
                    switch (separator.separator)
                    {
                        case Separator.OpenCurlyBoi:
                            invocationStack.Push(Separator.CloseCurlyBoi);
                            break;

                        case Separator.OpenIndexer:
                            invocationStack.Push(Separator.CloseIndexer);
                            break;

                        case Separator.OpenParen:
                            invocationStack.Push(Separator.CloseParen);
                            break;

                        case Separator.CloseCurlyBoi:
                        case Separator.CloseIndexer:
                        case Separator.CloseParen:
                            //Check for unmatched
                            if (invocationStack.Count == 0)
                            {
                                throw new ScriptParsingException(
                                    source: separator,
                                    message: $"Unmatched CloseParen: {separator.separator}");
                            }

                            Separator sep = invocationStack.Pop();
                            //Check closing type
                            if (sep != separator.separator)
                            {
                                throw new ScriptParsingException(
                                    source: separator,
                                    message: $"Unexpected separator {separator.separator}.  Expected separator: {sep}");
                            }
                            break;

                        default:
                            break;
                    }
                }

                yield return token;
            }

            if (invocationStack.Count != 0)
            {
                throw new ScriptParsingException(
                    source: tokens.LastOrDefault() ?? new EOFToken(0, 0),
                    message: $"Mismatched Parentheses found!");
            }
        }

        private static IEnumerable<Token> HandleCasting(this IEnumerable<Token> tokens)
        {
            RingBuffer<Token> tokenQueue = new RingBuffer<Token>(3);

            foreach (Token token in tokens)
            {
                tokenQueue.Add(token);

                if (tokenQueue.Count == 3)
                {
                    if (tokenQueue[2] is SeparatorToken openParen && openParen.separator == Separator.OpenParen &&
                        tokenQueue[1] is TypeToken typeToken &&
                        tokenQueue[0] is SeparatorToken closeParen && closeParen.separator == Separator.CloseParen)
                    {
                        yield return new CastingOperationToken(typeToken, typeToken.BuildType());
                        tokenQueue.Clear();
                    }
                    else
                    {
                        yield return tokenQueue.PopBack();
                    }
                }
            }

            while (tokenQueue.Count > 0)
            {
                yield return tokenQueue.PopBack();
            }
        }

        /// <summary>
        /// Replaces AmbiguousMinus, AmbiguousLessThan, and AmbiguousGreaterThan
        /// </summary>
        private static IEnumerable<Token> HandleAmbiguousMinus(this IEnumerable<Token> tokens)
        {
            Token priorToken = null;

            foreach (Token token in tokens)
            {
                if (token is OperatorToken op && op.operatorType == Operator.AmbiguousMinus)
                {
                    if (priorToken is OperatorToken ||
                        (priorToken is SeparatorToken sep &&
                         (sep.separator == Separator.OpenParen || sep.separator == Separator.Comma)))
                    {
                        yield return new OperatorToken(token, Operator.Negate);
                    }
                    else
                    {
                        yield return new OperatorToken(token, Operator.Minus);
                    }
                }
                else
                {
                    yield return token;
                }

                priorToken = token;
            }
        }

        /// <summary>
        /// Replaces AmbiguousMinus, AmbiguousLessThan, and AmbiguousGreaterThan
        /// </summary>
        private static IEnumerable<Token> HandleArrays(this IEnumerable<Token> tokens)
        {
            TypeToken priorTypeToken = null;
            SeparatorToken openBracketToken = null;

            foreach (Token token in tokens)
            {
                if (openBracketToken is not null)
                {
                    //Accumulated "Type["
                    if (token is SeparatorToken closeBracketToken && closeBracketToken.separator == Separator.CloseIndexer)
                    {
                        //stashing "Type[]"
                        priorTypeToken = new TypeToken(
                            source: priorTypeToken!,
                            alias: $"{priorTypeToken!.alias}[]",
                            type: priorTypeToken.BuildType().MakeArrayType());

                        openBracketToken = null;
                        continue;
                    }
                    else
                    {
                        //Output "Type" and "["
                        yield return priorTypeToken!;
                        priorTypeToken = null;

                        yield return openBracketToken;
                        openBracketToken = null;

                        //Continue on in case token is a type
                    }
                }

                if (priorTypeToken is not null)
                {
                    //Accumulated "Type"
                    if (token is SeparatorToken sepToken && sepToken.separator == Separator.OpenIndexer)
                    {
                        //stashing "["
                        openBracketToken = sepToken;

                        continue;
                    }
                    else
                    {
                        //Output "Type" and "["
                        yield return priorTypeToken!;
                        priorTypeToken = null;

                        //Continue on in case token is a type
                    }
                }

                if (token is TypeToken typeToken)
                {
                    priorTypeToken = typeToken;
                    continue;
                }
                else
                {
                    yield return token;
                }
            }

            if (priorTypeToken is not null)
            {
                yield return priorTypeToken;
            }

            if (openBracketToken is not null)
            {
                yield return openBracketToken;
            }
        }

        private static IEnumerable<Token> HandleElseIf(this IEnumerable<Token> tokens)
        {
            Token stashedToken = null;

            foreach (Token token in tokens)
            {
                if (stashedToken is null)
                {
                    //First stage of identification - Find Else
                    if (token is KeywordToken kw && kw.keyword == Keyword.Else)
                    {
                        //Hold it and don't yield it
                        stashedToken = token;
                    }
                    else
                    {
                        yield return token;
                    }
                }
                else
                {
                    //Second stage of identification - Find If
                    if (token is KeywordToken kwn && kwn.keyword == Keyword.If)
                    {
                        yield return new KeywordToken(token, Keyword.ElseIf);
                    }
                    else
                    {
                        //Not Else If: Return each of these
                        yield return stashedToken;
                        yield return token;
                    }

                    //Clear stashed token
                    stashedToken = null;
                }
            }

            //return stashedToken
            if (stashedToken is not null)
            {
                yield return stashedToken;
            }
        }

        /// <summary>
        /// Collect generic type arguments into a single meta token
        /// </summary>
        private static IEnumerable<Token> HandleGenericTypeArguments(this IEnumerable<Token> tokenEnumerable)
        {
            IEnumerator<Token> tokens = tokenEnumerable.GetEnumerator();
            tokens.MoveNext();

            while (tokens.Current is not EOFToken)
            {
                if (tokens.Current is IdentifierToken || tokens.Current is TypeToken)
                {
                    Token genericArgumentTarget = tokens.Current;
                    tokens.MoveNext();

                    if (tokens.Current is OperatorToken operatorToken && operatorToken.operatorType == Operator.IsLessThan)
                    {
                        foreach (Token token in PotentiallyCollapseGenericArguments(genericArgumentTarget, tokens))
                        {
                            //Includes genericArgumentTarget
                            yield return token;
                        }
                    }
                    else
                    {
                        yield return genericArgumentTarget;
                    }
                }
                else
                {
                    yield return tokens.Current;
                    tokens.MoveNext();
                }

            }

            //send EOF Token
            yield return tokens.Current;
        }

        private static IEnumerable<Token> PotentiallyCollapseGenericArguments(Token genericArgumentTarget, IEnumerator<Token> tokens)
        {
            if (tokens.Current is not OperatorToken initialOperatorToken || initialOperatorToken.operatorType != Operator.IsLessThan)
            {
                throw new Exception($"CollapseGenericArguments expectes to be passed an enumerator pointing at a '<' character");
            }

            Queue<Token> tokenQueue = new Queue<Token>();
            tokenQueue.Enqueue(tokens.Current);

            List<Type> typeList = new List<Type>();

            bool expectClass = true;

            tokens.MoveNext();

            while (tokens.Current is not EOFToken)
            {
                if (expectClass)
                {
                    expectClass = false;

                    //Class (Potentially Generic class)
                    if (tokens.Current is not TypeToken typeToken)
                    {
                        //Failed expectation. Treat this like an operator expression and output all the raw tokens instead.
                        break;
                    }

                    //Double check for generic arguments
                    tokens.MoveNext();

                    if (tokens.Current is OperatorToken operatorToken && operatorToken.operatorType == Operator.IsLessThan)
                    {
                        //An < operator inside a generic argument that isn't a generic argument renders the external set invalid as well
                        Token[] collectedTokens = PotentiallyCollapseGenericArguments(typeToken, tokens).ToArray();

                        //Accumulate all tokens to the queue
                        foreach (Token token in collectedTokens)
                        {
                            tokenQueue.Enqueue(token);
                        }

                        if (collectedTokens.Length == 1 && collectedTokens[0] is TypeToken collectedTypeToken)
                        {
                            //Valid, let's keep going
                            typeList.Add(collectedTypeToken.BuildType());
                        }
                        else
                        {
                            //Invalid. We give up on parsing it and we can give up trying to parse this as a generic argument list
                            break;
                        }
                    }
                    else
                    {
                        //Just enqueue the type we read
                        tokenQueue.Enqueue(typeToken);
                        typeList.Add(typeToken.BuildType());
                    }
                }
                else
                {
                    expectClass = true;

                    //Comma, or Close
                    if (tokens.Current is OperatorToken operatorToken && operatorToken.operatorType == Operator.IsGreaterThan)
                    {
                        //Done
                        //Skip > operator
                        tokens.MoveNext();
                        //Create and Output collapsed token
                        yield return ApplyGenericArguments(genericArgumentTarget, typeList);

                        //Exit
                        yield break;
                    }

                    if (tokens.Current is SeparatorToken separatorToken && separatorToken.separator == Separator.Comma)
                    {
                        //Accumulating more classes
                        tokenQueue.Enqueue(tokens.Current);
                        tokens.MoveNext();

                        //Advance
                        continue;
                    }

                    //Failed expectation. Treat this like an operator expression and output all the raw tokens instead.
                    break;
                }
            }

            //We failed to match a Generic Argument List. Returning all tokens.
            yield return genericArgumentTarget;
            while (tokenQueue.Count > 0)
            {
                yield return tokenQueue.Dequeue();
            }
        }

        private static Token ApplyGenericArguments(Token genericArgumentTarget, List<Type> types)
        {
            if (genericArgumentTarget is IdentifierToken identifierToken)
            {
                //Apply to Method
                identifierToken.ApplyGenericArguments(types);
                return identifierToken;
            }
            else if (genericArgumentTarget is TypeToken typeToken)
            {
                //Apply to Class
                typeToken.ApplyGenericArguments(types);
                return typeToken;
            }

            throw new ScriptParsingException(genericArgumentTarget, $"Cannot apply Generic Type Arguments to token of type {genericArgumentTarget.GetType().Name}");
        }

        private static IEnumerable<Token> DropComments(this IEnumerable<Token> tokens) =>
            tokens.Where(x => x is not CommentToken);
    }
}