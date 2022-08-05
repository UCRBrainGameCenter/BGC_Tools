using System;
using System.Linq;

namespace BGC.Scripting
{
    public readonly struct FunctionSignature
    {
        public readonly IdentifierToken identifierToken;
        public readonly Type returnType;
        public readonly ArgumentData[] arguments;
        public readonly Guid id;

        public FunctionSignature(
            IdentifierToken identifierToken,
            Type returnType,
            ArgumentData[] arguments)
        {
            this.identifierToken = identifierToken;
            this.returnType = returnType ?? typeof(void);
            this.arguments = arguments ?? Array.Empty<ArgumentData>();
            id = Guid.NewGuid();
        }

        public FunctionSignature(
            string identifier,
            Type returnType,
            params ArgumentData[] arguments)
        {
            identifierToken = new IdentifierToken(0, 0, identifier);
            this.returnType = returnType ?? typeof(void);
            this.arguments = arguments ?? Array.Empty<ArgumentData>();
            id = Guid.NewGuid();
        }

        public bool Matches(in FunctionSignature other)
        {
            if (identifierToken.identifier != other.identifierToken.identifier ||
                returnType != other.returnType)
            {
                return false;
            }

            if (arguments.Length != other.arguments.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].MatchesType(other.arguments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool MatchesArgs(InvocationArgument[] args)
        {
            if (arguments.Length != args.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].Matches(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool LooselyMatchesArgs(InvocationArgument[] args)
        {
            if (arguments.Length != args.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].LooselyMatches(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool MatchesArgs(ArgumentData[] args)
        {
            if (arguments.Length != args.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].Matches(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool LooselyMatchesArgs(ArgumentData[] args)
        {
            if (arguments.Length != args.Length)
            {
                return false;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (!arguments[i].LooselyMatches(args[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() =>
            $"{returnType.Name} {identifierToken.identifier}({string.Join(", ", arguments.Select(x => x.ToString()))})";
    }
}