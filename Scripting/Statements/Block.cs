using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BGC.Scripting
{
    public class Block : Statement
    {
        private readonly List<IExecutable> statements = new List<IExecutable>();

        public Block() { }

        //Parse
        public Block(
            IEnumerator<Token> tokens,
            CompilationContext compilationContext)
        {
            compilationContext = compilationContext.CreateChildScope();
            while (tokens.Current is not EOFToken)
            {
                if (tokens.TestWithoutAdvancing(Separator.CloseCurlyBoi) || tokens.TestWithoutAdvancing(Keyword.Default) || tokens.TestWithoutAdvancing(Keyword.Case))
                {
                    return;
                }

                IExecutable nextStatement = ParseNextStatement(tokens, compilationContext);
                if (nextStatement is not null)
                {
                    statements.Add(nextStatement);
                }
            }
        }

        public override FlowState Execute(
            ScopeRuntimeContext parentContext,
            CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            ScopeRuntimeContext context = new ScopeRuntimeContext(parentContext);

            foreach (IExecutable statement in statements)
            {
                FlowState state = statement.Execute(context, ct);

                ct.ThrowIfCancellationRequested();

                switch (state)
                {
                    case FlowState.Nominal:
                        //Continue
                        break;

                    case FlowState.LoopContinue:
                    case FlowState.LoopBreak:
                    case FlowState.Return:
                        return state;

                    default:
                        throw new Exception($"Unexpected FlowState: {state}");
                }
            }

            return FlowState.Nominal;
        }

        public override string ToString() => $"{{ {string.Join(" ", statements.Select(x => x.ToString()).ToArray())} }}";
    }
}