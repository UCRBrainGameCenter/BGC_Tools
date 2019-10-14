using System.Collections.Generic;
using System.Linq;

namespace BGC.Scripting
{
    public class MultiStatement : Statement
    {
        private readonly IEnumerable<IExecutable> statements;

        public MultiStatement(IEnumerable<IExecutable> statements)
        {
            this.statements = statements;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            FlowState state;
            foreach (IExecutable statement in statements)
            {
                state = statement.Execute(context);

                if (state != FlowState.Nominal)
                {
                    return state;
                }
            }

            return FlowState.Nominal;
        }
    }
}
