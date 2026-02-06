using NUnit.Framework;

namespace BGC.Procedures.Tests
{
    public class DifficultyOutputTests
    {
        [Test]
        public void Constructor_WithDifficultyOnly_SetsDefaults()
        {
            var output = new DifficultyOutput(50);
            
            Assert.That(output.Difficulty, Is.EqualTo(50));
            Assert.That(output.Threshold, Is.Null);
            Assert.That(output.IsComplete, Is.False);
        }

        [Test]
        public void Constructor_WithAllFields_SetsAll()
        {
            var output = new DifficultyOutput(42, threshold: 45.5, isComplete: true);
            
            Assert.That(output.Difficulty, Is.EqualTo(42));
            Assert.That(output.Threshold, Is.EqualTo(45.5));
            Assert.That(output.IsComplete, Is.True);
        }

        [Test]
        public void Equality_WorksByValue()
        {
            var o1 = new DifficultyOutput(50, threshold: 55.0);
            var o2 = new DifficultyOutput(50, threshold: 55.0);
            var o3 = new DifficultyOutput(50, threshold: 60.0);
            
            Assert.That(o1, Is.EqualTo(o2));
            Assert.That(o1, Is.Not.EqualTo(o3));
        }
    }
}
