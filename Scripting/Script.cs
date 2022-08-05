using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace BGC.Scripting
{
    public class Script
    {
        public readonly string scriptText;
        private readonly List<ScriptDeclaration> scriptDeclarations = new List<ScriptDeclaration>();

        private readonly Dictionary<string, List<ScriptFunction>> scriptFunctionsByName = new Dictionary<string, List<ScriptFunction>>();
        private readonly Dictionary<Guid, ScriptFunction> scriptFunctionsById = new Dictionary<Guid, ScriptFunction>();

        /// <summary>
        /// Parse Script
        /// </summary>
        /// <exception cref="ScriptParsingException"></exception>
        public Script(
            string scriptText,
            IEnumerator<Token> scriptTokens,
            params FunctionSignature[] expectedFunctions)
        {
            this.scriptText = scriptText;

            ScriptCompilationContext compilationContext = new ScriptCompilationContext();

            while (scriptTokens.Current is not EOFToken)
            {
                ParseNextGlobal(scriptTokens, compilationContext);
            }

            foreach (FunctionSignature data in expectedFunctions)
            {
                if (!scriptFunctionsByName.ContainsKey(data.identifierToken.identifier))
                {
                    throw new ScriptParsingException(
                        source: scriptTokens.Current ?? new EOFToken(0, 0),
                        message: $"Expected Function not found: {data}");
                }

                if (!scriptFunctionsByName[data.identifierToken.identifier].Any(x => data.Matches(x.functionSignature)))
                {
                    if (scriptFunctionsByName[data.identifierToken.identifier].Count == 1)
                    {
                        throw new ScriptParsingException(
                            source: new EOFToken(0, 0),
                            message: $"Expected Function: {data}  Found Function: {scriptFunctionsByName[data.identifierToken.identifier][0].functionSignature}");
                    }

                    throw new ScriptParsingException(
                        source: new EOFToken(0, 0),
                        message: $"Expected Function: {data}  Found Functions: [{string.Join(", ", scriptFunctionsByName[data.identifierToken.identifier].Select(x => x.functionSignature))}]");
                }
            }

            foreach (ScriptFunction scriptFunction in scriptFunctionsByName.Values.SelectMany(x => x))
            {
                scriptFunction.ParseFunctions(compilationContext);
            }
        }

        public bool HasFunction(Guid functionId) => scriptFunctionsById.ContainsKey(functionId);
        public bool HasFunction(string identifier) => scriptFunctionsByName.ContainsKey(identifier);
        public FunctionSignature GetFunctionSignature(string identifier, Type[] argumentTypes)
        {
            ArgumentData[] methodArguments = argumentTypes
                .Select(x => new ArgumentData("", x, ArgumentType.Standard))
                .ToArray();

            return scriptFunctionsByName[identifier].FirstOrDefault(x => x.functionSignature.MatchesArgs(methodArguments))?.functionSignature ??
                scriptFunctionsByName[identifier].Single(x => x.functionSignature.LooselyMatchesArgs(methodArguments)).functionSignature;
        }

        public IEnumerable<FunctionSignature> GetFunctionSignatures(string identifier) =>
            scriptFunctionsByName[identifier].Select(x => x.functionSignature);

        public bool HasFunction(FunctionSignature functionSignature)
        {
            string identifier = functionSignature.identifierToken.identifier;
            if (!scriptFunctionsByName.ContainsKey(identifier))
            {
                return false;
            }

            return scriptFunctionsByName[identifier].Any(x => x.functionSignature.Matches(functionSignature));
        }

        public void AddFunction(ScriptFunction scriptFunction)
        {
            List<ScriptFunction> functions;
            if (!scriptFunctionsByName.TryGetValue(scriptFunction.FunctionName, out functions!))
            {
                functions = new List<ScriptFunction>();
                scriptFunctionsByName.Add(scriptFunction.FunctionName, functions);
            }

            //Check for collision
            if (functions.Any(x => x.functionSignature.Matches(scriptFunction.functionSignature)))
            {
                throw new ScriptParsingException(
                    source: scriptFunction.functionSignature.identifierToken,
                    message: $"Two declarations of function {scriptFunction.FunctionName} found: {scriptFunction.functionSignature}");
            }

            functions.Add(scriptFunction);

            scriptFunctionsById.Add(scriptFunction.FunctionId, scriptFunction);
        }

        public ScriptFunction GetMatchingFunction(string functionName, object[] arguments)
        {
            if (!scriptFunctionsByName.TryGetValue(functionName, out List<ScriptFunction> functions))
            {
                return null;
            }

            ArgumentData[] methodArguments = arguments
                .Select(x => new ArgumentData("", x?.GetType() ?? typeof(object), ArgumentType.Standard))
                .ToArray();

            if (functions.FirstOrDefault(x => x.functionSignature.MatchesArgs(methodArguments)) is ScriptFunction exactMatch)
            {
                return exactMatch;
            }

            List<ScriptFunction> looselyMatchingFunctions = functions.Where(x => x.functionSignature.LooselyMatchesArgs(methodArguments)).ToList();

            if (looselyMatchingFunctions.Count > 1)
            {
                throw new ScriptRuntimeException($"Multiple functions named {functionName} matched the argument list [{string.Join(", ", methodArguments.Select(x => x.valueType.ToString()))}]");
            }

            return looselyMatchingFunctions.FirstOrDefault();
        }

        public void ParseNextGlobal(
            IEnumerator<Token> scriptTokens,
            ScriptCompilationContext context)
        {
            switch (scriptTokens.Current)
            {
                case KeywordToken kwToken:
                    //Valid operations:
                    //  Global declaration
                    //  Extern declaration
                    //  Member declaration
                    //  Class declaration
                    switch (kwToken.keyword)
                    {
                        case Keyword.Global:
                        case Keyword.Extern:
                            //Parse Global Declaration
                            {
                                scriptTokens.CautiousAdvance();

                                Type valueType = scriptTokens.ReadTypeAndAdvance();

                                IdentifierToken identToken = scriptTokens.GetTokenAndAdvance<IdentifierToken>();
                                IValueGetter initializerExpression = null;

                                if (scriptTokens.TestAndConditionallyAdvance(Operator.Assignment))
                                {
                                    initializerExpression = Expression.ParseNextGetterExpression(scriptTokens, context);
                                }

                                scriptTokens.AssertAndAdvance(Separator.Semicolon, false);

                                scriptDeclarations.Add(
                                    new GlobalDeclaration(
                                        identifierToken: identToken,
                                        valueType: valueType,
                                        isExtern: kwToken.keyword == Keyword.Extern,
                                        initializer: initializerExpression,
                                        context: context));
                            }
                            return;

                        case Keyword.Const:
                            {
                                scriptTokens.CautiousAdvance();

                                Type valueType = scriptTokens.ReadTypeAndAdvance();

                                IdentifierToken identToken = scriptTokens.GetTokenAndAdvance<IdentifierToken>();
                                scriptTokens.AssertAndAdvance(Operator.Assignment);
                                IValueGetter initializerExpression = Expression.ParseNextGetterExpression(scriptTokens, context);

                                scriptTokens.AssertAndAdvance(Separator.Semicolon, false);

                                if (initializerExpression is not LiteralToken litToken)
                                {
                                    throw new ScriptParsingException(
                                        source: kwToken,
                                        message: $"The value of Const declarations must be constant");
                                }

                                object value = litToken.GetAs<object>();

                                if (!valueType.IsAssignableFrom(litToken.GetValueType()))
                                {
                                    value = Convert.ChangeType(value, valueType);
                                }

                                context.DeclareConstant(
                                    identifierToken: identToken,
                                    type: valueType,
                                    value: value);
                            }
                            return;

                        default:
                            throw new ScriptParsingException(
                                source: kwToken,
                                message: $"Token not valid for global context: {kwToken}.");
                    }

                case TypeToken typeToken:
                    //Parse Function or Member Declaration
                    {
                        Type valueType = scriptTokens.ReadTypeAndAdvance();
                        IdentifierToken identToken = scriptTokens.GetTokenAndAdvance<IdentifierToken>();

                        if (scriptTokens.TestWithoutAdvancing(Separator.OpenParen))
                        {
                            ArgumentData[] arguments = ParseArgumentsDeclaration(scriptTokens);

                            AddFunction(new ScriptFunction(
                                functionTokens: scriptTokens,
                                functionSignature: new FunctionSignature(
                                    identifierToken: identToken,
                                    returnType: valueType,
                                    arguments: arguments),
                                context: context));
                        }
                        else
                        {
                            //Member declaration
                            if (valueType == typeof(void))
                            {
                                throw new ScriptParsingException(
                                    source: typeToken,
                                    message: $"Cannot declare a member of type Void");
                            }

                            IValueGetter initializerExpression = null;
                            if (scriptTokens.TestAndConditionallyAdvance(Operator.Assignment))
                            {
                                initializerExpression = Expression.ParseNextGetterExpression(scriptTokens, context);
                            }

                            scriptTokens.AssertAndAdvance(Separator.Semicolon, false);

                            scriptDeclarations.Add(
                                new MemberDeclaration(
                                    identifierToken: identToken,
                                    valueType: valueType,
                                    initializer: initializerExpression,
                                    context: context));
                        }
                    }

                    break;

                default:
                    throw new ScriptParsingException(
                        source: scriptTokens.Current,
                        message: $"Token not valid for global context: {scriptTokens.Current}.");

            }
        }

        public IEnumerable<KeyInfo> GetDeclarations()
        {
            foreach (ScriptDeclaration decl in scriptDeclarations)
            {
                if (decl is GlobalDeclaration globalDecl && !globalDecl.IsExtern)
                {
                    yield return globalDecl.KeyInfo;
                }
            }
        }

        public IEnumerable<KeyInfo> GetDependencies()
        {
            foreach (ScriptDeclaration decl in scriptDeclarations)
            {
                if (decl is GlobalDeclaration globalDecl && globalDecl.IsExtern)
                {
                    yield return globalDecl.KeyInfo;
                }
            }
        }

        /// <summary>
        /// Creates declared variables in the script context
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="ScriptRuntimeException"></exception>
        public ScriptRuntimeContext PrepareScript(GlobalRuntimeContext context)
        {
            ScriptRuntimeContext scriptContext = new ScriptRuntimeContext(context, this);

            foreach (ScriptDeclaration declaration in scriptDeclarations)
            {
                declaration.Execute(scriptContext);
            }

            return scriptContext;
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        public void ExecuteFunction(
            string functionName,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = GetMatchingFunction(functionName, arguments);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            matchingFunction.Execute(context, CancellationToken.None, arguments);
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        public void ExecuteFunction(
            Guid functionId,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = scriptFunctionsById.GetValueOrDefault(functionId);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function Id {functionId}.");
            }

            matchingFunction.Execute(context, CancellationToken.None, arguments);
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        public T ExecuteFunction<T>(
            string functionName,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = GetMatchingFunction(functionName, arguments);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            matchingFunction.Execute(context, CancellationToken.None, arguments);

            return context.PopReturnValue<T>();
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        public T ExecuteFunction<T>(
            Guid functionId,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = scriptFunctionsById.GetValueOrDefault(functionId);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function Id {functionId}.");
            }

            matchingFunction.Execute(context, CancellationToken.None, arguments);

            return context.PopReturnValue<T>();
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public T ExecuteFunction<T>(
            string functionName,
            int timeoutMS,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = GetMatchingFunction(functionName, arguments);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            using CancellationTokenSource tokenSource = new CancellationTokenSource(timeoutMS);

            matchingFunction.Execute(context, tokenSource.Token, arguments);

            return context.PopReturnValue<T>();
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public Task ExecuteFunctionAsync(
            string functionName,
            int timeoutMS,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = GetMatchingFunction(functionName, arguments);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            return Task.Run(() =>
            {
                using CancellationTokenSource tokenSource = new CancellationTokenSource(timeoutMS);
                matchingFunction.Execute(context, tokenSource.Token, arguments);
            });
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        public Task<T> ExecuteFunctionAsync<T>(
            string functionName,
            int timeoutMS,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = GetMatchingFunction(functionName, arguments);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            return Task.Run(() =>
            {
                using CancellationTokenSource tokenSource = new CancellationTokenSource(timeoutMS);
                matchingFunction.Execute(context, tokenSource.Token, arguments);
                return context.PopReturnValue<T>();
            });
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
#pragma warning disable CA1068 // CancellationToken parameters must come last
        public Task<T> ExecuteFunctionAsync<T>(
#pragma warning restore CA1068 // CancellationToken parameters must come last
            string functionName,
            int timeoutMS,
            CancellationToken ct,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = GetMatchingFunction(functionName, arguments);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            using CancellationTokenSource tokenSource = new CancellationTokenSource(timeoutMS);
            using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, ct);

            return Task.Run(() =>
            {
                matchingFunction.Execute(context, linkedSource.Token, arguments);
                return context.PopReturnValue<T>();
            },
            linkedSource.Token);
        }

        /// <summary>
        /// Executes the named function
        /// </summary>
        /// <exception cref="ScriptRuntimeException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
#pragma warning disable CA1068 // CancellationToken parameters must come last
        public Task ExecuteFunctionAsync(
#pragma warning restore CA1068 // CancellationToken parameters must come last
            string functionName,
            int timeoutMS,
            CancellationToken ct,
            ScriptRuntimeContext context,
            params object[] arguments)
        {
            ScriptFunction matchingFunction = GetMatchingFunction(functionName, arguments);

            if (matchingFunction is null)
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            using CancellationTokenSource tokenSource = new CancellationTokenSource(timeoutMS);
            using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, ct);

            return Task.Run(() =>
            {
                matchingFunction.Execute(context, linkedSource.Token, arguments);
            },
            linkedSource.Token);
        }

        private static ArgumentData[] ParseArgumentsDeclaration(IEnumerator<Token> tokens)
        {
            List<ArgumentData> arguments = new List<ArgumentData>();
            tokens.AssertAndAdvance(Separator.OpenParen);

            if (!tokens.TestWithoutAdvancing(Separator.CloseParen))
            {
                do
                {
                    ArgumentType argumentDeclarationType = tokens.GetArgumentType();
                    Type argumentValueType = tokens.ReadTypeAndAdvance();
                    IdentifierToken identToken = tokens.GetTokenAndAdvance<IdentifierToken>();

                    if (argumentDeclarationType == ArgumentType.Params && !argumentValueType.IsArray)
                    {
                        throw new ScriptParsingException(
                            identToken,
                            $"params argument must be an array. Instead found: {argumentValueType}");
                    }

                    arguments.Add(new ArgumentData(identToken, argumentValueType, argumentDeclarationType));
                }
                while (tokens.TestAndConditionallyAdvance(Separator.Comma));
            }

            tokens.AssertAndAdvance(Separator.CloseParen);

            return arguments.ToArray();
        }
    }
}