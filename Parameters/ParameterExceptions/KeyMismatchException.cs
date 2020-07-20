using System;

namespace BGC.Parameters.Exceptions
{
    public class KeyMismatchException : Exception
    {
        public readonly string keyName;
        public readonly string keyPath;
        public readonly Type desiredType;
        public readonly Type encounteredType;

        public KeyMismatchException(
            string keyName,
            string keyPath,
            Type desiredType,
            Type encounteredType,
            string message)
            : base(message)
        {
            this.keyName = keyName;
            this.keyPath = keyPath;

            this.desiredType = desiredType;
            this.encounteredType = encounteredType;
        }
    }
}
