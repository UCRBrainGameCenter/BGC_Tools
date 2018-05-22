using System.IO;
using BGC.Web;
using BGC.IO;
using UnityEngine.Networking;

namespace BGC.Utility
{
    public static class LogFilesTos3
    {
        /// <summary>
        /// Move all files to s3 and on success move them from the staging area
        /// to the permanent area of logs
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="serverPath"></param>
        public static void MigrateFiles(string bucketName, string serverPath)
        {
            string[] users = Directory.GetDirectories(LogDirectories.StagingDirectory);

            for (int i = 0; i < users.Length; ++i)
            {
                migrateUser(Path.GetFileName(users[i]), bucketName, serverPath);
            }
        }

        /// <summary>
        /// Migrate user logs from staging to permanent on succesful upload to s3
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="bucket"></param>
        /// <param name="serverPath"></param>
        private static void migrateUser(string userName, string bucket, string serverPath)
        {
            string stagingPath = LogDirectories.UserStagingDirectory(userName);
            string permanentPath = LogDirectories.UserPermanentDirectory(userName);
            string[] files = Directory.GetFiles(stagingPath);

            for (int i = 0; i < files.Length; ++i)
            {
                string stagingFile = Path.Combine(stagingPath, files[i]);
                if (ReservedFiles.IsFileReserved(stagingFile))
                {
                    continue;
                }

                string fileName = Path.GetFileName(stagingFile);
                AWSServer.PostFileToAWS(
                    stagingFile,
                    bucket,
                    AWSServer.Combine(serverPath, fileName),
                    (UnityWebRequest request) => {
                        if (request.responseCode == 200)
                        {
                            IO.Utility.SafeMove(stagingFile, Path.Combine(permanentPath, fileName));
                        }
                    });
            }
        }
    }
}