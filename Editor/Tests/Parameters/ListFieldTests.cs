using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BGC.Parameters;
using LightJson;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BGC.Tests
{
    /// <summary>
    /// List&lt;T&gt; input fields: initial values, round trips, the legacy delimited-string upgrade,
    /// the scalar-to-list upgrade, unparseable values, count warnings, mirrors, and the
    /// declaration contract.
    /// </summary>
    public class ListFieldTests
    {
        [PropertyChoiceTitle("List Group")]
        [IntListFieldDisplay("Ints", "Ints", initial: new[] { 1, 2, 3 }, minimum: 0, maximum: 10,
            maximumCount: 5, legacyDelimiters: new[] { ',', ';', '\n' })]
        [DoubleListFieldDisplay("Doubles", "Doubles", initial: new[] { 0.5 },
            legacyDelimiters: new[] { ',' })]
        [StringListFieldDisplay("Strings", "Strings", initial: new[] { "3", "2", "1" },
            legacyDelimiters: new[] { ',' })]
        [BoolListFieldDisplay("Bools", "Bools")]
        [StringListFieldDisplay("Words", "Words")]
        [IntFieldDisplay("Scalar", "Scalar", initial: 4)]
        private class ListGroup : CommonPropertyGroup
        {
            [DisplayInputField("Ints")]
            public List<int> Ints { get; set; }

            [DisplayInputField("Doubles")]
            public List<double> Doubles { get; set; }

            [DisplayInputField("Strings")]
            public List<string> Strings { get; set; }

            [DisplayInputField("Bools")]
            public List<bool> Bools { get; set; }

            [DisplayInputField("Words")]
            public List<string> Words { get; set; }

            [DisplayInputField("Scalar")]
            public int Scalar { get; set; }
        }

        private static ListGroup Load(JsonObject data)
        {
            ListGroup group = new ListGroup();
            group.Internal_RawDeserialize(data);
            return group;
        }

        private static ListGroup LoadField(string fieldName, JsonValue value) =>
            Load(new JsonObject { { fieldName, value } });

        // ---- Initial values -----------------------------------------------------------------

        [Test]
        public void NewGroup_HasAttributeInitialItems()
        {
            ListGroup group = new ListGroup();

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, group.Ints);
            CollectionAssert.AreEqual(new[] { 0.5 }, group.Doubles);
            CollectionAssert.AreEqual(new[] { "3", "2", "1" }, group.Strings);
            CollectionAssert.IsEmpty(group.Bools);
        }

        [Test]
        public void NewGroups_DoNotShareListInstances()
        {
            ListGroup first = new ListGroup();
            ListGroup second = new ListGroup();

            first.Ints.Add(9);

            Assert.AreNotSame(first.Ints, second.Ints);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, second.Ints);
        }

        // ---- Round trips --------------------------------------------------------------------

        [Test]
        public void Serialize_WritesArraysUnderFieldNames()
        {
            ListGroup group = new ListGroup();
            group.Bools.Add(true);

            JsonObject data = group.Serialize();

            Assert.AreEqual("[1,2,3]", data["Ints"].ToString());
            Assert.AreEqual("[0.5]", data["Doubles"].ToString());
            Assert.AreEqual("[\"3\",\"2\",\"1\"]", data["Strings"].ToString());
            Assert.AreEqual("[true]", data["Bools"].ToString());
        }

        [Test]
        public void RoundTrip_PreservesItemsAndIsStable()
        {
            ListGroup group = new ListGroup();
            group.Ints = new List<int> { 0, -7, 10, int.MaxValue };
            group.Doubles = new List<double> { 0.1, 1e-9, -2.5, 1234567.890123 };
            group.Strings = new List<string> { "a, b", "", "  spaced  ", "ü" };
            group.Bools = new List<bool> { true, false };

            JsonObject first = group.Serialize();
            ListGroup reloaded = Load(first);
            JsonObject second = reloaded.Serialize();

            CollectionAssert.AreEqual(group.Ints, reloaded.Ints);
            CollectionAssert.AreEqual(group.Doubles, reloaded.Doubles);
            CollectionAssert.AreEqual(group.Strings, reloaded.Strings);
            CollectionAssert.AreEqual(group.Bools, reloaded.Bools);
            Assert.AreEqual(first.ToString(), second.ToString());
        }

        [Test]
        public void EmptyList_IsWrittenAndReloadsEmpty()
        {
            // The initial value is non-empty, so omitting [] would reload as [1, 2, 3]
            ListGroup group = new ListGroup();
            group.Ints.Clear();

            JsonObject data = group.Serialize();
            Assert.AreEqual("[]", data["Ints"].ToString());

            CollectionAssert.IsEmpty(Load(data).Ints);
        }

        [Test]
        public void MissingKey_KeepsInitialItems()
        {
            ListGroup group = Load(new JsonObject());

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, group.Ints);
        }

        [Test]
        public void Clone_IsADeepCopy()
        {
            ListGroup group = new ListGroup();
            ListGroup clone = group.Clone();

            clone.Ints.Add(4);

            Assert.AreNotSame(group.Ints, clone.Ints);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, group.Ints);
        }

        // ---- Legacy delimited strings -------------------------------------------------------

        [Test]
        public void LegacyString_SplitsOnEveryDelimiterAndNewlineForm()
        {
            ListGroup group = LoadField("Ints", "1, 2;3\r\n4\n\n5\r6 ,");

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, group.Ints);
        }

        [Test]
        public void LegacyEmptyString_ReadsAsEmptyList()
        {
            CollectionAssert.IsEmpty(LoadField("Ints", "").Ints);
            CollectionAssert.IsEmpty(LoadField("Ints", "   ").Ints);
        }

        [Test]
        public void LegacyString_ParsesDoublesWithInvariantCulture()
        {
            ListGroup group = LoadField("Doubles", "0.25, 1e3, -4");

            CollectionAssert.AreEqual(new[] { 0.25, 1000.0, -4.0 }, group.Doubles);
        }

        [Test]
        public void LegacyString_TrimsStringItemsAndDropsEmptyOnes()
        {
            ListGroup group = LoadField("Strings", "3, 2,, 1");

            CollectionAssert.AreEqual(new[] { "3", "2", "1" }, group.Strings);
        }

        [Test]
        public void LegacyUpgrade_ThenSave_WritesAnArray()
        {
            JsonObject data = LoadField("Ints", "4,4,0").Serialize();

            Assert.AreEqual("[4,4,0]", data["Ints"].ToString());
        }

        // ---- Scalar to list -----------------------------------------------------------------

        [Test]
        public void ScalarValue_ReadsAsOneItemList()
        {
            CollectionAssert.AreEqual(new[] { 7 }, LoadField("Ints", 7).Ints);
            CollectionAssert.AreEqual(new[] { true }, LoadField("Bools", true).Bools);
            CollectionAssert.AreEqual(new[] { 7 }, LoadField("Ints", "7").Ints);
        }

        [Test]
        public void StringWithoutDelimiters_ReadsAsOneUnsplitItem()
        {
            ListGroup group = LoadField("Words", "a, b");

            CollectionAssert.AreEqual(new[] { "a, b" }, group.Words);
        }

        // ---- Unparseable values -------------------------------------------------------------

        [TestCase("1,x,3")]
        [TestCase("1.5")]
        public void UnparseableLegacyString_LoadsEmptyAndPreservesOriginal(string original)
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Ints\""));

            ListGroup group = LoadField("Ints", original);

            CollectionAssert.IsEmpty(group.Ints);
            CollectionAssert.AreEqual(new[] { "Ints" }, group.GetUnparsedListFields());
            Assert.AreEqual(original, group.Serialize()["Ints"].AsString,
                "An unedited field that failed to parse must be saved back unchanged.");
        }

        [Test]
        public void UnparseableValue_AfterEdit_SavesTheEditedList()
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field"));
            ListGroup group = LoadField("Ints", "1,x,3");

            group.ClearUnparsedListValue("Ints");
            group.Ints.Add(2);

            CollectionAssert.IsEmpty(group.GetUnparsedListFields());
            Assert.AreEqual("[2]", group.Serialize()["Ints"].ToString());
        }

        [Test]
        public void ArrayWithWrongItemKind_IsUnparseable()
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Ints\""));

            ListGroup group = LoadField("Ints", new JsonArray(1, "two"));

            CollectionAssert.IsEmpty(group.Ints);
            Assert.AreEqual("[1,\"two\"]", group.Serialize()["Ints"].ToString());
        }

        [Test]
        public void NullOrObject_IsUnparseable()
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Ints\""));
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Ints\""));

            Assert.IsTrue(LoadField("Ints", JsonValue.Null).Serialize()["Ints"].IsNull);
            Assert.IsTrue(LoadField("Ints", new JsonObject()).Serialize()["Ints"].IsJsonObject);
        }

        [Test]
        public void ReloadingValidData_ClearsAPreviousUnparsedValue()
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field"));
            ListGroup group = LoadField("Ints", "x");

            group.Internal_RawDeserialize(new JsonObject { { "Ints", new JsonArray(5) } });

            CollectionAssert.IsEmpty(group.GetUnparsedListFields());
            CollectionAssert.AreEqual(new[] { 5 }, group.Ints);
        }

        // ---- Bounds are not enforced on load ------------------------------------------------

        [Test]
        public void CountOutsideBounds_LoadsAsIsWithWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("has 6 items, outside its allowed 0 to 5"));

            ListGroup group = LoadField("Ints", new JsonArray(1, 2, 3, 4, 5, 6));

            Assert.AreEqual(6, group.Ints.Count);
        }

        [Test]
        public void ItemsOutsideBounds_LoadUnclampedLikeScalars()
        {
            ListGroup group = LoadField("Ints", new JsonArray(-3, 99));

            CollectionAssert.AreEqual(new[] { -3, 99 }, group.Ints);
            Assert.IsFalse(group.GetListFieldAttribute("Ints").IsListValid(group.Ints));
        }

        [Test]
        public void NewItemValue_IsClampedIntoItemBounds()
        {
            IntListFieldDisplayAttribute attribute = new IntListFieldDisplayAttribute(
                "X", "X", minimum: 2, maximum: 10, newItem: 0);

            Assert.AreEqual(2, attribute.NewItemValue);
        }

        // ---- Mirrors ------------------------------------------------------------------------

        [PropertyGroupTitle("Mirror Child")]
        private interface IMirrorChild : IPropertyGroup { }

        [PropertyChoiceTitle("Mirror Child")]
        [FieldMirrorDisplay("Levels", "SharedLevels", "Levels")]
        private class MirrorChild : CommonPropertyGroup, IMirrorChild
        {
            [DisplayInputField("Levels")]
            public List<int> Levels { get; set; }
        }

        [IntListFieldDisplay("SharedLevels", "Levels", initial: new[] { 40, 50 },
            legacyDelimiters: new[] { ',' })]
        private class MirrorParent : CommonPropertyGroup
        {
            [AppendSelection(typeof(MirrorChild))]
            public IMirrorChild Child { get; set; }
        }

        [Test]
        public void MirroredListField_UsesTheConcreteAttribute()
        {
            MirrorParent parent = new MirrorParent();
            MirrorChild child = (MirrorChild)typeof(MirrorChild).Build(
                parent, typeof(MirrorParent).GetProperty(nameof(MirrorParent.Child)));

            CollectionAssert.AreEqual(new[] { 40, 50 }, child.Levels);

            child.Internal_RawDeserialize(new JsonObject { { "Levels", "60, 70" } });
            CollectionAssert.AreEqual(new[] { 60, 70 }, child.Levels);
        }

        // ---- Declaration contract -----------------------------------------------------------

        [Test]
        public void WellFormedGroups_HaveNoDeclarationErrors()
        {
            CollectionAssert.IsEmpty(ListFieldSerialization.GetListFieldDeclarationErrors(typeof(ListGroup)));
            CollectionAssert.IsEmpty(ListFieldSerialization.GetListFieldDeclarationErrors(typeof(MirrorChild)));
        }

        [IntListFieldDisplay("WrongItems", "Wrong Items")]
        [IntFieldDisplay("ScalarAttribute", "Scalar Attribute")]
        [IntListFieldDisplay("Keyed", "Keyed")]
        [IntListFieldDisplay("ListOnScalar", "List On Scalar")]
        [IntListFieldDisplay("ReadOnly", "Read Only")]
        private class BadGroup : CommonPropertyGroup
        {
            [DisplayInputField("WrongItems")]
            public List<double> WrongItems { get; set; }

            [DisplayInputField("ScalarAttribute")]
            public List<int> ScalarAttribute { get; set; }

            [DisplayInputField("Missing")]
            public List<int> Missing { get; set; }

            [DisplayInputField("Unsupported")]
            public List<float> Unsupported { get; set; }

            [DisplayInputField("Keyed")]
            public List<int> Keyed { get; set; }

            [DisplayInputFieldKey("Keyed")]
            public string KeyedKey { get; set; }

            [DisplayInputField("ListOnScalar")]
            public int ListOnScalar { get; set; }

            [DisplayInputField("ReadOnly")]
            public List<int> ReadOnly { get; } = new List<int>();
        }

        [Test]
        public void MalformedGroup_ReportsEachDeclarationError()
        {
            List<string> errors = ListFieldSerialization.GetListFieldDeclarationErrors(typeof(BadGroup)).ToList();

            string Find(string property) => errors.FirstOrDefault(x => x.Contains($".{property}:"));

            StringAssert.Contains("attribute is for Int32 items", Find("WrongItems"));
            StringAssert.Contains("needs a list attribute, not IntFieldDisplayAttribute", Find("ScalarAttribute"));
            StringAssert.Contains("no FieldDisplay attribute", Find("Missing"));
            StringAssert.Contains("not a supported list field", Find("Unsupported"));
            StringAssert.Contains("cannot have a [DisplayInputFieldKey]", Find("Keyed"));
            StringAssert.Contains("list attribute on a non-List property", Find("ListOnScalar"));
            StringAssert.Contains("needs a setter", Find("ReadOnly"));
            Assert.AreEqual(7, errors.Count, string.Join("\n", errors));
        }
    }
}
