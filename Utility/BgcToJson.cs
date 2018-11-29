using System;
using System.Linq;
using UnityEngine.Assertions;
using BGC.IO.Logging;
using LightJson;

namespace BGC.Utility
{
    public static class BgcToJson
    {
        public static class RequiredFields
        {
            public const string MetaData = "meta_data";
            public const string Data = "data";

            public static readonly string[] MetaDataFields = new string[]
            {
                LoggingKeys.GameName,
                LoggingKeys.Version,
                LoggingKeys.UserName,
                LoggingKeys.DeviceID,
                LoggingKeys.Session,
                LoggingKeys.Delimiter,
                LoggingKeys.ColumnMapping,
                LoggingKeys.ValueMapping
            };

            public static readonly string[] RedundantFields = new string[]
            {
                LoggingKeys.ColumnMapping,
                LoggingKeys.ValueMapping,
                LoggingKeys.Delimiter
            };
        }


        /// <summary>
        /// Convert complete string of bgc to json
        /// </summary>
        public static JsonObject ConvertBgcToJson(
            string filepath,
            string bgc,
            char separator = '\n',
            bool verbose = false)
        {
            return ConvertBGCToJson(
                filepath: filepath,
                bgc: bgc.Split(separator),
                verbose: verbose);
        }

        /// <summary>
        /// Convert lines of bgc to complete json
        /// </summary>
        public static JsonObject ConvertBGCToJson(
            string filepath,
            string[] bgc,
            bool verbose = false)
        {
            JsonObject json = new JsonObject();
            JsonObject metaData = LightJson.Serialization.JsonReader.Parse(bgc[0]);

            //Apply any prepared MetaData Upgrades
            BGCRemapHelper remapHelper = LogUpgradeUtility.UpgradeMetaData(
                filePath: filepath,
                metaData: metaData);

            Assert.IsTrue(MetaDataHasCorrectFields(metaData, verbose));

            JsonArray data = new JsonArray();
            string[] delimiter = new string[] { metaData[LoggingKeys.Delimiter].AsString };
            for (int i = 1; i < bgc.Length; ++i)
            {
                if (string.IsNullOrEmpty(bgc[i]) == false)
                {
                    data.Add(ConvertLine(
                        metaData: metaData,
                        splitBy: delimiter,
                        line: bgc[i],
                        remapHelper: remapHelper));
                }
            }

            RemoveRedundantMetaData(metaData);
            json.Add(RequiredFields.MetaData, metaData);
            json.Add(RequiredFields.Data, data);

            return json;
        }

        /// <summary>
        /// Test to see whether or not the meta data has all the correct fields
        /// </summary>
        /// <returns>True if the meta data has all the correct fields</returns>
        private static bool MetaDataHasCorrectFields(JsonObject metaData, bool verbose)
        {
            bool hasRequiredFields = true;

            for (int i = 0; i < RequiredFields.MetaDataFields.Length; ++i)
            {
                if (metaData.ContainsKey(RequiredFields.MetaDataFields[i]) == false)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"Required meta data field \"{RequiredFields.MetaDataFields[i]}\" not found.");
                    }

                    hasRequiredFields = false;
                    break;
                }
            }

            return hasRequiredFields;
        }

        /// <summary>
        /// Remove any redundant meta data fields from the json object
        /// </summary>
        private static void RemoveRedundantMetaData(JsonObject metaData)
        {
            for (int i = 0; i < RequiredFields.RedundantFields.Length; ++i)
            {
                if (metaData.ContainsKey(RequiredFields.RedundantFields[i]))
                {
                    metaData.Remove(RequiredFields.RedundantFields[i]);
                }
            }
        }

        /// <summary>
        /// Convert every line into a json object based on the meta data
        /// </summary>
        /// <returns>Json object representing the line based on the colum manpping</returns>
        private static JsonObject ConvertLine(
            JsonObject metaData,
            string[] splitBy,
            string line,
            BGCRemapHelper remapHelper)
        {
            JsonObject jsonData = new JsonObject();
            string[] data = line.Split(splitBy, StringSplitOptions.None);

            string columnMappingKey;
            if (metaData[LoggingKeys.ColumnMapping].AsJsonObject.ContainsKey(data[0]))
            {
                columnMappingKey = data[0];
            }
            else
            {
                columnMappingKey = LoggingKeys.DefaultColumn;
            }

            JsonArray columnMapping = metaData[LoggingKeys.ColumnMapping][columnMappingKey];
            string[] columnLabels = columnMapping.Select(x => x.AsString).ToArray();

            remapHelper?.Apply(
                columnLabels: columnLabels,
                data: data);

            Assert.AreEqual(data.Length, columnLabels.Length);
            
            bool boolValue;
            double doubleValue;
            for (int i = 0; i < data.Length; ++i)
            {
                if (data[i].StartsWith("\"") && data[i].EndsWith("\"") && data[i].Length > 1)
                {
                    //Length test exists because technically the data could be a single quotation mark
                    //Remove quotation marks
                    jsonData.Add(columnLabels[i], data[i].Substring(1, data[i].Length - 2));
                }
                else if (bool.TryParse(data[i], out boolValue))
                {
                    jsonData.Add(columnLabels[i], boolValue);
                }
                else if (double.TryParse(data[i], out doubleValue))
                {
                    jsonData.Add(columnLabels[i], doubleValue);
                }
                else //as string
                {
                    jsonData.Add(columnLabels[i], data[i]);
                }

                JsonObject valueMapping = metaData[LoggingKeys.ValueMapping];
                if (valueMapping.ContainsKey(columnLabels[i]))
                {
                    JsonObject mapping = valueMapping[columnLabels[i]];
                    string key = jsonData[columnLabels[i]].AsString;

                    if (mapping.ContainsKey(key))
                    {
                        jsonData[columnLabels[i]] = mapping[key];
                    }
                }
            }

            return jsonData;
        }
    }
}