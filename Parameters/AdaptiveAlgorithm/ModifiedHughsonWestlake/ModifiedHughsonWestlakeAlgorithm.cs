using System;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;
using System.Collections.Generic;

namespace BGC.Parameters.Algorithms.ModifiedHughsonWestlake
{
    [PropertyChoiceTitle("Modified Hughson-Westlake", "ModifiedHughsonWestlake")]
    [PropertyChoiceInfo("An implementation of the Modified Hughson-Westlake adaptive procedure used in audiometric testing. " +
        "To implement proper support, presentation should begin at 30dB HL. " +
        "Terminates after a given step obtains at least half positive responses from ascending trials, with a minimum of 3 data points.")]
    [IntFieldDisplay("InitialJumpStepsUp", displayTitle: "Initial Jump Steps Up", initial: 4, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("InitialStepsUp", displayTitle: "Initial Steps Up", initial: 2, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("StepsUp", displayTitle: "Steps Up", initial: 1, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("StepsDown", displayTitle: "Steps Down", initial: 2, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("MinimumThresholdTrials", displayTitle: "Minimum Threshold Trials", initial: 3, minimum: 1, maximum: 10_000, postfix: "trials")]
    [DoubleFieldDisplay("MinimumPassingThreshold", displayTitle: "Minimum Passing Threshold", initial: 0.5, minimum: 0, maximum: 1)]
    [BoolDisplay("ShortCircuit", displayTitle: "Short Circuit", initial: false)]
    public class ModifiedHughsonWestlakeAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [DisplayInputField("InitialJumpStepsUp")]
        public int InitialJumpStepsUp { get; set; }

        [DisplayInputField("InitialStepsUp")]
        public int InitialStepsUp { get; set; }

        [DisplayInputField("StepsUp")]
        public int StepsUp { get; set; }

        [DisplayInputField("StepsDown")]
        public int StepsDown { get; set; }

        [DisplayInputField("MinimumThresholdTrials")]
        public int MinimumThresholdTrials { get; set; }

        [DisplayInputField("MinimumPassingThreshold")]
        public double MinimumPassingThreshold { get; set; }

        [DisplayInputField("ShortCircuit")]
        public bool ShortCircuit { get; set; }

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

            return "Hughson-Westlake Parameter";
        }

        #endregion IControlSource
        #region Handler

        private Phase phase;
        private int currentStepValue;
        private bool endTriggered;
        private int thresholdStep = 0;

        private readonly Dictionary<int, ModifiedHughsonWestlakeStep> stepDictionary = new Dictionary<int, ModifiedHughsonWestlakeStep>();

        private enum Phase
        {
            InitialBigAscension = 0,
            InitialAscension,
            Descending,
            Ascending,
            MAX
        }

        public void Initialize(double _)
        {
            stepDictionary.Clear();

            phase = Phase.InitialBigAscension;
            currentStepValue = 0;
            thresholdStep = 0;

            endTriggered = false;
        }

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        public void SubmitTrialResult(bool correct)
        {
            int stepDiff;

            switch (phase)
            {
                case Phase.InitialBigAscension:
                    if (correct)
                    {
                        phase = Phase.Descending;
                        stepDiff = StepsDown;
                    }
                    else
                    {
                        phase = Phase.InitialAscension;
                        stepDiff = -InitialJumpStepsUp;
                    }
                    break;

                case Phase.InitialAscension:
                    if (correct)
                    {
                        phase = Phase.Descending;
                        stepDiff = StepsDown;
                    }
                    else
                    {
                        //Remain in the current phase
                        stepDiff = -InitialStepsUp;
                    }
                    break;

                case Phase.Descending:
                    if (correct)
                    {
                        //Remain in the current phase
                        stepDiff = StepsDown;
                    }
                    else
                    {
                        phase = Phase.Ascending;
                        stepDiff = -StepsUp;
                    }
                    break;

                case Phase.Ascending:
                    {
                        //Add Trial
                        if (!stepDictionary.ContainsKey(currentStepValue))
                        {
                            stepDictionary.Add(currentStepValue, new ModifiedHughsonWestlakeStep());
                        }

                        stepDictionary[currentStepValue].AddTrial(correct);

                        if (stepDictionary[currentStepValue].IsPassing(ShortCircuit, MinimumThresholdTrials, MinimumPassingThreshold))
                        {
                            endTriggered = true;
                            thresholdStep = currentStepValue;
                        }

                        if (correct)
                        {
                            phase = Phase.Descending;
                            stepDiff = StepsDown;
                        }
                        else
                        {
                            //Remain in the current phase
                            stepDiff = -StepsUp;
                        }
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected Phase: {phase}");
                    goto case Phase.Descending;
            }

            StepStatus stepStatus = SetStepValue(0, currentStepValue + stepDiff);

            if (stepStatus == StepStatus.Success)
            {
                currentStepValue += stepDiff;
            }
            else
            {
                //Unable to step - Quit
                endTriggered = true;
                thresholdStep = currentStepValue;
            }
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(thresholdStep);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public override bool IsDone() => endTriggered;

        private class ModifiedHughsonWestlakeStep
        {
            private int trials = 0;
            private int hits = 0;

            public ModifiedHughsonWestlakeStep() { }

            /// <summary>
            /// Adds a trial and conditionally a hit
            /// </summary>
            public void AddTrial(bool hit)
            {
                trials++;

                if (hit)
                {
                    hits++;
                }
            }

            public bool IsPassing(bool shortCircuit, int minimumThresholdTrials, double minimumPassingThreshold) =>
                shortCircuit ?
                    IsPassingInevitable(minimumThresholdTrials, minimumPassingThreshold) :
                    IsPassingCurrently(minimumThresholdTrials, minimumPassingThreshold);


            private bool IsPassingCurrently(int minimumThresholdTrials, double minimumPassingThreshold) =>
                trials >= minimumThresholdTrials &&
                (hits / (double)trials) >= minimumPassingThreshold;

            private bool IsPassingInevitable(int minimumThresholdTrials, double minimumPassingThreshold) =>
                hits / (double)Math.Max(trials, minimumThresholdTrials) >= minimumPassingThreshold;
        }

        #endregion Handler
    }
}
