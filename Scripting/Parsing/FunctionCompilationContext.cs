using System;

namespace BGC.Scripting
{
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
            foreach (ArgumentData data in functionSignature.arguments)
            {
                DeclareVariable(data.identifierToken, data.valueType);
            }
        }

        public override Type GetReturnType() => functionSignature.returnType;
        protected override bool ControlKeywordValid() => false;
    }
}