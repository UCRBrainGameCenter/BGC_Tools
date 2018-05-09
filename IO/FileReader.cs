using LightJson.Serialization;
using LightJson;
using System.IO;
using System;
using UnityEngine;

namespace BGC.IO
{
    public static class FileReader
    {

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
            Action<JsonObject> successCallback,
            Action failCallback = null,
            Action fileNotFoundCallback = null,
            Action<string,Exception> overrideExceptionHandling = null)
        {
            bool jsonFIleRead = false;

            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"Unable to find Json file at path \"{path}\"");
                    fileNotFoundCallback?.Invoke();
                }
                else
                {
                    successCallback(JsonReader.ParseFile(path));
                }

                jsonFIleRead = true;
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

            return jsonFIleRead;
        }

        private static void HandleException(string path, Exception excp)
        {
            string[] errorMessage;

            if (excp is ParsingException)
            {
                ParsingException parsingExcp = excp as ParsingException;

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
            else if (excp is JsonParseException)
            {
                JsonParseException jsonExcp = excp as JsonParseException;

                errorMessage = new string[]
                {
                        "JSON parsing error",
                        "",
                        jsonExcp.Message,
                        "",
                        "Error Location:",
                        string.Format("Line {0}, Column {1}", jsonExcp.Position.line + 1, jsonExcp.Position.column + 1)
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