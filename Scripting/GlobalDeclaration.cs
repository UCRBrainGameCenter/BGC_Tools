using System;

namespace BGC.Scripting
{
    public class GlobalDeclaration : ScriptDeclaration
    {
        public bool IsExtern { get; }

        public GlobalDeclaration(
            IdentifierToken identifierToken,
            Type valueType,
            bool isExtern,
            IValueGetter initializer,
            CompilationContext context)
            : base(identifierToken, valueType, initializer, context)
        {
            IsExtern = isExtern;

            //Check initializer type
            if (initializer != null && IsExtern)
            {
                throw new ScriptParsingException(
                    source: identifierToken,
                    message: $"Invalid declaration.  Extern declarations cannot have initializations.");
            }
        }

        public override void Execute(ScriptRuntimeContext context)
        {
            if (context.VariableExists(identifier))
            {
                throw new ScriptRuntimeException($"Variable already declared in this context: {identifier}");
            }

            if (context.GlobalVariableExists(identifier))
            {
                //Adding an indirect reference to the existing variable
                context.DeclareExistingGlobal(identifier, valueType);

                //Not running initialization because it exists
                return;
            }

            if (IsExtern)
            {
                throw new ScriptRuntimeException($"Extern Variable does not already exist: {identifier}");
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

            context.DeclareNewGlobal(identifier, valueType, defaultValue);
        }

        public KeyInfo KeyInfo => new KeyInfo(valueType, identifier);
    }
}
