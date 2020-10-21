using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;
using BGC.DataStructures.Generic;

namespace BGC.Parameters.Algorithms.BlockwiseStaircase
{
    [PropertyChoiceTitle("Blockwise Staircase")]
    [IntFieldDisplay("StepsUp", displayTitle: "Staircase Steps Up", initial: 2, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("StepsDown", displayTitle: "Staircase Steps Down", initial: 1, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("BlockSteps", displayTitle: "Block Step Count", initial: 5, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("BlockTracks", displayTitle: "Block Track Count", initial: 1, minimum: 1, maximum: 10_000, postfix: "tracks")]
    [BoolDisplay("ShuffleBlockTrials", displayTitle: "Shuffle Block Trials", initial: true)]
    public class BlockwiseStaircaseAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [AppendSelection(
            typeof(BlockCountTermination),
            typeof(TrialCountTermination),
            typeof(ReversalCountTermination),
            typeof(TestDurationTermination))]
        public ITerminationRule TerminationRule { get; set; }

        [AppendSelection(
            typeof(ErrorCountStepSpecification),
            typeof(ErrorRateStepSpecification))]
        public IStepSpecification StepSpecification { get; set; }

        [DisplayInputField("StepsUp")]
        public int StepsUp { get; set; }
        [DisplayInputField("StepsDown")]
        public int StepsDown { get; set; }

        [DisplayInputField("ShuffleBlockTrials")]
        public bool ShuffleBlockTrials { get; set; }

        [DisplayInputField("BlockSteps")]
        public int BlockSteps { get; set; }

        [DisplayInputField("BlockTracks")]
        public int BlockTracks { get; set; }

        private enum ControlParameter
        {
            Staircase = 0,
            Block,
            MAX
        }

        #region IControlSource

        public override int GetSourceCount() => (int)ControlParameter.MAX;

        public override string GetSourcePathDisplayName(int index)
        {
            switch ((ControlParameter)index)
            {
                case ControlParameter.Staircase: return "Staircase Parameter";
                case ControlParameter.Block: return "Block Parameter";

                default:
                    throw new ParameterizedCompositionException(
                        $"Unexpected Source index: {index}",
                        this.GetGroupPath());
            }
        }

        #endregion IControlSource
        #region Handler

        private int trial;
        private int blockTrial;
        private int blockErrorCount;
        private int block;

        private int correctCount;

        private int reversals;
        private int stepValue;
        private int lastStep;

        private double taskGuessRate;

        private List<int> reversalValues;
        private IDepletable<int> blockCollection;

        public void Initialize(double taskGuessRate)
        {
            this.taskGuessRate = taskGuessRate;

            trial = 0;
            blockTrial = 0;

            block = 0;

            correctCount = 0;

            blockErrorCount = 0;

            reversals = 0;
            stepValue = 0;
            lastStep = 0;

            reversalValues = new List<int>();

            if (ShuffleBlockTrials)
            {
                blockCollection = new DepletableBag<int>();
            }
            else
            {
                blockCollection = new DepletableList<int>();
            }

            for (int blockStep = 0; blockStep < BlockSteps; blockStep++)
            {
                for (int blockTrack = 0; blockTrack < BlockTracks; blockTrack++)
                {
                    blockCollection.Add(blockStep);
                }
            }

        }

        protected override void FinishInitialization()
        {
            SetStepValue((int)ControlParameter.Staircase, 0);
            SetStepValue((int)ControlParameter.Block, blockCollection.PopNext());
        }

        public void SubmitTrialResult(bool correct)
        {
            trial++;
            blockTrial++;

            if (correct)
            {
                correctCount++;
            }
            else
            {
                blockErrorCount++;
            }

            int staircaseStepDiff = 0;

            //Block ends when the blockCollection is empty
            if (blockCollection.Count == 0)
            {
                staircaseStepDiff = StepSpecification.GetStep(blockErrorCount, blockTrial);

                if (staircaseStepDiff > 0)
                {
                    //Adjust by StepsDown
                    staircaseStepDiff *= StepsDown;
                }
                else if (staircaseStepDiff < 0)
                {
                    //Adjust by StepsUp
                    staircaseStepDiff *= StepsUp;
                }

                blockErrorCount = 0;
                blockTrial = 0;
                block++;

                blockCollection.Reset();
            }

            SetStepValue((int)ControlParameter.Block, blockCollection.PopNext());

            if (staircaseStepDiff != 0)
            {
                if (lastStep != 0 && (staircaseStepDiff > 0) != (lastStep > 0))
                {
                    //Accumulate Reversals
                    reversals++;
                    reversalValues.Add(stepValue);
                }

                if (lastStep != 0 || staircaseStepDiff > 0)
                {
                    //Don't update lastStep until we take a Step Down
                    //This prevents reversal accumulation until our first Downward step
                    lastStep = staircaseStepDiff;
                }

                StepStatus stepStatus = SetStepValue((int)ControlParameter.Staircase, stepValue + staircaseStepDiff);

                if (stepStatus == StepStatus.Success)
                {
                    stepValue += staircaseStepDiff;
                }
                else
                {
                    //Accumulate reversals if we hit the floor or ceiling
                    ++reversals;
                    reversalValues.Add(stepValue);
                }
            }
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            double averageOfReversals = reversalValues.Sum() / (double)reversalValues.Count;

            if (double.IsNaN(averageOfReversals))
            {
                averageOfReversals = stepValue;
            }

            foreach (ControlledParameterTemplate template in controlledParameters
                .Where(x => x.ControllerParameter == (int)ControlParameter.Staircase))
            {
                template.FinalizeParameters(averageOfReversals);
                template.PopulateScriptContextOutputs(scriptContext);
            }

            double blockStepValue = (correctCount - trial * taskGuessRate) / ((trial / BlockSteps) * (1.0 - taskGuessRate));

            foreach (ControlledParameterTemplate template in controlledParameters
                .Where(x => x.ControllerParameter == (int)ControlParameter.Block))
            {
                template.FinalizeParameters(blockStepValue);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public override bool IsDone() => TerminationRule.IsDone(trial, block, reversals);

        #endregion Handler
    }
}
