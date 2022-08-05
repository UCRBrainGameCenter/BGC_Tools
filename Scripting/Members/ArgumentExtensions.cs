using System;
using System.Reflection;

namespace BGC.Scripting
{
    public static class ArgumentExtensions
    {
        public static object[] GetArgs(
            this InvocationArgument[] args,
            FunctionSignature functionSignature,
            RuntimeContext context)
        {
            object[] values = new object[functionSignature.arguments.Length];

            for (int i = 0; i < values.Length; i++)
            {
                switch (args[i].argumentType)
                {
                    case ArgumentType.Standard:
                    case ArgumentType.In:
                    case ArgumentType.Ref:
                        {
                            IValueGetter valueGetter = (args[i].expression as IValueGetter)!;
                            values[i] = valueGetter.GetAs<object>(context);

                            if (!functionSignature.arguments[i].valueType.IsAssignableFrom(valueGetter.GetValueType()))
                            {
                                values[i] = Convert.ChangeType(values[i], functionSignature.arguments[i].valueType);
                            }
                        }
                        break;

                    case ArgumentType.Out:
                        values[i] = (args[i].expression as IValueSetter)!.GetValueType().GetDefaultValue();
                        break;

                    default:
                        throw new NotSupportedException($"ArgumentType not supported: {args[i].argumentType}");
                }

            }

            return values;
        }

        public static object[] GetArgs(
            this InvocationArgument[] args,
            MethodInfo methodInfo,
            RuntimeContext context)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] values = new object[parameters.Length];

            for (int i = 0; i < values.Length; i++)
            {
                switch (args[i].argumentType)
                {
                    case ArgumentType.Standard:
                    case ArgumentType.In:
                    case ArgumentType.Ref:
                        {
                            IValueGetter valueGetter = (args[i].expression as IValueGetter)!;
                            values[i] = valueGetter.GetAs<object>(context);

                            if (!parameters[i].ParameterType.IsAssignableFrom(valueGetter.GetValueType()))
                            {
                                values[i] = Convert.ChangeType(values[i], parameters[i].ParameterType);
                            }
                        }
                        break;

                    case ArgumentType.Out:
                        values[i] = (args[i].expression as IValueSetter)!.GetValueType().GetDefaultValue();
                        break;

                    default:
                        throw new NotSupportedException($"ArgumentType not supported: {args[i].argumentType}");
                }
            }

            return values;
        }

        public static object[] GetArgs(
            this InvocationArgument[] args,
            RuntimeContext context)
        {
            object[] values = new object[args.Length];

            for (int i = 0; i < values.Length; i++)
            {
                switch (args[i].argumentType)
                {
                    case ArgumentType.Standard:
                    case ArgumentType.In:
                    case ArgumentType.Ref:
                        {
                            IValueGetter valueGetter = (args[i].expression as IValueGetter)!;
                            values[i] = valueGetter.GetAs<object>(context);
                        }
                        break;

                    case ArgumentType.Out:
                        values[i] = (args[i].expression as IValueSetter)!.GetValueType().GetDefaultValue();
                        break;

                    default:
                        throw new NotSupportedException($"ArgumentType not supported: {args[i].argumentType}");
                }
            }

            return values;
        }

        public static void HandlePostInvocation(
            this InvocationArgument[] args,
            object[] values,
            RuntimeContext context)
        {
            for (int i = 0; i < values.Length; i++)
            {
                switch (args[i].argumentType)
                {
                    case ArgumentType.Standard:
                    case ArgumentType.In:
                        //Do nothing
                        break;

                    case ArgumentType.Ref:
                    case ArgumentType.Out:
                        (args[i].expression as IValueSetter)!.Set(context, values[i]);
                        break;

                    default:
                        throw new NotSupportedException($"ArgumentType not supported: {args[i].argumentType}");
                }
            }
        }

        public static ParameterModifier[] CreateParameterModifiers(this InvocationArgument[] argumentTypes)
        {
            if (argumentTypes.Length == 0)
            {
                return null;
            }

            ParameterModifier[] modifiers = new[] { new ParameterModifier(argumentTypes.Length) };

            for (int i = 0; i < argumentTypes.Length; i++)
            {
                switch (argumentTypes[i].argumentType)
                {
                    case ArgumentType.Standard:
                    case ArgumentType.In:
                        //Do Nothing
                        break;

                    case ArgumentType.Ref:
                    case ArgumentType.Out:
                        modifiers[0][i] = true;
                        break;

                    default:
                        throw new NotSupportedException($"ArgumentType not supported: {argumentTypes[i].argumentType}");
                }
            }

            return modifiers;
        }

        public static Type[] GetEffectiveTypes(this InvocationArgument[] argumentTypes)
        {
            Type[] types = new Type[argumentTypes.Length];

            for (int i = 0; i < argumentTypes.Length; i++)
            {
                switch (argumentTypes[i].argumentType)
                {
                    case ArgumentType.Standard:
                    case ArgumentType.In:
                        types[i] = (argumentTypes[i].expression as IValueGetter)!.GetValueType();
                        break;

                    case ArgumentType.Ref:
                        types[i] = (argumentTypes[i].expression as IValueGetter)!.GetValueType().MakeByRefType();
                        break;

                    case ArgumentType.Out:
                        types[i] = (argumentTypes[i].expression as IValueSetter)!.GetValueType().MakeByRefType();
                        break;

                    default:
                        throw new NotSupportedException($"ArgumentType not supported: {argumentTypes[i].argumentType}");
                }
            }

            return types;
        }
    }
}