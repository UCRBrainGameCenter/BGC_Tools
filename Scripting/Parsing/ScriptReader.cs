using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BGC.Scripting
{
    /// <summary>
    /// Inspiration taken from LightJson.Serialization
    /// </summary>
    public class ScriptReader : IDisposable
    {
        private readonly TextReader textReader;

        private int line = 0;
        private int column = 0;

        /// <summary>Indicates whether there are still characters to be read.</summary>
        public bool CanRead => textReader.Peek() != -1;

        public ScriptReader(string script)
        {
            textReader = new StringReader(script);
        }

        public ScriptReader(TextReader script)
        {
            textReader = script;
        }

        public IEnumerable<Token> GetTokens()
        {
            //Skip opening whitespace
            SkipWhitespace();

            while (CanRead)
            {
                yield return ReadNextToken();

                //Skip to next meaningful character
                SkipWhitespace();
            }

            yield return new EOFToken(line, column);
        }

        private Token ReadNextToken()
        {
            char next = Peek();

            switch (next)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ParseNumber();

                case '\'':
                case '"':
                    return ParseStringLiteral();

                case '-':
                case '=':
                case '+':
                case '*':
                case '/':
                case '^':
                case '!':
                case '<':
                case '>':
                case '&':
                case '|':
                case '%':
                case '?':
                case '.':
                    return ParseOperatorOrComment();

                case '(':
                case ')':
                case '{':
                case '}':
                case ';':
                case ':':
                case ',':
                case '[':
                case ']':
                    return ParseSeparator();

                default:
                    return ParseWord();
            }
        }

        private Token ParseNumber()
        {
            StringBuilder numberBuilder = new StringBuilder();

            int startLine = line;
            int startColumn = column;

            while (CanRead && IsNumberCharacter(Peek()))
            {
                numberBuilder.Append(Read());
            }

            string numberString = numberBuilder.ToString();

            if (int.TryParse(numberString, out int integerResult))
            {
                return new LiteralToken<int>(startLine, startColumn, integerResult);
            }
            else if (double.TryParse(numberString, out double doubleResult))
            {
                return new LiteralToken<double>(startLine, startColumn, doubleResult);
            }
            else
            {
                throw new ScriptParsingException(startLine, startColumn, $"Unable to parse Number: {numberString}");
            }
        }

        private Token ParseBlockComment()
        {
            StringBuilder commentBuilder = new StringBuilder();

            int startLine = line;
            int startColumn = column - 2;

            while (CanRead)
            {
                char next = Read();

                if (next == '*' && Peek() == '/')
                {
                    Read();
                    break;
                }

                commentBuilder.Append(next);
            }

            return new BlockCommentToken(startLine, startColumn, commentBuilder.ToString());
        }

        private Token ParseInlineComment()
        {
            StringBuilder commentBuilder = new StringBuilder();

            int startLine = line;
            int startColumn = column - 2;

            while (CanRead)
            {
                if (Peek() == '\n' || Peek() == '\r')
                {
                    break;
                }

                commentBuilder.Append(Read());
            }

            return new InlineCommentToken(startLine, startColumn, commentBuilder.ToString());
        }

        private Token ParseStringLiteral()
        {
            //Kill open Quote
            char openQuote = Read();

            StringBuilder stringBuilder = new StringBuilder();

            int startLine = line;
            int startColumn = column - 1;

            char temp;
            while (CanRead && (temp = Read()) != openQuote)
            {
                if (CanRead && temp == '\\')
                {
                    //Escaped Character
                    switch (temp = Read())
                    {
                        case '\\':
                        case '\'':
                        case '\"':
                            stringBuilder.Append(temp);
                            break;

                        case 'n':
                            stringBuilder.Append('\n');
                            break;

                        case 'r':
                            stringBuilder.Append('\r');
                            break;

                        default:
                            throw new ScriptParsingException(line, column, $"Unexpected escaped character {temp}");
                    }
                }
                else if (temp == '\n' || temp == '\r')
                {
                    throw new ScriptParsingException(line, column, $"Line ended before string terminated: {stringBuilder.ToString()}");
                }
                else
                {
                    stringBuilder.Append(temp);
                }
            }

            return new LiteralToken<string>(startLine, startColumn, stringBuilder.ToString());
        }

        private Token ParseOperatorOrComment()
        {
            char temp;
            switch (temp = Read())
            {
                case '/':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.DivideEquals);
                    }
                    else if (ConsumeIfNext('*'))
                    {
                        //It's a comment
                        return ParseBlockComment();
                    }
                    else if (ConsumeIfNext('/'))
                    {
                        return ParseInlineComment();
                    }
                    return new OperatorToken(line, column - 1, Operator.Divide);

                case '=':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.IsEqualTo);
                    }
                    else if (ConsumeIfNext('>'))
                    {
                        return new SeparatorToken(line, column - 2, Separator.Arrow);
                    }
                    return new OperatorToken(line, column - 1, Operator.Assignment);

                case '<':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.IsLessThanOrEqualTo);
                    }
                    return new OperatorToken(line, column - 1, Operator.IsLessThan);

                case '>':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.IsGreaterThanOrEqualTo);
                    }
                    return new OperatorToken(line, column - 1, Operator.IsGreaterThan);

                case '!':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.IsNotEqualTo);
                    }
                    return new OperatorToken(line, column - 1, Operator.Not);

                case '-':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.MinusEquals);
                    }
                    else if (ConsumeIfNext('-'))
                    {
                        return new OperatorToken(line, column - 2, Operator.Decrement);
                    }
                    //Unspecified minus token - either Negation or Subtraction
                    return new OperatorToken(line, column - 1, Operator.AmbiguousMinus);

                case '+':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.PlusEquals);
                    }
                    else if (ConsumeIfNext('+'))
                    {
                        return new OperatorToken(line, column - 2, Operator.Increment);
                    }
                    return new OperatorToken(line, column - 1, Operator.Plus);

                case '*':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.TimesEquals);
                    }
                    return new OperatorToken(line, column - 1, Operator.Times);

                case '%':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.ModuloEquals);
                    }
                    return new OperatorToken(line, column - 1, Operator.Modulo);

                case '^':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.PowerEquals);
                    }
                    return new OperatorToken(line, column - 1, Operator.Power);

                case '&':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.AndEquals);
                    }
                    else if (ConsumeIfNext('&'))
                    {
                        return new OperatorToken(line, column - 2, Operator.And);
                    }
                    throw new ScriptParsingException(line, column - 1, "And operator is \"&&\"");

                case '|':
                    if (ConsumeIfNext('='))
                    {
                        return new OperatorToken(line, column - 2, Operator.OrEquals);
                    }
                    else if (ConsumeIfNext('|'))
                    {
                        return new OperatorToken(line, column - 2, Operator.Or);
                    }
                    throw new ScriptParsingException(line, column - 1, "Or operator is \"||\"");

                case '?':
                    return new OperatorToken(line, column - 1, Operator.Ternary);

                case '.':
                    return new OperatorToken(line, column - 1, Operator.MemberAccess);

                default:
                    throw new ScriptParsingException(line, column - 1, $"Unexpected char for Operator: {temp}");
            }
        }

        private Token ParseSeparator()
        {
            char temp;
            switch (temp = Read())
            {
                case '(': return new SeparatorToken(line, column - 1, Separator.OpenParen);
                case ')': return new SeparatorToken(line, column - 1, Separator.CloseParen);
                case '{': return new SeparatorToken(line, column - 1, Separator.OpenCurlyBoi);
                case '}': return new SeparatorToken(line, column - 1, Separator.CloseCurlyBoi);
                case ':': return new SeparatorToken(line, column - 1, Separator.Colon);
                case ';': return new SeparatorToken(line, column - 1, Separator.Semicolon);
                case ',': return new SeparatorToken(line, column - 1, Separator.Comma);
                case '[': return new SeparatorToken(line, column - 1, Separator.OpenIndexer);
                case ']': return new SeparatorToken(line, column - 1, Separator.CloseIndexer);

                default:
                    throw new ScriptParsingException(line, column - 1, $"Unexpected char for Separator: {temp}");
            }
        }

        private Token ParseWord()
        {
            //First, figure out if it's a Keyword
            StringBuilder tokenBuilder = new StringBuilder();

            int startingColumn = column;

            while (CanRead && IsWordCharacter(Peek()))
            {
                tokenBuilder.Append(Read());
            }

            string token = tokenBuilder.ToString();

            switch (token)
            {
                //Literals
                case "true": return new LiteralToken<bool>(line, startingColumn, true);
                case "false": return new LiteralToken<bool>(line, startingColumn, false);
                case "NaN": return new LiteralToken<double>(line, startingColumn, double.NaN);
                case "null": return new NullLiteralToken(line, startingColumn);

                //Conditionals
                case "if": return new KeywordToken(line, startingColumn, Keyword.If);
                case "else": return new KeywordToken(line, startingColumn, Keyword.Else);

                //Loops
                case "while": return new KeywordToken(line, startingColumn, Keyword.While);
                case "for": return new KeywordToken(line, startingColumn, Keyword.For);
                case "foreach": return new KeywordToken(line, startingColumn, Keyword.ForEach);
                case "in": return new KeywordToken(line, startingColumn, Keyword.In);

                //Flow Control
                case "continue": return new KeywordToken(line, startingColumn, Keyword.Continue);
                case "break": return new KeywordToken(line, startingColumn, Keyword.Break);
                case "return": return new KeywordToken(line, startingColumn, Keyword.Return);

                //Declaration Modifiers
                case "extern": return new KeywordToken(line, startingColumn, Keyword.Extern);
                case "global": return new KeywordToken(line, startingColumn, Keyword.Global);
                case "const": return new KeywordToken(line, startingColumn, Keyword.Const);

                case "void": return new KeywordToken(line, startingColumn, Keyword.Void);

                //Value Types
                case "bool": return new KeywordToken(line, startingColumn, Keyword.Bool);
                case "double": return new KeywordToken(line, startingColumn, Keyword.Double);
                case "int": return new KeywordToken(line, startingColumn, Keyword.Integer);
                case "string": return new KeywordToken(line, startingColumn, Keyword.String);

                //Container Types
                case "List": return new KeywordToken(line, startingColumn, Keyword.List);
                case "Queue": return new KeywordToken(line, startingColumn, Keyword.Queue);
                case "Stack": return new KeywordToken(line, startingColumn, Keyword.Stack);
                case "DepletableBag": return new KeywordToken(line, startingColumn, Keyword.DepletableBag);
                case "DepletableList": return new KeywordToken(line, startingColumn, Keyword.DepletableList);
                case "RingBuffer": return new KeywordToken(line, startingColumn, Keyword.RingBuffer);
                case "Dictionary": return new KeywordToken(line, startingColumn, Keyword.Dictionary);
                case "HashSet": return new KeywordToken(line, startingColumn, Keyword.HashSet);

                //Other Types
                case "Random": return new KeywordToken(line, startingColumn, Keyword.Random);

                //Static Types
                case "System": return new KeywordToken(line, startingColumn, Keyword.System);
                case "Debug": return new KeywordToken(line, startingColumn, Keyword.Debug);
                case "User": return new KeywordToken(line, startingColumn, Keyword.User);
                case "Math": return new KeywordToken(line, startingColumn, Keyword.Math);

                case "new": return new KeywordToken(line, startingColumn, Keyword.New);

                default: return new IdentifierToken(line, startingColumn, token);
            }
        }

        public char Read()
        {
            int next = textReader.Read();

            if (next == -1)
            {
                throw new ScriptParsingException(line, column + 1, "Tried to Read past the end of the script");
            }


            switch (next)
            {
                case '\n':
                    line++;
                    column = 0;
                    break;

                default:
                    column++;
                    break;
            }

            return (char)next;
        }

        public char Peek()
        {
            int next = textReader.Peek();

            if (next == -1)
            {
                throw new ScriptParsingException(line, column + 1, "Tried to Peek past the end of the script");
            }

            return (char)next;
        }

        public bool ConsumeIfNext(char value)
        {
            int next = textReader.Peek();

            if (next == -1)
            {
                return false;
            }

            if (value == next)
            {
                textReader.Read();

                if (value == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                {
                    column++;
                }

                return true;
            }

            return false;
        }

        public void SkipWhitespace()
        {
            int next;

            while (((next = textReader.Peek()) != -1) && char.IsWhiteSpace((char)next))
            {
                Read();
            }
        }

        private static bool IsWordCharacter(char c) => char.IsLetterOrDigit(c) || c == '_';
        private static bool IsNumberCharacter(char c) =>
            char.IsNumber(c) || c == '.' || c == 'E' || c == 'e';

        #region IDisposable Support

        // To detect redundant calls
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    textReader.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion
    }
}
