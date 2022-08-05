using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class ForEachStatement : Statement
    {
        private readonly IExecutable declarationStatement;
        private readonly IValueGetter containerExpression;
        private readonly IValue loopVariable;

        private readonly IExecutable loopBody;

        private ScopeRuntimeContext loopContext;
        private ScopeRuntimeContext bodyContext;

        public ForEachStatement(
            IExecutable declarationStatement,
            IValue loopVariable,
            IValueGetter containerExpression,
            IExecutable loopBody,
            KeywordToken keywordToken)
        {
            if (!typeof(IEnumerable).IsAssignableFrom(containerExpression.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: keywordToken,
                    message: $"Collection of ForEach statement is not an Enumerable collection: {containerExpression.GetValueType().Name}");
            }

            //Find the IEnumerable interfaces
            Type containerType = containerExpression.GetValueType();
            Type enumerableType = null;
            foreach (Type containerInterface in containerType.GetInterfaces())
            {
                if (containerInterface.IsGenericType && containerInterface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    //Found an IEnumerable<T> Implementation
                    if (loopVariable.GetValueType().AssignableOrConvertableFromType(containerInterface.GetGenericArguments()[0]))
                    {
                        enumerableType = containerInterface.GetGenericArguments()[0];
                        break;
                    }
                }
            }

            if (enumerableType is null)
            {
                throw new ScriptParsingException(
                    source: keywordToken,
                    message: $"Collection items of type " +
                        $"({containerExpression.GetValueType().GetGenericArguments()[0].Name}) " +
                        $"not assignable to declared item type: {loopVariable.GetValueType().Name}");
            }

            this.loopVariable = loopVariable;

            this.declarationStatement = declarationStatement;
            this.containerExpression = containerExpression;
            this.loopBody = loopBody;
        }

        public override FlowState Execute(
            ScopeRuntimeContext context,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            loopContext = new ScopeRuntimeContext(context);
            FlowState state = declarationStatement?.Execute(loopContext, ct) ?? FlowState.Nominal;

            switch (state)
            {
                case FlowState.Nominal:
                    //Continue
                    break;

                case FlowState.LoopContinue:
                case FlowState.LoopBreak:
                case FlowState.Return:
                default:
                    throw new Exception($"Unexpected FlowState: {state}");
            }

            bool continuing = true;

            foreach (object item in containerExpression.GetAs<IEnumerable>(loopContext)!)
            {
                ct.ThrowIfCancellationRequested();

                loopVariable.Set(loopContext, item);
                bodyContext = new ScopeRuntimeContext(loopContext);

                state = loopBody?.Execute(bodyContext, ct) ?? FlowState.Nominal;

                switch (state)
                {
                    case FlowState.Nominal:
                    case FlowState.LoopContinue:
                        //Do Nothing
                        break;

                    case FlowState.LoopBreak:
                        continuing = false;
                        break;

                    case FlowState.Return:
                        return state;

                    default:
                        throw new Exception($"Unexpected FlowState: {state}");
                }

                if (!continuing)
                {
                    break;
                }
            }

            return FlowState.Nominal;
        }
    }
}