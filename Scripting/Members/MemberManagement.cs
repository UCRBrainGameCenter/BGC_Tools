using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BGC.DataStructures.Generic;
using BGC.Scripting.Parsing;

namespace BGC.Scripting
{
    public static class MemberManagement
    {
        public static IExpression HandleMemberExpression(
            IValueGetter value,
            Type[] genericMethodArguments,
            InvocationArgument[] args,
            string identifier,
            Token source)
        {
            if (args is null)
            {
                if (genericMethodArguments is not null)
                {
                    throw new ScriptParsingException(source, $"Unexpected GenericArguments in MemberAccess context");
                }

                return HandleMemberValueExpression(value, identifier, source);
            }
            else
            {
                return HandleMemberValueMethodExpression(value, genericMethodArguments, args, identifier, source);
            }
        }

        public static IExpression HandleStaticExpression(
            Type type,
            Type[] genericMethodArguments,
            InvocationArgument[] args,
            string identifier,
            Token source)
        {
            if (args is null)
            {
                if (genericMethodArguments is not null)
                {
                    throw new ScriptParsingException(source, $"Unexpected GenericArguments in Static MemberAccess context");
                }

                return HandleStaticPropertyExpression(type, identifier, source);
            }
            else
            {
                return HandleStaticMethodExpression(type, genericMethodArguments, args, identifier, source);
            }
        }

        private static IExpression HandleMemberValueExpression(
            IValueGetter value,
            string identifier,
            Token source)
        {
            Type valueType = value.GetValueType();

            IExpression registeredMember = ClassRegistrar.GetMemberExpression(
                value: value,
                memberName: identifier,
                source: source);

            if (registeredMember is not null)
            {
                return registeredMember;
            }

            throw new ScriptParsingException(
                source: source,
                message: $"Type \"{valueType.Name}\" has no registered member \"{identifier}\"");
        }

        private static IExpression HandleMemberValueMethodExpression(
            IValueGetter value,
            Type[] genericMethodArguments,
            InvocationArgument[] args,
            string identifier,
            Token source)
        {
            Type valueType = value.GetValueType();

            IExpression registeredMethod = ClassRegistrar.GetMethodExpression(
                value: value,
                genericMethodArguments: genericMethodArguments,
                args: args,
                methodName: identifier,
                source: source);

            if (registeredMethod is not null)
            {
                return registeredMethod;
            }

            throw new ScriptParsingException(
                source: source,
                message: $"Type \"{valueType.Name}\" has no registered method \"{identifier}\"");
        }

        private static IExpression HandleStaticPropertyExpression(
            Type type,
            string identifier,
            Token source)
        {
            IExpression registeredProperty = ClassRegistrar.GetStaticExpression(
                type: type,
                propertyName: identifier,
                source: source);

            if (registeredProperty is not null)
            {
                return registeredProperty;
            }

            throw new ScriptParsingException(
                source: source,
                message: $"Type \"{type.Name}\" has no registered static property \"{identifier}\"");
        }

        private static IExpression HandleStaticMethodExpression(
            Type type,
            Type[] genericMethodArguments,
            InvocationArgument[] args,
            string identifier,
            Token source)
        {
            IExpression registeredMethod = ClassRegistrar.GetStaticMethodExpression(
                type: type,
                genericMethodArguments: genericMethodArguments,
                args: args,
                methodName: identifier,
                source: source);

            if (registeredMethod is not null)
            {
                return registeredMethod;
            }

            throw new ScriptParsingException(
                source: source,
                message: $"Type \"{type.Name}\" has no registered static method \"{identifier}\"");
        }
    }
}