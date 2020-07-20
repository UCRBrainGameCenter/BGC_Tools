using System;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.FixedPresentation
{
    [PropertyChoiceTitle("Fixed Presentation")]
    [IntFieldDisplay("Trials", displayTitle: "Trials", initial: 12, minimum: 1, maximum: 10_000)]
    public class FixedPresentationAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [DisplayInputField("Trials")]
        public int Trials { get; set; }

        #region IControlSource

        public override int GetSourceCount() => 0;

        public override string GetSourcePathDisplayName(int index)
        {
            throw new ParameterizedCompositionException(
                $"Unexpected Source index: {index}",
                this.GetGroupPath());
        }

        #endregion IControlSource
        #region Handler


        int trial;
        int correctCount;

        public void Initialize(double taskGuessRate)
        {
            trial = 0;
            correctCount = 0;
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
        }

        public override bool IsDone() => trial == Trials;

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(0.0);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        #endregion Handler
    }
}
