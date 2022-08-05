using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Scripting
{
    public class ScriptCompilationContext : CompilationContext
    {
        protected readonly Dictionary<string, List<FunctionSignature>> functionDictionary = new Dictionary<string, List<FunctionSignature>>();

        protected override ScriptCompilationContext ScriptContext => this;

        public ScriptCompilationContext()
            : base(null)
        {

        }

        public override FunctionSignature? GetMatchingFunctionSignature(
            IdentifierToken identToken,
            InvocationArgument[] arguments)
        {
            if (!functionDictionary.TryGetValue(identToken.identifier, out List<FunctionSignature> functionSignatures))
            {
                return null;
            }

            if (functionSignatures.Any(x => x.MatchesArgs(arguments)))
            {
                return functionSignatures.Single(x => x.MatchesArgs(arguments));
            }

            int looseCount = functionSignatures.Count(x => x.LooselyMatchesArgs(arguments));

            if (looseCount > 1)
            {
                throw new ScriptParsingException(identToken,
                    $"Ambiguous function invocation. " +
                    $"Argument type list [{string.Join(", ", arguments.Select(x => x.expression))}] exactly matches no function " +
                    $"and loosely matches {looseCount} functions.");
            }

            return functionSignatures.FirstOrDefault(x => x.LooselyMatchesArgs(arguments));
        }

        protected override bool HasExistingFunction(string identifier) =>
            functionDictionary.ContainsKey(identifier);

        public void DeclareFunction(in FunctionSignature functionSignature)
        {
            if (HasExistingConstant(functionSignature.identifierToken.identifier))
            {
                IdentifierToken constantToken = GetExistingConstantIdentifier(functionSignature.identifierToken.identifier);
                throw new ScriptParsingException(
                    source: functionSignature.identifierToken,
                    message: $"Function {functionSignature.identifierToken.identifier} already defined in context as constant, on line {constantToken?.line} column {constantToken?.column}.");
            }

            if (HasExistingValue(functionSignature.identifierToken.identifier))
            {
                IdentifierToken memberToken = GetExistingValueIdentifier(functionSignature.identifierToken.identifier);
                throw new ScriptParsingException(
                    source: functionSignature.identifierToken,
                    message: $"Function {functionSignature.identifierToken.identifier} already defined in context as variable, on line {memberToken?.line} column {memberToken?.column}.");
            }

            List<FunctionSignature> functions;
            if (!functionDictionary.TryGetValue(functionSignature.identifierToken.identifier, out functions!))
            {
                functions = new List<FunctionSignature>();
                functionDictionary.Add(functionSignature.identifierToken.identifier, functions);
            }

            foreach (FunctionSignature function in functions)
            {
                if (function.Matches(functionSignature))
                {
                    throw new ScriptParsingException(
                        source: function.identifierToken,
                        message: $"Function {functionSignature.identifierToken.identifier} already defined with same argument list, on line {function.identifierToken.line} column {function.identifierToken.column}.");
                }
            }

            functions.Add(functionSignature);
        }


        public FunctionCompilationContext CreateFunction(in FunctionSignature functionSignature) =>
            new FunctionCompilationContext(this, functionSignature);

        public override ScopeCompilationContext CreateChildScope(bool loopContext = false) =>
            throw new NotSupportedException($"Global Contexts do not have scopes.");

        public override Type GetReturnType() =>
            throw new NotSupportedException($"Global Contexts do not have a return type.");
        protected override bool ControlKeywordValid() =>
            throw new NotSupportedException($"Global Contexts do not support Control Keywords.");
    }
}