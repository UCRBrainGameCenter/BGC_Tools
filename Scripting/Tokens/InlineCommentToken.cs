namespace BGC.Scripting
{
    public class InlineCommentToken : CommentToken
    {
        public InlineCommentToken(int line, int column, string comment)
            : base(line, column, comment)
        {

        }

        public InlineCommentToken(Token source, string comment)
            : base(source, comment)
        {

        }

        public override string ToString() => $"//{comment}";
    }
}