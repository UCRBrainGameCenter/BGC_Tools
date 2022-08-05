using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Scripting
{
    public class TypeToken : Token
    {
        public readonly Type type;
        public readonly string alias;
        //public bool IsGenericType => type.ContainsGenericParameters;
        public Type[] genericArguments = null;

        public TypeToken(int line, int column, string alias, Type type)
            : base(line, column)
        {
            this.type = type;
            this.alias = alias;
        }

        public TypeToken(Token source, string alias, Type type)
            : base(source)
        {
            this.type = type;
            this.alias = alias;
        }

        public override string ToString() => type.ToString();

        public Type BuildType() => genericArguments is null ? type : type.MakeGenericType(genericArguments!);

        public void ApplyGenericArguments(IEnumerable<Type> genericArguments)
        {
            if (this.genericArguments is not null)
            {
                throw new ScriptParsingException(this, $"Attempted to apply generic arguments to type that already had generic arguments: {this}");
            }

            this.genericArguments = genericArguments.ToArray();
        }
    }
}