using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class DeclarationOperation : Statement
    {
        private readonly string identifier;
        private readonly Type valueType;

        public DeclarationOperation(
            IdentifierToken identifierToken,
            Type valueType,
            CompilationContext context)
        {
            //Try to declare and throw exceptions if invalid
            context.DeclareVariable(identifierToken, valueType);

            identifier = identifierToken.identifier;
            this.valueType = valueType;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            if (context.VariableExists(identifier))
            {
                //You cannot declare a local variable to shadow an existing global
                throw new ScriptRuntimeException($"Variable already declared in this context: {identifier}");
            }

            object defaultValue = valueType.GetDefaultValue();

            context.DeclareVariable(identifier, valueType, defaultValue);

            return FlowState.Nominal;
        }
    }
}
