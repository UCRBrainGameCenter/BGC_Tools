using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    /// <summary>
    /// This class is used to track variable declaration and usage for each scope during compilation
    /// </summary>
    public abstract class CompilationContext
    {
        protected readonly CompilationContext parent;
        protected readonly Dictionary<string, VariableData> valueDictionary = new Dictionary<string, VariableData>();
        protected readonly Dictionary<string, ConstantData> constantDictionary = new Dictionary<string, ConstantData>();

        private ScriptCompilationContext scriptContext = null;
        protected virtual ScriptCompilationContext ScriptContext => scriptContext ??= parent?.ScriptContext;

        public enum IdentifierType
        {
            Unidentified = 0,
            Constant,
            Variable,
            Function,
            MAX
        }

        public CompilationContext(CompilationContext parent)
        {
            this.parent = parent;
        }

        public virtual ScopeCompilationContext CreateChildScope(bool loopContext = false) =>
            new ScopeCompilationContext(this, loopContext);

        public IdentifierType GetIdentifierType(string identifier)
        {
            if (HasExistingConstant(identifier))
            {
                return IdentifierType.Constant;
            }

            if (HasExistingValue(identifier))
            {
                return IdentifierType.Variable;
            }

            if (HasExistingFunction(identifier))
            {
                return IdentifierType.Function;
            }

            return IdentifierType.Unidentified;
        }

        public Type GetValueType(string key)
        {
            if (valueDictionary.ContainsKey(key))
            {
                return valueDictionary[key].valueType;
            }

            if (parent is null)
            {
                return null;
            }

            return parent.GetValueType(key);
        }

        public Type GetConstantType(string key)
        {
            if (constantDictionary.ContainsKey(key))
            {
                return constantDictionary[key].valueType;
            }

            if (parent is null)
            {
                return null;
            }

            return parent.GetConstantType(key);
        }

        public object GetConstantValue(string key)
        {
            if (constantDictionary.ContainsKey(key))
            {
                return constantDictionary[key].value;
            }

            if (parent is null)
            {
                return null;
            }

            return parent.GetConstantValue(key);
        }

        public virtual FunctionSignature? GetMatchingFunctionSignature(IdentifierToken identToken, InvocationArgument[] arguments) =>
            ScriptContext!.GetMatchingFunctionSignature(identToken, arguments);

        public void DeclareConstant(
            IdentifierToken identifierToken,
            Type type,
            object value)
        {
            if (HasExistingConstant(identifierToken.identifier))
            {
                IdentifierToken originalToken = GetExistingConstantIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Constant {identifierToken.identifier} already defined in context, on line {originalToken?.line} column {originalToken?.column}.");
            }

            if (HasExistingValue(identifierToken.identifier))
            {
                IdentifierToken originalToken = GetExistingValueIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Constant {identifierToken.identifier} already defined in context as variable, on line {originalToken?.line} column {originalToken?.column}.");
            }

            if (HasExistingFunction(identifierToken.identifier))
            {
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Constant {identifierToken.identifier} already defined in context as function.");
            }

            constantDictionary.Add(
                key: identifierToken.identifier,
                value: new ConstantData(
                    identifierToken: identifierToken,
                    valueType: type,
                    value: value));
        }

        public void DeclareVariable(
            IdentifierToken identifierToken,
            Type type)
        {
            if (HasExistingConstant(identifierToken.identifier))
            {
                IdentifierToken originalToken = GetExistingConstantIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Variable {identifierToken.identifier} already defined in context as Constant, on line {originalToken?.line} column {originalToken?.column}.");
            }

            if (HasExistingValue(identifierToken.identifier))
            {
                IdentifierToken originalToken = GetExistingValueIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Variable {identifierToken.identifier} already defined in context, on line {originalToken?.line} column {originalToken?.column}.");
            }

            if (HasExistingFunction(identifierToken.identifier))
            {
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Variable {identifierToken.identifier} already defined in context as function.");
            }

            valueDictionary.Add(
                key: identifierToken.identifier,
                value: new VariableData(
                    identifierToken: identifierToken,
                    valueType: type));
        }

        protected bool HasExistingConstant(string identifier)
        {
            if (constantDictionary.ContainsKey(identifier))
            {
                return true;
            }

            if (parent is null)
            {
                return false;
            }

            return parent.HasExistingConstant(identifier);
        }

        protected IdentifierToken GetExistingConstantIdentifier(string identifier)
        {
            if (constantDictionary.ContainsKey(identifier))
            {
                return constantDictionary[identifier].identifierToken;
            }

            if (parent is null)
            {
                return null;
            }

            return parent.GetExistingConstantIdentifier(identifier);
        }

        protected bool HasExistingValue(string identifier)
        {
            if (valueDictionary.ContainsKey(identifier))
            {
                return true;
            }

            if (parent is null)
            {
                return false;
            }

            return parent.HasExistingValue(identifier);
        }

        protected IdentifierToken GetExistingValueIdentifier(string identifier)
        {
            if (valueDictionary.ContainsKey(identifier))
            {
                return valueDictionary[identifier].identifierToken;
            }

            if (parent is null)
            {
                return null;
            }

            return parent.GetExistingValueIdentifier(identifier);
        }

        protected virtual bool HasExistingFunction(string identifier) =>
            ScriptContext!.HasExistingFunction(identifier);

        protected virtual IdentifierToken GetExistingFunctionIdentifier(string identifier) =>
            ScriptContext?.GetExistingFunctionIdentifier(identifier);

        protected virtual bool ControlKeywordValid() => parent!.ControlKeywordValid();
        public virtual Type GetReturnType() => parent!.GetReturnType();

        public void ValidateReturn(KeywordToken returnKeyword, Type returnType)
        {
            if (!GetReturnType().AssignableOrConvertableFromType(returnType))
            {
                throw new ScriptParsingException(
                    source: returnKeyword,
                    message: $"Unable to return value.  Expected type {GetReturnType().Name}, received type {returnType.Name}.");
            }
        }

        public void ValidateFlowControlKeyword(KeywordToken controlKeyword)
        {
            if (!ControlKeywordValid())
            {
                throw new ScriptParsingException(
                    source: controlKeyword,
                    message: $"Unable to use control keyword {controlKeyword.keyword} here.  No Loop present in parent contexts.");
            }
        }
    }
}