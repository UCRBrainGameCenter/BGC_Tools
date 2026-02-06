using NUnit.Framework;

namespace BGC.Procedures.Tests
{
    public class TrialResultTests
    {
        [Test]
        public void Constructor_SetsOutcome()
        {
            var result = new TrialResult(TrialOutcome.Correct);
            Assert.That(result.Outcome, Is.EqualTo(TrialOutcome.Correct));
        }

        [Test]
        public void Equality_WorksByValue()
        {
            var r1 = new TrialResult(TrialOutcome.Correct);
            var r2 = new TrialResult(TrialOutcome.Correct);
            var r3 = new TrialResult(TrialOutcome.Incorrect);
            
            Assert.That(r1, Is.EqualTo(r2));
            Assert.That(r1, Is.Not.EqualTo(r3));
        }
    }
}
