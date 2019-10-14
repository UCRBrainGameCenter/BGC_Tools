using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using BGC.Users;
using BGC.DataStructures.Generic;
using BGC.UI.Dialogs;

namespace BGC.Scripting
{
    public static class MemberManagement
    {
        public static IExpression HandleMemberExpression(
            IValueGetter value,
            IValueGetter[] args,
            string identifier,
            Token source)
        {
            if (args == null)
            {
                return HandleMemberValueExpression(value, identifier, source);
            }
            else
            {
                return HandleMemberValueMethodExpression(value, args, identifier, source);
            }
        }

        private static IExpression HandleMemberValueExpression(
            IValueGetter value,
            string identifier,
            Token source)
        {
            Type valueType = value.GetValueType();

            if (valueType == typeof(string))
            {
                switch (identifier)
                {
                    case "Length":
                        return new GettablePropertyValueOperation<string, int>(
                            value: value,
                            operation: (string input) => input.Length,
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (typeof(IList).IsAssignableFrom(valueType))
            {
                switch (identifier)
                {
                    case "Count":
                        return new GettablePropertyValueOperation<IList, int>(
                            value: value,
                            operation: (IList input) => input.Count,
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Queue<>))
            {
                switch (identifier)
                {
                    case "Count":
                        return new GettablePropertyValueOperation<ICollection, int>(
                            value: value,
                            operation: (ICollection input) => input.Count,
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Stack<>))
            {
                switch (identifier)
                {
                    case "Count":
                        return new GettablePropertyValueOperation<ICollection, int>(
                            value: value,
                            operation: (ICollection input) => input.Count,
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && valueType.GetInterfaces().Any(
                        x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDepletable<>)))
            {
                PropertyInfo property;

                switch (identifier)
                {
                    case "Count":
                    case "TotalCount":
                        property = valueType.GetProperty(identifier);
                        return new GettablePropertyValueOperation<object, int>(
                            value: value,
                            operation: (object depletable) => (int)property.GetValue(depletable),
                            source: source);

                    case "AutoRefill":
                        property = valueType.GetProperty(identifier);
                        return new SettablePropertyValueOperation<object, bool>(
                            value: value,
                            getOperation: (object depletable) => (bool)property.GetValue(depletable),
                            setOperation: (object depletable, bool newValue) => property.SetValue(depletable, newValue),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && typeof(RingBuffer<>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))
            {
                PropertyInfo property;
                Type itemType = valueType.GetGenericArguments()[0];

                switch (identifier)
                {
                    case "Count":
                    case "Size":
                        property = valueType.GetProperty(identifier);
                        return new GettablePropertyValueOperation<object, int>(
                            value: value,
                            operation: (object ringBuffer) => (int)property.GetValue(ringBuffer),
                            source: source);

                    case "Head":
                    case "Tail":
                        property = valueType.GetProperty(identifier);
                        return new CastingPropertyValueOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object ringBuffer) => property.GetValue(ringBuffer));

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))
            {
                PropertyInfo property;

                switch (identifier)
                {
                    case "Count":
                        property = valueType.GetProperty(identifier);
                        return new GettablePropertyValueOperation<object, int>(
                            value: value,
                            operation: (object ringBuffer) => (int)property.GetValue(ringBuffer),
                            source: source);

                    case "Values":
                        {
                            Type varType = valueType.GetGenericArguments()[1];
                            Type returnType = typeof(IEnumerable<>).MakeGenericType(varType);
                            property = valueType.GetProperty(identifier);
                            return new CastingPropertyValueOperation(
                                value: value,
                                outputType: returnType,
                                operation: (object ringBuffer) => property.GetValue(ringBuffer));
                        }

                    case "Keys":
                        {
                            Type keyType = valueType.GetGenericArguments()[0];
                            Type returnType = typeof(IEnumerable<>).MakeGenericType(keyType);
                            property = valueType.GetProperty(identifier);
                            return new CastingPropertyValueOperation(
                                value: value,
                                outputType: returnType,
                                operation: (object ringBuffer) => property.GetValue(ringBuffer));
                        }

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && typeof(HashSet<>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))
            {
                PropertyInfo property;

                switch (identifier)
                {
                    case "Count":
                        property = valueType.GetProperty(identifier);
                        return new GettablePropertyValueOperation<object, int>(
                            value: value,
                            operation: (object ringBuffer) => (int)property.GetValue(ringBuffer),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }

            throw new ScriptParsingException(
                source: source,
                message: $"Unable to identify any members for type {valueType.Name}.  Requested member: {identifier}");
        }

        private static IExpression HandleMemberValueMethodExpression(
            IValueGetter value,
            IValueGetter[] args,
            string identifier,
            Token source)
        {
            Type valueType = value.GetValueType();

            if (valueType == typeof(string))
            {
                switch (identifier)
                {
                    case "Substring":
                        if (args.Length == 1)
                        {
                            args.VerifyArgs(typeof(int), source, identifier);
                            return new MemberArgumentValueOperation<string, string>(
                                value: value,
                                operation: (string input, RuntimeContext context) =>
                                    input.Substring(args[0].GetAs<int>(context)),
                                source: source);
                        }

                        if (args.Length == 2)
                        {
                            args.VerifyArgs(typeof(int), typeof(int), source, identifier);
                            return new MemberArgumentValueOperation<string, string>(
                                value: value,
                                operation: (string input, RuntimeContext context) =>
                                    input.Substring(args[0].GetAs<int>(context), args[1].GetAs<int>(context)),
                                source: source);
                        }

                        throw new ScriptParsingException(
                            source: source,
                            message: $"Expected 1 or 2 Arguments to {identifier}, found: {args.Length}");

                    case "IndexOf":
                        if (args.Length == 1)
                        {
                            args.VerifyArgs(typeof(string), source, identifier);
                            return new MemberArgumentValueOperation<string, int>(
                                value: value,
                                operation: (string input, RuntimeContext context) =>
                                    input.IndexOf(args[0].GetAs<string>(context)),
                                source: source);
                        }

                        if (args.Length == 2)
                        {
                            args.VerifyArgs(typeof(string), typeof(int), source, identifier);
                            return new MemberArgumentValueOperation<string, int>(
                                value: value,
                                operation: (string input, RuntimeContext context) =>
                                    input.IndexOf(args[0].GetAs<string>(context), args[1].GetAs<int>(context)),
                                source: source);
                        }

                        if (args.Length == 3)
                        {
                            args.VerifyArgs(typeof(string), typeof(int), typeof(int), source, identifier);
                            return new MemberArgumentValueOperation<string, int>(
                                value: value,
                                operation: (string input, RuntimeContext context) =>
                                    input.IndexOf(args[0].GetAs<string>(context), args[1].GetAs<int>(context), args[2].GetAs<int>(context)),
                                source: source);
                        }

                        throw new ScriptParsingException(
                            source: source,
                            message: $"Expected 1, 2, or 3 Arguments to {identifier}, found: {args.Length}");

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (typeof(IList).IsAssignableFrom(valueType))
            {
                Type itemType = valueType.GetGenericArguments()[0];

                switch (identifier)
                {
                    case "Add":
                        args.VerifyArgs(itemType, source, identifier);
                        return new MemberArgumentStatementOperation<IList>(
                            value: value,
                            operation: (IList input, RuntimeContext context) =>
                                input.Add(args[0].GetAs<object>(context)),
                            source: source);

                    case "Insert":
                        args.VerifyArgs(itemType, typeof(int), source, identifier);
                        return new MemberArgumentStatementOperation<IList>(
                            value: value,
                            operation: (IList input, RuntimeContext context) =>
                                input.Insert(args[0].GetAs<int>(context), args[1].GetAs<object>(context)),
                            source: source);

                    case "Remove":
                        args.VerifyArgs(itemType, source, identifier);
                        return new MemberArgumentStatementOperation<IList>(
                            value: value,
                            operation: (IList input, RuntimeContext context) =>
                                input.Remove(args[0].GetAs<object>(context)),
                            source: source);

                    case "Contains":
                        args.VerifyArgs(itemType, source, identifier);
                        return new MemberArgumentValueOperation<IList, bool>(
                            value: value,
                            operation: (IList input, RuntimeContext context) =>
                                input.Contains(args[0].GetAs<object>(context)),
                            source: source);

                    case "IndexOf":
                        args.VerifyArgs(itemType, source, identifier);
                        return new MemberArgumentValueOperation<IList, int>(
                            value: value,
                            operation: (IList input, RuntimeContext context) =>
                                input.IndexOf(args[0].GetAs<object>(context)),
                            source: source);

                    case "Clear":
                        args.VerifyArgs(source, identifier);
                        return new MemberStatementOperation<IList>(
                            value: value,
                            operation: (IList input) => input.Clear(),
                            source: source);

                    case "RemoveAt":
                        args.VerifyArgs(typeof(int), source, identifier);
                        return new MemberArgumentStatementOperation<IList>(
                            value: value,
                            operation: (IList input, RuntimeContext context) =>
                                input.RemoveAt(args[0].GetAs<int>(context)),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Queue<>))
            {
                Type itemType = valueType.GetGenericArguments()[0];
                MethodInfo method;

                switch (identifier)
                {
                    case "Enqueue":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentStatementOperation<object>(
                            value: value,
                            operation: (object queue, RuntimeContext context) =>
                                method.Invoke(queue, args.GetArgs(itemType, context)),
                            source: source);

                    case "Contains":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod("Contains");
                        return new MemberArgumentValueOperation<object, bool>(
                            value: value,
                            operation: (object queue, RuntimeContext context) =>
                                (bool)method.Invoke(queue, args.GetArgs(itemType, context)),
                            source: source);

                    case "Clear":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberStatementOperation<object>(
                            value: value,
                            operation: (object queue) => method.Invoke(queue, null),
                            source: source);

                    case "Dequeue":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new CastingMemberValueExecutableOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object queue) => method.Invoke(queue, null));

                    case "Peek":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new CastingMemberValueOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object queue) => method.Invoke(queue, null));

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Stack<>))
            {
                Type itemType = valueType.GetGenericArguments()[0];
                MethodInfo method;

                switch (identifier)
                {
                    case "Push":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentStatementOperation<object>(
                            value: value,
                            operation: (object stack, RuntimeContext context) =>
                                method.Invoke(stack, args.GetArgs(itemType, context)),
                            source: source);

                    case "Pop":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new CastingMemberValueExecutableOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object stack) => method.Invoke(stack, null));

                    case "Peek":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new CastingMemberValueOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object stack) => method.Invoke(stack, null));

                    case "Contains":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod("Contains");
                        return new MemberArgumentValueOperation<object, bool>(
                            value: value,
                            operation: (object stack, RuntimeContext context) =>
                                (bool)method.Invoke(stack, args.GetArgs(itemType, context)),
                            source: source);

                    case "Clear":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberStatementOperation<object>(
                            value: value,
                            operation: (object stack) => method.Invoke(stack, null),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && valueType.GetInterfaces().Any(
                        x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDepletable<>)))
            {
                Type itemType = valueType.GetGenericArguments()[0];
                MethodInfo method;

                switch (identifier)
                {
                    case "Contains":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentValueOperation<object, bool>(
                            value: value,
                            operation: (object depletable, RuntimeContext context) =>
                                (bool)method.Invoke(depletable, args.GetArgs(itemType, context)),
                            source: source);

                    case "Add":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentStatementOperation<object>(
                            value: value,
                            operation: (object depletable, RuntimeContext context) =>
                                method.Invoke(depletable, args.GetArgs(itemType, context)),
                            source: source);

                    case "Remove":
                    case "DepleteValue":
                    case "DepleteAllValue":
                    case "ReplenishValue":
                    case "ReplenishAllValue":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentValueExecutableOperation<object, bool>(
                            value: value,
                            operation: (object depletable, RuntimeContext context) =>
                                (bool)method.Invoke(depletable, args.GetArgs(itemType, context)),
                            source: source);

                    case "PopNext":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod("PopNext");
                        return new CastingMemberValueExecutableOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object depletable) => method.Invoke(depletable, null));

                    case "Clear":
                    case "Reset":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberStatementOperation<object>(
                            value: value,
                            operation: (object depletable) => method.Invoke(depletable, null),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && typeof(RingBuffer<>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))
            {
                Type itemType = valueType.GetGenericArguments()[0];
                MethodInfo method;

                switch (identifier)
                {
                    case "Add":
                    case "Push":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentStatementOperation<object>(
                            value: value,
                            operation: (object ringBuffer, RuntimeContext context) =>
                                method.Invoke(ringBuffer, args.GetArgs(itemType, context)),
                            source: source);

                    case "Contains":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentValueOperation<object, bool>(
                            value: value,
                            operation: (object ringBuffer, RuntimeContext context) =>
                                (bool)method.Invoke(ringBuffer, args.GetArgs(itemType, context)),
                            source: source);

                    case "Remove":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentValueExecutableOperation<object, bool>(
                            value: value,
                            operation: (object ringBuffer, RuntimeContext context) =>
                                (bool)method.Invoke(ringBuffer, args.GetArgs(itemType, context)),
                            source: source);

                    case "RemoveAt":
                    case "Resize":
                        args.VerifyArgs(typeof(int), source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentStatementOperation<object>(
                            value: value,
                            operation: (object ringBuffer, RuntimeContext context) =>
                                method.Invoke(ringBuffer, args.GetArgs(typeof(int), context)),
                            source: source);

                    case "GetIndex":
                    case "CountElement":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentValueOperation<object, int>(
                            value: value,
                            operation: (object ringBuffer, RuntimeContext context) =>
                                (int)method.Invoke(ringBuffer, args.GetArgs(itemType, context)),
                            source: source);

                    case "Pop":
                    case "PopBack":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new CastingMemberValueExecutableOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object ringBuffer) => method.Invoke(ringBuffer, null));

                    case "PeekHead":
                    case "PeekTail":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new CastingMemberValueOperation(
                            value: value,
                            outputType: itemType,
                            operation: (object ringBuffer) => method.Invoke(ringBuffer, null));

                    case "Clear":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberStatementOperation<object>(
                            value: value,
                            operation: (object ringBuffer) => method.Invoke(ringBuffer, null),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && typeof(Dictionary<,>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))
            {
                Type keyType = valueType.GetGenericArguments()[0];
                Type varType = valueType.GetGenericArguments()[1];
                MethodInfo method;

                switch (identifier)
                {
                    case "Add":
                        args.VerifyArgs(keyType, varType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentStatementOperation<object>(
                            value: value,
                            operation: (object dictionary, RuntimeContext context) =>
                                method.Invoke(dictionary, args.GetArgs(keyType, varType, context)),
                            source: source);

                    case "ContainsKey":
                        args.VerifyArgs(keyType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentValueOperation<object, bool>(
                            value: value,
                            operation: (object dictionary, RuntimeContext context) =>
                                (bool)method.Invoke(dictionary, args.GetArgs(keyType, context)),
                            source: source);

                    case "ContainsValue":
                        args.VerifyArgs(varType, source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberArgumentValueOperation<object, bool>(
                            value: value,
                            operation: (object dictionary, RuntimeContext context) =>
                                (bool)method.Invoke(dictionary, args.GetArgs(varType, context)),
                            source: source);

                    case "Clear":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberStatementOperation<object>(
                            value: value,
                            operation: (object dictionary) => method.Invoke(dictionary, null),
                            source: source);

                    case "Remove":
                        args.VerifyArgs(keyType, source, identifier);
                        method = valueType.GetMethod(identifier, new Type[] { keyType });
                        return new MemberArgumentValueExecutableOperation<object, bool>(
                            value: value,
                            operation: (object dictionary, RuntimeContext context) =>
                                (bool)method.Invoke(dictionary, args.GetArgs(keyType, context)),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (valueType.IsGenericType && typeof(HashSet<>).IsAssignableFrom(valueType.GetGenericTypeDefinition()))
            {
                Type itemType = valueType.GetGenericArguments()[0];
                MethodInfo method;

                switch (identifier)
                {
                    case "Contains":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier, new Type[] { itemType });
                        return new MemberArgumentValueOperation<object, bool>(
                            value: value,
                            operation: (object dictionary, RuntimeContext context) =>
                                (bool)method.Invoke(dictionary, args.GetArgs(itemType, context)),
                            source: source);

                    case "Add":
                    case "Remove":
                        args.VerifyArgs(itemType, source, identifier);
                        method = valueType.GetMethod(identifier, new Type[] { itemType });
                        return new MemberArgumentValueExecutableOperation<object, bool>(
                            value: value,
                            operation: (object dictionary, RuntimeContext context) =>
                                (bool)method.Invoke(dictionary, args.GetArgs(itemType, context)),
                            source: source);

                    case "Clear":
                        args.VerifyArgs(source, identifier);
                        method = valueType.GetMethod(identifier);
                        return new MemberStatementOperation<object>(
                            value: value,
                            operation: (object ringBuffer) => method.Invoke(ringBuffer, null),
                            source: source);

                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }
            else if (typeof(Random) == valueType)
            {
                switch (identifier)
                {
                    case "Next":
                        if (args.Length == 0)
                        {
                            args.VerifyArgs(source, identifier);
                            return new GettablePropertyValueOperation<Random, int>(
                                value: value,
                                operation: (Random input) => input.Next(),
                                source: source);
                        }
                        else if (args.Length == 1)
                        {
                            args.VerifyArgs(typeof(int), source, identifier);
                            return new MemberArgumentValueOperation<Random, int>(
                                value: value,
                                operation: (Random input, RuntimeContext context) =>
                                    input.Next(args[0].GetAs<int>(context)),
                                source: source);
                        }
                        else if (args.Length == 2)
                        {
                            args.VerifyArgs(typeof(int), typeof(int), source, identifier);
                            return new MemberArgumentValueOperation<Random, int>(
                                value: value,
                                operation: (Random input, RuntimeContext context) =>
                                    input.Next(args[0].GetAs<int>(context), args[1].GetAs<int>(context)),
                                source: source);
                        }
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Expected 0, 1, or 2 Arguments to {identifier}, found: {args.Length}");

                    case "NextDouble":
                        args.VerifyArgs(source, identifier);
                        return new GettablePropertyValueOperation<Random, double>(
                            value: value,
                            operation: (Random input) => input.NextDouble(),
                            source: source);


                    default:
                        throw new ScriptParsingException(
                            source: source,
                            message: $"Unable to identify {valueType.Name} member {identifier}");
                }
            }

            throw new ScriptParsingException(
                source: source,
                message: $"Unable to identify any member methods for type {valueType.Name}.  Requested member: {identifier}");
        }

        public static IExpression HandleStaticMemberExpression(
            KeywordToken keywordToken,
            string identifier)
        {
            switch (keywordToken.keyword)
            {
                case Keyword.System:
                    switch (identifier)
                    {
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.Debug:
                    switch (identifier)
                    {
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.User:
                    switch (identifier)
                    {
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.Math:
                    switch (identifier)
                    {
                        case "PI":
                            return new LiteralToken<double>(
                                source: keywordToken,
                                value: Math.PI);

                        case "E":
                            return new LiteralToken<double>(
                                source: keywordToken,
                                value: Math.E);

                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                default:
                    throw new ScriptParsingException(
                        source: keywordToken,
                        message: $"Unsupported Generic Static method class for expression {keywordToken.keyword}");
            }
        }

        public static IExpression HandleStaticGenericMethodExpression(
            KeywordToken keywordToken,
            IValueGetter[] args,
            string identifier,
            Type[] genericTypes)
        {
            switch (keywordToken.keyword)
            {
                case Keyword.System:
                    switch (identifier)
                    {
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.Debug:
                    switch (identifier)
                    {
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.User:
                    UserMethod userMethod = TranslateUserMethod(identifier);
                    switch (userMethod)
                    {
                        case UserMethod.GetList:
                            if (genericTypes.Length != 1)
                            {
                                throw new ScriptParsingException(
                                    source: keywordToken,
                                    message: $"Expected 1 type arguemnt for method expression {keywordToken.keyword}.{identifier}.  Found {genericTypes.Length}");
                            }

                            return GetUserListFunction.Create(
                                args: args,
                                itemType: genericTypes[0],
                                source: keywordToken);

                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.Math:
                    switch (identifier)
                    {
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Generic Static method expression {keywordToken.keyword}.{identifier}");
                    }

                default:
                    throw new ScriptParsingException(
                        source: keywordToken,
                        message: $"Unsupported Generic Static method class for expression {keywordToken.keyword}");
            }
        }

        public static IExpression HandleStaticMethodExpression(
            KeywordToken keywordToken,
            IValueGetter[] args,
            string identifier)
        {
            switch (keywordToken.keyword)
            {

                case Keyword.System:
                    switch (identifier)
                    {
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.Debug:
                    switch (identifier)
                    {
                        case "Log":
                            args.VerifyArgs(typeof(string), keywordToken, identifier);
                            return new StaticArgumentStatementOperation(
                                operation: (RuntimeContext context) =>
                                    UnityEngine.Debug.Log(args[0].GetAs<string>(context)));

                        case "LogWarning":
                            args.VerifyArgs(typeof(string), keywordToken, identifier);
                            return new StaticArgumentStatementOperation(
                                operation: (RuntimeContext context) =>
                                    UnityEngine.Debug.LogWarning(args[0].GetAs<string>(context)));

                        case "LogError":
                            args.VerifyArgs(typeof(string), keywordToken, identifier);
                            return new StaticArgumentStatementOperation(
                                operation: (RuntimeContext context) =>
                                    UnityEngine.Debug.LogError(args[0].GetAs<string>(context)));

                        case "PopUp":
                            args.VerifyArgs(typeof(string), typeof(string), keywordToken, identifier);
                            return new StaticArgumentStatementOperation(
                                operation: (RuntimeContext context) =>
                                    ModalDialog.ShowSimpleModal(
                                        mode: ModalDialog.Mode.Accept,
                                        headerText: args[0].GetAs<string>(context),
                                        bodyText: args[1].GetAs<string>(context)));
                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.User:
                    UserMethod userMethod = TranslateUserMethod(identifier);
                    switch (userMethod)
                    {
                        case UserMethod.HasData:
                            args.VerifyArgs(typeof(string), keywordToken, identifier);
                            return new HasDataOperation(
                                keyArg: args[0],
                                source: keywordToken);

                        case UserMethod.GetBool:
                        case UserMethod.GetInt:
                        case UserMethod.GetString:
                        case UserMethod.GetDouble:
                            return GetUserValueFunction.Create(
                                args: args,
                                userMethod: userMethod,
                                source: keywordToken);

                        case UserMethod.ClearData:
                            args.VerifyArgs(typeof(string), keywordToken, identifier);
                            return new ClearDataOperation(
                                keyArg: args[0],
                                source: keywordToken);

                        case UserMethod.SetBool:
                        case UserMethod.SetInt:
                        case UserMethod.SetDouble:
                        case UserMethod.SetString:
                        case UserMethod.SetList:
                            args.VerifyArgs(typeof(string), typeof(object), keywordToken, identifier);
                            return new SetUserValueMethod(
                                keyArg: args[0],
                                valueArg: args[1],
                                userMethod: userMethod,
                                source: keywordToken);

                        case UserMethod.AddToReport:
                            args.VerifyArgs(typeof(string), typeof(string), keywordToken, identifier);
                            return new AddToReportMethod(
                                headerArg: args[0],
                                valueArg: args[1],
                                source: keywordToken);

                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Static method expression {keywordToken.keyword}.{identifier}");
                    }

                case Keyword.Math:
                    MathMethod mathMethod = TranslateMathMethod(identifier);
                    switch (mathMethod)
                    {
                        case MathMethod.Floor:
                        case MathMethod.Ceiling:
                        case MathMethod.Round:
                        case MathMethod.Abs:
                        case MathMethod.Sign:
                        case MathMethod.Ln:
                        case MathMethod.Log10:
                        case MathMethod.Exp:
                        case MathMethod.Sqrt:
                        case MathMethod.Sin:
                        case MathMethod.Cos:
                        case MathMethod.Tan:
                        case MathMethod.Asin:
                        case MathMethod.Acos:
                        case MathMethod.Atan:
                        case MathMethod.Sinh:
                        case MathMethod.Cosh:
                        case MathMethod.Tanh:
                            args.VerifyArgs(typeof(double), keywordToken, identifier);
                            return new SingleArgumentMathFunction(
                                arg: args[0],
                                mathMethod: mathMethod,
                                source: keywordToken);

                        case MathMethod.Max:
                        case MathMethod.Min:
                            args.VerifyArgs(typeof(double), typeof(double), keywordToken, identifier);
                            return new DoubleArgumentMathFunction(
                                arg1: args[0],
                                arg2: args[1],
                                mathMethod: mathMethod,
                                source: keywordToken);

                        case MathMethod.Clamp:
                            args.VerifyArgs(typeof(double), typeof(double), typeof(double), keywordToken, identifier);
                            return new ClampMathFunction(
                                value: args[0],
                                lowerbound: args[1],
                                upperbound: args[2],
                                source: keywordToken);

                        case MathMethod.IsNaN:
                            args.VerifyArgs(typeof(double), keywordToken, identifier);
                            return new IsNaNMathFunction(
                                arg: args[0],
                                source: keywordToken);

                        default:
                            throw new ScriptParsingException(
                                source: keywordToken,
                                message: $"Unsupported Static method expression {keywordToken.keyword}.{identifier}");
                    }

                default:
                    throw new ScriptParsingException(
                        source: keywordToken,
                        message: $"Unsupported Static method class for expression {keywordToken.keyword}");
            }
        }

        private static MathMethod TranslateMathMethod(string identifier)
        {
            switch (identifier)
            {
                case "Floor": return MathMethod.Floor;
                case "Ceiling": return MathMethod.Ceiling;
                case "Round": return MathMethod.Round;
                case "Abs": return MathMethod.Abs;
                case "Sign": return MathMethod.Sign;

                case "Ln": return MathMethod.Ln;
                case "Log10": return MathMethod.Log10;
                case "Exp": return MathMethod.Exp;
                case "Sqrt": return MathMethod.Sqrt;

                case "Sin": return MathMethod.Sin;
                case "Cos": return MathMethod.Cos;
                case "Tan": return MathMethod.Tan;
                case "Asin": return MathMethod.Asin;
                case "Acos": return MathMethod.Acos;
                case "Atan": return MathMethod.Atan;
                case "Sinh": return MathMethod.Sinh;
                case "Cosh": return MathMethod.Cosh;
                case "Tanh": return MathMethod.Tanh;

                case "IsNaN": return MathMethod.IsNaN;

                case "Max": return MathMethod.Max;
                case "Min": return MathMethod.Min;
                case "Clamp": return MathMethod.Clamp;

                default:
                    UnityEngine.Debug.LogError($"Invalid MathMethod: {identifier}");
                    return MathMethod.MAX;
            }
        }

        private static UserMethod TranslateUserMethod(string identifier)
        {
            switch (identifier)
            {
                case "HasData": return UserMethod.HasData;
                case "ClearData": return UserMethod.ClearData;

                case "SetInt": return UserMethod.SetInt;
                case "SetDouble": return UserMethod.SetDouble;
                case "SetBool": return UserMethod.SetBool;
                case "SetString": return UserMethod.SetString;
                case "SetList": return UserMethod.SetList;

                case "GetInt": return UserMethod.GetInt;
                case "GetDouble": return UserMethod.GetDouble;
                case "GetBool": return UserMethod.GetBool;
                case "GetString": return UserMethod.GetString;
                case "GetList": return UserMethod.GetList;

                case "AddToReport": return UserMethod.AddToReport;

                default:
                    UnityEngine.Debug.LogError($"Invalid UserMethod: {identifier}");
                    return UserMethod.MAX;
            }
        }

    }
}
