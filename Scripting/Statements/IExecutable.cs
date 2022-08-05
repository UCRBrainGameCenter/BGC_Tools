using System;
using System.Threading;

namespace BGC.Scripting
{
    public interface IExecutable : IExpression
    {
        FlowState Execute(ScopeRuntimeContext context, CancellationToken ct);
    }
}