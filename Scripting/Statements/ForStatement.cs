using System;

namespace BGC.Scripting
{
    public class ForStatement : Statement
    {
        private readonly IExecutable initializationStatement;
        private readonly IValueGetter continueExpression;
        private readonly IExecutable incrementStatement;
        private readonly IExecutable loopBody;

        private ScopeRuntimeContext loopContext;
        private ScopeRuntimeContext bodyContext;

        public ForStatement(
            IExecutable initializationStatement,
            IValueGetter continueExpression,
            IExecutable incrementStatement,
            IExecutable loopBody,
            KeywordToken keywordToken)
        {
            if (continueExpression.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: keywordToken,
                    message: $"Condition of {keywordToken} statement is not a boolean value: type {continueExpression.GetValueType().Name}");
            }

            this.initializationStatement = initializationStatement;
            this.continueExpression = continueExpression;
            this.incrementStatement = incrementStatement;
            this.loopBody = loopBody;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            loopContext = new ScopeRuntimeContext(context);
            FlowState state = initializationStatement?.Execute(loopContext) ?? FlowState.Nominal;

            switch (state)
            {
                case FlowState.Nominal:
                    //Continue
                    break;

                case FlowState.LoopContinue:
                case FlowState.LoopBreak:
                case FlowState.Return:
                default:
                    throw new Exception($"Unexpected FlowState: {state}");
            }

            bool continuing = true;

            while (continuing && continueExpression.GetAs<bool>(loopContext))
            {
                bodyContext = new ScopeRuntimeContext(loopContext);

                state = loopBody.Execute(bodyContext);

                switch (state)
                {
                    case FlowState.Nominal:
                    case FlowState.LoopContinue:
                        //Do Nothing
                        break;

                    case FlowState.LoopBreak:
                        continuing = false;
                        break;

                    case FlowState.Return:
                        return state;

                    default:
                        throw new Exception($"Unexpected FlowState: {state}");
                }

                //Don't run incrementStatement if we are breaking out
                if (continuing)
                {
                    state = incrementStatement?.Execute(loopContext) ?? FlowState.Nominal;

                    switch (state)
                    {
                        case FlowState.Nominal:
                            //Do Nothing
                            break;

                        case FlowState.Return:
                        case FlowState.LoopContinue:
                        case FlowState.LoopBreak:
                        default:
                            throw new Exception($"Unexpected FlowState: {state}");
                    }
                }
            }

            return FlowState.Nominal;
        }
    }
}
