namespace BGC.Scripting
{
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

        protected override bool ControlKeywordValid() => loopContext || base.ControlKeywordValid();
    }
}