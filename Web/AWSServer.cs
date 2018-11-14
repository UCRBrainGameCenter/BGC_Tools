using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Assertions;
using BGC.Web.Utility;
using BGC.MonoUtility;
using System.IO;
using LightJson;
using System;

namespace BGC.Web
{
    public static class AWSServer
    {
        public static class HeaderKeys
        {
            public const string ApiKey = "x-api-key";
            public const string CacheControl = "Cache-Control";
        }

        public static class BodyKeys
        {
            public const string Oganization = "organization";
            public const string Condition = "condition";
            public const string FileName = "file_name";
            public const string Content = "content";
            public const string Study = "study";
            public const string Game = "game";
            public const string Code = "code";
        }

        public const string ConditionURL = "https://84cje3rj4j.execute-api.us-east-1.amazonaws.com/Production/condition-retriever";
        public const string CodeURL = "https://84cje3rj4j.execute-api.us-east-1.amazonaws.com/Production/code-retriever";
        public const string ToS3URL = "https://84cje3rj4j.execute-api.us-east-1.amazonaws.com/Production/to-s3";

        public const string PathSeparator = "/";
        public const string CacheControl = "no-cache";
        public const string BGCExtension = ".bgc";
        public const string JSONExtension = ".json";

        private const char extensionSplitter = '.';

        public static Dictionary<string, string> Header(string apiKey)
        {
            return new Dictionary<string, string>
            {
                { HeaderKeys.ApiKey, apiKey},
                { "Content-Type", "application/x-www-form-urlencoded" }
            };
        }

        public static void PostBGCToJSonToAWS(
            string filePath,
            string organization,
            string study,
            string game,
            string apiKey,
            Action<UnityWebRequest, bool> callBack = null)
        {
            Assert.IsFalse(String.IsNullOrEmpty(filePath));
            Assert.IsFalse(String.IsNullOrEmpty(organization));
            Assert.IsFalse(String.IsNullOrEmpty(study));
            Assert.IsFalse(String.IsNullOrEmpty(game));
            Assert.IsTrue(ContainsBGCExtension(filePath));
            Assert.IsTrue(File.Exists(filePath));

            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            try
            {

                JsonObject jsonContent = BGC.Utility.BgcToJson.ConvertBgcToJson(content);
                PostToAWS(
                    Path.GetFileName(filePath).Replace(BGCExtension, JSONExtension),
                    organization,
                    study,
                    game,
                    jsonContent,
                    apiKey,
                    callBack);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error sending file {filePath} with exception {e.Message}.");
                callBack(null, false);
            }
        }

        /// <summary>
        /// Post to aws
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="organization"></param>
        /// <param name="study"></param>
        /// <param name="game"></param>
        /// <param name="callBack"></param>
        public static void PostToAWS(
            string fileName,
            string organization,
            string study,
            string game,
            JsonObject content,
            string apiKey,
            Action<UnityWebRequest, bool> callBack = null)
        {
            Assert.IsFalse(String.IsNullOrEmpty(fileName));
            Assert.IsFalse(String.IsNullOrEmpty(organization));
            Assert.IsFalse(String.IsNullOrEmpty(study));
            Assert.IsFalse(String.IsNullOrEmpty(game));
            Assert.IsTrue(ContainsJSONExtension(fileName));

#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("Overwritting organization and study.");
            organization = "braingamecenter";
            study = "default";
#endif

            JsonObject body = new JsonObject
            {
                {
                    "body",
                    new JsonObject
                    {
                        { BodyKeys.Oganization, organization },
                        { BodyKeys.Study, study },
                        { BodyKeys.Game, game },
                        { BodyKeys.FileName, fileName },
                        { BodyKeys.Content, content.ToString() }
                    }
                }
            };

            Rest.PostRequest(
                ToS3URL,
                Header(apiKey),
                body.ToString(),
                callBack);
        }

        public static void GetCodeConfig(
            string code,
            string game, 
            string apiKey,
            StatusPanel statusPanel,
            Action<string, int> callback=null)
        {
            Assert.IsNotNull(statusPanel);
            Assert.IsFalse(String.IsNullOrEmpty(code));
            Assert.IsFalse(String.IsNullOrEmpty(game));
            Assert.IsFalse(String.IsNullOrEmpty(apiKey));

            statusPanel.Status = "Requesting code configuration...";

            JsonObject body = new JsonObject
            {
                {
                    "body",
                    new JsonObject
                    {
                        { BodyKeys.Game, game },
                        { BodyKeys.Code, code }
                    }
                }
            };

            Rest.PostRequest(
                CodeURL,
                Header(apiKey),
                body.ToString(),
                (UnityWebRequest uwr, bool valid) => {
                    if (callback != null)
                    {
                        statusPanel.Status = "Downloading server response...";
                        DownloadHandler downloader = uwr.downloadHandler;
                        callback(downloader.text, (int) uwr.responseCode);
                    }
                });
        }

        public static void GetCondition(string path, string apiKey, StatusPanel statusPanel, Action<string, int> callback = null)
        {
            Assert.IsFalse(String.IsNullOrEmpty(apiKey));
            Assert.IsFalse(String.IsNullOrEmpty(path));
            Assert.IsNotNull(statusPanel);

            statusPanel.Status = "Requesting condition...";

            JsonObject body = new JsonObject
            {
                {
                    "body",
                    new JsonObject
                    {
                        { BodyKeys.Condition, path }
                    }
               }
            };

            Rest.PostRequest(
                ConditionURL,
                Header(apiKey),
                body.ToString(),
                (uwr, validJson) =>
                {
                    if (callback != null)
                    {
                        statusPanel.Status = "Downloading server response...";
                        DownloadHandler downloader = uwr.downloadHandler;
                        callback(downloader.text, (int)uwr.responseCode);
                    }
                });
        }

        /// <summary>
        /// Combine two strings to form a path in AWS
        /// </summary>
        /// <param name="str"></param>
        /// <returns>a + "/" + b</returns>
        public static string Combine(string a, string b)
        {
            return $"{a}{PathSeparator}{b}";
        }

        private static bool ContainsBGCExtension(string path)
        {
            return Path.HasExtension(BGCExtension);
        }

        private static bool ContainsJSONExtension(string fileName)
        {
            return Path.HasExtension(JSONExtension);
        }
    }
}