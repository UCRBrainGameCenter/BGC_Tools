using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Scripting
{
    public class IdentifierToken : Token
    {
        public readonly string identifier;
        public Type[] genericArguments = null;

        public IdentifierToken(int line, int column, string identifier)
            : base(line, column)
        {
            this.identifier = identifier;
        }

        public IdentifierToken(Token source, string identifier)
            : base(source)
        {
            this.identifier = identifier;
        }

        public void ApplyGenericArguments(IEnumerable<Type> genericArguments)
        {
            if (this.genericArguments is not null)
            {
                throw new ScriptParsingException(this, $"Attempted to apply genericArguments when already non-null: {this}");
            }

            this.genericArguments = genericArguments.ToArray();
        }

        public override string ToString() =>
            genericArguments is null ? identifier : $"{identifier}<{string.Join(",", genericArguments.Select(x => x.Name))}>";
    }
}