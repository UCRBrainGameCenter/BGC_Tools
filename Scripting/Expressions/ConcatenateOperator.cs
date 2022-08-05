using System;

namespace BGC.Scripting
{
    public class ConcatenateOperator : IValueGetter
    {
        private readonly IValueGetter arg1;
        private readonly IValueGetter arg2;

        public static IExpression CreateConcatenateOperator(
            IValueGetter arg1,
            IValueGetter arg2)
        {
            //Constant case
            if (arg1 is LiteralToken litArg1 && arg2 is LiteralToken litArg2)
            {
                return new LiteralToken<string>(
                    source: litArg1,
                    value: GetStringValue(litArg1) + GetStringValue(litArg2));
            }

            return new ConcatenateOperator(arg1, arg2);
        }

        private ConcatenateOperator(
            IValueGetter arg1,
            IValueGetter arg2)
        {
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        public T GetAs<T>(RuntimeContext context)
        {
            if (!typeof(T).AssignableOrConvertableFromType(typeof(string)))
            {
                throw new ScriptRuntimeException($"Concatenation operator can only return strings: type {typeof(T).Name}");
            }

            return (T)(object)(GetStringValue(arg1, context) + GetStringValue(arg2, context));
        }

        public Type GetValueType() => typeof(string);


        private static string GetStringValue(IValueGetter arg, RuntimeContext context)
        {
            Type argType = arg.GetValueType();

            if (argType == typeof(string))
            {
                return arg.GetAs<string>(context)!;
            }

            return arg.GetAs<object>(context)!.ToString()!;
        }

        private static string GetStringValue(LiteralToken arg)
        {
            Type argType = arg.GetValueType();

            if (argType == typeof(string))
            {
                return arg.GetAs<string>();
            }

            return arg.GetAs<object>().ToString()!;
        }
    }
}