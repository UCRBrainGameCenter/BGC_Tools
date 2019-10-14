using System;

namespace BGC.Scripting
{
    public class ScriptRuntimeException : Exception
    {
        public ScriptRuntimeException(string message)
            : base(message)
        {

        }
    }
}
