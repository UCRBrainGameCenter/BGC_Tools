using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.IO.Logging;
using BGC.Utility;
using LightJson;
using NUnit.Framework;

public class BGCFormatEditorTests
{
    #region MetaData Update Tests

    /// <summary>
    /// A test to update metadata using patternmatching.
    /// </summary>
    [Test]
    public void UpgradeMetaDataWithPattern()
    {
        const string initialVersion = "50.50.1";
        const string updatedVersion = "25.25.2.1";
        string[] inputData = new string[] { "test", "test", "test", "test" };

        //Set up fake Metadata
        JsonObject metaData = GenerateMetaData(
            version: initialVersion,
            labels: new string[] { "field1", "field2", "unusualField3", "field4" });

        BGCMetaDataEditor fixUnusalFieldName = (string logName, JsonObject data) =>
        {
            data[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] = "field3";
        };

        BGCRemapper stringifyField3 = (string logName, string[] columnLabels, string[] data) =>
        {
            for (int i = 0; i < columnLabels.Length; i++)
            {
                if (columnLabels[i].Contains("3"))
                {
                    data[i] = $"\"{data[i]}\"";
                }
            }
        };

        BGCRemapHelper helper = new BGCRemapHelper("someDir/testFile.bgc");

        LogUpgradePatternStep upgradePatternMiss = new LogUpgradePatternStep(
            upgradePattern: "50.*.2",
            outputVersion: updatedVersion,
            metaDataEditor: fixUnusalFieldName,
            remapper: stringifyField3);

        LogUpgradePatternStep upgradePatternHit = new LogUpgradePatternStep(
            upgradePattern: "50.*.1",
            outputVersion: updatedVersion,
            metaDataEditor: fixUnusalFieldName,
            remapper: stringifyField3);

        // Test Miss
        {
            upgradePatternMiss.TestAndApply(metaData, helper);

            //Version should not change because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.Version] == initialVersion);
            //Field should not be remapped because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] == "unusualField3");
            //Helper should be empty because of pattern miss
            Assert.IsTrue(helper.IsEmpty());
        }

        //Test Hit
        {
            upgradePatternHit.TestAndApply(metaData, helper);

            //Version should change because of pattern hit
            Assert.IsTrue(metaData[LoggingKeys.Version] == updatedVersion);
            //Field should be remapped because of pattern hit
            Assert.IsTrue(metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] == "field3");
            //Helper should not empty because of pattern hit
            Assert.IsTrue(helper.IsEmpty() == false);
        }
        
