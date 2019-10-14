using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class MemberArgumentStatementOperation<TInput> : IExecutable
    {
        private readonly IValueGetter value;
        private readonly Action<TInput, RuntimeContext> operation;

        public MemberArgumentStatementOperation(
            IValueGetter value,
            Action<TInput, RuntimeContext> operation,
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
            operation.Invoke(value.GetAs<TInput>(context), context);
            return FlowState.Nominal;
        }
    }

}
