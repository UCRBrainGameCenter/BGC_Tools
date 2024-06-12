using System;
using BGC.Scripting;
using BGC.Parameters.Exceptions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace BGC.Parameters.Algorithms.AdaptiveScan
{
    [PropertyChoiceTitle("Adaptive Scan", "AdaptiveScan")]
    [PropertyChoiceInfo("A Divide-And-Conquer algorithm for quickly scanning a parameter space.  " +
        "Ideally, you want the threshold to lie somewhere inside the first scan.  " +
        "The stepsize halves each (successful) scan, to a minimum of 1.  " +
        "A successful scan is one where the estimated threshold is inside the scan range.")]
    [IntFieldDisplay("Steps", displayTitle: "Steps Per Scan", initial: 11, minimum: 1, maximum: 10_000)]
    [IntFieldDisplay("InitialStepSize", displayTitle: "Initial Step Size", initial: 4, minimum: 1, maximum: 10_000)]
    [IntFieldDisplay("MaximumSlideDistance", displayTitle: "Maximum Slide Distance", initial: 999, minimum: 0)]
    [BoolDisplay("NarrowOnInvalidScan", "Narrow on Invalid Scan", false)]
    [IntFieldDisplay("ThresholdScanCount", "Threshold Scan Count", initial: 1, minimum: 1)]
    [BoolDisplay("NarrowingTermination", "Stop At Max Narrowing", true)]
    [IntFieldDisplay("NonAdaptiveScansCount", "Non-Adaptive Scans Count", initial: 0, minimum: 0)]
    public class AdaptiveScanAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [DisplayInputField("Steps")]
        public int Steps { get; set; }

        [DisplayInputField("InitialStepSize")]
        public int InitialStepSize { get; set; }

        [DisplayInputField("MaximumSlideDistance")]
        public int MaximumSlideDistance { get; set; }

        [DisplayInputField("NarrowOnInvalidScan")]
        public bool NarrowOnInvalidScan { get; set; }

        [DisplayInputField("ThresholdScanCount")]
        public int ThresholdScanCount { get; set; }

        [DisplayInputField("NonAdaptiveScansCount")]
        public int NonAdaptiveScansCount { get; set; }

        [AppendSelection(
            typeof(ScalarNarrowBehavior),
            typeof(DifferenceNarrowBehavior))]
        public INarrowingBehavior NarrowingBehavior { get; set; }

        [AppendSelection(
            typeof(ClampOutOfBoundsBehavior),
            typeof(RepeatOutOfBoundsBehavior),
            typeof(TruncateOutOfBoundsBehavior))]
        public IOutOfBoundsBehavior OutOfBoundsBehavior { get; set; }

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
        private int nonAdaptiveScansCount;

        private List<int> curScanSteps;

        private double taskGuessRate;
        private readonly List<double> thresholdList = new();

        private bool exceededMaxNarrowing;

        public void Initialize(double taskGuessRate)
        {
            trial = 0;
            correctCount = 0;
            stepSize = InitialStepSize;
            scanStartStep = 0;
            scanCount = 0;
            nonAdaptiveScansCount = NonAdaptiveScansCount;
            exceededMaxNarrowing = false;
            thresholdList.Clear();

            this.taskGuessRate = taskGuessRate;
        }

        protected override void FinishInitialization()
        {
            curScanSteps = GenerateScanWithNarrowing();
            if (curScanSteps.Count != 0 && curScanSteps.Count > OutOfBoundsBehavior.MinimumSteps)
            {
                SetStepValue(0, curScanSteps[0]);
            }
        }

        public void SubmitTrialResult(bool correct)
        {
            if (correct)
            {
                correctCount++;
            }

            trial++;

            if (trial >= curScanSteps.Count || ScanTerminationRule.IsDone(trial - correctCount))
            {
                // Handle Narrowing or Sliding
                double stepThreshold = (correctCount - trial * taskGuessRate) / (1 - taskGuessRate);

                int stepThresholdFloor = Math.Clamp((int)Math.Floor(stepThreshold), 0, trial - 1);
                int stepThresholdCeil = Math.Clamp((int)Math.Ceiling(stepThreshold), 0, trial - 1);
                double stepThresholdRemainder = stepThreshold - stepThresholdFloor;

                // The threshold is based on the average of the closest two steps
                double newThresholdStep = curScanSteps[stepThresholdFloor] * (1 - stepThresholdRemainder) +
                    curScanSteps[stepThresholdCeil] * stepThresholdRemainder;

                // Store the threshold for this scan
                thresholdList.Add(newThresholdStep);

                if (nonAdaptiveScansCount <= 0)
                {
                    int newScanStartStep = scanStartStep;
                    if (stepThreshold < 2.0)
                    {
                        //Slide Up
                        newScanStartStep -= (stepSize * curScanSteps.Count) / 2;
                    }
                    else if (stepThreshold >= trial - 1)
                    {
                        //Slide Down
                        newScanStartStep += (stepSize * curScanSteps.Count) / 2;
                    }
                    else
                    {
                        //Narrow
                        Narrow();

                        newScanStartStep = (int)Math.Round(newThresholdStep - stepSize * (curScanSteps.Count - 1) / 2.0);
                    }

                    // Clamp the scan start to the configured bounds
                    newScanStartStep = ClampStep(scanStartStep, newScanStartStep);

                    // Clamp the distance of the slide
                    int distance = newScanStartStep - scanStartStep;
                    int distanceAbs = Math.Abs(distance);
                    int distanceSign = Math.Sign(distance);
                    if (distanceAbs > MaximumSlideDistance)
                    {
                        newScanStartStep = scanStartStep + MaximumSlideDistance * distanceSign;
                    }

                    // Store the new scan start
                    scanStartStep = newScanStartStep;

                    // Generate the new scan
                    curScanSteps = GenerateScanWithNarrowing();
                }
                else
                {
                    nonAdaptiveScansCount--;
                }

                scanCount++;
                trial = 0;
                correctCount = 0;
            }

            // This is just an edge case check, and if this happens IsDone() ought to be returning true at this point
            // which will prevent this trial from actually being used.
            if (trial < curScanSteps.Count)
            {
                SetStepValue(0, curScanSteps[trial]);
            }
            else if (!IsDone())
            {
                UnityEngine.Debug.LogError($"IsDone() returned false, but scan is invlid.\ntrial: {trial}\ncurScanSteps ({curScanSteps.Count}): [{string.Join(", ", curScanSteps)}]");
            }
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            double avgThreshold = 0.0;
            if (thresholdList.Count > 0 && ThresholdScanCount > 0)
            {
                avgThreshold = thresholdList.TakeLast(ThresholdScanCount).Average();
            }

            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(avgThreshold);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public override bool IsDone() =>
            (NarrowingTermination && exceededMaxNarrowing) ||
            StoppingRule.IsDone(scanCount) ||
            (curScanSteps != null && curScanSteps.Count < OutOfBoundsBehavior.MinimumSteps);

        private int ClampStep(int fromStep, int toStep)
        {
            // Quick check to see if toStep is valid
            if (CouldStepTo(toStep))
            {
                return toStep;
            }

            // It is easier to search within a normalized range
            int distance = Math.Abs(fromStep - toStep);
            int low = 1; // No need to check 0 since fromStep is assumed valid
            int high = distance - 1; // No need to check distance because toStep was checked above and shown to be invalid
            int lastValidStep = fromStep; // Default return if the search finds no valid values

            // Binary search for the closest value to toStep that can be stepped to
            while (low <= high)
            {
                // 'mid' is in normalized space
                int mid = low + (high - low) / 2;

                // Convert 'mid' back to original range
                int actualMid = fromStep + (toStep > fromStep ? mid : -mid);

                if (CouldStepTo(actualMid))
                {
                    lastValidStep = actualMid;
                    low = mid + 1;  // Expand search
                }
                else
                {
                    high = mid - 1;  // Shrink search
                }
            }

            // Convert last valid step back to original range for the return value
            return lastValidStep;
        }

        private bool CouldStepTo(int step) => CouldStepTo(0, step);

        private List<int> GenerateScanWithNarrowing()
        {
            (var scan, bool scanIsValid) = GenerateScan();
            if (NarrowOnInvalidScan && !scanIsValid)
            {
                Narrow();
                scan = GenerateScan().Item1;
            }

            return scan;
        }

        private (List<int>, bool) GenerateScan()
        {
            bool scanIsValid = true;

            // The first step is always valid because this is enforced by ClampStep()
            List<int> result = new List<int>() { scanStartStep };
            
            for (int stepIndex = 1; stepIndex < Steps; stepIndex++)
            {
                int curStep = scanStartStep + stepSize * stepIndex;

                if (CouldStepTo(curStep))
                {
                    result.Add(curStep);
                }
                else
                {
                    scanIsValid = false;

                    if (OutOfBoundsBehavior is RepeatOutOfBoundsBehavior)
                    {
                        result.Add(result[^1]);
                    }
                    else if (OutOfBoundsBehavior is ClampOutOfBoundsBehavior)
                    {
                        int clampedStep = ClampStep(result[^1], curStep);
                        result.Add(clampedStep);
                    }
                }
            }

            return (result, scanIsValid);
        }

        private void Narrow()
        {
            stepSize = NarrowingBehavior.Narrow(stepSize);
            if (stepSize <= 0)
            {
                exceededMaxNarrowing = true;
                stepSize = 1;
            }
        }

        #endregion Handler
    }
}
