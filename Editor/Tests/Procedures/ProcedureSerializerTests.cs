using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace BGC.Procedures.Tests
{
    public class ProcedureSerializerTests
    {
        private ProcedureSerializer _serializer;

        // Simple test state
        private record SimpleState : ProcedureState
        {
            public int Value { get; init; }
            public string Name { get; init; }
            public double? NullableValue { get; init; }
        }

        // State with collection
        private record StateWithList : ProcedureState
        {
            public IReadOnlyList<int> Values { get; init; } = Array.Empty<int>();
            public IReadOnlyList<bool> Flags { get; init; } = Array.Empty<bool>();
        }

        // State with dictionary
        private record StateWithDictionary : ProcedureState
        {
            public IReadOnlyDictionary<string, int> Scores { get; init; } =
                new Dictionary<string, int>();
        }

        [SetUp]
        public void SetUp()
        {
            _serializer = new ProcedureSerializer();
        }

        // --- Simple Round-Trip ---

        [Test]
        public void RoundTrip_SimpleState_PreservesValues()
        {
            var original = new SimpleState
            {
                StepNumber = 5,
                Value = 42,
                Name = "Test",
                NullableValue = 3.14
            };

            var json = _serializer.Serialize(original);
            var restored = _serializer.Deserialize<SimpleState>(json);

            Assert.That(restored, Is.EqualTo(original));
        }

        [Test]
        public void RoundTrip_SimpleState_PreservesNull()
        {
            var original = new SimpleState
            {
                StepNumber = 0,
                Value = 10,
                Name = null,
                NullableValue = null
            };

            var json = _serializer.Serialize(original);
            var restored = _serializer.Deserialize<SimpleState>(json);

            Assert.That(restored, Is.EqualTo(original));
            Assert.That(restored.Name, Is.Null);
            Assert.That(restored.NullableValue, Is.Null);
        }

        [Test]
        public void TestRoundTrip_SimpleState_ReturnsTrue()
        {
            var state = new SimpleState
            {
                StepNumber = 3,
                Value = 100,
                Name = "Test"
            };

            Assert.That(_serializer.TestRoundTrip(state), Is.True);
        }

        // --- IReadOnlyList Round-Trip ---

        [Test]
        public void RoundTrip_StateWithList_PreservesValues()
        {
            var original = new StateWithList
            {
                StepNumber = 2,
                Values = new[] { 1, 2, 3, 4, 5 },
                Flags = new[] { true, false, true }
            };

            var json = _serializer.Serialize(original);
            var restored = _serializer.Deserialize<StateWithList>(json);

            Assert.That(restored.StepNumber, Is.EqualTo(original.StepNumber));
            Assert.That(restored.Values, Is.EqualTo(original.Values));
            Assert.That(restored.Flags, Is.EqualTo(original.Flags));
        }

        [Test]
        public void RoundTrip_StateWithEmptyList_PreservesEmpty()
        {
            var original = new StateWithList
            {
                StepNumber = 0,
                Values = Array.Empty<int>(),
                Flags = Array.Empty<bool>()
            };

            var json = _serializer.Serialize(original);
            var restored = _serializer.Deserialize<StateWithList>(json);

            Assert.That(restored.Values, Is.Empty);
            Assert.That(restored.Flags, Is.Empty);
        }

        // --- IReadOnlyDictionary Round-Trip ---

        [Test]
        public void RoundTrip_StateWithDictionary_PreservesValues()
        {
            var original = new StateWithDictionary
            {
                StepNumber = 3,
                Scores = new Dictionary<string, int>
                {
                    ["alice"] = 100,
                    ["bob"] = 85
                }
            };

            var json = _serializer.Serialize(original);
            var restored = _serializer.Deserialize<StateWithDictionary>(json);

            Assert.That(restored.StepNumber, Is.EqualTo(original.StepNumber));
            Assert.That(restored.Scores["alice"], Is.EqualTo(100));
            Assert.That(restored.Scores["bob"], Is.EqualTo(85));
            Assert.That(restored.Scores.Count, Is.EqualTo(2));
        }

        [Test]
        public void RoundTrip_StateWithEmptyDictionary_PreservesEmpty()
        {
            var original = new StateWithDictionary
            {
                StepNumber = 0,
                Scores = new Dictionary<string, int>()
            };

            var json = _serializer.Serialize(original);
            var restored = _serializer.Deserialize<StateWithDictionary>(json);

            Assert.That(restored.Scores, Is.Empty);
        }

        // --- Serialization Format ---

        [Test]
        public void Serialize_ProducesValidJson()
        {
            var state = new SimpleState { Value = 42, Name = "Test" };

            var json = _serializer.Serialize(state);

            Assert.That(json, Does.Contain("\"Value\":42"));
            Assert.That(json, Does.Contain("\"Name\":\"Test\""));
        }

        [Test]
        public void SerializePretty_ProducesIndentedJson()
        {
            var state = new SimpleState { Value = 42 };

            var json = _serializer.SerializePretty(state);

            Assert.That(json, Does.Contain("\n"));
        }

        // --- Error Cases ---

        [Test]
        public void Deserialize_InvalidJson_Throws()
        {
            Assert.Throws<Newtonsoft.Json.JsonReaderException>(() =>
            {
                _serializer.Deserialize<SimpleState>("not valid json");
            });
        }

        [Test]
        public void Deserialize_NullJson_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _serializer.Deserialize<SimpleState>(null);
            });
        }
    }
}