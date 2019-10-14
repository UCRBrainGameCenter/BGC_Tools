using System;

namespace BGC.Scripting
{
    public abstract class ScriptDeclaration
    {
        protected readonly string identifier;
        protected readonly Type valueType;
        protected readonly IValueGetter initializer;

        public ScriptDeclaration(
            IdentifierToken identifierToken,
            Type valueType,
            IValueGetter initializer,
            CompilationContext context)
        {
            identifier = identifierToken.identifier;
            this.valueType = valueType;
            this.initializer = initializer;

            //Try to declare and throw exceptions if invalid
            context.DeclareVariable(identifierToken, valueType);

            //Check initializer type
            if (initializer != null && !valueType.AssignableFromType(initializer.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Incompatible type in declaration.  Expected type {valueType.Name}, Received {initializer.GetValueType().Name}");
            }
        }

        public abstract void Execute(ScriptRuntimeContext context);

    }
}
