using System;
using System.Threading;

namespace BGC.Scripting
{
    public class FunctionValueOperation : IValueGetter, IExecutable
    {
        private readonly FunctionSignature functionSignature;
        private readonly InvocationArgument[] arguments;

        public FunctionValueOperation(
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

            context.RunFunction(functionSignature.id, args);

            arguments.HandlePostInvocation(args, context);

            return FlowState.Nominal;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(functionSignature.returnType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Indexing with type {functionSignature.returnType.Name} as type {returnType.Name}");
            }

            object[] args = arguments.GetArgs(functionSignature, context);

            object result = context.RunFunction(functionSignature.id, args);

            arguments.HandlePostInvocation(args, context);

            if (!returnType.IsAssignableFrom(functionSignature.returnType))
            {
                return (T)Convert.ChangeType(result, returnType);
            }

            return (T)result;
        }

        public Type GetValueType() => functionSignature.returnType;
    }
}