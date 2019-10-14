using System;
using BGC.Users;

namespace BGC.Scripting
{
    public class GetUserValueFunction : IValueGetter
    {
        private readonly IValueGetter keyArg;
        private readonly IValueGetter defaultArg;
        private readonly UserMethod userMethod;
        private readonly Type getType;

        public static GetUserValueFunction Create(
            IValueGetter[] args,
            UserMethod userMethod,
            Token source)
        {
            if (args.Length == 1)
            {
                return new GetUserValueFunction(
                    keyArg: args[0],
                    defaultArg: null,
                    userMethod: userMethod,
                    source: source);
            }
            else if (args.Length == 2)
            {
                return new GetUserValueFunction(
                    keyArg: args[0],
                    defaultArg: args[1],
                    userMethod: userMethod,
                    source: source);
            }
            else
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Expected 1 or 2 Arguments to User.{userMethod}, found: {args.Length}");
            }
        }

        public GetUserValueFunction(
            IValueGetter keyArg,
            IValueGetter defaultArg,
            UserMethod userMethod,
            Token source)
        {
            this.keyArg = keyArg;
            this.defaultArg = defaultArg;
            this.userMethod = userMethod;

            if (keyArg.GetValueType() != typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Key argument of User.{userMethod} must be a string: type {keyArg.GetValueType().Name}");
            }

            switch (userMethod)
            {
                case UserMethod.GetInt:
                    getType = typeof(int);
                    break;

                case UserMethod.GetDouble:
                    getType = typeof(double);
                    break;

                case UserMethod.GetBool:
                    getType = typeof(bool);
                    break;

                case UserMethod.GetString:
                    getType = typeof(string);
                    break;

                default:
                    throw new Exception($"Unsupported UserMethod: {userMethod}");
            }

            if (defaultArg != null && !getType.AssignableFromType(defaultArg.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Default Value argument of User.{userMethod} must be of type {getType.Name}: type {defaultArg.GetValueType().Name}");
            }
        }

        public T GetAs<T>(RuntimeContext context)
        {
            Type returnType = typeof(T);

            if (!returnType.AssignableFromType(getType))
            {
                throw new ScriptRuntimeException($"Tried to retrieve result of User.{userMethod} of type {getType.Name} as type {returnType.Name}");
            }

            switch (userMethod)
            {
                case UserMethod.GetInt:
                    return (T)Convert.ChangeType(PlayerData.GetInt(keyArg.GetAs<string>(context), defaultArg?.GetAs<int>(context) ?? 0), typeof(T));

                case UserMethod.GetDouble:
                    return (T)Convert.ChangeType(PlayerData.GetDouble(keyArg.GetAs<string>(context), defaultArg?.GetAs<double>(context) ?? 0.0), typeof(T));

                case UserMethod.GetBool:
                    return (T)Convert.ChangeType(PlayerData.GetBool(keyArg.GetAs<string>(context), defaultArg?.GetAs<bool>(context) ?? false), typeof(T));

                case UserMethod.GetString:
                    return (T)Convert.ChangeType(PlayerData.GetString(keyArg.GetAs<string>(context), defaultArg?.GetAs<string>(context) ?? ""), typeof(T));

                default:
                    throw new Exception($"Unexpected UserMethod: {userMethod}");
            }
        }

        public Type GetValueType() => getType;
    }
}
