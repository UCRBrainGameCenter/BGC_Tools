using System;
using System.Collections;

namespace BGC.Scripting
{
    public class AddToReportMethod : Statement
    {
        private readonly IValueGetter headerArg;
        private readonly IValueGetter valueArg;

        public AddToReportMethod(
            IValueGetter headerArg,
            IValueGetter valueArg,
            Token source)
        {

            if (headerArg.GetValueType() != typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Header Argument {headerArg} of User.AddToReport is not a string: type {headerArg.GetValueType().Name}");
            }

            if (!typeof(string).AssignableFromType(valueArg.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Value Argument {valueArg} of User.AddToReport is type string and not assignable to type {valueArg.GetValueType().Name}");
            }

            this.headerArg = headerArg;
            this.valueArg = valueArg;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            context.AddToReport(headerArg.GetAs<string>(context), valueArg.GetAs<string>(context));

            return FlowState.Nominal;
        }
    }
}
