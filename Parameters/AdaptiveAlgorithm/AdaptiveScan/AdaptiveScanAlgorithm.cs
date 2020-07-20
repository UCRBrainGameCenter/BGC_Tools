using System;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.AdaptiveScan
{
    [PropertyChoiceTitle("Adaptive Scan", "AdaptiveScan")]
    [PropertyChoiceInfo("A Divide-And-Conquer algorithm for quickly scanning a parameter space.  " +
        "Ideally, you want the threshold to lie somewhere inside the first scan.  " +
        "The stepsize halves each (successful) scan, to a minimum of 1.  " +
        "A successful scan is one where the estimated threshold is inside the scan range.")]
    [IntFieldDisplay("Steps", displayTitle: "Steps Per Scan", initial: 11, minimum: 1, maximum: 10_000)]
    [IntFieldDisplay("InitialStepSize", displayTitle: "Initial Step Size", initial: 4, minimum: 1, maximum: 10_000)]
    [BoolDisplay("NarrowingTermination", "Stop At Max Narrowing", true)]
    public class AdaptiveScanAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [DisplayInputField("Steps")]
        public int Steps { get; set; }

        [DisplayInputField("InitialStepSize")]
        public int InitialStepSize { get; set; }

        [AppendSelection(
            typeof(ErrorCountScanTerminationRule),
            typeof(NoScanTerminationRule))]
        public IScanTerminationRule ScanTerminationRule { get; set; }

        [AppendSelection(
            typeof(NoAdditionalStoppingRule),
            typeof(TotalScansStoppingRule),
            typeof(TestDurationStoppingRule))]
        public IStoppingRule StoppingRule { get; set; }

        [DisplayInputField("NarrowingTermination")]
        public bool NarrowingTermination { get; set; }

        #region IControlSource

        public override int GetSourceCount() => 1;

        public override string GetSourcePathDisplayName(int index)
        {
            if (index != 0)
            {
                throw new ParameterizedCompositionException(
                    $"Unexpected Source index: {index}",
                    this.GetGroupPath());
            }

            return "Scan Parameter";
        }

        #endregion IControlSource
        #region Handler

        private int trial;
        private int correctCount;
        private int stepSize;
        private int scanStartStep;
        private int scanCount;

        private double taskGuessRate;
        private double lastThresholdStep;

        private bool exceededMaxNarrowing;

        public void Initialize(double taskGuessRate)
        {
            trial = 0;
            correctCount = 0;
            stepSize = InitialStepSize;
            scanStartStep = 0;
            scanCount = 0;
            exceededMaxNarrowing = false;
            lastThresholdStep = 0.0;

            this.taskGuessRate = taskGuessRate;
        }

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        public void SubmitTrialResult(bool correct)
        {
            if (correct)
            {
                correctCount++;
            }

            trial++;

            if (trial % Steps == 0 || ScanTerminationRule.IsDone(trial - correctCount))
            {
                //Handle Narrowing or Sliding
                double stepThreshold = (correctCount - trial * taskGuessRate) / (1 - taskGuessRate);
                lastThresholdStep = scanStartStep + stepThreshold * stepSize;

                if (stepThreshold < 2.0)
                {
                    //Slide Up
                    scanStartStep -= (stepSize * Steps) / 2;
                }
                else if (stepThreshold >= trial - 1)
                {
                    //Slide Down
                    scanStartStep += (stepSize * Steps) / 2;
                }
                else
                {
                    //Narrow
                    stepSize /= 2;

                    if (stepSize == 0)
                    {
                        exceededMaxNarrowing = true;
                        stepSize = 1;
                    }

                    scanStartStep = (int)Math.Round(lastThresholdStep - stepSize * (Steps - 1) / 2.0);
                }

                scanCount++;
                trial = 0;
                correctCount = 0;
            }

            SetStepValue(0, scanStartStep + stepSize * trial);
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(lastThresholdStep);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public override bool IsDone() =>
            (NarrowingTermination && exceededMaxNarrowing) ||
            StoppingRule.IsDone(scanCount);

        #endregion Handler
    }
}
