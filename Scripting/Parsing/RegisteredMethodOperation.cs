using System;
using System.Threading;
using System.Reflection;
using System.Linq;

namespace BGC.Scripting.Parsing
{
    public abstract class RegisteredMethodOperation : IValueGetter, IExecutable
    {
        private readonly InvocationArgument[] args;
        private readonly MethodInfo methodInfo;
        private readonly Type returnType;
        private readonly Token source;

        protected abstract object GetInstanceValue(RuntimeContext context);

        public RegisteredMethodOperation(
            InvocationArgument[] args,
            MethodInfo methodInfo,
            Token source)
        {
            this.args = args;
            this.source = source;
            this.methodInfo = methodInfo;

            returnType = methodInfo.ReturnType;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableOrConvertableFromType(this.returnType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of Method Invocation with type {this.returnType.Name} as type {returnType.Name}");
            }

            object[] argumentValues = args.GetArgs(methodInfo, context);

            object result = methodInfo.Invoke(
                obj: GetInstanceValue(context),
                parameters: argumentValues);

            //Handles By-Ref arguments
            args.HandlePostInvocation(argumentValues, context);

            if (!returnType.IsAssignableFrom(this.returnType))
            {
                return (T)Convert.ChangeType(result, returnType);
            }

            return (T)result;
        }

        public FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            object[] argumentValues = args.GetArgs(methodInfo, context);

            methodInfo.Invoke(
                obj: GetInstanceValue(context),
                parameters: argumentValues);

            //Handles By-Ref arguments
            args.HandlePostInvocation(argumentValues, context);

            return FlowState.Nominal;
        }

        public Type GetValueType() => returnType;
        public override string ToString() => $"{GetType()}: From {source}.";
    }

    public class RegisteredInstanceMethodOperation : RegisteredMethodOperation
    {
        private readonly IValueGetter value;

        protected override object GetInstanceValue(RuntimeContext context) => value.GetAs<object>(context);

        public RegisteredInstanceMethodOperation(
            IValueGetter value,
            InvocationArgument[] args,
            MethodInfo methodInfo,
            Token source)
            : base(args, methodInfo, source)
        {
            this.value = value;
        }
    }


    public class RegisteredStaticMethodOperation : RegisteredMethodOperation
    {
        protected override object GetInstanceValue(RuntimeContext context) => null;

        public RegisteredStaticMethodOperation(
            InvocationArgument[] args,
            MethodInfo methodInfo,
            Token source)
            : base(args, methodInfo, source)
        {
        }
    }
}