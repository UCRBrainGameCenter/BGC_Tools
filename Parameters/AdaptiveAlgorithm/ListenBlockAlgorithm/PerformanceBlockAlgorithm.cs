using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Parameters.Algorithms.SimpleStaircase;
using BGC.Parameters.Exceptions;
using BGC.Scripting;
using LightJson;
using UnityEngine;

namespace BGC.Parameters.Algorithms
{
    /// <summary>Uses participant performance to determine whether to step up or down.</summary>
    [PropertyChoiceTitle("Performance Block Algorithm")]
    [DoubleFieldDisplay("PerformanceThreshold", displayTitle: "Performance Threshold", initial: 0.75d, minimum: 0d, maximum: 1d)]
    [DoubleFieldDisplay("StepDownThreshold", displayTitle: "Step Down Threshold", initial: 1.0d, minimum: 0d, maximum: 1d)]
    public class PerformanceBlockAlgorithm : AlgorithmBase, IBlockOutcomeAlgorithm
    {
        [AppendSelection(
            typeof(TrialCountTermination),
            typeof(TestDurationTermination))]
        public ITerminationRule TerminationRule { get; set; }

        [DisplayInputField("PerformanceThreshold")]
        [PropertyGroupInfo("Specifies the minimum performance required (correct / totalTrials) in a block to maintain the current difficulty. If user performs worse than this value, then the algorithm steps up the difficulty, making it easier.")]
        public double PerformanceThreshold { get; set; }

        [DisplayInputField("StepDownThreshold")]
        [PropertyGroupInfo("Specifies the minimum performance required (correct / totalTrials) in a block to increase difficulty for the next block. If user performs greater than or equal to this value, then the algorithm steps down the difficulty, making it easier.")]
        public double StepDownThreshold { get; set; }

        // [DisplayInputField("MaxTrialsPerBlock")]
        // public int MaxTrialsPerBlock { get; set; }

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

        public double SubmitBlockResults(
            int numTrials,
            int numTrialsCorrect)
        {
            ++trial;
            int stepDiff = 0;

            double accuracy = numTrialsCorrect / (double)numTrials;
            int errorCount = numTrials - numTrialsCorrect;
        
            //Check for advancement or regression
            if (accuracy < PerformanceThreshold)
            {
                // decrease difficulty.
                stepDiff = errorCount;
            }
            else if (accuracy >= StepDownThreshold)
            {
                // increase difficulty
                stepDiff = -1;
            }
            
            if (stepDiff != 0)
            {
                if (lastStep != 0 || stepDiff > 0)
                {
                    lastStep = stepDiff;
                }

                Debug.Log($"Block Algo is Stepping: Diff = {stepDiff}; final = {stepValue + stepDiff}");
                StepStatus stepStatus = SetStepValue(0, stepValue + stepDiff);

                if (stepStatus == StepStatus.Success)
                {
                    Debug.Log("Block Algo successfully stepped");
                    stepValue += stepDiff;
                    correctCount = 0;
                    incorrectCount = 0;
                }
                else
                {
                    Debug.Log($"Block Algo did not successfully step. Result = {stepStatus}");
                }
            }
        
            return accuracy;
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