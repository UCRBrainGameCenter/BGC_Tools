﻿using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;
using BGC.Parameters.Algorithms.SimpleStaircase;

namespace BGC.Parameters.Algorithms.StagedStaircase
{
    [PropertyChoiceTitle("Staged Staircase")]
    [IntFieldDisplay("BigSteps", displayTitle: "BigSteps", initial: 3, minimum: 1, maximum: 10_000)]
    [IntFieldDisplay("Steps", displayTitle: "Steps", initial: 1, minimum: 1, maximum: 10_000)]
    [IntFieldDisplay("Reversals", displayTitle: "Reversals", initial: 5, minimum: 1, maximum: 10_000)]
    [IntFieldDisplay("CorrectToStepDown", displayTitle: "Correct Responses To Step Down", initial: 3, minimum: 1, maximum: 10000, postfix: "hits")]
    [IntFieldDisplay("WrongToStepUp", displayTitle: "Incorrect Responses To Step Up", initial: 2, minimum: 1, maximum: 10000, postfix: "misses")]
    [BoolDisplay("LastCorrectTrialIsReversal", displayTitle: "Last Correct Trial Is Reversal", initial: false)]
    public class StagedStaircaseAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
    {
        [AppendSelection(
            typeof(ReversalCountTermination),
            typeof(TrialCountTermination),
            typeof(TestDurationTermination))]
        public ITerminationRule TerminationRule { get; set; }

        [DisplayInputField("CorrectToStepDown")]
        public int CorrectToStepDown { get; set; }
        [DisplayInputFieldKey("CorrectToStepDown")]
        public string CorrectToStepDownKey { get; set; }

        [DisplayInputField("WrongToStepUp")]
        public int WrongToStepUp { get; set; }
        [DisplayInputFieldKey("WrongToStepUp")]
        public string WrongToStepUpKey { get; set; }

        [DisplayInputField("LastCorrectTrialIsReversal")]
        public bool LastCorrectTrialIsReversal { get; set; }
        [DisplayInputFieldKey("LastCorrectTrialIsReversal")]
        public string LastCorrectTrialIsReversalKey { get; set; }

        [AppendSelection(
            typeof(Staircase1Stage),
            typeof(Staircase2Stage),
            typeof(Staircase3Stage))]
        public IStaircaseStage StaircaseValues { get; set; }

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
        private int reversals;
        private int prevCorrectStepValue;
        private bool havePrevCorrectStepValue;
        private bool prevCorrectStepWasReversal;
        private int stepValue;
        private int lastStep;
        private List<int> reversalValues;

        public void Initialize(double taskGuessRate)
        {
            trial = 0;
            reversals = 0;

            correctCount = 0;
            incorrectCount = 0;

            prevCorrectStepValue = 0;
            havePrevCorrectStepValue = false;
            prevCorrectStepWasReversal = false;
            stepValue = 0;
            lastStep = 0;

            reversalValues = new List<int>();
        }

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        private bool InFinalReversalStage()
        {
            int threshold = 0;

            for (StaircaseStage stage = StaircaseStage.Stage1; stage < StaircaseValues.GetStageLimit(); stage++)
            {
                threshold += StaircaseValues.GetReversals(stage);
            }

            if (reversals < threshold)
            {
                return false;
            }

            return true;
        }

        private int GetStepUpSize()
        {
            int threshold = 0;

            for (StaircaseStage stage = StaircaseStage.Stage1; stage < StaircaseValues.GetStageLimit(); stage++)
            {
                threshold += StaircaseValues.GetReversals(stage);

                if (reversals < threshold)
                {
                    return StaircaseValues.GetStepsUp(stage);
                }

            }

            return StaircaseValues.GetStepsUp(StaircaseValues.GetStageLimit());
        }

        private int GetStepDownSize()
        {
            int threshold = 0;

            for (StaircaseStage stage = StaircaseStage.Stage1; stage < StaircaseValues.GetStageLimit(); stage++)
            {
                threshold += StaircaseValues.GetReversals(stage);

                if (reversals < threshold)
                {
                    return StaircaseValues.GetStepsDown(stage);
                }

            }

            return StaircaseValues.GetStepsDown(StaircaseValues.GetStageLimit());
        }

        public void SubmitTrialResult(bool correct)
        {
            ++trial;

            int stepDiff = 0;

            if (correct)
            {
                prevCorrectStepValue = stepValue;
                havePrevCorrectStepValue = true;
                prevCorrectStepWasReversal = false;

                incorrectCount = 0;

                if (++correctCount >= CorrectToStepDown)
                {
                    stepDiff = GetStepDownSize();
                }
            }
            else
            {
                correctCount = 0;

                if (++incorrectCount >= WrongToStepUp)
                {
                    stepDiff = -1 * GetStepUpSize();
                }
            }

            if (stepDiff != 0)
            {
                if (lastStep != 0 && (stepDiff > 0) != (lastStep > 0))
                {
                    if (InFinalReversalStage())
                    {
                        reversalValues.Add(stepValue);

                        if (correct)
                        {
                            prevCorrectStepWasReversal = true;
                        }
                    }

                    reversals++;
                }

                if (lastStep != 0 || stepDiff > 0)
                {
                    lastStep = stepDiff;
                }

                StepStatus stepStatus = SetStepValue(0, stepValue + stepDiff);

                if (stepStatus == StepStatus.Success)
                {
                    stepValue += stepDiff;
                    correctCount = 0;
                    incorrectCount = 0;
                }
                else
                {
                    ++reversals;
                }
            }
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            List<int> allReversalValues = reversalValues.ToList();
            if (havePrevCorrectStepValue && !prevCorrectStepWasReversal && LastCorrectTrialIsReversal)
            {
                allReversalValues.Add(prevCorrectStepValue);
            }

            double averageOfReversals = allReversalValues.Count > 0 ? allReversalValues.Sum() / (double)allReversalValues.Count : stepValue;

            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(averageOfReversals);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public override bool IsDone() => TerminationRule.IsDone(trial, reversals);

        public override JsonObject GetTrialMetaData() => new JsonObject()
        {
            ["Trial"] = trial,
            ["Reversals"] = reversals,
        };

        #endregion Handler
    }
}
