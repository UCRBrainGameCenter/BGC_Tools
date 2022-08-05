using System;
using System.Threading;

namespace BGC.Scripting
{
    public class IfStatement : Statement
    {
        private readonly IValueGetter condition;
        private readonly IExecutable trueBlock;
        private readonly IExecutable falseBlock;

        public IfStatement(
            IValueGetter condition,
            IExecutable trueBlock,
            IExecutable falseBlock,
            KeywordToken keywordToken)
        {
            if (condition.GetValueType() != typeof(bool))
            {
                throw new ScriptParsingException(
                    source: keywordToken,
                    message: $"Condition of {keywordToken} statement is not a boolean value: type {condition.GetValueType().Name}");
            }

            this.condition = condition;
            this.trueBlock = trueBlock;
            this.falseBlock = falseBlock;
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            if (condition.GetAs<bool>(context))
            {
                return trueBlock?.Execute(new ScopeRuntimeContext(context), ct) ?? FlowState.Nominal;
            }
            else
            {
                return falseBlock?.Execute(new ScopeRuntimeContext(context), ct) ?? FlowState.Nominal;
            }
        }
    }
}