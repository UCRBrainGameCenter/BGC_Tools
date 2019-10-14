namespace BGC.Scripting
{
    public class BlockCommentToken : CommentToken
    {
        public BlockCommentToken(int line, int column, string comment)
            : base(line, column, comment)
        {

        }

        public BlockCommentToken(Token source, string comment)
            : base(source, comment)
        {

        }

        public override string ToString() => $"/*{comment}*/";
    }
}
