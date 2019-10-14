using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class StaticArgumentStatementOperation : IExecutable
    {
        private readonly Action<RuntimeContext> operation;

        public StaticArgumentStatementOperation(
            Action<RuntimeContext> operation)
        {
            this.operation = operation;
        }

        FlowState IExecutable.Execute(ScopeRuntimeContext context)
        {
            operation.Invoke(context);
            return FlowState.Nominal;
        }
    }

}
