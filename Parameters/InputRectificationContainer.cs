using System.Collections.Generic;

namespace BGC.Parameters
{
    public class InputRectificationContainer
    {
        public readonly List<string> unsatisfiedVariables = new List<string>();
        public readonly Dictionary<string, BGC.Scripting.KeyInfo> typeMapping = new Dictionary<string, BGC.Scripting.KeyInfo>();
    }
}
