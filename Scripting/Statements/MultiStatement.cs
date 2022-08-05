using System;
using System.Collections.Generic;
using System.Threading;


namespace BGC.Scripting
{
    public class MultiStatement : Statement
    {
        private readonly IEnumerable<IExecutable> statements;

        public MultiStatement(IEnumerable<IExecutable> statements)
        {
            this.statements = statements;
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            FlowState state;
            foreach (IExecutable statement in statements)
            {
                ct.ThrowIfCancellationRequested();

                state = statement.Execute(context, ct);

                if (state != FlowState.Nominal)
                {
                    return state;
                }
            }

            return FlowState.Nominal;
        }
    }
}