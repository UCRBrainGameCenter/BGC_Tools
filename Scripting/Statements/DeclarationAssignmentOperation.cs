using System;
using System.Collections.Generic;

namespace BGC.Scripting
{
    public class DeclarationAssignmentOperation : Statement
    {
        private readonly string identifier;
        private readonly Type valueType;
        private readonly IValueGetter initializer;

        public static DeclarationAssignmentOperation CreateDelcaration(
            IdentifierToken identifierToken,
            Type valueType,
            IValueGetter initializer,
            bool isConstant,
            CompilationContext context)
        {
            //Check initializer type
            if (!valueType.AssignableFromType(initializer.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Incompatible type in declaration.  Expected type {valueType.Name}, Received {initializer.GetValueType().Name}");
            }

            if (isConstant)
            {
                if (initializer is LiteralToken litToken)
                {
                    object value = litToken.GetAs<object>();

                    if (!valueType.IsAssignableFrom(initializer.GetValueType()))
                    {
                        value = Convert.ChangeType(value, valueType);
                    }

                    context.DeclareConstant(
                        identifierToken: identifierToken,
                        type: valueType,
                        value: value);

                    //Const declaration can't have sideeffects, thus no statement is generated
                    return null;
                }

                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Constants cannot be initialized with non-constant values.");
            }

            //Try to declare and throw exceptions if invalid
            context.DeclareVariable(identifierToken, valueType);

            return new DeclarationAssignmentOperation(
                identifier: identifierToken.identifier,
                valueType: valueType,
                initializer: initializer);
        }


        private DeclarationAssignmentOperation(
            string identifier,
            Type valueType,
            IValueGetter initializer)
        {
            this.identifier = identifier;
            this.valueType = valueType;
            this.initializer = initializer;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            if (context.VariableExists(identifier))
            {
                //You cannot declare a local variable to shadow an existing global
                throw new ScriptRuntimeException($"Variable already declared in this context: {identifier}");
            }

            object defaultValue = initializer.GetAs<object>(context);

            context.DeclareVariable(identifier, valueType, defaultValue);

            return FlowState.Nominal;
        }
    }
}
