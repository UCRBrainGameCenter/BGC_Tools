using System;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.FixedCollection
{
    [PropertyChoiceTitle("Fixed Collection")]
    [IntFieldDisplay("Trials", displayTitle: "Trials", initial: 3, minimum: 1, maximum: 10_000)]
    public class FixedCollectionAlgorithm : AlgorithmBase, IResponseCollectionAlgorithm
    {
        [DisplayInputField("Trials")]
        public int Trials { get; set; }

        [DisplayInputFieldKey("Trials")]
        public string TrialsKey { get; set; }

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

        int trialCount;
        int cumulativeStepValues;

        public void Initialize()
        {
            trialCount = 0;
            cumulativeStepValues = 0;
        }

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        public void SubmitTrialResult(int stepValue)
        {
            cumulativeStepValues += stepValue;
            trialCount++;
        }

        public override bool IsDone() => trialCount == Trials;

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(0);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public double GetOutputStepValue() => cumulativeStepValues / (double)trialCount;

        #endregion Handler
    }
}
