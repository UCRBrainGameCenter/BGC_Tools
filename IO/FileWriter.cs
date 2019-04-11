using System;
using System.IO;
using UnityEngine;
using LightJson;
using LightJson.Serialization;

namespace BGC.IO
{
    public static class FileWriter
    {
        public static bool WriteJson(
            string path,
            Func<JsonObject> createJson,
            bool pretty = false,
            Action failCallback = null,
            Action<string, Exception> overrideExceptionHandling = null)
        {
            bool wroteToFile = false;

            try
            {
                File.WriteAllText(path, createJson().ToString(pretty));
                wroteToFile = true;
            }
            catch (Exception excp)
            {
                Debug.LogError($"Error serializing {path}: {excp.Message}");

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

            return wroteToFile;
        }

        private static void HandleException(string path, Exception excp)
        {
            string[] errorMessage;

            if (excp is JsonSerializationException)
            {
                errorMessage = new string[]
                {
                    "JSON serialization error.",
                    "",
                    excp.Message
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

            HandleExceptionFile(path, errorMessage);
        }

        private static void HandleExceptionFile(string path, string[] errorMessage)
        {
            string fileName = Path.GetFileName(path);

            string directoryPath = Path.GetDirectoryName(path);
            string directoryName = Path.GetFileName(directoryPath);
            string errorDirectory = $"Error{directoryName}";
            string errorDirectoryPath = Path.Combine(Path.GetDirectoryName(directoryPath), errorDirectory);
            string errorLogPath = Path.Combine(errorDirectoryPath, $"{fileName}_error.txt");

            if (!Directory.Exists(errorDirectoryPath))
            {
                Directory.CreateDirectory(errorDirectoryPath);
            }

            if (File.Exists(errorLogPath))
            {
                File.Delete(errorLogPath);
            }

            File.WriteAllLines(errorLogPath, errorMessage);
        }
    }
}
