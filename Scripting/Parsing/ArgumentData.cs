using System;

namespace BGC.Scripting
{
    public readonly struct ArgumentData
    {
        public readonly IdentifierToken identifierToken;
        public readonly Type valueType;
        public readonly ArgumentType argumentType;

        public ArgumentData(
            IdentifierToken identifierToken,
            Type valueType,
            ArgumentType argumentType = ArgumentType.Standard)
        {
            this.identifierToken = identifierToken;

            if (valueType.IsByRef)
            {
                valueType = valueType.GetElementType()!;
                if (argumentType == ArgumentType.Standard)
                {
                    argumentType = ArgumentType.Ref;
                }
            }

            this.valueType = valueType;
            this.argumentType = argumentType;
        }

        public ArgumentData(
            string identifier,
            Type valueType,
            ArgumentType argumentType = ArgumentType.Standard)
        {
            identifierToken = new IdentifierToken(0, 0, identifier);

            if (valueType.IsByRef)
            {
                valueType = valueType.GetElementType()!;
                if (argumentType == ArgumentType.Standard)
                {
                    argumentType = ArgumentType.Ref;
                }
            }

            this.valueType = valueType;
            this.argumentType = argumentType;
        }

        private bool Matches(Type valueType, ArgumentType argumentType)
        {
            if (this.argumentType != argumentType)
            {
                if (this.argumentType == ArgumentType.In && argumentType == ArgumentType.Standard)
                {
                    //Allow the in parameters to be passed without the keyword
                }
                else
                {
                    return false;
                }
            }

            switch (this.argumentType)
            {
                case ArgumentType.Standard:
                case ArgumentType.In:
                case ArgumentType.Ref:
                case ArgumentType.Out:
                    return this.valueType == valueType;

                default:
                    throw new Exception($"Unsupported ArgumentType: {this.argumentType}");
            }
        }

        private bool LooselyMatches(Type valueType, ArgumentType argumentType)
        {
            if (this.argumentType != argumentType)
            {
                if (this.argumentType == ArgumentType.In && argumentType == ArgumentType.Standard)
                {
                    //Allow the in parameters to be passed without the keyword
                }
                else
                {
                    return false;
                }
            }

            switch (this.argumentType)
            {
                case ArgumentType.Standard:
                case ArgumentType.In:
                    return this.valueType.AssignableOrConvertableFromType(valueType);

                case ArgumentType.Ref:
                    return this.valueType == valueType;

                case ArgumentType.Out:
                    return valueType.IsAssignableFrom(this.valueType);

                default:
                    throw new Exception($"Unsupported ArgumentType: {this.argumentType}");
            }
        }

        public bool Matches(in InvocationArgument other)
        {
            Type otherType;

            if (other.expression is IValueGetter valueGetter)
            {
                otherType = valueGetter.GetValueType();
            }
            else if (other.expression is IValueSetter valueSetter)
            {
                otherType = valueSetter.GetValueType();
            }
            else
            {
                throw new Exception($"Unable to get ValueType: {other.expression}");
            }

            return Matches(otherType, other.argumentType);
        }

        public bool LooselyMatches(in InvocationArgument other)
        {
            Type otherType;

            if (other.expression is IValueGetter valueGetter)
            {
                otherType = valueGetter.GetValueType();
            }
            else if (other.expression is IValueSetter valueSetter)
            {
                otherType = valueSetter.GetValueType();
            }
            else
            {
                throw new Exception($"Unable to get ValueType: {other.expression}");
            }

            return LooselyMatches(otherType, other.argumentType);
        }

        public bool Matches(in ArgumentData other) => Matches(other.valueType, other.argumentType);

        public bool LooselyMatches(in ArgumentData other) => LooselyMatches(other.valueType, other.argumentType);

        public bool MatchesType(in ArgumentData other) =>
            valueType == other.valueType;

        public override string ToString() =>
            $"{valueType.Name} {PrintArgumentType(argumentType)}{identifierToken.identifier}";

        private static string PrintArgumentType(ArgumentType argumentType)
        {
            switch (argumentType)
            {
                case ArgumentType.Standard: return "";
                case ArgumentType.In: return "in ";
                case ArgumentType.Ref: return "ref ";
                case ArgumentType.Out: return "out ";
                case ArgumentType.Params: return "params ";

                default:
                    throw new Exception($"Unsupported ArgumentType: {argumentType}");
            }
        }
    }
}