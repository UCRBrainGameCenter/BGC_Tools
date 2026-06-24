using BGC.Parameters;
using LightJson;
using NUnit.Framework;
using UnityEngine.TestTools;

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

        // ---- Item-title defaulting -----------------------------------------------------------
        // A group carrying [PropertyGroupItemTitle] that is default-constructed (absent key ->
        // Internal_RawDeserialize(new JsonObject())) has no serialized title. That is a normal
        // default, not an error: Serialize() always writes the title, so a title is only ever
        // absent for default construction or for legacy data that predates the title field.

        [PropertyGroupTitle("Titled Child")]
        private interface ITitledChild : IPropertyGroup { }

        [PropertyChoiceTitle("Default Titled Child")]
        private class TitledChild : CommonPropertyGroup, ITitledChild
        {
            [PropertyGroupItemTitle("ItemTitle")]
            public string ItemTitle { get; set; }
        }

        private class TitledRoot : CommonPropertyGroup
        {
            [AppendSelection(typeof(TitledChild))]
            public ITitledChild Child { get; set; }
        }

        [Test]
        public void AbsentTitledPropertyGroupKey_DefaultsWithoutLoggingError()
        {
            IPropertyGroup root = new TitledRoot();
            root.InitializeProperties();

            // "Child" key absent -> default-constructed from an empty object. The child carries
            // a [PropertyGroupItemTitle], so its title is legitimately absent. There is no
            // LogAssert.Expect here on purpose: before the fix this logged
            // "No Item Title Found ... Creating Guid", which the Unity test runner reports as a
            // failure (and which surfaced in researcher logs for every old battery that omitted
            // a now-nested titled group).
            root.Internal_RawDeserialize(new JsonObject());

            TitledChild child = (TitledChild)((TitledRoot)root).Child;

            Assert.IsNotNull(
                child,
                "An absent nested titled property group should be default-constructed.");
            Assert.IsFalse(
                string.IsNullOrEmpty(child.ItemTitle),
                "A default-constructed titled group should still receive a generated item title.");

            LogAssert.NoUnexpectedReceived();
        }
    }
}
