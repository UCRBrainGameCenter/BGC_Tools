using System.IO;

namespace BGC.IO
{
    public class LogDirectories
    {
        public const string LogsDirectory = "Logs";
        public const string S3StagingDirectory = "Staging";
        public const string ExceptionsDirectory = "Exceptions";
        public const string S3PermanentDirectory = "Permanent";
        public const string ErrorLogsDirectory = "ErrorLogs";

        private static string logDirectory = null;
        private static string stagingDirectory = null;
        private static string permanentDirectory = null;
        private static string exceptionDirectory = null;
        private static string errorLogDirectory = null;

        /// <summary> Logs directory for all users </summary>
        public static string LogDirectory => logDirectory ??
            (logDirectory = DataManagement.PathForDataDirectory(LogsDirectory));

        /// <summary>
        /// Directory for all staged files that are to be moved to s3 and then
        /// put into the PermanentDirectory
        /// </summary>
        public static string StagingDirectory => stagingDirectory ??
            (stagingDirectory = DataManagement.PathForDataDirectory(S3StagingDirectory));

        /// <summary> Directory for all logs that have been merged into s3 </summary>
        public static string PermanentDirectory => permanentDirectory ??
            (permanentDirectory = DataManagement.PathForDataDirectory(S3PermanentDirectory));

        /// <summary> Directory for all logs that have errors and can't be merged into s3 </summary>
        public static string ErrorLogDirectory => errorLogDirectory ??
            (errorLogDirectory = DataManagement.PathForDataDirectory(ErrorLogsDirectory));

        /// <summary>
        /// Get path for exceptions directory. Creates directory if it does not
        /// exist
        /// </summary>
        public static string ExceptionDirectory => exceptionDirectory ??
            (exceptionDirectory = DataManagement.PathForDataDirectory(ExceptionsDirectory));

        /// <summary>
        /// Get the path to the user in the staging directory. This automatically creates 
        /// a directory if it does not exist
        /// </summary>
        public static string UserStagingDirectory(string user) => SafeCombine(StagingDirectory, user);

        /// <summary>
        /// Get the path to the user in the permanent directory. This automatically
        /// creates a directory if it does not exists
        /// </summary>
        public static string UserPermanentDirectory(string user) => SafeCombine(PermanentDirectory, user);

        /// <summary>
        /// Get the path to the user in the ErrorLog directory. This automatically
        /// creates a directory if it does not exists
        /// </summary>
        public static string UserErrorLogDirectory(string user) => SafeCombine(ErrorLogDirectory, user);

        /// <summary>
        /// Returns the Path-combined directories, and creates them they don't exist
        /// </summary>
        /// <returns>Combined path</returns>
        private static string SafeCombine(string directory, string user)
        {
            string path = Path.Combine(directory, user);

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
    }
}