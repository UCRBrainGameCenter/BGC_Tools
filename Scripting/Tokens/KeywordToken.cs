using System;

namespace BGC.Scripting
{
    public class KeywordToken : Token
    {
        public readonly Keyword keyword;

        public KeywordToken(int line, int column, Keyword keyword)
            : base(line, column)
        {
            this.keyword = keyword;
        }

        public KeywordToken(Token source, Keyword keyword)
            : base(source)
        {
            this.keyword = keyword;
        }

        public override string ToString()
        {
            switch (keyword)
            {
                case Keyword.If: return "if";
                case Keyword.ElseIf: return "else if";
                case Keyword.Else: return "else";
                case Keyword.Switch: return "switch";

                case Keyword.While: return "while";
                case Keyword.For: return "for";

                case Keyword.Continue: return "continue";
                case Keyword.Break: return "break";
                case Keyword.Return: return "return";
                case Keyword.Case: return "case";
                case Keyword.Default: return "default";

                case Keyword.Extern: return "extern";
                case Keyword.Global: return "global";
                case Keyword.Const: return "const";

                case Keyword.New: return "new";

                default:
                    throw new ArgumentException($"Unexpected Keyword: {keyword}");
            }
        }
    }
}