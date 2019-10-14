using System;

namespace BGC.Scripting
{
    public class MemberDeclaration : ScriptDeclaration
    {
        public MemberDeclaration(
            IdentifierToken identifierToken,
            Type valueType,
            IValueGetter initializer,
            CompilationContext context)
            : base(identifierToken, valueType, initializer, context)
        {
        }

        public override void Execute(ScriptRuntimeContext context)
        {
            if (context.VariableExists(identifier))
            {
                //You cannot declare a local variable to shadow an existing global
                throw new ScriptRuntimeException($"Variable already declared in this context: {identifier}");
            }

            object defaultValue;

            if (initializer == null)
            {
                defaultValue = valueType.GetDefaultValue();
            }
            else
            {
                defaultValue = initializer.GetAs<object>(context);
            }

            context.DeclareVariable(identifier, valueType, defaultValue);
        }
    }
}
