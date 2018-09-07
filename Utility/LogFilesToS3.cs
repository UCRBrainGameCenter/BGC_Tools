using UnityEngine.Networking;
using UnityEngine.Assertions;
using UnityEngine;
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
        public static void MigrateFiles(string organization, string study, string game, string apiKey)
        {
            Assert.IsFalse(string.IsNullOrEmpty(organization));
            Assert.IsFalse(string.IsNullOrEmpty(study));
            Assert.IsFalse(string.IsNullOrEmpty(game));

            string[] users = Directory.GetDirectories(LogDirectories.StagingDirectory);

            for (int i = 0; i < users.Length; ++i)
            {
                MigrateUser(Path.GetFileName(users[i]), organization, study, game, apiKey);
            }
        }

        /// <summary>
        /// Migrate user logs from staging to permanent on succesful upload to s3
        /// </summary>
        private static void MigrateUser(
            string userName,
            string organization,
            string study,
            string game,
            string apiKey)
        {
            string permanentPath = LogDirectories.UserPermanentDirectory(userName);
            string stagingPath = LogDirectories.UserStagingDirectory(userName);
            string[] files = Directory.GetFiles(stagingPath);

#if !UNITY_EDITOR
            for (int i = 0; i < files.Length; ++i)
            {
                string stagingFile = Path.Combine(stagingPath, files[i]);
                if (ReservedFiles.IsFileReserved(stagingFile))
                {
                    continue;
                }

                AWSServer.PostBGCToJSonToAWS(
                    stagingFile,
                    organization,
                    study, 
                    game,
                    apiKey,
                    (UnityWebRequest request) => {
                        if (request.responseCode == 200)
                        {
                            IO.Utility.SafeMove(stagingFile, Path.Combine(permanentPath, Path.GetFileName(stagingFile)));
                        }
                        else
                        {
                            Debug.LogError(request);
                        }
                    });
            }
#endif
        }
    }
}