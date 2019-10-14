using BGC.Users;

namespace BGC.Scripting
{
    public class ClearDataOperation : Statement
    {
        private readonly IValueGetter keyArg;

        public ClearDataOperation(
            IValueGetter keyArg,
            Token source)
        {
            if (keyArg.GetValueType() != typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Key Argument {keyArg} of User.ClearData is not a string: type {keyArg.GetValueType().Name}");
            }

            this.keyArg = keyArg;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            PlayerData.RemoveKey(keyArg.GetAs<string>(context));
            return FlowState.Nominal;
        }
    }
}
