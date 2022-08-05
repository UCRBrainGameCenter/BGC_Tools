using System;
using System.Collections.Generic;
using System.Threading;

namespace BGC.Scripting
{
    public class SwitchStatement : Statement
    {
        private readonly IValueGetter switchValue;
        private readonly Block defaultBlock;
        private readonly Dictionary<object, Block> switchBlocks;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly KeywordToken token;
#pragma warning restore IDE0052 // Remove unread private members

        public SwitchStatement(
            IValueGetter switchValue,
            Block defaultBlock,
            Dictionary<object, Block> switchBlocks,
            KeywordToken keywordToken)
        {
            this.switchValue = switchValue;
            this.defaultBlock = defaultBlock;
            this.switchBlocks = switchBlocks;
            token = keywordToken;
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            object value = switchValue.GetAs<object>(context)!;

            if (switchBlocks.ContainsKey(value))
            {
                FlowState state = switchBlocks[value].Execute(new ScopeRuntimeContext(context), ct);

                //intercept and handle break
                if (state == FlowState.LoopBreak)
                {
                    state = FlowState.Nominal;
                }

                return state;
            }
            else if (defaultBlock is not null)
            {
                FlowState state = defaultBlock.Execute(new ScopeRuntimeContext(context), ct);

                //intercept and handle break
                if (state == FlowState.LoopBreak)
                {
                    state = FlowState.Nominal;
                }

                return state;
            }

            return FlowState.Nominal;
        }
    }
}