namespace BGC.Scripting
{
    public class SeparatorToken : Token
    {
        public readonly Separator separator;

        public SeparatorToken(int line, int column, Separator separator)
            : base(line, column)
        {
            this.separator = separator;
        }

        public SeparatorToken(Token source, Separator separator)
            : base(source)
        {
            this.separator = separator;
        }

        public override string ToString()
        {
            switch (separator)
            {
                case Separator.Colon: return ":";
                case Separator.Semicolon: return ";";
                case Separator.Comma: return ",";

                case Separator.OpenParen: return "(";
                case Separator.CloseParen: return ")";

                case Separator.OpenCurlyBoi: return "{";
                case Separator.CloseCurlyBoi: return "}";

                case Separator.OpenIndexer: return "[";
                case Separator.CloseIndexer: return "]";

                default:
                    throw new ScriptParsingException(line, column, $"Unexpected Separator: {separator}");
            }
        }
    }
}