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

                case Keyword.While: return "while";
                case Keyword.For: return "for";

                case Keyword.Continue: return "continue";
                case Keyword.Break: return "break";
                case Keyword.Return: return "return";

                case Keyword.Extern: return "extern";
                case Keyword.Global: return "global";
                case Keyword.Const: return "const";

                case Keyword.Void: return "void";

                case Keyword.Bool: return "bool";
                case Keyword.Double: return "double";
                case Keyword.Integer: return "int";
                case Keyword.String: return "string";

                case Keyword.List: return "List";
                case Keyword.Queue: return "Queue";
                case Keyword.Stack: return "Stack";
                case Keyword.DepletableBag: return "DepletableBag";
                case Keyword.DepletableList: return "DepletableList";
                case Keyword.RingBuffer: return "RingBuffer";
                case Keyword.Dictionary: return "Dictionary";
                case Keyword.HashSet: return "HashSet";

                case Keyword.Random: return "Random";

                case Keyword.New: return "new";

                default:
                    throw new ArgumentException($"Unexpected Keyword: {keyword}");
            }
        }
    }
}
