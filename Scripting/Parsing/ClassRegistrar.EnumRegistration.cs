using System;

namespace BGC.Scripting.Parsing
{
    public static partial class ClassRegistrar
    {
        public class EnumRegistration : IRegistration
        {
            public Type ClassType { get; }

            public EnumRegistration(
                Type type)
            {
                ClassType = type;
            }

            public IExpression GetMethodExpression(
                IValueGetter value,
                Type[] genericClassArguments,
                Type[] genericMethodArguments,
                InvocationArgument[] args,
                string methodName,
                Token source)
            {
                return null;
            }

            public IExpression GetPropertyExpression(
                IValueGetter value,
                Type[] genericClassArguments,
                string propertyName,
                Token source)
            {
                return null;
            }

            public IExpression GetStaticMethodExpression(
                Type[] genericClassArguments,
                Type[] genericMethodArguments,
                InvocationArgument[] args,
                string methodName,
                Token source)
            {
                return null;
            }

            public IExpression GetStaticPropertyExpression(
                Type[] genericClassArguments,
                string propertyName,
                Token source)
            {
                //This will be retrieving an Enum value
                if (genericClassArguments is not null)
                {
                    throw new ScriptParsingException(source, $"Enum {ClassType} does not take Generic Class Arguments.");
                }

                if (!Enum.TryParse(ClassType, propertyName, out object result))
                {
                    throw new ScriptParsingException(source, $"Enum {ClassType} does not have a {propertyName} value. " +
                        $"Valid values: {{{string.Join(", ", Enum.GetNames(ClassType))}}}");
                }

                return new EnumValueToken(source, result!, ClassType);
            }
        }
    }
}