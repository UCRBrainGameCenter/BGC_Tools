using UnityEngine.Networking;
using UnityEngine.Assertions;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using BGC.Web;
using BGC.IO;

namespace BGC.Utility
{
    public struct MigrationData
    {
        public string userName;
        public string organization;
        public string study;
    }

    public static class LogFilesTos3
    {
        /// <summary>
        /// Move all files to s3 and on success move them from the staging area
        /// to the permanent area of logs
        /// </summary>
        public static void MigrateFiles(IEnumerable<MigrationData> userData, string game, string apiKey)
        {
            Assert.IsFalse(string.IsNullOrEmpty(game));
            Assert.IsFalse(string.IsNullOrEmpty(apiKey));

            foreach (MigrationData data in userData)
            {
                MigrateUser(data, game, apiKey);
            }
        }

        /// <summary>
        /// Reupload all of the permanent logs for the indicated users
        /// </summary>
        public static void ReUploadFiles(IEnumerable<MigrationData> userData, string game, string apiKey)
        {
            Assert.IsFalse(string.IsNullOrEmpty(game));
            Assert.IsFalse(string.IsNullOrEmpty(apiKey));

            foreach (MigrationData data in userData)
            {
                ReUploadUser(data, game, apiKey);
            }
        }

        /// <summary>
        /// Migrate user logs from staging to permanent on succesful upload to s3
        /// </summary>
        private static void MigrateUser(
            MigrationData migrationData,
            string game,
            string apiKey)
        {
#if !UNITY_EDITOR || EDITOR_SERVER_ENABLED
            string permanentPath = LogDirectories.UserPermanentDirectory(migrationData.userName);
            string stagingPath = LogDirectories.UserStagingDirectory(migrationData.userName);
            string errorPath = LogDirectories.UserErrorLogDirectory(migrationData.userName);
            string[] files = Directory.GetFiles(stagingPath);

            for (int i = 0; i < files.Length; ++i)
            {
                string stagingFile = Path.Combine(stagingPath, files[i]);
                if (ReservedFiles.IsFileReserved(stagingFile))
                {
                    continue;
                }

                AWSServer.PostBGCToJSonToAWS(
                    stagingFile,
                    migrationData.organization,
                    migrationData.study,
                    game,
                    apiKey,
                    (UnityWebRequest request, bool validJson) =>
                    {
                        if (validJson == true)
                        {
                            if (request.responseCode == 200)
                            {
                                IO.Utility.SafeMove(stagingFile, Path.Combine(permanentPath, Path.GetFileName(stagingFile)));
                            }
                            else
                            {
                                Debug.LogError(request);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Invalid json file: {stagingFile}");
                            IO.Utility.SafeMove(stagingFile, Path.Combine(errorPath, Path.GetFileName(stagingFile)));
                        }
                    });
            }
#endif
        }

        /// <summary>
        /// Reupload user logs from permanent
        /// </summary>
        public static void ReUploadUser(
            MigrationData migrationData,
            string game,
            string apiKey)
        {
#if !UNITY_EDITOR || EDITOR_SERVER_ENABLED
            string permanentPath = LogDirectories.UserPermanentDirectory(migrationData.userName);
            string[] files = Directory.GetFiles(permanentPath);

            for (int i = 0; i < files.Length; ++i)
            {
                string file = Path.Combine(permanentPath, files[i]);
                if (ReservedFiles.IsFileReserved(file))
                {
                    continue;
                }

                AWSServer.PostBGCToJSonToAWS(
                    file,
                    migrationData.organization,
                    migrationData.study,
                    game,
                    apiKey,
                    (UnityWebRequest request, bool validJson) =>
                    {
                        if (validJson)
                        {
                            if (request.responseCode != 200)
                            {
                                Debug.LogError(request);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Invalid json file: {file}");
                        }
                    });
            }
#endif
        }
    }
}