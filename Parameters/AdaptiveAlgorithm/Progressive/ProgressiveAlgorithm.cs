using System;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.Progressive
{
    [PropertyChoiceTitle("Progressive")]
    [IntFieldDisplay("Tracks", displayTitle: "Tracks", initial: 2, minimum: 1, maximum: 10000)]
    [IntFieldDisplay("Steps", displayTitle: "Steps", initial: 10, minimum: 1, maximum: 10000)]
    public class ProgressiveAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [DisplayInputField("Tracks")]
        public int Tracks { get; set; }
        [DisplayInputField("Steps")]
        public int Steps { get; set; }

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

            return "Progressive Parameter";
        }

        #endregion IControlSource
        #region Handler

        private int trial;
        private int correctCount;

        private double taskGuessRate;

        public void Initialize(double taskGuessRate)
        {
            trial = 0;
            correctCount = 0;

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

            if (trial % Tracks == 0)
            {
                SetStepValue(0, trial / Tracks);
            }
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            double stepValue = (correctCount - trial * taskGuessRate) / (Tracks * (1.0 - taskGuessRate));

            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(stepValue);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public override bool IsDone() => trial == Tracks * Steps;

        #endregion Handler
    }
}
