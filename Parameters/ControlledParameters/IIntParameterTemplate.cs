namespace BGC.Parameters
{
    public interface IIntParameterTemplate
    {
        bool CouldStepTo(int stepNumber);
        int GetValue(int stepNumber);
        double GetPartialValue(double stepValue);
        double GetThresholdEstimate();
    }
}
