using System;

namespace BGC.Scripting
{
    public class ScriptParsingException : Exception
    {
        public int line;
        public int column;

        public ScriptParsingException(Token source, string message)
            : base(message)
        {
            line = source.line;
            column = source.column;
        }

        public ScriptParsingException(int line, int column, string message)
            : base(message)
        {
            this.line = line;
            this.column = column;
        }

        public override string Message => $"Script parsing exception on Line {line}, Column {column}: {base.Message}";
    }
}