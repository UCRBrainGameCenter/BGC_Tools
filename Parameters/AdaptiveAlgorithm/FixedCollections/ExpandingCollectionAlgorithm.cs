using System;
using System.Linq;
using System.Collections.Generic;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.FixedCollection
{
    [PropertyChoiceTitle("Expanding Collection")]
    [IntFieldDisplay("InitialTrials", displayTitle: "Initial Trials", initial: 2, minimum: 1, maximum: 10_000)]
    [IntFieldDisplay("StepDiffLimit", displayTitle: "Step Difference Limit", initial: 2, minimum: 0, maximum: 10_000)]
    [IntFieldDisplay("ExpandedTrials", displayTitle: "Expanded Trials", initial: 2, minimum: 1, maximum: 10_000)]
    [StringFieldDisplay("Expanded", displayTitle: "Trials Expanded")]
    public class ExpandingCollectionAlgorithm : AlgorithmBase, IResponseCollectionAlgorithm
    {
        [DisplayInputField("InitialTrials")]
        public int InitialTrials { get; set; }

        [DisplayInputFieldKey("InitialTrials")]
        public string InitialTrialsKey { get; set; }


        [DisplayInputField("StepDiffLimit")]
        public int StepDiffLimit { get; set; }

        [DisplayInputFieldKey("StepDiffLimit")]
        public string StepDiffLimitKey { get; set; }


        [DisplayInputField("ExpandedTrials")]
        public int ExpandedTrials { get; set; }

        [DisplayInputFieldKey("ExpandedTrials")]
        public string ExpandedTrialsKey { get; set; }

        [OutputField("Expanded")]
        public bool Expanded { get; set; }

        [DisplayOutputFieldKey("Expanded")]
        public string ExpandedKey { get; set; }


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

        private readonly List<int> stepValues = new List<int>();

        public void Initialize()
        {
            stepValues.Clear();
        }

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        public void SubmitTrialResult(int stepValue) => stepValues.Add(stepValue);

        public override bool IsDone()
        {
            if (stepValues.Count >= (InitialTrials + ExpandedTrials))
            {
                return true;
            }

            if (stepValues.Count == InitialTrials)
            {
                return (stepValues.Max() - stepValues.Min() <= StepDiffLimit);
            }

            return false;
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            Expanded = stepValues.Count > InitialTrials;

            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(0);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public double GetOutputStepValue() => stepValues.Average();

        #endregion Handler
    }
}
