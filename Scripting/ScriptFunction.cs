using System;
using System.Collections.Generic;
using System.Threading;

namespace BGC.Scripting
{
    public class ScriptFunction
    {
        private readonly List<IExecutable> statements = new List<IExecutable>();
        public readonly FunctionSignature functionSignature;
        private readonly List<Token> cachedFunctionTokens = new List<Token>();

        public string FunctionName => functionSignature.identifierToken.identifier;
        public Guid FunctionId => functionSignature.id;

        public ScriptFunction(
            IEnumerator<Token> functionTokens,
            in FunctionSignature functionSignature,
            ScriptCompilationContext context)
        {
            this.functionSignature = functionSignature;

            context.DeclareFunction(functionSignature);

            //Determine Type
            //Function Body
            if (functionTokens.TestAndConditionallyAdvance(Separator.Arrow))
            {
                if (functionSignature.returnType != typeof(void))
                {
                    //Add in an effective return token
                    cachedFunctionTokens.Add(new KeywordToken(functionTokens.Current, Keyword.Return));
                }

                //Expression-bodied Functions end at the semicolon
                while (true)
                {
                    cachedFunctionTokens.Add(functionTokens.Current);

                    if (functionTokens.TestAndConditionallyAdvance(Separator.Semicolon, false))
                    {
                        break;
                    }

                    functionTokens.CautiousAdvance();
                }

                //Cap off function with EOF Token
                cachedFunctionTokens.Add(new EOFToken(functionTokens.Current));
            }
            else if (functionTokens.TestWithoutAdvancing(Separator.OpenCurlyBoi))
            {
                //standard function
                Stack<SeparatorToken> separators = new Stack<SeparatorToken>();
                separators.Push(functionTokens.GetTokenAndAdvance<SeparatorToken>());

                while (true)
                {
                    if (functionTokens.Current is SeparatorToken sepToken)
                    {
                        switch (sepToken.separator)
                        {
                            case Separator.OpenParen:
                            case Separator.OpenIndexer:
                            case Separator.OpenCurlyBoi:
                                separators.Push(sepToken);
                                break;

                            case Separator.CloseParen:
                                {
                                    SeparatorToken match = separators.Pop();
                                    if (match.separator != Separator.OpenParen)
                                    {
                                        throw new ScriptParsingException(
                                            source: match,
                                            message: $"Unexpected CloseParen: {sepToken}");
                                    }
                                }
                                break;

                            case Separator.CloseIndexer:
                                {
                                    SeparatorToken match = separators.Pop();
                                    if (match.separator != Separator.OpenIndexer)
                                    {
                                        throw new ScriptParsingException(
                                            source: match,
                                            message: $"Unexpected CloseIndexer: {sepToken}");
                                    }
                                }
                                break;

                            case Separator.CloseCurlyBoi:
                                {
                                    SeparatorToken match = separators.Pop();
                                    if (match.separator != Separator.OpenCurlyBoi)
                                    {
                                        throw new ScriptParsingException(
                                            source: match,
                                            message: $"Unexpected CloseCurlyBoi: {sepToken}");
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                    }

                    if (separators.Count == 0)
                    {
                        //Finished
                        break;
                    }

                    cachedFunctionTokens.Add(functionTokens.Current);
                    functionTokens.CautiousAdvance();
                }

                functionTokens.AssertAndAdvance(Separator.CloseCurlyBoi, false);

                //Cap off function with EOF Token
                cachedFunctionTokens.Add(new EOFToken(functionTokens.Current));
            }
            else
            {
                throw new ScriptParsingException(
                    source: functionTokens.Current,
                    message: $"Functions must begin with a CurlyBoi or written Expression-Bodied.  Found: {functionTokens.Current}");
            }
        }

        public void ParseFunctions(ScriptCompilationContext context) =>
            CompleteParsing(context.CreateFunction(functionSignature));

        private void CompleteParsing(FunctionCompilationContext context)
        {
            IEnumerator<Token> functionTokens = cachedFunctionTokens.GetEnumerator();
            functionTokens.MoveNext();

            while (functionTokens.Current is not EOFToken)
            {
                IExecutable nextStatement = Statement.ParseNextStatement(functionTokens, context);
                if (nextStatement is not null)
                {
                    statements.Add(nextStatement);
                }
            }

            cachedFunctionTokens.Clear();
        }

        public void Execute(
            ScriptRuntimeContext context,
            CancellationToken ct,
            params object[] arguments)
        {
            FunctionRuntimeContext functionContext = new FunctionRuntimeContext(
                scriptContext: context,
                functionSignature: functionSignature,
                arguments: arguments);

            ScopeRuntimeContext scopeContext = new ScopeRuntimeContext(functionContext);


            foreach (IExecutable statement in statements)
            {
                ct.ThrowIfCancellationRequested();

                FlowState state = statement.Execute(scopeContext, ct);

                switch (state)
                {
                    case FlowState.Nominal:
                        //Continue
                        break;

                    case FlowState.Return:
                        return;

                    case FlowState.LoopContinue:
                    case FlowState.LoopBreak:
                        throw new ScriptRuntimeException($"Invalid {state} thrown and not handled during execution.");

                    default:
                        throw new Exception($"Unexpected FlowState: {state}");
                }
            }

            for (int i = 0; i < functionSignature.arguments.Length; i++)
            {
                switch (functionSignature.arguments[i].argumentType)
                {
                    case ArgumentType.Ref:
                    case ArgumentType.Out:
                        //Copy Ref and Output values back to input
                        arguments[i] = functionContext.GetExistingValue<object>(functionSignature.arguments[i].identifierToken.identifier);
                        break;

                    default:
                        //Do nothing
                        break;
                }
            }
        }
    }
}