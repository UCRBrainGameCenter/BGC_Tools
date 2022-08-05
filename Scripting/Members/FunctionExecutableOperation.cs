using System;
using System.Threading;

namespace BGC.Scripting
{
    public class FunctionExecutableOperation : IExecutable
    {
        private readonly FunctionSignature functionSignature;
        private readonly InvocationArgument[] arguments;

        public FunctionExecutableOperation(
            FunctionSignature functionSignature,
            InvocationArgument[] arguments)
        {
            this.functionSignature = functionSignature;
            this.arguments = arguments;
        }

        public FlowState Execute(ScopeRuntimeContext context, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            object[] args = arguments.GetArgs(functionSignature, context);

            context.RunVoidFunction(functionSignature.id, args);

            arguments.HandlePostInvocation(args, context);

            return FlowState.Nominal;
        }
    }
}