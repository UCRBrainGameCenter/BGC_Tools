using UnityEngine;
using System.IO;

namespace BGC.IO
{
    public class LogManagement
    {
        public const string LogsDirectory = "Logs";
        public const string S3StagingDirectory = "Staging";
        public const string ExceptionsDirectory = "Exceptions";
        public const string S3PermanentDirectory = "Permanent";

        private static string logDirectory = null;
        private static string rootDirectory = null;
        private static string stagingDirectory = null;
        private static string permanentDirectory = null;
        private static string exceptionDirectory = null;

        /// <summary>
        /// Root directory for all users data
        /// </summary>
        public static string RootDirectory
        {
            get
            {
                if (rootDirectory == null)
                {
                    rootDirectory = Application.persistentDataPath;

                    if (Application.platform != RuntimePlatform.IPhonePlayer &&
                        Application.platform != RuntimePlatform.Android)
                    {
                        rootDirectory = Application.dataPath;
                        rootDirectory = rootDirectory.Substring(0, rootDirectory.LastIndexOf('/'));
                    }
                }

                return rootDirectory;
            }
        }

        /// <summary>
        /// Logs directory for all users
        /// </summary>
        public static string LogDirectory
        {
            get
            {
                if (logDirectory == null)
                {
                    logDirectory = Path.Combine(RootDirectory, LogsDirectory);

                    if (Directory.Exists(logDirectory) == false)
                    {
                        Directory.CreateDirectory(logDirectory);
                    }
                }

                return logDirectory;
            }
        }

        /// <summary>
        /// Directory for all staged files that are to be moved to s3 and then
        /// put into the PermanentDirectory
        /// </summary>
        public static string StagingDirectory
        {
            get
            {
                if (stagingDirectory == null)
                {
                    stagingDirectory = Path.Combine(LogDirectory, S3StagingDirectory);

                    if (Directory.Exists(stagingDirectory) == false)
                    {
                        Directory.CreateDirectory(stagingDirectory);
                    }
                }

                return stagingDirectory;
            }
        }

        /// <summary>
        /// Directory for all logs that have been merged into s3
        /// </summary>
        public static string PermanentDirectory
        {
            get
            {
                if (permanentDirectory == null)
                {
                    permanentDirectory = Path.Combine(LogDirectory, S3PermanentDirectory);

                    if (Directory.Exists(permanentDirectory) == false)
                    {
                        Directory.CreateDirectory(permanentDirectory);
                    }
                }

                return permanentDirectory;
            }
        }

        /// <summary>
        /// Get path for exceptions directory. Creates directory if it does not
        /// exist
        /// </summary>
        public static string ExceptionDirectory
        {
            get
            {
                if (exceptionDirectory == null)
                {
                    exceptionDirectory = Path.Combine(LogDirectory, ExceptionsDirectory);

                    if (Directory.Exists(exceptionDirectory) == false)
                    {
                        Directory.CreateDirectory(exceptionDirectory);
                    }
                }

                return exceptionDirectory;
            }
        }

        /// <summary>
        /// Get the path to the user in the stagin directory. This automatically creates 
        /// a directory if it does not exist
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string UserStagingDirectory(string user)
        {
            string dir = Path.Combine(StagingDirectory, user);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

        /// <summary>
        /// Get the path to the user in the permanent directory. This automatically
        /// creates a directory if it does not exists
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string UserPermanentDirectory(string user)
        {
            string dir = Path.Combine(PermanentDirectory, user);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }
    }
}