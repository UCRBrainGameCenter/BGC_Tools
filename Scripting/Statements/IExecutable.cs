using System.Collections.Generic;

namespace BGC.Scripting
{
    public interface IExecutable : IExpression
    {
        FlowState Execute(ScopeRuntimeContext context);
    }
}
