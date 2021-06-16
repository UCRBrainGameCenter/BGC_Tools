using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using BGC.Scripting;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.SimpleStaircase
{
    [PropertyChoiceTitle("Simple Staircase")]
    [IntFieldDisplay("CorrectToStepDown", displayTitle: "Correct Responses To Step Down", initial: 3, minimum: 1, maximum: 10_000, postfix: "hits")]
    [IntFieldDisplay("WrongToStepUp", displayTitle: "Incorrect Responses To Step Up", initial: 2, minimum: 1, maximum: 10_000, postfix: "misses")]
    [IntFieldDisplay("StepsUp", displayTitle: "Steps Up", initial: 2, minimum: 1, maximum: 10_000, postfix: "steps")]
    [IntFieldDisplay("StepsDown", displayTitle: "Steps Down", initial: 1, minimum: 1, maximum: 10_000, postfix: "steps")]
    public class SimpleStaircaseAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm
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

        [DisplayInputField("StepsUp")]
        public int StepsUp { get; set; }
        [DisplayInputFieldKey("StepsUp")]
        public string StepsUpKey { get; set; }

        [DisplayInputField("StepsDown")]
        public int StepsDown { get; set; }
        [DisplayInputFieldKey("StepsDown")]
        public string StepsDownKey { get; set; }

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
        private int stepValue;
        private int lastStep;
        private List<int> reversalValues;

        public void Initialize(double taskGuessRate)
        {
            trial = 0;
            reversals = 0;

            correctCount = 0;
            incorrectCount = 0;

            stepValue = 0;
            lastStep = 0;

            reversalValues = new List<int>();
        }

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        public void SubmitTrialResult(bool correct)
        {
            ++trial;

            int stepDiff = 0;

            if (correct)
            {
                incorrectCount = 0;

                if (++correctCount >= CorrectToStepDown)
                {
                    stepDiff = StepsDown;
                }
            }
            else
            {
                correctCount = 0;

                if (++incorrectCount >= WrongToStepUp)
                {
                    stepDiff = -1 * StepsUp;
                }
            }

            if (stepDiff != 0)
            {
                if (lastStep != 0 && (stepDiff > 0) != (lastStep > 0))
                {
                    reversals++;
                    reversalValues.Add(stepValue);
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
            ["Reversals"] = reversals
        };

        #endregion Handler
    }
}
