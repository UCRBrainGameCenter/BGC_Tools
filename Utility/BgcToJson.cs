using UnityEngine.Assertions;
using LightJson;
using System;
using BGC.IO.Logging;

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
        /// (Potentially) modifies the <paramref name="metaData"/> argument.
        /// </summary>
        /// <remarks>Intended to fix critical versioning issues.</remarks>
        public delegate void EditBGCMetaData(JsonObject metaData);

        /// <summary>
        /// Uses the column label and data to determine if it should override default BGC Parsing.
        /// </summary>
        /// <returns>Whether this method overrode the standard parsing.</returns>
        /// <remarks>Intended to fix critical versioning issues.</remarks>
        public delegate bool OverrideBGCParsing(string columnLabel, string data, out JsonValue parsedValue);


        /// <summary>
        /// Convert complete string of bgc to json
        /// </summary>
        /// <param name="editMetaData">A function to remap or fix MetaData logging issues</param>
        /// <param name="overrideBGCParsing">A function to remap or fix logging issues</param>
        public static JsonObject ConvertBgcToJson(
            string bgc,
            char separator = '\n',
            EditBGCMetaData editMetaData = null,
            OverrideBGCParsing overrideBGCParsing = null,
            bool verbose = false)
        {
            return ConvertBGCToJson(
                bgc: bgc.Split(separator),
                editMetaData: editMetaData,
                overrideBGCParsing: overrideBGCParsing,
                verbose: verbose);
        }

        /// <summary>
        /// Convert lines of bgc to complete json
        /// </summary>
        /// <param name="editMetaData">A function to remap or fix MetaData logging issues</param>
        /// <param name="overrideBGCParsing">A function to remap or fix logging issues</param>
        /// <returns></returns>
        public static JsonObject ConvertBGCToJson(
            string[] bgc,
            EditBGCMetaData editMetaData = null,
            OverrideBGCParsing overrideBGCParsing = null,
            bool verbose = false)
        {
            JsonObject json = new JsonObject();
            JsonObject metaData = LightJson.Serialization.JsonReader.Parse(bgc[0]);

            //If a MetaData-editing delegate was passed in, execute it
            editMetaData?.Invoke(metaData);

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
                        overrideBGCParsing: overrideBGCParsing));
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
            OverrideBGCParsing overrideBGCParsing)
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
            Assert.AreEqual(data.Length, columnMapping.Count);

            JsonValue jsonValue;
            bool boolValue;
            int intValue;
            double doubleValue;
            for (int i = 0; i < data.Length; ++i)
            {
                if (overrideBGCParsing != null &&
                    overrideBGCParsing.Invoke(columnMapping[i], data[i], out jsonValue))
                {
                    jsonData.Add(columnMapping[i], jsonValue);
                }
                else if (data[i].StartsWith("\"") && data[i].EndsWith("\"") && data[i].Length > 1)
                {
                    //Length test exists because technically the data could be a single quotation mark
                    //Remove quotation marks
                    jsonData.Add(columnMapping[i], data[i].Substring(1, data[i].Length - 2));
                }
                else if (bool.TryParse(data[i], out boolValue))
                {
                    jsonData.Add(columnMapping[i], boolValue);
                }
                else if (int.TryParse(data[i], out intValue))
                {
                    jsonData.Add(columnMapping[i], intValue);
                }
                else if (double.TryParse(data[i], out doubleValue))
                {
                    jsonData.Add(columnMapping[i], doubleValue);
                }
                else //as string
                {
                    jsonData.Add(columnMapping[i], data[i]);
                }

                JsonObject valueMapping = metaData[LoggingKeys.ValueMapping];
                if (valueMapping.ContainsKey(columnMapping[i]))
                {
                    JsonObject mapping = valueMapping[columnMapping[i]];
                    string key = jsonData[columnMapping[i]].AsString;

                    if (mapping.ContainsKey(key))
                    {
                        jsonData[columnMapping[i]] = mapping[key];
                    }
                }
            }

            return jsonData;
        }
    }
}