        //Test Field Updates
        {

            string[] newLabels =
                metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray
                .Select(x => x.ToString())
                .ToArray();
            
            helper.Apply(newLabels, inputData);

            Assert.IsTrue(inputData[0] == "test");
            Assert.IsTrue(inputData[1] == "test");
            Assert.IsTrue(inputData[2] == "\"test\"");
            Assert.IsTrue(inputData[3] == "test");
        }

    }

    /// <summary>
    /// A test to update metadata using range.
    /// </summary>
    [Test]
    public void UpgradeMetaDataWithRange()
    {
        const string initialVersion = "50.50.1";
        const string updatedVersion = "25.25.2.1";
        string[] inputData = new string[] { "test", "test", "test", "test" };

        //Set up fake Metadata
        JsonObject metaData = GenerateMetaData(
            version: initialVersion,
            labels: new string[] { "field1", "field2", "unusualField3", "field4" });

        BGCMetaDataEditor fixUnusalFieldName = (string logName, JsonObject data) =>
        {
            data[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] = "field3";
        };

        BGCRemapper stringifyField3 = (string logName, string[] columnLabels, string[] data) =>
        {
            for (int i = 0; i < columnLabels.Length; i++)
            {
                if (columnLabels[i].Contains("3"))
                {
                    data[i] = $"\"{data[i]}\"";
                }
            }
        };

        BGCRemapHelper helper = new BGCRemapHelper("someDir/testFile.bgc");

        LogUpgradeRangeStep upgradeRangeMissBelow = new LogUpgradeRangeStep(
            lowestVersion: "50.17",
            highestVersion: "50.49",
            outputVersion: updatedVersion,
            metaDataEditor: fixUnusalFieldName,
            remapper: stringifyField3);

        LogUpgradeRangeStep upgradeRangeMissBelowTouch = new LogUpgradeRangeStep(
            lowestVersion: "0.0",
            highestVersion: initialVersion,
            outputVersion: updatedVersion,
            metaDataEditor: fixUnusalFieldName,
            remapper: stringifyField3);

        LogUpgradeRangeStep upgradeRangeMissAbove = new LogUpgradeRangeStep(
            lowestVersion: "50.51",
            highestVersion: "52.55.1",
            outputVersion: updatedVersion,
            metaDataEditor: fixUnusalFieldName,
            remapper: stringifyField3);

        LogUpgradeRangeStep upgradeRangeMissAboveTouch = new LogUpgradeRangeStep(
            lowestVersion: "50.50.1.1",
            highestVersion: "100",
            outputVersion: updatedVersion,
            metaDataEditor: fixUnusalFieldName,
            remapper: stringifyField3);

        LogUpgradeRangeStep upgradeRangeHit = new LogUpgradeRangeStep(
            lowestVersion: "49.52.1.1",
            highestVersion: "50.50.1.1",
            outputVersion: updatedVersion,
            metaDataEditor: fixUnusalFieldName,
            remapper: stringifyField3);

        //Test Miss Below
        {
            upgradeRangeMissBelow.TestAndApply(metaData, helper);

            //Version should not change because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.Version] == initialVersion);
            //Field should not be remapped because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] == "unusualField3");
            //Helper should be empty because of pattern miss
            Assert.IsTrue(helper.IsEmpty());
        }

        //Test Miss Below Touch
        {
            upgradeRangeMissBelowTouch.TestAndApply(metaData, helper);

            //Version should not change because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.Version] == initialVersion);
            //Field should not be remapped because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] == "unusualField3");
            //Helper should be empty because of pattern miss
            Assert.IsTrue(helper.IsEmpty());
        }

        //Test Miss Above
        {
            upgradeRangeMissAbove.TestAndApply(metaData, helper);

            //Version should not change because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.Version] == initialVersion);
            //Field should not be remapped because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] == "unusualField3");
            //Helper should be empty because of pattern miss
            Assert.IsTrue(helper.IsEmpty());
        }

        //Test Miss Above Touch
        {
            upgradeRangeMissAboveTouch.TestAndApply(metaData, helper);

            //Version should not change because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.Version] == initialVersion);
            //Field should not be remapped because of pattern miss
            Assert.IsTrue(metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] == "unusualField3");
            //Helper should be empty because of pattern miss
            Assert.IsTrue(helper.IsEmpty());
        }

        //Test Hit
        {
            upgradeRangeHit.TestAndApply(metaData, helper);

            //Version should change because of pattern hit
            Assert.IsTrue(metaData[LoggingKeys.Version] == updatedVersion);
            //Field should be remapped because of pattern hit
            Assert.IsTrue(metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray[2] == "field3");
            //Helper should not empty because of pattern hit
            Assert.IsTrue(helper.IsEmpty() == false);
        }
        
        //Test Field Updates
        {

            string[] newLabels =
                metaData[LoggingKeys.ColumnMapping][LoggingKeys.DefaultColumn].AsJsonArray
                .Select(x => x.ToString())
                .ToArray();

            helper.Apply(newLabels, inputData);

            Assert.IsTrue(inputData[0] == "test");
            Assert.IsTrue(inputData[1] == "test");
            Assert.IsTrue(inputData[2] == "\"test\"");
            Assert.IsTrue(inputData[3] == "test");
        }
    }
    
    private JsonObject GenerateMetaData(
        ApplicationVersion version,
        string[] labels)
    {
        JsonArray columnLabels = new JsonArray(labels.Select(x => new JsonValue(x)).ToArray());

        JsonObject columnMapping = new JsonObject() { { LoggingKeys.DefaultColumn, columnLabels } };

        //Set up fake Metadata
        return new JsonObject()
        {
            { LoggingKeys.GameName, "TestGame" },
            { LoggingKeys.Version, version.ToString() },
            { LoggingKeys.UserName, "TestUser" },
            { LoggingKeys.Session, 1 },
            { LoggingKeys.DeviceID, "-1" },
            { LoggingKeys.Delimiter, "|" },

            { LoggingKeys.ValueMapping, new JsonObject() },
            { LoggingKeys.ColumnMapping, columnMapping },
            { LoggingKeys.AdditionalHeaders, new JsonObject() }
        };
    }

    #endregion MetaData Update Tests
}
