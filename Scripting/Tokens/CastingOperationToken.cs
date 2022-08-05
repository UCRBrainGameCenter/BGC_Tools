using System;

namespace BGC.Scripting
{
    public class CastingOperationToken : Token
    {
        public readonly Type type;

        public CastingOperationToken(int line, int column, Type type)
            : base(line, column)
        {
            this.type = type;
        }

        public CastingOperationToken(Token source, Type type)
            : base(source)
        {
            this.type = type;
        }

        public override string ToString() => type.ToString();
    }
}