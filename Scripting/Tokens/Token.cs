namespace BGC.Scripting
{
    public abstract class Token
    {
        public int line;
        public int column;

        public Token(int line, int column)
        {
            this.line = line;
            this.column = column;
        }

        public Token(Token source)
        {
            line = source.line;
            column = source.column;
        }
    }
}