using System;

namespace BGC.Scripting
{
    public static class ArgumentExtensions
    {
        public static void VerifyArgs(
            this IValueGetter[] args,
            Token source,
            string identifier)
        {
            if (args.Length != 0)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 0 Arguments for method {identifier}, found: {args.Length} arguments");
            }
        }

        public static void VerifyArgs(
            this IValueGetter[] args,
            Type arg1Type,
            Token source,
            string identifier)
        {
            if (args.Length != 1)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 1 Argument of type {arg1Type.Name} for method {identifier}, found: {args.Length} arguments");
            }

            if (!arg1Type.AssignableFromType(args[0].GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 1 Argument of type {arg1Type.Name} for method {identifier}, found argument of type {args[0].GetValueType().Name}");
            }
        }

        public static void VerifyArgs(
            this IValueGetter[] args,
            Type arg1Type,
            Type arg2Type,
            Token source,
            string identifier)
        {
            if (args.Length != 2)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 2 Arguments of types ({arg1Type.Name},{arg2Type.Name}) for method {identifier}, found: {args.Length} arguments");
            }

            if (!arg1Type.AssignableFromType(args[0].GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 2 Arguments of types (**{arg1Type.Name}**,{arg2Type.Name}) for method {identifier}, found arguments of types (**{args[0].GetValueType().Name}**,{args[1].GetValueType().Name})");
            }

            if (!arg2Type.AssignableFromType(args[1].GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 2 Arguments of types ({arg1Type.Name},**{arg2Type.Name}**) for method {identifier}, found arguments of types ({args[0].GetValueType().Name},**{args[1].GetValueType().Name}**)");
            }
        }

        public static void VerifyArgs(
            this IValueGetter[] args,
            Type arg1Type,
            Type arg2Type,
            Type arg3Type,
            Token source,
            string identifier)
        {
            if (args.Length != 3)
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 3 Arguments of types ({arg1Type.Name},{arg2Type.Name},{arg3Type.Name}) for method {identifier}, found: {args.Length} arguments");
            }

            if (!arg1Type.AssignableFromType(args[0].GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 3 Arguments of types (**{arg1Type.Name}**,{arg2Type.Name},{arg3Type.Name}) for method {identifier}, found arguments of types (**{args[0].GetValueType().Name}**,{args[1].GetValueType().Name},{args[2].GetValueType().Name})");
            }

            if (!arg2Type.AssignableFromType(args[1].GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 3 Arguments of types ({arg1Type.Name},**{arg2Type.Name}**,{arg3Type.Name}) for method {identifier}, found arguments of types ({args[0].GetValueType().Name},**{args[1].GetValueType().Name}**,{args[2].GetValueType().Name})");
            }

            if (!arg3Type.AssignableFromType(args[2].GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 3 Arguments of types ({arg1Type.Name},{arg2Type.Name},**{arg3Type.Name}**) for method {identifier}, found arguments of types ({args[0].GetValueType().Name},{args[1].GetValueType().Name},**{args[2].GetValueType().Name}**)");
            }
        }

        public static object[] GetArgs(
            this IValueGetter[] args,
            Type arg1Type,
            RuntimeContext context)
        {
            object[] values = new object[1];

            values[0] = args[0].GetAs<object>(context);

            if (arg1Type != args[0].GetValueType())
            {
                //We've already established they're compatible, so convert
                values[0] = Convert.ChangeType(values[0], arg1Type);
            }

            return values;
        }

        public static object[] GetArgs(
            this IValueGetter[] args,
            Type arg1Type,
            Type arg2Type,
            RuntimeContext context)
        {
            object[] values = new object[2];

            values[0] = args[0].GetAs<object>(context);
            values[1] = args[1].GetAs<object>(context);

            if (arg1Type != args[0].GetValueType())
            {
                //We've already established they're compatible, so convert
                values[0] = Convert.ChangeType(values[0], arg1Type);
            }

            if (arg2Type != args[1].GetValueType())
            {
                //We've already established they're compatible, so convert
                values[1] = Convert.ChangeType(values[1], arg2Type);
            }

            return values;
        }

        public static object[] GetArgs(
            this IValueGetter[] args,
            FunctionSignature functionSignature,
            RuntimeContext context)
        {
            object[] values = new object[functionSignature.arguments.Length];

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = args[i].GetAs<object>(context);
                if (!functionSignature.arguments[i].valueType.IsAssignableFrom(args[i].GetValueType()))
                {
                    values[i] = Convert.ChangeType(values[i], functionSignature.arguments[i].valueType);
                }
            }

            return values;
        }
    }
}
