using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class MemberStatementOperation<TInput> : IExecutable
    {
        private readonly IValueGetter value;
        private readonly Action<TInput> operation;

        public MemberStatementOperation(
            IValueGetter value,
            Action<TInput> operation,
            Token source)
        {
            this.value = value;
            this.operation = operation;

            if (!typeof(TInput).AssignableFromType(value.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Incorrect value type.  Expected: {typeof(TInput).Name}.  Received: {value.GetValueType().Name}. ");
            }
        }

        FlowState IExecutable.Execute(ScopeRuntimeContext context)
        {
            operation.Invoke(value.GetAs<TInput>(context));
            return FlowState.Nominal;
        }
    }

}
