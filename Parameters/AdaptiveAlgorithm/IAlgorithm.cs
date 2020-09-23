namespace BGC.Parameters.Algorithms
{
    [PropertyGroupTitle("Algorithm")]
    public interface IAlgorithm : IPropertyGroup
    {
        bool IsDone();

        ControlledParameterTemplate BuildTemplate(IControlled controlledParameter);
    }
}
