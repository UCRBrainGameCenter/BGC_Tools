using UnityEngine.Networking;
using BGC.Utility;
using UnityEngine;
using System.Linq;
using LightJson;
using System.IO;
using BGC.Web;
using System;

namespace BGC.IO.Logging
{
    public abstract class Logger
    {
        protected const string bgcExtension = ".bgc";

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

        protected void ApplyHeaders()
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
        /// Push a line to the logger.
        /// a file
        /// </summary>
        /// <param name="line"></param>
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
        /// Push a string to the logger. Will throw an exception if the logger 
        /// hasn't been opened
        /// </summary>
        /// <param name="str"></param>
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
                throw new InvalidOperationException($"Must close the current file before opening a new one for \"{path}\"");
            }

            path = GetNewLogName();

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

#if UNITY_EDITOR
                if (pushToServer == true)
                {
                    AWSServer.PostBGCToJSonToAWS(
                        path,
                        organization,
                        study,
                        applicationName,
                        apiKey,
                        (UnityWebRequest request) =>
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
                        });
                }
#endif
            }
        }

        private string GetNewLogName()
        {
            string indexString = string.Join("", fileIndices.Select(i => $"_{i.ToString("000")}"));

            return PathForLogFile(
                userName: userName,
                filename: $"{userName}_{sessionNumber.ToString("000")}{indexString}_{Name}_{TimeStamp}{bgcExtension}");
        }

        protected string GetExceptionFileName()
        {
            return Path.Combine(
                LogDirectories.ExceptionDirectory,
                $"Exception_{TimeStamp}{bgcExtension}");
        }

        protected string PathForLogFile(string userName, string filename)
        {
            return Path.Combine(LogDirectories.UserStagingDirectory(userName), filename);
        }

        protected void ApplyRequiredFields(JsonObject jo)
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
            return string.Join(delimiter, strings.Select(x => x.ToString()).ToArray());
        }
    }
}
