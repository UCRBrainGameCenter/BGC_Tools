using System.IO;
using BGC.Web;
using BGC.IO;

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
            string stagingPath = Path.Combine(LogDirectories.StagingDirectory, userName);
            string permanentPath = Path.Combine(LogDirectories.PermanentDirectory, userName);
            string[] files = Directory.GetFiles(stagingPath);

            if(Directory.Exists(permanentPath) == false)
            {
                Directory.CreateDirectory(permanentPath);
            }

            for (int i = 0; i < files.Length; ++i)
            {
                string stagingFile = Path.Combine(stagingPath, files[i]);
                string fileName = Path.GetFileName(stagingFile);

                // skip files in use
                if (ReservedFiles.IsFileReserved(stagingFile))
                {
                    continue;
                }

                AWSServer.PostFileToAWS(
                    stagingFile,
                    bucket,
                    serverPath + "/" + fileName,
                    (bool error) => {
                        if (error == false)
                        {
                            string permanentFile = Path.Combine(permanentPath, fileName);
                            if (File.Exists(permanentFile))
                            {
                                File.Delete(permanentFile);
                            }

                            File.Move(stagingFile, permanentFile);
                        }
                    });
            }
        }
    }
}