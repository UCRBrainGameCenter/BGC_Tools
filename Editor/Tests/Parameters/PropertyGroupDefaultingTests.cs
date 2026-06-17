using BGC.Parameters;
using LightJson;
using NUnit.Framework;

namespace BGC.Tests
{
    /// <summary>
    /// Regression tests for property-group defaulting during deserialization.
    ///
    /// A property-group key that is absent from the serialized data must be constructed as a
    /// fully-defaulted group, including its own nested [AppendSelection] children. Previously
    /// the absent-key path only called Build() (which initializes field properties but does
    /// not recurse into nested property groups), so a defaulted group's children were left
    /// null -- which downstream code then dereferenced (NullReferenceException). The present
    /// key path did not have this problem because it followed Build() with Deserialize().
    /// </summary>
    public class PropertyGroupDefaultingTests
    {
        // ---- Minimal self-contained property-group hierarchy ----
        // Root -> Child (AppendSelection) -> Grandchild (AppendSelection)

        [PropertyGroupTitle("Test Grandchild")]
        private interface ITestGrandchild : IPropertyGroup { }

        [PropertyChoiceTitle("Default Grandchild")]
        private class TestGrandchild : CommonPropertyGroup, ITestGrandchild { }

        [PropertyGroupTitle("Test Child")]
        private interface ITestChild : IPropertyGroup
        {
            ITestGrandchild Grandchild { get; }
        }

        [PropertyChoiceTitle("Default Child")]
        private class TestChild : CommonPropertyGroup, ITestChild
        {
            [AppendSelection(typeof(TestGrandchild))]
            public ITestGrandchild Grandchild { get; set; }
        }

        private class TestRoot : CommonPropertyGroup
        {
            [AppendSelection(typeof(TestChild))]
            public ITestChild Child { get; set; }
        }

        [Test]
        public void AbsentPropertyGroupKey_DefaultsNestedChildrenRecursively()
        {
            IPropertyGroup root = new TestRoot();
            root.InitializeProperties();

            // Deserialize with the "Child" key absent. Child must be default-constructed,
            // and -- the regression under test -- its own nested "Grandchild" must be
            // recursively defaulted rather than left null.
            root.Internal_RawDeserialize(new JsonObject());

            TestRoot typedRoot = (TestRoot)root;

            Assert.IsNotNull(
                typedRoot.Child,
                "An absent nested property group should be default-constructed.");

            Assert.IsNotNull(
                typedRoot.Child.Grandchild,
                "A defaulted property group's own nested [AppendSelection] child must be " +
                "recursively defaulted, not left null.");
        }
    }
}
