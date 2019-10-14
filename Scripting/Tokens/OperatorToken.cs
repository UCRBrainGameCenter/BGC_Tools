using System;

namespace BGC.Scripting
{
    public class OperatorToken : Token
    {
        public readonly Operator operatorType;

        public OperatorToken(int line, int column, Operator operatorType)
            : base(line, column)
        {
            this.operatorType = operatorType;
        }

        public OperatorToken(Token source, Operator operatorType)
            : base(source)
        {
            this.operatorType = operatorType;
        }

        public override string ToString()
        {
            switch (operatorType)
            {
                case Operator.Assignment: return "=";

                case Operator.Plus: return "+";
                case Operator.Minus: return "-";
                case Operator.Times: return "*";
                case Operator.Divide: return "/";
                case Operator.Power: return "^";
                case Operator.Modulo: return "%";

                case Operator.PlusEquals: return "+=";
                case Operator.MinusEquals: return "-=";
                case Operator.TimesEquals: return "*=";
                case Operator.DivideEquals: return "/=";
                case Operator.PowerEquals: return "^=";
                case Operator.ModuloEquals: return "%=";

                case Operator.Increment: return "++";
                case Operator.Decrement: return "--";

                case Operator.Negate: return "-";
                case Operator.Not: return "!";

                case Operator.IsEqualTo: return "==";
                case Operator.IsNotEqualTo: return "!=";

                case Operator.IsGreaterThan: return ">";
                case Operator.IsGreaterThanOrEqualTo: return ">=";
                case Operator.IsLessThan: return "<";
                case Operator.IsLessThanOrEqualTo: return "<=";

                case Operator.And: return "&&";
                case Operator.Or: return "||";

                case Operator.Ternary: return "?";

                case Operator.AndEquals: return "&=";
                case Operator.OrEquals: return "|=";

                case Operator.CastDouble: return "(double)";
                case Operator.CastInteger: return "(int)";

                case Operator.MemberAccess: return ".";
                case Operator.Indexing: return "[]";

                case Operator.AmbiguousMinus: return "-";

                default:
                    throw new ArgumentException($"Unexpected Operator: {operatorType}");
            }
        }
    }
}
