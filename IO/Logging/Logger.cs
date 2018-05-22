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
        public enum LogType
        {
            Regular,
            Summary,
            Exception
        }

        protected const string dateTimeFormat = "yy_MM_dd_HH_mm_ss";
        protected const string bgcExtension = ".bgc";

        protected string delimiter;
        private StreamWriter logger;
        private string path;
        private LogType type;

        private readonly string applicationName;
        private readonly string applicationVersion;
        private readonly string userName;
        private readonly int sessionNumber;
        private readonly int runNumber;

        public abstract string Name { get; }

        public Logger(
            LogType type,
            string applicationName,
            string applicationVersion,
            string userName,
            int sessionNumber,
            int runNumber,
            string delimiter = "|")
        {
            this.type               = type;
            this.applicationName    = applicationName;
            this.applicationVersion = applicationVersion;
            this.userName           = userName;
            this.sessionNumber      = sessionNumber;
            this.runNumber          = runNumber;
            this.delimiter          = delimiter;
        }

        ~Logger()
        {
            if(logger != null)
            {
                logger.Close();
            }

            ReservedFiles.UnReserveFile(path);
        }

        protected abstract JsonObject ConstructColumnMapping();
        protected abstract JsonObject ConstructValueMapping();

        protected void ApplyHeaders()
        {
            JsonObject header = new JsonObject();
            ApplyRequiredFields(header);
            header.Add(LoggingKeys.ColumnMapping, ConstructColumnMapping());
            header.Add(LoggingKeys.ValueMapping, ConstructValueMapping());

            PushLine(header.ToString());
        }
        
        /// <summary>
        /// Push a line to the logger.
        /// a file
        /// </summary>
        /// <param name="line"></param>
        public void PushLine(params IConvertible[] strings)
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
        public void PushString(string str)
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
                throw new InvalidOperationException("Must close the current file before opening a new one for: " + path);
            }

            switch (type)
            {
                case LogType.Regular:
                    path = GetNewLogName(userName, runNumber, sessionNumber);
                    break;
                case LogType.Summary:
                    path = GetSummaryFileName(userName, sessionNumber);
                    break;
                case LogType.Exception:
                    path = GetExceptionFileName();
                    break;
                default:
                    throw new InvalidDataException("Log type " + type + " is invalid");
            }

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

        public void CloseFile(string userName, string bucket, string serverPath)
        {
            if (logger != null)
            {
                logger.Close();
                logger = null;
                ReservedFiles.UnReserveFile(path);

                #if !UNITY_EDITOR
                    AWSServer.PostFileToAWS(
                        path, 
                        bucket, 
                        AWSServer.Combine(serverPath, Path.GetFileName(path)),
                        (bool error) => {
                            if(error == false)
                            {
                                Utility.SafeMove(path, Path.Combine(
                                    LogDirectories.UserPermanentDirectory(userName),
                                    Path.GetFileName(path)));
                            }
                        });
                #endif
            }
        }

        protected string GetNewLogName(string userName, int runNumber, int session)
        {
            return PathForLogFile(userName, string.Format(
                "{0}_{1}_{2}_{3}_{4}.bgc",
                userName,
                session.ToString("000"),
                runNumber.ToString("000"),
                Name,
                DateTime.Now.ToString("yy_MM_dd_HH_mm_ss")));
        }

        protected string GetSummaryFileName(string userName, int sessionNumber)
        {
            return PathForLogFile(userName, string.Format(
                "{0}_{1}_{2}_{3}.bgc",
                userName,
                sessionNumber,
                Name,
                DateTime.Now.ToString(dateTimeFormat)));
        }

        protected string GetExceptionFileName()
        {
            return Path.Combine(LogDirectories.ExceptionDirectory, string.Format(
                "{0}_{1}" + bgcExtension,
                "Exception",
                DateTime.Now.ToString(dateTimeFormat)));
        }

        protected string PathForLogFile(string userName, string filename)
        {
            return Path.Combine(LogDirectories.UserStagingDirectory(userName), filename);
        }

        protected void ApplyRequiredFields(JsonObject jo)
        {
            jo.Add(LoggingKeys.GameName,  applicationName);
            jo.Add(LoggingKeys.Version,   applicationVersion);
            jo.Add(LoggingKeys.UserName,  userName);
            jo.Add(LoggingKeys.DeviceID,  SystemInfo.deviceUniqueIdentifier);
            jo.Add(LoggingKeys.Session,   sessionNumber);
            jo.Add(LoggingKeys.RunNumber, runNumber);
            jo.Add(LoggingKeys.Delimiter, delimiter);
        }

        private string DelimiterLine(params IConvertible[] strings)
        {
            return string.Join(delimiter, strings.Select(x => x.ToString()).ToArray());
        }
    }
}
