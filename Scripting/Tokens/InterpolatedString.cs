using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Scripting
{
    public class InterpolatedString : Token
    {
        private readonly string formatString;
        private readonly List<List<Token>> arguments;

        public InterpolatedString(
            int line,
            int column,
            string formatString,
            List<List<Token>> arguments)
            : base(line, column)
        {
            this.formatString = formatString;
            this.arguments = arguments;
        }

        public InterpolatedString(
            Token source,
            string formatString,
            List<List<Token>> arguments)
            : base(source)
        {
            this.formatString = formatString;
            this.arguments = arguments;
        }

        public override string ToString() => $"$\"string.Format(\"{formatString}\", {string.Join(", ", arguments.Select(arg => string.Join("", arg.Select(x => x.ToString()))))}\"";

        //Replace it with a String.Format call
        public IEnumerable<Token> RewriteToken()
        {
            yield return new TypeToken(line, column, "string", typeof(string));
            yield return new OperatorToken(line, column, Operator.MemberAccess);
            yield return new IdentifierToken(line, column, "Format");
            yield return new SeparatorToken(line, column, Separator.OpenParen);

            yield return new LiteralToken<string>(line, column, formatString);

            yield return new SeparatorToken(line, column, Separator.Comma);

            //new object[] { .... }
            yield return new KeywordToken(line, column, Keyword.New);
            yield return new TypeToken(line, column, "object[]", typeof(object[]));
            yield return new SeparatorToken(line, column, Separator.OpenCurlyBoi);

            //1st Argument
            foreach (Token token in arguments[0])
            {
                yield return token;
            }

            foreach (List<Token> argument in arguments.Skip(1))
            {
                //Return a comma and then the argument, for each argument
                yield return new SeparatorToken(argument[0], Separator.Comma);

                foreach (Token token in argument)
                {
                    yield return token;
                }
            }

            yield return new SeparatorToken(line, column, Separator.CloseCurlyBoi);
            yield return new SeparatorToken(line, column, Separator.CloseParen);
        }
    }
}