using LightJson.Serialization;
using UnityEngine;
using LightJson;
using System.IO;
using System;

namespace BGC.IO
{
    public static class FileReader
    {
        public const string ErrorLogExtension = "_error.txt";

        /// <summary>
        /// Gives two callbacks for when a file is not found and for when a file has
        /// been succesfully parsed into a json object. With the latter, you can use 
        /// the parsed object to build out whatever you need. 
        /// 
        /// There are also two functions where you return an array of strings for 
        /// error message that will be placed into your defined error directory for
        /// logging.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="errorLogDirectory"></param>
        /// <param name="jsonParseExceptionMessage"></param>
        /// <param name="parsingExceptionMessage"></param>
        /// <param name="fileNotFoundCallback"></param>
        /// <param name="successCallback"></param>
        /// <returns>Boolean if a file has been sucesfully read</returns>
        public static bool ReadJsonFile(
            string path,
            string errorLogDirectory,
            Func<JsonParseException, string[]> jsonParseExceptionMessage,
            Func<ParsingException, string[]> parsingExceptionMessage,
            Action fileNotFoundCallback, 
            Action<JsonObject> successCallback)
        {
            bool jsonFIleRead = false;

            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogError(string.Format("Unable to find Protocols file at path \"{1}\"", path));
                    fileNotFoundCallback();
                }
                else
                {
                    successCallback(JsonReader.ParseFile(path));
                }

                jsonFIleRead = true;
            }
            catch (JsonParseException excp)
            {
                Debug.LogError(string.Format("JSON error parsing Protocols at {0}: {1}", path, excp.Message));
                parseException(path, errorLogDirectory, jsonParseExceptionMessage(excp));
            }
            catch (ParsingException excp)
            {
                Debug.LogError(string.Format("JSON error parsing Protocols at {0}: {1}", path, excp.Message));
                parseException(path, errorLogDirectory, parsingExceptionMessage(excp));
            }

            return jsonFIleRead;
        }

        private static void parseException(string path, string errorDirectory, string[] errorMessage)
        {
            string fileName    = Path.GetFileName(path);
            string errorDir    = Path.Combine(LogManagement.LogDirectory, errorDirectory);
            string newFilePath = Path.Combine(errorDir, fileName);

            if (File.Exists(newFilePath))
            {
                File.Delete(newFilePath);
            }
            File.Move(path, newFilePath);

            string newFileErrLog = Path.Combine(errorDir, fileName + ErrorLogExtension);

            if (File.Exists(newFileErrLog))
            {
                File.Delete(newFileErrLog);
            }
            File.WriteAllLines(newFileErrLog, errorMessage);
        }
    }
}