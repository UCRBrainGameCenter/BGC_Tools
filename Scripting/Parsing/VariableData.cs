using System;

namespace BGC.Scripting
{
    public readonly struct VariableData
    {
        public readonly IdentifierToken identifierToken;
        public readonly Type valueType;

        public VariableData(
            IdentifierToken identifierToken,
            Type valueType)
        {
            this.identifierToken = identifierToken;
            this.valueType = valueType;
        }

        public VariableData(
            string identifier,
            Type valueType)
        {
            identifierToken = new IdentifierToken(0, 0, identifier);
            this.valueType = valueType;
        }

        public bool Matches(in VariableData other) =>
            identifierToken.identifier == other.identifierToken.identifier &&
            valueType == other.valueType;

        public bool MatchesType(in VariableData other) =>
            valueType == other.valueType;

        public bool MatchesType(Type other) =>
            valueType == other;

        public bool LooselyMatchesType(Type other) =>
            valueType.AssignableOrConvertableFromType(other);

        public override string ToString() => $"{valueType.Name} {identifierToken.identifier}";
    }
}