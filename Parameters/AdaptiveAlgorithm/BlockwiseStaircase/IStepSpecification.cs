namespace BGC.Parameters.Algorithms.BlockwiseStaircase
{
    [PropertyGroupTitle("Step Specification")]
    public interface IStepSpecification : IPropertyGroup
    {
        int GetStep(int errors, int blockTrials);
    }

    [PropertyChoiceTitle("Error Count")]
    [IntFieldDisplay("StepDownErrorCount", displayTitle: "Step Down Error Count", initial: 1, minimum: -1, maximum: 100_000)]
    [IntFieldDisplay("StepUpErrorCount", displayTitle: "Step Up Error Count", initial: 3, minimum: 0, maximum: 100_000)]
    public class ErrorCountStepSpecification : StimulusPropertyGroup, IStepSpecification
    {

        [DisplayInputField("StepDownErrorCount")]
        public int StepDownErrorCount { get; set; }

        [DisplayInputField("StepUpErrorCount")]
        public int StepUpErrorCount { get; set; }

        int IStepSpecification.GetStep(int errors, int blockTrials)
        {
            if (errors >= StepUpErrorCount)
            {
                return -1;
            }

            if (errors <= StepDownErrorCount)
            {
                return 1;
            }

            return 0;
        }
    }

    [PropertyChoiceTitle("Error Rate")]
    [DoubleFieldDisplay("StepDownErrorThreshold", displayTitle: "Step Down Error Threshold", initial: 0.2, minimum: 0, maximum: 1)]
    [DoubleFieldDisplay("StepUpErrorThreshold", displayTitle: "Step Up Error Threshold", initial: 0.4, minimum: 0, maximum: 1)]
    public class ErrorRateStepSpecification : StimulusPropertyGroup, IStepSpecification
    {
        [DisplayInputField("StepDownErrorThreshold")]
        public double StepDownErrorThreshold { get; set; }

        [DisplayInputField("StepUpErrorThreshold")]
        public double StepUpErrorThreshold { get; set; }

        int IStepSpecification.GetStep(int errors, int blockTrials)
        {
            double errorRate = errors / (double)blockTrials;


            if (errorRate >= StepUpErrorThreshold)
            {
                return -1;
            }

            if (errorRate <= StepDownErrorThreshold)
            {
                return 1;
            }

            return 0;
        }
    }
}
