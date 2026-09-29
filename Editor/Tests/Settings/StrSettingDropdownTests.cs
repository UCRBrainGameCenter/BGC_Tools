using System.Collections.Generic;
using BGC.Settings;
using NUnit.Framework;

namespace BGC.Tests
{
    /// <summary>
    /// String settings with <c>dropdownOptions</c>: edited through a dropdown whose options are re-read each time it
    /// opens, storing the chosen option string (not its index). These tests never call ApplyValue, so no setting is
    /// written to PlayerData.
    /// </summary>
    public class StrSettingDropdownTests
    {
        // StrSetting is a protected nested type; a subclass of the menu can construct and drive it
        private abstract class MenuAccess : BaseSettingsMenu
        {
            public sealed class Probe
            {
                private readonly StrSetting setting;

                public Probe(System.Func<IReadOnlyList<string>> options, System.Func<string, string> optionLabel = null)
                {
                    setting = new StrSetting(
                        SettingScope.Global,
                        SettingProtection.Open,
                        "Test Setting",
                        "BGCToolsTests_StrSettingDropdown_Unset",
                        defaultVal: "",
                        dropdownOptions: options,
                        optionLabel: optionLabel);
                }

                public bool UsesDropdown => setting.EditButtonPressed() == UIState.SelectValue;
                public List<string> Open() => setting.GetValueList();
                public int Index => setting.GetDropdownIndex();
                public void Select(int index) => setting.SetValueFromDropdown(index);
                public string Value => setting.GetValue();
            }
        }

        [Test]
        public void OptionsAreReadEachTimeTheDropdownOpens()
        {
            List<string> devices = new List<string> { "default", "Mic A" };
            var probe = new MenuAccess.Probe(() => devices);

            Assert.IsTrue(probe.UsesDropdown);
            CollectionAssert.AreEqual(new[] { "default", "Mic A" }, probe.Open());

            devices.Insert(1, "Capture Card");
            CollectionAssert.AreEqual(new[] { "default", "Capture Card", "Mic A" }, probe.Open());
        }

        [Test]
        public void SelectionStoresTheOptionStringNotItsPosition()
        {
            List<string> devices = new List<string> { "default", "Mic A", "Mic B" };
            var probe = new MenuAccess.Probe(() => devices);

            probe.Open();
            probe.Select(2);
            Assert.AreEqual("Mic B", probe.Value);

            // A device plugged in ahead of it doesn't change what was chosen
            devices.Insert(1, "Capture Card");
            probe.Open();
            Assert.AreEqual("Mic B", probe.Value);
            Assert.AreEqual(3, probe.Index);
        }

        [Test]
        public void CurrentValueMissingFromOptionsIsShownFirst()
        {
            List<string> devices = new List<string> { "default", "Mic A", "Mic B" };
            var probe = new MenuAccess.Probe(() => devices, value => value == "Mic B" && !devices.Contains(value) ? value + " (gone)" : value);

            probe.Open();
            probe.Select(2);
            devices.Remove("Mic B");

            CollectionAssert.AreEqual(new[] { "Mic B (gone)", "default", "Mic A" }, probe.Open());
            Assert.AreEqual(0, probe.Index);
        }

        [Test]
        public void UnsetValueSelectsTheFirstOption()
        {
            var probe = new MenuAccess.Probe(() => new[] { "default", "Mic A" });
            probe.Open();
            Assert.AreEqual(0, probe.Index);
        }

        [Test]
        public void EmptyAndDuplicateOptionsAreDropped()
        {
            var probe = new MenuAccess.Probe(() => new[] { "default", "", null, "Mic A", "Mic A" });
            CollectionAssert.AreEqual(new[] { "default", "Mic A" }, probe.Open());
        }

        [Test]
        public void WithoutOptionsTheSettingStaysATextField()
        {
            var probe = new MenuAccess.Probe(null);
            Assert.IsFalse(probe.UsesDropdown);
        }
    }
}
