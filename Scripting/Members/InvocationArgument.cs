namespace BGC.Scripting
{
    public struct InvocationArgument
    {
        public readonly IExpression expression;
        public readonly ArgumentType argumentType;

        public InvocationArgument(
            IExpression expression,
            ArgumentType argumentType)
        {
            this.expression = expression;
            this.argumentType = argumentType;
        }
    }
}