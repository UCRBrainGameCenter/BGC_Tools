using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class Script
    {
        private readonly List<ScriptDeclaration> scriptDeclarations = new List<ScriptDeclaration>();
        private readonly Dictionary<string, ScriptFunction> scriptFunctions = new Dictionary<string, ScriptFunction>();

        /// <summary>
        /// Parse Script
        /// </summary>
        /// <exception cref="ScriptParsingException"></exception>
        public Script(
            IEnumerator<Token> scriptTokens,
            params FunctionSignature[] expectedFunctions)
        {
            ScriptCompilationContext compilationContext = new ScriptCompilationContext();

            while (!(scriptTokens.Current is EOFToken))
            {
                ParseNextGlobal(scriptTokens, compilationContext);
            }

            foreach (FunctionSignature data in expectedFunctions)
            {
                if (!scriptFunctions.ContainsKey(data.identifierToken.identifier))
                {
                    throw new ScriptParsingException(
                        source: scriptTokens.Current ?? new EOFToken(0, 0),
                        message: $"Expected Function not found: {data}");
                }

                if (!data.Matches(scriptFunctions[data.identifierToken.identifier].functionSignature))
                {
                    throw new ScriptParsingException(
                        source: scriptFunctions[data.identifierToken.identifier].functionSignature.identifierToken,
                        message: $"Expected Function: {data}  Found Function: {scriptFunctions[data.identifierToken.identifier].functionSignature}");
                }
            }

            foreach (ScriptFunction scriptFunction in scriptFunctions.Values)
            {
                scriptFunction.ParseFunctions(compilationContext);
            }
        }

        public bool HasFunction(string identifier) => scriptFunctions.ContainsKey(identifier);
        public FunctionSignature GetFunctionSignature(string identifier) => scriptFunctions[identifier].functionSignature;

        public bool HasFunction(FunctionSignature functionSignature)
        {
            string identifier = functionSignature.identifierToken.identifier;
            if (!scriptFunctions.ContainsKey(identifier))
            {
                return false;
            }

            return functionSignature.Matches(scriptFunctions[identifier].functionSignature);
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

                                Type valueType = scriptTokens.ReadType();

                                IdentifierToken identToken = scriptTokens.GetTokenAndAdvance<IdentifierToken>();
                                IValueGetter initializerExpression = null;

                                if (scriptTokens.TestAndConditionallySkip(Operator.Assignment))
                                {
                                    initializerExpression = Expression.ParseNextGetterExpression(scriptTokens, context);
                                }

                                scriptTokens.AssertAndSkip(Separator.Semicolon, false);

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

                                Type valueType = scriptTokens.ReadType();

                                IdentifierToken identToken = scriptTokens.GetTokenAndAdvance<IdentifierToken>();
                                scriptTokens.AssertAndSkip(Operator.Assignment);
                                IValueGetter initializerExpression = Expression.ParseNextGetterExpression(scriptTokens, context);

                                scriptTokens.AssertAndSkip(Separator.Semicolon, false);

                                if (!(initializerExpression is LiteralToken litToken))
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

                        case Keyword.Void:
                        case Keyword.Bool:
                        case Keyword.Double:
                        case Keyword.Integer:
                        case Keyword.String:
                        case Keyword.List:
                        case Keyword.Queue:
                        case Keyword.Stack:
                        case Keyword.DepletableBag:
                        case Keyword.DepletableList:
                        case Keyword.RingBuffer:
                        case Keyword.Dictionary:
                        case Keyword.HashSet:
                        case Keyword.Random:
                        case Keyword.DataFile:
                        case Keyword.IScriptedAlgorithmQuerier:
                        case Keyword.IMultiParamScriptedAlgorithmQuerier:
                            //Parse Function or Member Declaration
                            {
                                Type valueType = scriptTokens.ReadType();
                                IdentifierToken identToken = scriptTokens.GetTokenAndAdvance<IdentifierToken>();

                                if (scriptTokens.TestWithoutSkipping(Separator.OpenParen))
                                {
                                    VariableData[] arguments = ParseArgumentsDeclaration(scriptTokens);

                                    if (scriptFunctions.ContainsKey(identToken.identifier))
                                    {
                                        throw new ScriptParsingException(
                                            source: identToken,
                                            message: $"Two declarations of function {identToken.identifier} found.");
                                    }

                                    scriptFunctions.Add(identToken.identifier,
                                        new ScriptFunction(
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
                                    if (kwToken.keyword == Keyword.Void)
                                    {
                                        throw new ScriptParsingException(
                                            source: kwToken,
                                            message: $"Cannot declare a member of type Void");
                                    }

                                    IValueGetter initializerExpression = null;
                                    if (scriptTokens.TestAndConditionallySkip(Operator.Assignment))
                                    {
                                        initializerExpression = Expression.ParseNextGetterExpression(scriptTokens, context);
                                    }

                                    scriptTokens.AssertAndSkip(Separator.Semicolon, false);

                                    scriptDeclarations.Add(
                                        new MemberDeclaration(
                                            identifierToken: identToken,
                                            valueType: valueType,
                                            initializer: initializerExpression,
                                            context: context));
                                }
                            }
                            return;

                        default:
                            throw new ScriptParsingException(
                                source: kwToken,
                                message: $"Token not valid for global context: {kwToken}.");
                    }

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
            if (!scriptFunctions.ContainsKey(functionName))
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            scriptFunctions[functionName].Execute(context, arguments);
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
            if (!scriptFunctions.ContainsKey(functionName))
            {
                throw new ScriptRuntimeException($"Unable to find function {functionName} for external invocation.");
            }

            scriptFunctions[functionName].Execute(context, arguments);

            return context.PopReturnValue<T>();
        }

        private static VariableData[] ParseArgumentsDeclaration(IEnumerator<Token> tokens)
        {
            List<VariableData> arguments = new List<VariableData>();
            tokens.AssertAndSkip(Separator.OpenParen);

            if (!tokens.TestWithoutSkipping(Separator.CloseParen))
            {
                do
                {
                    Type argumentType = tokens.ReadType();
                    IdentifierToken identToken = tokens.GetTokenAndAdvance<IdentifierToken>();

                    arguments.Add(new VariableData(identToken, argumentType));
                }
                while (tokens.TestAndConditionallySkip(Separator.Comma));
            }

            tokens.AssertAndSkip(Separator.CloseParen);

            return arguments.ToArray();
        }
    }
}
