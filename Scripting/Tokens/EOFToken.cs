namespace BGC.Scripting
{
    public class EOFToken : Token
    {
        public EOFToken(int line, int column)
            : base(line, column)
        {

        }
        public EOFToken(Token source)
            : base(source)
        {

        }

        public override string ToString() => "EOF";
    }
}
