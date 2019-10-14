using System;

namespace BGC.Scripting
{
    public class WhileStatement : Statement
    {
        private readonly IValueGetter continueExpression;
        private readonly IExecutable loopBody;

        private ScopeRuntimeContext bodyContext;

        public WhileStatement(
            IValueGetter continueExpression,
            IExecutable loopBody,
            KeywordToken keywordToken)
        {
            if (continueExpression.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: keywordToken,
                    message: $"ContinueExpression of {keywordToken} statement is not a boolean value: type {continueExpression.GetValueType().Name}");
            }

            this.continueExpression = continueExpression;
            this.loopBody = loopBody;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            FlowState state;

            bool continuing = true;
            while (continuing && continueExpression.GetAs<bool>(context))
            {
                bodyContext = new ScopeRuntimeContext(context);

                state = loopBody.Execute(bodyContext);

                switch (state)
                {
                    case FlowState.Nominal:
                    case FlowState.LoopContinue:
                        //Do nothing
                        break;

                    case FlowState.LoopBreak:
                        continuing = false;
                        break;

                    case FlowState.Return:
                        return state;

                    default:
                        throw new Exception($"Unexpected FlowState: {state}");
                }
            }

            return FlowState.Nominal;
        }
    }
}
