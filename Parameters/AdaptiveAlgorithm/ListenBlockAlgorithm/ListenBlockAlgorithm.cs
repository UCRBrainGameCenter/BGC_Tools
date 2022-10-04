using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Parameters.Algorithms.SimpleStaircase;
using BGC.Parameters.Exceptions;
using BGC.Scripting;
using LightJson;

namespace BGC.Parameters.Algorithms
{
    [PropertyChoiceTitle("Listen Block Algorithm")]
    [IntFieldDisplay("MaxTrialsPerBlock", displayTitle: "Maximum number of trials in a block.", initial: 5, minimum: 1, maximum: 10_000)]
    public class ListenBlockAlgorithm : AlgorithmBase, IListenBlockOutcomeAlgorithm
    {
        [AppendSelection(
            typeof(TrialCountTermination),
            typeof(TestDurationTermination))]
        public ITerminationRule TerminationRule { get; set; }

        [DisplayInputField("MaxTrialsPerBlock")]
        public int MaxTrialsPerBlock { get; set; }

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

            return "Staircase Parameter";
        }

        #endregion IControlSource
        #region Handler

        private int trial;
        private int correctCount;
        private int incorrectCount;
        private int stepValue;
        private int lastStep;

        public void Initialize()
        {
            trial = 0;
        
            correctCount = 0;
            incorrectCount = 0;
        
            stepValue = 0;
            lastStep = 0;
        }

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        public int SubmitBlockResults(
            int trialsPerBlock,
            int trialCorrectCount,
            out double performance)
        {
            ++trial;
            
            int newTrialCount = Math.Min(trialCorrectCount + 1, MaxTrialsPerBlock);
            
            double accuracy = trialCorrectCount / (double)trialsPerBlock;
        
            // StepStatus stepStatus = SetStepValue(0, stepValue + stepDiff);
        
            performance = accuracy;
        
            return newTrialCount;
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            // double averageOfReversals = reversalValues.Sum() / (double)reversalValues.Count;
            //
            // if (double.IsNaN(averageOfReversals))
            // {
            //     averageOfReversals = stepValue;
            // }

            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                // template.FinalizeParameters(averageOfReversals);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public override bool IsDone() => TerminationRule.IsDone(trial, new());

        public override JsonObject GetTrialMetaData() => new JsonObject()
        {
            ["Trial"] = trial
        };

        #endregion Handler

        // public void Initialize(double taskGuessRate)
        // {
        //     trial = 0;
        //
        //     correctCount = 0;
        //     incorrectCount = 0;
        //
        //     stepValue = 0;
        //     lastStep = 0;
        // }
        //
        // public void SubmitTrialResult(bool correct)
        // { 
        //     ++trial;
        //     if (correct)
        //     {
        //         correctCount++;
        //     }
        //     else
        //     {
        //         incorrectCount++;
        //     }
        //
        //     if (trial % MaxTrialsPerBlock == 0)
        //     {
        //         // generate new block
        //         int newTrialCount = Math.Min(correctCount + 1, MaxTrialsPerBlock);
        //         
        //         this.trial = 0;
        //         this.correctCount = 0;
        //         this.incorrectCount = 0;
        //     }
        // }
    }
}