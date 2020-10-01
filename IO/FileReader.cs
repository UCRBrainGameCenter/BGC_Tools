using System;
using System.IO;
using UnityEngine;
using LightJson;
using LightJson.Serialization;

namespace BGC.IO
{
    public static class FileReader
    {
        /// <summary>
        /// Offers two error callbacks for when a file is not found and for when a file failed to
        /// parse correctly, and one for when the file has been succesfully parsed into a json object.
        /// With the latter, you can use the parsed object to build out whatever you need. 
        /// 
        /// There is also an optional override for exception handling.
        /// </summary>
        /// <param name="path">File Path to load</param>
        /// <param name="successCallback">Executed only on successful read</param>
        /// <param name="failCallback">Notifies caller of failure</param>
        /// <param name="fileNotFoundCallback">Notifies caller of failure to locate file.
        /// Supplying this argument suppresses the default Debug.LogError</param>
        /// <param name="overrideExceptionHandling">Optionally replaces default exception handling</param>
        /// <returns>Whether the json file was successfully read</returns>
        public static bool ReadJsonFile(
            string path,
            Action<JsonObject> successCallback,
            Action failCallback = null,
            Action fileNotFoundCallback = null,
            Action<string,Exception> overrideExceptionHandling = null)
        {
            bool jsonFileRead = false;

            try
            {
                if (!File.Exists(path))
                {
                    if (fileNotFoundCallback == null)
                    {
                        Debug.LogError($"Unable to find Json file at path \"{path}\"");
                    }
                    else
                    {
                        fileNotFoundCallback.Invoke();
                    }
                }
                else
                {
                    successCallback(JsonReader.ParseFile(path));
                    //If the above callback itself throws an exception, it will be caught here and
                    //ultimately the method will return false.
                    jsonFileRead = true;
                }
            }
            catch (Exception excp)
            {
                Debug.LogError($"Error parsing file at {path}: {excp.Message}");

                failCallback?.Invoke();

                if (overrideExceptionHandling != null)
                {
                    overrideExceptionHandling.Invoke(path, excp);
                }
                else
                {
                    HandleException(path, excp);
                }
            }

            return jsonFileRead;
        }

        /// <summary>
        /// Tries reading Json from a TextReader and returns the parsed JsonValue.
        /// Includes an optional exceptionHandler argument
        /// </summary>
        /// <param name="jsonReader">As reader containing serialized Json</param>
        /// <param name="exceptionHandler">An optional exception handler</param>
        /// <returns>Whether the json was successfully read</returns>
        public static JsonValue ReadJsonStream(
            TextReader jsonReader,
            Action<Exception> exceptionHandler = null)
        {
            Debug.Assert(jsonReader != null);

            JsonValue parsedJson;

            try
            {
                parsedJson = JsonReader.Parse(jsonReader);
            }
            catch (Exception excp)
            {
                Debug.LogError($"Error parsing Json by text: {excp.Message}");
                parsedJson = JsonValue.Null;
                exceptionHandler?.Invoke(excp);
            }

            return parsedJson;
        }

        /// <summary>
        /// Tries reading a Json string and returns the parsed JsonValue.
        /// Includes an optional exceptionHandler argument.
        /// </summary>
        /// <param name="jsonText">As string containing deserialized Json</param>
        /// <param name="exceptionHandler">An optional exception handler</param>
        /// <returns>Whether the json was successfully read</returns>
        public static JsonValue SafeReadJsonString(
            string jsonText,
            Action<Exception> exceptionHandler = null)
        {
            Debug.Assert(string.IsNullOrEmpty(jsonText) == false);

            JsonValue parsedJson;

            try
            {
                parsedJson = JsonReader.Parse(jsonText);
            }
            catch (Exception excp)
            {
                Debug.LogError($"Error parsing Json by text: {excp.Message}");
                parsedJson = JsonValue.Null;
                exceptionHandler?.Invoke(excp);
            }

            return parsedJson;
        }

        private static void HandleException(string path, Exception excp)
        {
            string[] errorMessage;

            if (excp is ParsingException parsingExcp)
            {
                errorMessage = new string[]
                {
                    "Internal parsing error.",
                    "",
                    parsingExcp.Message,
                    "",
                    "Suggested corrective action:",
                    parsingExcp.correctiveAction
                };
            }
            else if (excp is JsonParseException jsonExcp)
            {
                errorMessage = new string[]
                {
                    "JSON parsing error",
                    "",
                    jsonExcp.Message,
                    "",
                    "Error Location:",
                    $"Line {jsonExcp.Position.line + 1}, Column {jsonExcp.Position.column + 1}"
                };
            }
            else
            {
                errorMessage = new string[]
                {
                    "Exception Encountered",
                    "",
                    excp.Message,
                };
            }

            HandleExceptionFiles(path, errorMessage);
        }

        private static void HandleExceptionFiles(string path, string[] errorMessage)
        {
            string fileName = Path.GetFileName(path);

            string directoryPath = Path.GetDirectoryName(path);
            string directoryName = Path.GetFileName(directoryPath);
            string errorDirectory = $"Error{directoryName}";
            string errorDirectoryPath = Path.Combine(Path.GetDirectoryName(directoryPath), errorDirectory);
            string errorFilePath = Path.Combine(errorDirectoryPath, fileName);
            string errorLogPath = $"{errorFilePath}_error.txt";

            if (!Directory.Exists(errorDirectoryPath))
            {
                Directory.CreateDirectory(errorDirectoryPath);
            }

            if (File.Exists(errorFilePath))
            {
                File.Delete(errorFilePath);
            }
            File.Move(path, errorFilePath);

            if (File.Exists(errorLogPath))
            {
                File.Delete(errorLogPath);
            }

            File.WriteAllLines(errorLogPath, errorMessage);
        }
    }
}