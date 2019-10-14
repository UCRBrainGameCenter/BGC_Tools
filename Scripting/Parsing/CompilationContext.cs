using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;

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
        protected virtual ScriptCompilationContext ScriptContext =>
            scriptContext ?? (scriptContext = parent.ScriptContext);

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

        public virtual FunctionSignature GetFunctionSignature(string key) =>
            ScriptContext.GetFunctionSignature(key);

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
                    message: $"Constant {identifierToken.identifier} already defined in context, on line {originalToken.line} column {originalToken.column}.");
            }

            if (HasExistingValue(identifierToken.identifier))
            {
                IdentifierToken originalToken = GetExistingValueIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Constant {identifierToken.identifier} already defined in context as variable, on line {originalToken.line} column {originalToken.column}.");
            }

            if (HasExistingFunction(identifierToken.identifier))
            {
                IdentifierToken functionToken = GetExistingFunctionIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Constant {identifierToken.identifier} already defined in context as function, on line {functionToken.line} column {functionToken.column}.");
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
                    message: $"Variable {identifierToken.identifier} already defined in context as Constant, on line {originalToken.line} column {originalToken.column}.");
            }

            if (HasExistingValue(identifierToken.identifier))
            {
                IdentifierToken originalToken = GetExistingValueIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Variable {identifierToken.identifier} already defined in context, on line {originalToken.line} column {originalToken.column}.");
            }

            if (HasExistingFunction(identifierToken.identifier))
            {
                IdentifierToken functionToken = GetExistingFunctionIdentifier(identifierToken.identifier);
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Variable {identifierToken.identifier} already defined in context as function, on line {functionToken.line} column {functionToken.column}.");
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
            ScriptContext.HasExistingFunction(identifier);

        protected virtual IdentifierToken GetExistingFunctionIdentifier(string identifier) =>
            ScriptContext.GetExistingFunctionIdentifier(identifier);

        protected virtual bool ControlKeywordValid() => parent.ControlKeywordValid();
        public virtual Type GetReturnType() => parent.GetReturnType();

        public void ValidateReturn(KeywordToken returnKeyword, Type returnType)
        {
            if (!GetReturnType().AssignableFromType(returnType))
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

    public class ScriptCompilationContext : CompilationContext
    {
        protected readonly Dictionary<string, FunctionSignature> functionDictionary = new Dictionary<string, FunctionSignature>();

        protected override ScriptCompilationContext ScriptContext => this;

        public ScriptCompilationContext()
            : base(null)
        {

        }

        protected override IdentifierToken GetExistingFunctionIdentifier(string identifier)
        {
            if (functionDictionary.ContainsKey(identifier))
            {
                return functionDictionary[identifier].identifierToken;
            }

            return null;
        }

        public override FunctionSignature GetFunctionSignature(string key)
        {
            if (functionDictionary.ContainsKey(key))
            {
                return functionDictionary[key];
            }

            throw new Exception("Failed to check if function signature existed first");
        }

        protected override bool HasExistingFunction(string identifier) =>
            functionDictionary.ContainsKey(identifier);

        public void DeclareFunction(in FunctionSignature functionSignature)
        {
            if (HasExistingFunction(functionSignature.identifierToken.identifier))
            {
                IdentifierToken originalToken = GetExistingFunctionIdentifier(functionSignature.identifierToken.identifier);
                throw new ScriptParsingException(
                    source: functionSignature.identifierToken,
                    message: $"Function {functionSignature.identifierToken.identifier} already defined in context, on line {originalToken.line} column {originalToken.column}.");
            }

            if (HasExistingConstant(functionSignature.identifierToken.identifier))
            {
                IdentifierToken constantToken = GetExistingConstantIdentifier(functionSignature.identifierToken.identifier);
                throw new ScriptParsingException(
                    source: functionSignature.identifierToken,
                    message: $"Function {functionSignature.identifierToken.identifier} already defined in context as constant, on line {constantToken.line} column {constantToken.column}.");
            }

            if (HasExistingValue(functionSignature.identifierToken.identifier))
            {
                IdentifierToken memberToken = GetExistingValueIdentifier(functionSignature.identifierToken.identifier);
                throw new ScriptParsingException(
                    source: functionSignature.identifierToken,
                    message: $"Function {functionSignature.identifierToken.identifier} already defined in context as variable, on line {memberToken.line} column {memberToken.column}.");
            }

            functionDictionary.Add(functionSignature.identifierToken.identifier, functionSignature);
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

    public class FunctionCompilationContext : CompilationContext
    {
        private readonly FunctionSignature functionSignature;

        public FunctionCompilationContext(
            ScriptCompilationContext globalContext,
            in FunctionSignature functionSignature)
            : base(globalContext)
        {
            this.functionSignature = functionSignature;

            DeclareArguments();
        }

        private void DeclareArguments()
        {
            foreach (VariableData data in functionSignature.arguments)
            {
                DeclareVariable(data.identifierToken, data.valueType);
            }
        }

        public override Type GetReturnType() => functionSignature.returnType;
        protected override bool ControlKeywordValid() => false;
    }

    public class ScopeCompilationContext : CompilationContext
    {
        private readonly bool loopContext;

        public ScopeCompilationContext(
            CompilationContext parent,
            bool loopContext)
            : base(parent)
        {
            this.loopContext = loopContext;
        }

        protected override bool ControlKeywordValid() =>
            loopContext ? true : base.ControlKeywordValid();
    }

    public readonly struct VariableData
    {
        public readonly IdentifierToken identifierToken;
        public readonly Type valueType;

        public VariableData(
            IdentifierToken identifierToken,
            Type valueType)
        {
            this.identifierToken = identifierToken;
            this.valueType = valueType;
        }

        public VariableData(
            string identifier,
            Type valueType)
        {
            identifierToken = new IdentifierToken(0, 0, identifier);
            this.valueType = valueType;
        }

        public bool Matches(in VariableData other) =>
            identifierToken.identifier == other.identifierToken.identifier &&
            valueType == other.valueType;

        public bool MatchesType(in VariableData other) =>
            valueType == other.valueType;

        public override string ToString() => $"{valueType.Name} {identifierToken.identifier}";
    }

    public readonly struct ConstantData
    {
        public readonly IdentifierToken identifierToken;
        public readonly Type valueType;
        public readonly object value;

        public ConstantData(
            IdentifierToken identifierToken,
            Type valueType,
            object value)
        {
            this.identifierToken = identifierToken;
            this.valueType = valueType;
            this.value = value;
        }

        public bool MatchesType(in VariableData other) =>
            valueType == other.valueType;

        public override string ToString() => $"{valueType.Name} {identifierToken.identifier}";
    }

    public readonly struct FunctionSignature
    {
        public readonly IdentifierToken identifierToken;
        public readonly Type returnType;
        public readonly VariableData[] arguments;

        public FunctionSignature(
            IdentifierToken identifierToken,
            Type returnType,
            in VariableData[] arguments)
        {
            this.identifierToken = identifierToken;
            this.returnType = returnType ?? typeof(void);
            this.arguments = arguments ?? new VariableData[0];
        }

        public FunctionSignature(
            string identifier,
            Type returnType,
            params VariableData[] arguments)
        {
            identifierToken = new IdentifierToken(0, 0, identifier);
            this.returnType = returnType ?? typeof(void);
            this.arguments = arguments ?? new VariableData[0];
        }

        public bool Matches(in FunctionSignature other)
        {
            if (identifierToken.identifier != other.identifierToken.identifier ||
                returnType != other.returnType)
            {
                return false;
            }

            if (arguments.Length != other.arguments.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].MatchesType(other.arguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() =>
            $"{returnType.Name} {identifierToken.identifier}({string.Join(", ", arguments.Select(x => x.ToString()))})";
    }
}
