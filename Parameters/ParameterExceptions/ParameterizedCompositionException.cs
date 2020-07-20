using System;

namespace BGC.Parameters.Exceptions
{
    public class ParameterizedCompositionException : Exception
    {
        public string source;

        public ParameterizedCompositionException(string message, string source)
            : base(message)
        {
            this.source = source;
        }
    }
}
