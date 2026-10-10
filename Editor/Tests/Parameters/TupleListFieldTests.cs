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
    /// Tuple-list input fields: List&lt;(T1, T2[, T3[, T4]])&gt; with one class-level column
    /// attribute per item. Initial rows, round trips, missing and unknown columns, the legacy
    /// string format, unreadable values, count warnings, and the declaration contract.
    /// </summary>
    public class TupleListFieldTests
    {
        [PropertyChoiceTitle("Tuple Group")]
        [TupleListFieldDisplay("Levels", "Levels", minimumCount: 1, maximumCount: 4,
            legacyDelimiters: new[] { ';', '\n' }, legacyColumnDelimiters: new[] { ',' })]
        [IntTupleColumn("Levels", 0, "Input", "Input Level", initial: new[] { 0, 10 }, minimum: 0, maximum: 100)]
        [DoubleTupleColumn("Levels", 1, "Output", "Output Level", initial: new[] { 0.5, 1.0 }, postfix: "dB", newItem: 0.25)]
        [TupleListFieldDisplay("Words", "Words")]
        [IntTupleColumn("Words", 0, "Count", "Count", newItem: 1)]
        [StringTupleColumn("Words", 1, "Word", "Word", newItem: "go")]
        [BoolTupleColumn("Words", 2, "Bold", "Bold")]
        private class TupleGroup : CommonPropertyGroup
        {
            [DisplayInputField("Levels")]
            public List<(int Input, double Output)> Levels { get; set; }

            [DisplayInputField("Words")]
            public List<(int Count, string Word, bool Bold)> Words { get; set; }
        }

        private static TupleGroup LoadField(string fieldName, JsonValue value)
        {
            TupleGroup group = new TupleGroup();
            group.Internal_RawDeserialize(new JsonObject { { fieldName, value } });
            return group;
        }

        private static JsonObject Row(params (string key, JsonValue value)[] items)
        {
            JsonObject row = new JsonObject();
            foreach ((string key, JsonValue value) in items)
            {
                row.Add(key, value);
            }
            return row;
        }

        [Test]
        public void NewGroup_HasInitialRowsFromTheColumns()
        {
            TupleGroup group = new TupleGroup();

            CollectionAssert.AreEqual(new[] { (0, 0.5), (10, 1.0) }, group.Levels);
            CollectionAssert.IsEmpty(group.Words);
            Assert.AreNotSame(group.Levels, new TupleGroup().Levels);
        }

        [Test]
        public void Serialize_WritesRowsAsObjectsKeyedByColumnName()
        {
            TupleGroup group = new TupleGroup();
            group.Words.Add((2, "ready", true));

            JsonObject data = group.Serialize();

            Assert.AreEqual("[{\"Input\":0,\"Output\":0.5},{\"Input\":10,\"Output\":1}]", data["Levels"].ToString());
            Assert.AreEqual("[{\"Count\":2,\"Word\":\"ready\",\"Bold\":true}]", data["Words"].ToString());
        }

        [Test]
        public void RoundTrip_PreservesRowsAndIsStable()
        {
            TupleGroup group = new TupleGroup();
            group.Levels = new List<(int, double)> { (100, -2.5), (0, 1e-6) };
            group.Words = new List<(int, string, bool)> { (0, "", false), (-3, "a, b", true) };

            JsonObject first = group.Serialize();
            TupleGroup reloaded = new TupleGroup();
            reloaded.Internal_RawDeserialize(first);

            CollectionAssert.AreEqual(group.Levels, reloaded.Levels);
            CollectionAssert.AreEqual(group.Words, reloaded.Words);
            Assert.AreEqual(first.ToString(), reloaded.Serialize().ToString());
        }

        [Test]
        public void EmptyList_IsWrittenAndReloadsEmpty()
        {
            TupleGroup group = new TupleGroup();
            group.Levels.Clear();

            JsonObject data = group.Serialize();
            TupleGroup reloaded = new TupleGroup();
            reloaded.Internal_RawDeserialize(data);

            Assert.AreEqual("[]", data["Levels"].ToString());
            CollectionAssert.IsEmpty(reloaded.Levels);
        }

        [Test]
        public void MissingColumn_TakesTheColumnsNewItemValue()
        {
            // A column added after a battery was saved
            TupleGroup group = LoadField("Levels", new JsonArray((JsonValue)Row(("Input", 7))));

            CollectionAssert.AreEqual(new[] { (7, 0.25) }, group.Levels);
        }

        [Test]
        public void UnknownColumn_IsUnparseableAndPreserved()
        {
            // A renamed or removed column must not silently drop data
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Levels\""));

            TupleGroup group = LoadField("Levels", new JsonArray((JsonValue)Row(("Input", 7), ("Gain", 3))));

            CollectionAssert.IsEmpty(group.Levels);
            Assert.AreEqual("[{\"Input\":7,\"Gain\":3}]", group.Serialize()["Levels"].ToString());
        }

        [Test]
        public void WrongItemKind_IsUnparseable()
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Levels\""));
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Levels\""));

            CollectionAssert.IsEmpty(LoadField("Levels", new JsonArray((JsonValue)Row(("Input", 1.5)))).Levels);
            CollectionAssert.IsEmpty(LoadField("Levels", new JsonArray((JsonValue)new JsonArray(1, 0.5))).Levels);
        }

        [Test]
        public void LegacyString_SplitsIntoRowsThenColumns()
        {
            TupleGroup group = LoadField("Levels", "1, 0.5; 2,0.75\r\n3 ,1;");

            CollectionAssert.AreEqual(new[] { (1, 0.5), (2, 0.75), (3, 1.0) }, group.Levels);
            Assert.AreEqual("[{\"Input\":1,\"Output\":0.5},{\"Input\":2,\"Output\":0.75},{\"Input\":3,\"Output\":1}]",
                group.Serialize()["Levels"].ToString());
        }

        [TestCase("1, 0.5, 3")]
        [TestCase("1")]
        [TestCase("1, x")]
        public void LegacyRowWithWrongColumns_IsUnparseable(string original)
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Levels\""));

            TupleGroup group = LoadField("Levels", original);

            CollectionAssert.IsEmpty(group.Levels);
            Assert.AreEqual(original, group.Serialize()["Levels"].AsString);
        }

        [Test]
        public void StringWithoutLegacyDelimiters_IsUnparseable()
        {
            LogAssert.Expect(LogType.Error, new Regex("Unable to read list field \"Words\""));

            CollectionAssert.IsEmpty(LoadField("Words", "1,go,true").Words);
        }

        [Test]
        public void CountOutsideBounds_LoadsAsIsWithWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("has 5 items, outside its allowed 1 to 4"));

            TupleGroup group = LoadField("Levels", "1,0;2,0;3,0;4,0;5,0");

            Assert.AreEqual(5, group.Levels.Count);
        }

        [Test]
        public void Layout_ChecksBoundsAndReplacesItems()
        {
            TupleGroup group = new TupleGroup();
            TupleListLayout layout = TupleListLayout.Get(group, typeof(TupleGroup).GetProperty(nameof(TupleGroup.Levels)));

            Assert.IsTrue(layout.IsListValid(group.Levels));
            Assert.IsFalse(layout.IsRowValid((200, 0.5)));
            Assert.AreEqual((5, 0.5), layout.WithItem((0, 0.5), 0, 5));
            Assert.AreEqual((0, 0.25), layout.CreateNewRow());
            Assert.AreEqual(new[] { "Input", "Output" }, layout.Columns.Select(x => x.columnName).ToArray());
        }

        // ---- Declaration contract -----------------------------------------------------------

        [Test]
        public void WellFormedGroup_HasNoDeclarationErrors()
        {
            CollectionAssert.IsEmpty(ListFieldSerialization.GetListFieldDeclarationErrors(typeof(TupleGroup)));
        }

        [TupleListFieldDisplay("MissingColumn", "Missing Column")]
        [IntTupleColumn("MissingColumn", 0, "A", "A")]
        [TupleListFieldDisplay("WrongType", "Wrong Type")]
        [IntTupleColumn("WrongType", 0, "A", "A")]
        [IntTupleColumn("WrongType", 1, "B", "B")]
        [TupleListFieldDisplay("SameNames", "Same Names")]
        [IntTupleColumn("SameNames", 0, "A", "A")]
        [IntTupleColumn("SameNames", 1, "A", "B")]
        [TupleListFieldDisplay("UnevenInitial", "Uneven Initial")]
        [IntTupleColumn("UnevenInitial", 0, "A", "A", initial: new[] { 1, 2 })]
        [IntTupleColumn("UnevenInitial", 1, "B", "B", initial: new[] { 1 })]
        [TupleListFieldDisplay("HalfLegacy", "Half Legacy", legacyDelimiters: new[] { ';' })]
        [IntTupleColumn("HalfLegacy", 0, "A", "A")]
        [IntTupleColumn("HalfLegacy", 1, "B", "B")]
        [IntListFieldDisplay("PlainAttribute", "Plain Attribute")]
        [TupleListFieldDisplay("TooWide", "Too Wide")]
        [TupleListFieldDisplay("NotATuple", "Not A Tuple")]
        [IntTupleColumn("Orphan", 0, "A", "A")]
        private class BadTupleGroup : CommonPropertyGroup
        {
            [DisplayInputField("MissingColumn")]
            public List<(int, int)> MissingColumn { get; set; }

            [DisplayInputField("WrongType")]
            public List<(int, string)> WrongType { get; set; }

            [DisplayInputField("SameNames")]
            public List<(int, int)> SameNames { get; set; }

            [DisplayInputField("UnevenInitial")]
            public List<(int, int)> UnevenInitial { get; set; }

            [DisplayInputField("HalfLegacy")]
            public List<(int, int)> HalfLegacy { get; set; }

            [DisplayInputField("PlainAttribute")]
            public List<(int, int)> PlainAttribute { get; set; }

            [DisplayInputField("TooWide")]
            public List<(int, int, int, int, int)> TooWide { get; set; }

            [DisplayInputField("NotATuple")]
            public List<int> NotATuple { get; set; }
        }

        [Test]
        public void MalformedGroup_ReportsEachDeclarationError()
        {
            List<string> errors = ListFieldSerialization.GetListFieldDeclarationErrors(typeof(BadTupleGroup)).ToList();

            string Find(string marker) => errors.FirstOrDefault(x => x.Contains(marker)) ?? "<none>";

            StringAssert.Contains("no tuple column 1", Find(".MissingColumn:"));
            StringAssert.Contains("but item 2 is String", Find(".WrongType:"));
            StringAssert.Contains("non-empty and unique", Find(".SameNames:"));
            StringAssert.Contains("same number of initial values", Find(".UnevenInitial:"));
            StringAssert.Contains("both legacyDelimiters and legacyColumnDelimiters", Find(".HalfLegacy:"));
            StringAssert.Contains("needs a TupleListFieldDisplay attribute", Find(".PlainAttribute:"));
            StringAssert.Contains("2 to 4 items", Find(".TooWide:"));
            StringAssert.Contains("needs a List of value tuples", Find(".NotATuple:"));
            StringAssert.Contains("\"Orphan\", which is not a tuple list field", Find("Orphan"));
            Assert.AreEqual(9, errors.Count, string.Join("\n", errors));
        }
    }
}
