namespace BGC.Scripting
{
    public abstract class CommentToken : Token
    {
        public readonly string comment;

        public CommentToken(int line, int column, string comment)
            : base(line, column)
        {
            this.comment = comment;
        }

        public CommentToken(Token source, string comment)
            : base(source)
        {
            this.comment = comment;
        }
    }
}
