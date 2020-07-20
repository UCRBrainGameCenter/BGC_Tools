namespace BGC.Parameters
{
    public interface IStringParameterTemplate
    {
        bool CouldStepTo(int stepNumber);
        string GetValue(int stepNumber);
        string GetOutput();
    }
}
