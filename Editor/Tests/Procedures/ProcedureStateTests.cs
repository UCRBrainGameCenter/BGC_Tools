using NUnit.Framework;

namespace BGC.Procedures.Tests
{
    public class ProcedureStateTests
    {
        // Concrete implementation to test the abstract base
        private record TestState : ProcedureState
        {
            public int Value { get; init; }
        }

        [Test]
        public void DefaultStepNumber_IsZero()
        {
            var state = new TestState();
            Assert.That(state.StepNumber, Is.EqualTo(0));
        }

        [Test]
        public void WithExpression_CreatesNewInstance()
        {
            var state1 = new TestState { Value = 10 };
            var state2 = state1 with { Value = 20 };
            
            Assert.That(state1.Value, Is.EqualTo(10));
            Assert.That(state2.Value, Is.EqualTo(20));
            Assert.That(state1, Is.Not.SameAs(state2));
        }

        [Test]
        public void Equality_WorksByValue()
        {
            var state1 = new TestState { StepNumber = 5, Value = 10 };
            var state2 = new TestState { StepNumber = 5, Value = 10 };
            var state3 = new TestState { StepNumber = 5, Value = 99 };
            
            Assert.That(state1, Is.EqualTo(state2));
            Assert.That(state1, Is.Not.EqualTo(state3));
        }
    }
}
