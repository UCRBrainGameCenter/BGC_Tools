namespace BGC.Parameters.Algorithms.Scripted
{
    public static class ScriptedAlgorithmExtensions
    {
        public static string ToDisplayName(this StepScheme scheme)
        {
            switch (scheme)
            {
                case StepScheme.Relative: return "Relative";
                case StepScheme.Absolute: return "Absolute";

                default:
                    UnityEngine.Debug.LogError($"Unexpected StepScheme: {scheme}");
                    return "";
            }
        }

    }
}
