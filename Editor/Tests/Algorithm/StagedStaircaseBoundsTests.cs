using BGC.Extensions.Linq;
using BGC.Parameters;
using BGC.Parameters.Algorithms.SimpleStaircase;
using BGC.Parameters.Algorithms.StagedStaircase;
using LightJson;
using NUnit.Framework;

namespace BGC.Tests
{
    /// <summary>
    /// <see cref="StagedStaircaseAlgorithm"/> at its Min/Max bound (PART-976). A step that would
    /// leave the parameter's range fails. It counts as a reversal, and it must reset the
    /// correct/wrong counters, as a successful step does. Otherwise every further response in the
    /// same direction retries the step, adds another reversal, and a staircase pinned at a bound
    /// runs up its reversal count and terminates early.
    /// </summary>
    public class StagedStaircaseBoundsTests
    {
        private class ControlledValue : IControlled
        {
            public double stepValue;
            public ControlledBasis ControlledBasis => ControlledBasis.FloatingPoint;

            public void Deserialize(JsonObject data) => throw new System.NotImplementedException();
            public IPropertyGroup GetParent() => throw new System.NotImplementedException();
            public JsonObject Serialize() => throw new System.NotImplementedException();
            public void SetParent(IPropertyGroup parent) => throw new System.NotImplementedException();

            public StepStatus StepTo(int stepNumber, ControlledParameterTemplate template)
            {
                stepValue = ((IDoubleParameterTemplate)template).GetValue(stepNumber);
                return StepStatus.Success;
            }

            public double GetPartialStepValue(double stepValue, ControlledParameterTemplate template) =>
                ((IDoubleParameterTemplate)template).GetPartialValue(stepValue);

            public string GetValueString() => stepValue.ToString();
        }

        private ControlledValue value;
        private StagedStaircaseAlgorithm algorithm;

        /// <summary>
        /// Starts at 50, 5 per step, bounded to [45, 55], so one step in either direction reaches a
        /// bound. Correct responses raise the value (DecreaseParameter false, as the notch maskers).
        /// 3 correct to step one way, 2 wrong to step the other.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            SimpleDoubleLinearSteps steps = new SimpleDoubleLinearSteps()
            {
                BaseValue = 50,
                Minimum = 45,
                Maximum = 55,
                BaseStepSize = 5,
                DecreaseParameter = false
            };
            ((ISimpleDoubleStepTemplate)steps).Initialize();

            value = new ControlledValue();
            ControlledDoubleParameterTemplate template = new ControlledDoubleParameterTemplate(value)
            {
                ControllerParameter = 0,
                StepTemplate = steps
            };

            algorithm = new StagedStaircaseAlgorithm()
            {
                CorrectToStepDown = 3,
                WrongToStepUp = 2,
                TerminationRule = new ReversalCountTermination() { Value = 5 },
                StaircaseValues = new Staircase1Stage() { Stage1StepsUp = 1, Stage1StepsDown = 1 }
            };

            algorithm.Initialize(0.5);
            algorithm.RegisterControlledParameters(template.ToSingleItemEnumberable());
            Assert.AreEqual(50.0, value.stepValue);
        }

        private int Reversals => algorithm.GetTrialMetaData()["Reversals"].AsInteger;

        private void Submit(bool correct, int count)
        {
            for (int i = 0; i < count; i++)
            {
                algorithm.SubmitTrialResult(correct);
            }
        }

        [Test]
        public void AtMaximum_FailedStepResetsCorrectCount()
        {
            Submit(true, 3);
            Assert.AreEqual(55.0, value.stepValue, "Three correct step to the maximum");
            Assert.AreEqual(0, Reversals);

            Submit(true, 3);
            Assert.AreEqual(55.0, value.stepValue);
            Assert.AreEqual(1, Reversals, "The failed step past the maximum counts as one reversal");

            Submit(true, 2);
            Assert.AreEqual(1, Reversals, "Two more correct must not retry the step (the count was reset)");

            Submit(true, 1);
            Assert.AreEqual(2, Reversals, "The third correct retries it");
        }

        [Test]
        public void AtMinimum_FailedStepResetsWrongCount()
        {
            Submit(false, 2);
            Assert.AreEqual(45.0, value.stepValue, "Two wrong step to the minimum");
            Assert.AreEqual(0, Reversals);

            Submit(false, 2);
            Assert.AreEqual(45.0, value.stepValue);
            Assert.AreEqual(1, Reversals, "The failed step past the minimum counts as one reversal");

            Submit(false, 1);
            Assert.AreEqual(1, Reversals, "One more wrong must not retry the step (the count was reset)");

            Submit(false, 1);
            Assert.AreEqual(2, Reversals, "The second wrong retries it");
        }

        [Test]
        public void PinnedAtMaximum_TerminatesAfterFullRunsOnly()
        {
            // 3 correct reach the maximum. Each further reversal then takes 3 correct responses.
            int trials = 0;
            while (!algorithm.IsDone() && trials < 100)
            {
                algorithm.SubmitTrialResult(true);
                trials++;
            }

            Assert.IsTrue(algorithm.IsDone());
            Assert.AreEqual(3 + 5 * 3, trials, "Trials to reach 5 reversals while pinned at the maximum");
        }
    }
}
