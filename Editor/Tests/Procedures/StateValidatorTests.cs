using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace BGC.Procedures.Tests
{
    public class StateValidatorTests
    {
        // --- Valid States ---

        private record ValidSimpleState : ProcedureState
        {
            public int Value { get; init; }
            public string Name { get; init; }
        }

        private record ValidStateWithCollections : ProcedureState
        {
            public IReadOnlyList<int> Values { get; init; } = Array.Empty<int>();
            public int[] ArrayValues { get; init; } = Array.Empty<int>();
        }

        private record ValidStateWithDictionary : ProcedureState
        {
            public IReadOnlyDictionary<string, int> Scores { get; init; } =
                new Dictionary<string, int>();
            public IReadOnlyDictionary<int, double> Mappings { get; init; } =
                new Dictionary<int, double>();
        }

        private record ValidStateWithReadOnlyCollection : ProcedureState
        {
            public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();
        }

        private record ValidStateWithNullable : ProcedureState
        {
            public int? NullableInt { get; init; }
            public double? NullableDouble { get; init; }
        }

        private enum TestEnum { A, B, C }

        private record ValidStateWithEnum : ProcedureState
        {
            public TestEnum EnumValue { get; init; }
        }

        private record NestedState : ProcedureState
        {
            public int InnerValue { get; init; }
        }

        private record ValidStateWithNestedRecord : ProcedureState
        {
            public NestedState Nested { get; init; }
        }

        // --- Invalid States ---

        private record StateWithMutableList : ProcedureState
        {
            public List<int> Values { get; init; }
        }

        private record StateWithDictionary : ProcedureState
        {
            public Dictionary<string, int> Map { get; init; }
        }

        private record StateWithHashSet : ProcedureState
        {
            public HashSet<int> Items { get; init; }
        }

        // Note: Can't test "not a record" case because compiler enforces that
        // anything inheriting from a record must also be a record.

        // --- Tests: Valid States ---

        [Test]
        public void Validate_ValidSimpleState_ReturnsNoErrors()
        {
            var errors = StateValidator.Validate(typeof(ValidSimpleState));
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_ValidStateWithCollections_ReturnsNoErrors()
        {
            var errors = StateValidator.Validate(typeof(ValidStateWithCollections));
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_ValidStateWithDictionary_ReturnsNoErrors()
        {
            var errors = StateValidator.Validate(typeof(ValidStateWithDictionary));
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_ValidStateWithReadOnlyCollection_ReturnsNoErrors()
        {
            var errors = StateValidator.Validate(typeof(ValidStateWithReadOnlyCollection));
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_ValidStateWithNullable_ReturnsNoErrors()
        {
            var errors = StateValidator.Validate(typeof(ValidStateWithNullable));
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_ValidStateWithEnum_ReturnsNoErrors()
        {
            var errors = StateValidator.Validate(typeof(ValidStateWithEnum));
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_ValidStateWithNestedRecord_ReturnsNoErrors()
        {
            var errors = StateValidator.Validate(typeof(ValidStateWithNestedRecord));
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void ValidateOrThrow_ValidState_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => StateValidator.ValidateOrThrow<ValidSimpleState>());
        }

        // --- Tests: Invalid States ---

        [Test]
        public void Validate_StateWithMutableList_ReturnsError()
        {
            var errors = StateValidator.Validate(typeof(StateWithMutableList));

            Assert.That(errors, Has.Count.GreaterThan(0));
            Assert.That(errors, Has.Some.Contains("List"));
        }

        [Test]
        public void Validate_StateWithDictionary_ReturnsError()
        {
            var errors = StateValidator.Validate(typeof(StateWithDictionary));

            Assert.That(errors, Has.Count.GreaterThan(0));
            Assert.That(errors, Has.Some.Contains("Dictionary"));
        }

        [Test]
        public void Validate_StateWithHashSet_ReturnsError()
        {
            var errors = StateValidator.Validate(typeof(StateWithHashSet));

            Assert.That(errors, Has.Count.GreaterThan(0));
            Assert.That(errors, Has.Some.Contains("HashSet"));
        }

        [Test]
        public void ValidateOrThrow_InvalidState_ThrowsWithDetails()
        {
            var ex = Assert.Throws<StateValidationException>(
                () => StateValidator.ValidateOrThrow<StateWithMutableList>());

            Assert.That(ex.StateType, Is.EqualTo(typeof(StateWithMutableList)));
            Assert.That(ex.Errors, Has.Count.GreaterThan(0));
            Assert.That(ex.Message, Does.Contain("StateWithMutableList"));
        }
    }
}