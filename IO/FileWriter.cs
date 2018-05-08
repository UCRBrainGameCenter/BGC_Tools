using LightJson.Serialization;
using LightJson;
using System.IO;
using System;
using UnityEngine;

namespace BGC.IO
{
    public static class FileWriter
    {
        public static bool WriteJson(string path, string errorDirectory, Func<JsonObject> createJson, bool pretty = false)
        {
            bool wroteToFile = false;

            try
            {
                File.WriteAllText(path, createJson().ToString(pretty));
                wroteToFile = true;
            }
            catch(JsonSerializationException excp)
            {
                Debug.LogError(string.Format("JSON error serializing Protocols at {0}: {1}",
                    path, excp.Message));

                string fileName = Path.GetFileName(path);

                string[] errorLog = new string[]
                {
                    "JSON serialization error.",
                    "",
                    excp.Message
                };

                File.WriteAllLines(
                    Path.Combine(
                        Path.Combine(LogManagement.GetDataRootDir(), errorDirectory), 
                        fileName + FileReader.ErrorLogExtension),
                    errorLog);
            }

            return wroteToFile;
        }
    }
}
