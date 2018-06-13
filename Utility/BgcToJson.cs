using UnityEngine.Assertions;
using LightJson;
using System;

public static class BgcToJson
{
    public static class RequiredFields
    {
        public const string DefaultColumn = "default";
        public const string Delimiter = "delimiter";
        public const string ColumnMapping = "column_mapping";
        public const string ValueMapping = "value_mapping";
        public const string MetaData = "meta_data";
        public const string Data = "data";

        public static readonly string[] MetaDataFields = new string[] {
            "game_name", "version", "user_name", "device_id", "session_number",
            "run_number", Delimiter, ColumnMapping, ValueMapping};

        public static readonly string[] RedundantFields = new string[] {
            ColumnMapping, ValueMapping, Delimiter};
    }

    /// <summary>
    /// Convert complete string of bgc to json
    /// </summary>
    /// <param name="bgc"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    public static JsonObject ConvertBgcToJson(string bgc, char separator = '\n', bool verbose = false)
    {
        return ConvertBGCToJson(bgc.Split(separator), verbose);
    }

    /// <summary>
    /// Convert lines of bgc to complete json
    /// </summary>
    /// <param name="bgc"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    public static JsonObject ConvertBGCToJson(string[] bgc, bool verbose = false)
    {
        JsonObject json = new JsonObject();
        JsonObject metaData = LightJson.Serialization.JsonReader.Parse(bgc[0]);
        Assert.IsTrue(MetaDataHasCorrectFields(metaData, verbose));

        JsonArray data = new JsonArray();
        string[] delimiter = new string[] { metaData[RequiredFields.Delimiter].AsString };
        for (int i = 1; i < bgc.Length; ++i)
        {
            data.Add(ConvertLine(metaData, delimiter, bgc[i]));
        }

        json.Add(RequiredFields.MetaData, metaData);
        json.Add(RequiredFields.Data, data);

        return json;
    }

    /// <summary>
    /// Test to see whether or not the meta data has all the correct fields
    /// </summary>
    /// <param name="metaData"></param>
    /// <param name="verbose"></param>
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
    /// <param name="metaData"></param>
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
    /// <param name="metaData"></param>
    /// <param name="splitBy"></param>
    /// <param name="line"></param>
    /// <returns>Json object representing the line based on the colum manpping</returns>
    private static JsonObject ConvertLine(JsonObject metaData, string[] splitBy, string line)
    {
        JsonObject jsonData = new JsonObject();
        string[] data = line.Split(splitBy, StringSplitOptions.None);

        string columnMappingKey;
        if (metaData[RequiredFields.ColumnMapping].AsJsonObject.ContainsKey(data[0]))
        {
            columnMappingKey = data[0];
        }
        else
        {
            columnMappingKey = RequiredFields.DefaultColumn;
        }

        JsonArray columnMapping = metaData[RequiredFields.ColumnMapping][columnMappingKey];
        Assert.AreEqual(data.Length, columnMapping.Count);

        for (int i = 0; i < data.Length; ++i)
        {
            bool b;
            int intNum;
            float floatNum;

            if (bool.TryParse(data[i], out b))
            {
                jsonData.Add(columnMapping[i], b);
            }
            else if (int.TryParse(data[i], out intNum))
            {
                jsonData.Add(columnMapping[i], intNum);
            }
            else if (float.TryParse(data[i], out floatNum))
            {
                jsonData.Add(columnMapping[i], floatNum);
            }
            else
            {
                jsonData.Add(columnMapping[i], data[i]);
            }
        }

        return jsonData;
    }
}