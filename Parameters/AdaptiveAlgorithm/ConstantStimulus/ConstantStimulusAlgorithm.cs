using System;
using System.Linq;
using LightJson;
using BGC.DataStructures.Generic;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.ConstantStimulus
{
    [PropertyChoiceTitle("Method of Constant Stimulus", "ConstantStimulus")]
    [IntFieldDisplay("Tracks", displayTitle: "Tracks", initial: 1, minimum: 1, maximum: 10_000, postfix: "tracks")]
    [IntFieldDisplay("Steps", displayTitle: "Steps", initial: 10, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("Repetitions", displayTitle: "Repetitions", initial: 2, minimum: 1, maximum: 10_000, postfix: "sets")]
    public class ConstantStimulusAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [DisplayInputField("Tracks")]
        public int Tracks { get; set; }

        [AppendSelection(
            typeof(ConstantStim1Dim),
            typeof(ConstantStim2Dim),
            typeof(ConstantStim3Dim),
            typeof(ConstantStim4Dim))]
        public IConstantStimulusDimensions StimDimensions { get; set; }

        [DisplayInputField("Repetitions")]
        public int Repetitions { get; set; }

        #region IControlSource

        public override int GetSourceCount() => StimDimensions.DimensionLimit;

        public override string GetSourcePathDisplayName(int index)
        {
            switch (index)
            {
                case 0: return "Dimension 1";
                case 1: return "Dimension 2";
                case 2: return "Dimension 3";
                case 3: return "Dimension 4";

                default:
                    throw new ParameterizedCompositionException($"Unexpected SourceIndex: {index}", this.GetGroupPath());
            }
        }

        #endregion IControlSource
        #region Handler

        private double taskGuessRate;

        private int trial;
        private int repetitions;
        private int correctCount;

        private DepletableBag<ConstantStimSet> stepBag;
        private ConstantStimSet currentStimSet;

        public void Initialize(double taskGuessRate)
        {
            trial = 0;
            repetitions = 0;
            correctCount = 0;

            stepBag = new DepletableBag<ConstantStimSet>()
            {
                AutoRefill = false
            };

            int dim1Limit = StimDimensions.GetSteps(0);
            int dim2Limit = StimDimensions.GetSteps(1);
            int dim3Limit = StimDimensions.GetSteps(2);
            int dim4Limit = StimDimensions.GetSteps(3);

            for (int dim1 = 0; dim1 < dim1Limit; dim1++)
            {
                for (int dim2 = 0; dim2 < dim2Limit; dim2++)
                {
                    for (int dim3 = 0; dim3 < dim3Limit; dim3++)
                    {
                        for (int dim4 = 0; dim4 < dim4Limit; dim4++)
                        {
                            stepBag.Add(new ConstantStimSet(dim1, dim2, dim3, dim4));
                        }
                    }
                }
            }

            this.taskGuessRate = taskGuessRate;
        }


        private readonly struct ConstantStimSet
        {
            public int this[int key]
            {
                get
                {
                    switch (key)
                    {
                        case 0: return dim1Step;
                        case 1: return dim2Step;
                        case 2: return dim3Step;
                        case 3: return dim4Step;
                        default: return 0;
                    }
                }
            }

            private readonly int dim1Step;
            private readonly int dim2Step;
            private readonly int dim3Step;
            private readonly int dim4Step;

            public ConstantStimSet(int dim1Step, int dim2Step, int dim3Step, int dim4Step)
            {
                this.dim1Step = dim1Step;
                this.dim2Step = dim2Step;
                this.dim3Step = dim3Step;
                this.dim4Step = dim4Step;
            }
        }

        protected override void FinishInitialization()
        {
            currentStimSet = stepBag.PopNext();

            for (int parameter = 0; parameter < 4; parameter++)
            {
                SetStepValue(parameter, currentStimSet[parameter]);
            }
        }

        public void SubmitTrialResult(bool correct)
        {
            trial++;

            if (correct)
            {
                correctCount++;
            }

            if (stepBag.Count == 0)
            {
                repetitions++;
                stepBag.Reset();
            }

            currentStimSet = stepBag.PopNext();

            for (int parameter = 0; parameter < 4; parameter++)
            {
                SetStepValue(parameter, currentStimSet[parameter]);
            }
        }

        public override bool IsDone() => repetitions == Repetitions;

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            for (int parameter = 0; parameter < 4; parameter++)
            {
                double stepValue = (correctCount - trial * taskGuessRate) / (GetOrthogonalCount(parameter) * (1.0 - taskGuessRate));

                foreach (ControlledParameterTemplate template in controlledParameters.Where(x => x.ControllerParameter == parameter))
                {
                    template.FinalizeParameters(stepValue);
                    template.PopulateScriptContextOutputs(scriptContext);
                }
            }
        }

        private int GetOrthogonalCount(int targetParameter)
        {
            int stepCount = Repetitions * Tracks;
            for (int parameter = 0; parameter < 4; parameter++)
            {
                if (targetParameter == parameter)
                {
                    continue;
                }

                stepCount *= StimDimensions.GetSteps(parameter);
            }

            return stepCount;
        }

        #endregion Handler
    }
}
