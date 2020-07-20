using System;

namespace BGC.Parameters
{
    public class KeyEmptyException : Exception
    {
        public readonly string keyPath;

        public KeyEmptyException(string keyPath, string message)
            : base(message)
        {
            this.keyPath = keyPath;
        }
    }
}
