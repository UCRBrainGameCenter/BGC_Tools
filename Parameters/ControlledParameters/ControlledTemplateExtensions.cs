namespace BGC.Parameters
{
    public static class ControlledTemplateExtensions
    {
        public static string GetThresholdEstimate(this ControlledParameterTemplate template)
        {
            if (template is IDoubleParameterTemplate doubleTemplate)
            {
                return doubleTemplate.GetThresholdEstimate().ToString();
            }
            else if (template is IIntParameterTemplate intTemplate)
            {
                return intTemplate.GetThresholdEstimate().ToString();
            }
            else if (template is IStringParameterTemplate strTemplate)
            {
                return strTemplate.GetOutput();
            }

            UnityEngine.Debug.LogError($"Parameter Template Type not supported: {template}");
            return "";
        }

        public static ControlledParameterTemplate BuildTemplate(this IControlled controlledParameter)
        {
            switch (controlledParameter.ControlledBasis)
            {
                case ControlledBasis.FloatingPoint:
                    return new ControlledDoubleParameterTemplate(controlledParameter);

                case ControlledBasis.Integer:
                    return new ControlledIntParameterTemplate(controlledParameter);

                case ControlledBasis.String:
                    return new ControlledStringParameterTemplate(controlledParameter);

                default:
                    UnityEngine.Debug.LogError($"Unexpected ControlledBasis: {controlledParameter.ControlledBasis}");
                    return null;
            }
        }
    }
}
