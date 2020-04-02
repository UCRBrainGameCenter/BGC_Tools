using System;
using System.Collections.Generic;
using LightJson;
using BGC.IO;

namespace BGC.Reports
{
    public class ReportElement
    {
        public readonly string userName;
        public readonly string batteryName;
        public readonly DateTime startTime;

        public readonly string fileName;
        public readonly string filePath;

        public readonly Dictionary<string, string> data = new Dictionary<string, string>();

        private bool dataDirty = false;

        private string CleanedBatteryName => FilePath.SanitizeForFilename(
            name: batteryName,
            additionalExclusion: ' ',
            fallback: "CustomBattery");

        public ReportElement(
            string userName,
            string batteryName,
            DateTime startTime)
        {
            this.userName = userName;
            this.batteryName = batteryName;
            this.startTime = startTime;

            fileName = $"{userName}_{CleanedBatteryName}_{startTime:yy_MM_dd_HH_mm_ss}";

            filePath = DataManagement.PathForDataFile(
                dataDirectory: "Reports",
                fileName: $"{fileName}.json");
        }

        public ReportElement(JsonObject reportData)
        {
            userName = reportData["UserName"];
            batteryName = reportData["BatteryName"];
            startTime = reportData["StartTime"];

            foreach (KeyValuePair<string, JsonValue> value in reportData["Data"].AsJsonObject)
            {
                data[value.Key] = value.Value.AsString;
            }

            fileName = $"{userName}_{CleanedBatteryName}_{startTime:yy_MM_dd_HH_mm_ss}";

            filePath = DataManagement.PathForDataFile(
                dataDirectory: "Reports",
                fileName: $"{fileName}.json");
        }

        public void AddData(string header, string value)
        {
            data[header] = value;
            dataDirty = true;
        }

        public void SaveIfNecessary()
        {
            if (!dataDirty)
            {
                //No saving necessary
                return;
            }

            FileWriter.WriteJson(
                path: filePath,
                createJson: GetSerialized,
                pretty: true);

            dataDirty = false;
        }

        private JsonObject GetSerialized()
        {
            JsonObject data = new JsonObject();

            foreach (KeyValuePair<string, string> element in this.data)
            {
                data.Add(element.Key, element.Value);
            }

            return new JsonObject()
            {
                { "UserName", userName },
                { "BatteryName", batteryName },
                { "StartTime", startTime },
                { "Data", data }
            };
        }
    }
}
