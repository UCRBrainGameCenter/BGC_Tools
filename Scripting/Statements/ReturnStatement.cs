using System;
using System.Threading;

namespace BGC.Scripting
{
    public class ReturnStatement : Statement
    {
        private readonly IValueGetter returnValue;
        private readonly Type returnType;

        public ReturnStatement(
            KeywordToken keywordToken,
            IValueGetter returnValue,
            CompilationContext context)
        {
            this.returnValue = returnValue;

            context.ValidateReturn(
                returnKeyword: keywordToken,
                returnType: returnValue?.GetValueType() ?? typeof(void));

            returnType = context.GetReturnType();
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (returnType == typeof(void))
            {
                context.PushReturnValue(null);
            }
            else
            {
                if (returnType.IsAssignableFrom(returnValue!.GetValueType()))
                {
                    context.PushReturnValue(returnValue.GetAs<object>(context));
                }
                else
                {
                    context.PushReturnValue(
                        Convert.ChangeType(returnValue.GetAs<object>(context), returnType));
                }
            }

            return FlowState.Return;
        }
    }
}