using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using LightJson;
using BGC.Web;
using BGC.Utility;

namespace BGC.IO.Logging
{
    public abstract class Logger
    {
        protected static string TimeStamp => DateTime.Now.ToString("yy_MM_dd_HH_mm_ss");

        protected string delimiter;
        private StreamWriter logger;
        private string path;

        private readonly string applicationName;
        private readonly string applicationVersion;
        private readonly string userName;
        private readonly int sessionNumber;

        private readonly int[] fileIndices;

        private readonly bool pushToServer;

        public abstract string Name { get; }

        public Logger(
            string applicationName,
            string applicationVersion,
            string userName,
            int sessionNumber,
            int[] fileIndices,
            bool pushToServer,
            string delimiter = "|")
        {
            if (fileIndices == null)
            {
                fileIndices = new int[] { };
            }

            this.applicationName = applicationName;
            this.applicationVersion = applicationVersion;
            this.userName = userName;
            this.sessionNumber = sessionNumber;
            this.fileIndices = fileIndices;
            this.pushToServer = pushToServer;
            this.delimiter = delimiter;
        }

        ~Logger()
        {
            CloseFile();
        }

        protected virtual JsonObject ConstructAdditionalHeaders() { return new JsonObject(); }
        protected abstract JsonObject ConstructColumnMapping();
        protected abstract JsonObject ConstructValueMapping();

        private void ApplyHeaders()
        {
            JsonObject header = new JsonObject
            {
                { LoggingKeys.AdditionalHeaders, ConstructAdditionalHeaders() },
                { LoggingKeys.ColumnMapping, ConstructColumnMapping() },
                { LoggingKeys.ValueMapping, ConstructValueMapping() }
            };

            ApplyRequiredFields(header);
            PushLine(header.ToString());
        }

        /// <summary>
        /// Push a line to the logger. Open the logger if it's null.  
        /// DON'T call "ToString()" unnecessarily unless you want the field written to file in 
        /// quotation marks.
        /// </summary>
        protected void PushLine(params IConvertible[] strings)
        {
            if (logger == null)
            {
                OpenFile();
            }

            logger.WriteLine(DelimiterLine(strings));
            logger.Flush();
        }

        /// <summary>
        /// Push a string to the logger. Open the logger if it's null.
        /// </summary>
        protected void PushString(string str)
        {
            if (logger == null)
            {
                OpenFile();
            }

            logger.Write(str);
            logger.Flush();
        }

        private void OpenFile()
        {
            if (logger != null)
            {
                throw new InvalidOperationException(
                    $"Must close the current file before opening a new one for \"{path}\"");
            }

            // if fileIndices is { }:           ""
            // if fileIndices is {5}:           "_005"
            // if fileIndices is {1, 2}:        "_001_002"
            // if fileIndices is {0, 1, 2}:     "_000_001_002"
            string indexString = string.Join("", fileIndices.Select(i => $"_{i.ToString("000")}"));
            string fileName = $"{userName}_{sessionNumber.ToString("000")}{indexString}_{Name}_{TimeStamp}{FileExtensions.BGC}";

            path = PathForLogFile(
                userName: userName,
                filename: fileName,
                pushToServer: pushToServer);

            ReservedFiles.ReserveFile(path);
            logger = File.AppendText(path);
            ApplyHeaders();
        }

        public void CloseFile()
        {
            if (logger != null)
            {
                logger.Close();
                ReservedFiles.UnReserveFile(path);
                logger = null;
            }
        }

        public void CloseFile(string userName, string organization, string study, string apiKey)
        {
            if (logger != null)
            {
                logger.Close();
                logger = null;
                ReservedFiles.UnReserveFile(path);

                if (pushToServer == true)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(userName));
                    Assert.IsFalse(string.IsNullOrEmpty(organization));
                    Assert.IsFalse(string.IsNullOrEmpty(study));
                    Assert.IsFalse(string.IsNullOrEmpty(apiKey));

#if !UNITY_EDITOR || EDITOR_SERVER_ENABLED
                    AWSServer.PostBGCToJSonToAWS(
                        path,
                        organization,
                        study,
                        applicationName,
                        apiKey,
                        (UnityWebRequest request, bool validJson) =>
                        {
                            if (validJson == true)
                            {
                                if (request.isNetworkError == false)
                                {
                                    Utility.SafeMove(path, Path.Combine(
                                        LogDirectories.UserPermanentDirectory(userName),
                                        Path.GetFileName(path)));
                                }
                                else
                                {
                                    Debug.LogError(request.ToString());
                                }
                            }
                            else
                            {
                                Utility.SafeMove(path, Path.Combine(
                                    LogDirectories.UserErrorLogDirectory(userName),
                                    Path.GetFileName(path)));
                            }
                        });
#endif
                }
            }
        }

        private string GetExceptionFileName()
        {
            return Path.Combine(
                LogDirectories.ExceptionDirectory,
                $"Exception_{TimeStamp}{FileExtensions.BGC}");
        }

        private static string PathForLogFile(string userName, string filename, bool pushToServer)
        {
            if (pushToServer == false)
            {
                //Files not being pushed to the server are written straight to the permanent dir
                return Path.Combine(LogDirectories.UserPermanentDirectory(userName), filename);
            }

            return Path.Combine(LogDirectories.UserStagingDirectory(userName), filename);
        }

        private void ApplyRequiredFields(JsonObject jo)
        {
            jo.Add(LoggingKeys.GameName, applicationName);
            jo.Add(LoggingKeys.Version, applicationVersion);
            jo.Add(LoggingKeys.UserName, userName);
            jo.Add(LoggingKeys.DeviceID, SystemInfo.deviceUniqueIdentifier);
            jo.Add(LoggingKeys.Session, sessionNumber);
            jo.Add(LoggingKeys.Delimiter, delimiter);
        }

        private string DelimiterLine(params IConvertible[] strings)
        {
            return string.Join(delimiter, strings.Select(ConvertToBGCString).ToArray());
        }

        /// <summary>
        /// Used on each element of our varargs conversion method to generate a bgcString
        /// </summary>
        private string ConvertToBGCString(IConvertible element)
        {
            switch (element.GetTypeCode())
            {
                case TypeCode.Char:
                case TypeCode.String:
                    return $"\"{element.ToString()}\"";
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.DateTime:
                case TypeCode.DBNull:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Empty:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Object:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return element.ToString();
                default:
                    Debug.LogError($"Unexpected Typecode: {element.GetTypeCode()}");
                    return element.ToString();
            }
        }
    }
}
