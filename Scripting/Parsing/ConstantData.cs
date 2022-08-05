using System;

namespace BGC.Scripting
{
    public readonly struct ConstantData
    {
        public readonly IdentifierToken identifierToken;
        public readonly Type valueType;
        public readonly object value;

        public ConstantData(
            IdentifierToken identifierToken,
            Type valueType,
            object value)
        {
            this.identifierToken = identifierToken;
            this.valueType = valueType;
            this.value = value;
        }

        public bool MatchesType(in VariableData other) =>
            valueType == other.valueType;

        public override string ToString() => $"{valueType.Name} {identifierToken.identifier}";
    }
}