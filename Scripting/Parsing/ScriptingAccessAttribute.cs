using System;

namespace BGC.Scripting.Parsing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ScriptingAccessAttribute : Attribute
    {
        public readonly string alias;

        public ScriptingAccessAttribute(string alias = "")
        {
            this.alias = alias;
        }
    }
}