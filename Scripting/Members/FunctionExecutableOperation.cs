using System;

namespace BGC.Scripting
{
    public class FunctionExecutableOperation : IExecutable
    {
        private readonly Action<RuntimeContext> operation;

        public FunctionExecutableOperation(
            Action<RuntimeContext> operation)
        {
            this.operation = operation;
        }

        public FlowState Execute(ScopeRuntimeContext context)
        {
            operation(context);

            return FlowState.Nominal;
        }
    }

}
