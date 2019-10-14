using System;
using System.Collections.Generic;
using System.Linq;

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
            while (!(tokens.Current is EOFToken))
            {
                if (tokens.TestWithoutSkipping(Separator.CloseCurlyBoi))
                {
                    return;
                }

                IExecutable nextStatement = ParseNextStatement(tokens, compilationContext);
                if (nextStatement != null)
                {
                    statements.Add(nextStatement);
                }
            }
        }

        public override FlowState Execute(ScopeRuntimeContext parentContext)
        {
            ScopeRuntimeContext context = new ScopeRuntimeContext(parentContext);

            foreach (IExecutable statement in statements)
            {
                FlowState state = statement.Execute(context);

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
