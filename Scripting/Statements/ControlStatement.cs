using System;

namespace BGC.Scripting
{
    public class ControlStatement : Statement
    {
        public readonly Keyword keyword;

        public ControlStatement(
            KeywordToken keywordToken,
            CompilationContext context)
        {
            keyword = keywordToken.keyword;

            context.ValidateFlowControlKeyword(keywordToken);

            switch (keyword)
            {
                case Keyword.Continue:
                case Keyword.Break:
                    //Acceptable
                    break;

                default: throw new ArgumentException($"Unexpected Keyword: {keyword}");
            }
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            switch (keyword)
            {
                case Keyword.Continue: return FlowState.LoopContinue;
                case Keyword.Break: return FlowState.LoopBreak;

                default: throw new ArgumentException($"Unexpected Keyword: {keyword}");
            }
        }
    }
}
