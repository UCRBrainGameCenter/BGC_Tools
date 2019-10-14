using System;
using System.Collections;
using BGC.Users;

namespace BGC.Scripting
{
    public class SetUserValueMethod : Statement
    {
        private readonly IValueGetter keyArg;
        private readonly IValueGetter valueArg;
        private readonly Type valueType;

        public SetUserValueMethod(
            IValueGetter keyArg,
            IValueGetter valueArg,
            UserMethod userMethod,
            Token source)
        {
            switch (userMethod)
            {
                case UserMethod.SetString:
                    valueType = typeof(string);
                    break;

                case UserMethod.SetBool:
                    valueType = typeof(bool);
                    break;

                case UserMethod.SetDouble:
                    valueType = typeof(double);
                    break;

                case UserMethod.SetInt:
                    valueType = typeof(int);
                    break;

                case UserMethod.SetList:
                    valueType = typeof(IList);
                    break;

                default:
                    throw new Exception($"Unexpected UserMethod: {userMethod}");
            }


            if (keyArg.GetValueType() != typeof(string))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Key Argument {keyArg} of User.{userMethod} is not a string: type {keyArg.GetValueType().Name}");
            }

            if (!valueType.AssignableFromType(valueArg.GetValueType()))
            {
                throw new ScriptParsingException(
                    source: source,
                    message: $"Value Argument {valueArg} of User.{userMethod} is type {valueType.Name} and not assignable to type {valueArg.GetValueType().Name}");
            }

            this.keyArg = keyArg;
            this.valueArg = valueArg;
        }

        public override FlowState Execute(ScopeRuntimeContext context)
        {
            if (valueType == typeof(string))
            {
                PlayerData.SetString(keyArg.GetAs<string>(context), valueArg.GetAs<string>(context));
            }
            else if (valueType == typeof(bool))
            {
                PlayerData.SetBool(keyArg.GetAs<string>(context), valueArg.GetAs<bool>(context));
            }
            else if (valueType == typeof(double))
            {
                PlayerData.SetDouble(keyArg.GetAs<string>(context), valueArg.GetAs<double>(context));
            }
            else if (valueType == typeof(int))
            {
                PlayerData.SetInt(keyArg.GetAs<string>(context), valueArg.GetAs<int>(context));
            }
            else if (valueType == typeof(IList))
            {
                PlayerData.SetJsonValue(
                    key: keyArg.GetAs<string>(context),
                    value: GetUserListFunction.SerializeList(valueArg.GetAs<IList>(context)));
            }
            else
            {
                throw new Exception($"Unexpected ValueType for SetValue: {valueType.Name}");
            }

            return FlowState.Nominal;
        }
    }
}
