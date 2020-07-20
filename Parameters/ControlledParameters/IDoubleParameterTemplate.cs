namespace BGC.Parameters
{
    public interface IDoubleParameterTemplate
    {
        bool CouldStepTo(int stepNumber);
        double GetValue(int stepNumber);
        double GetPartialValue(double stepNumber);
        double GetThresholdEstimate();
    }
}
