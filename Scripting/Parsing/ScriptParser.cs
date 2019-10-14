using System;
using System.Collections.Generic;
using System.Linq;

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
            using (ScriptReader reader = new ScriptReader(script))
            {
                IEnumerator<Token> tokens = reader
                    .GetTokens()
                    .DropComments()
                    .HandleElseIf()
                    .HandleAmbiguousMinus()
                    .HandleCasting()
                    .CheckParens()
                    .GetEnumerator();

                tokens.MoveNext();

                scriptObject = new Script(tokens, expectedFunctions);
            }

            return scriptObject;
        }

        //Check that all parens are matched
        private static IEnumerable<Token> CheckParens(this IEnumerable<Token> tokens)
        {
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
                    source: tokens.LastOrDefault() ?? new EOFToken(0,0),
                    message: $"Mismatched Parentheses found!");
            }
        }

        private static IEnumerable<Token> HandleCasting(this IEnumerable<Token> tokens)
        {
            //Handle conversion of ( double ) to cast operation
            Operator operatorType = Operator.CastDouble;
            Queue<Token> tokenQueue = new Queue<Token>(3);

            foreach (Token token in tokens)
            {
                switch (tokenQueue.Count)
                {
                    case 0:
                        if (token is SeparatorToken sep && sep.separator == Separator.OpenParen)
                        {
                            tokenQueue.Enqueue(token);
                        }
                        else
                        {
                            yield return token;
                        }
                        break;

                    case 1:
                        if (token is KeywordToken kw && (kw.keyword == Keyword.Integer || kw.keyword == Keyword.Double))
                        {
                            tokenQueue.Enqueue(token);

                            if (kw.keyword == Keyword.Integer)
                            {
                                operatorType = Operator.CastInteger;
                            }
                            else
                            {
                                operatorType = Operator.CastDouble;
                            }
                        }
                        else
                        {
                            yield return tokenQueue.Dequeue();
                            yield return token;
                        }
                        break;

                    case 2:
                        if (token is SeparatorToken sep2 && sep2.separator == Separator.CloseParen)
                        {
                            tokenQueue.Clear();
                            yield return new OperatorToken(token, operatorType);
                        }
                        else
                        {
                            yield return tokenQueue.Dequeue();
                            yield return tokenQueue.Dequeue();
                            yield return token;
                        }
                        break;

                    default:
                        throw new Exception($"Serious parsing error.  Too many queued tokens.");
                }
            }

            while (tokenQueue.Count > 0)
            {
                yield return tokenQueue.Dequeue();
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
            if (stashedToken != null)
            {
                yield return stashedToken;
            }
        }

        private static IEnumerable<Token> DropComments(this IEnumerable<Token> tokens) =>
            tokens.Where(x => !(x is CommentToken));
    }
}
