namespace BGC.Scripting
{
    public class IdentifierToken : Token
    {
        public readonly string identifier;

        public IdentifierToken(int line, int column, string identifier)
            : base(line, column)
        {
            this.identifier = identifier;
        }

        public IdentifierToken(Token source, string identifier)
            : base(source)
        {
            this.identifier = identifier;
        }

        public override string ToString() => identifier;
    }
}
