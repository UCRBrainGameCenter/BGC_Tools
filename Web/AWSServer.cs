﻿using System.Collections.Generic;
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

        // @note: this is currently not used, but will be used in the future
        ///// <summary>
        ///// Post a file to s3.
        ///// </summary>
        ///// <param name="filePath"></param>
        ///// <param name="bucket"></param>
        ///// <param name="serverPath"></param>
        ///// <param name="callBack"></param>
        //public static void PostFileToAWS(
        //    string filePath,
        //    string organization,
        //    string study,
        //    string game,
        //    Action<UnityWebRequest> callBack = null)
        //{
        //    Assert.IsFalse(String.IsNullOrEmpty(filePath));
        //    Assert.IsFalse(String.IsNullOrEmpty(organization));
        //    Assert.IsFalse(String.IsNullOrEmpty(study));
        //    Assert.IsFalse(String.IsNullOrEmpty(game));
        //    Assert.IsTrue(File.Exists(filePath));

        //    StreamReader reader = new StreamReader(filePath);
        //    string content = reader.ReadToEnd();
        //    reader.Close();

        //    PostToAWS(
        //        Path.GetFileName(filePath),
        //        organization,
        //        study,
        //        game,
        //        content,
        //        callBack);
        //}

        public static void PostBGCToJSonToAWS(
            string filePath,
            string organization,
            string study,
            string game,
            string apiKey,
            Action<UnityWebRequest> callBack = null)
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
            Action<UnityWebRequest> callBack = null)
        {
            Assert.IsFalse(String.IsNullOrEmpty(fileName));
            Assert.IsFalse(String.IsNullOrEmpty(organization));
            Assert.IsFalse(String.IsNullOrEmpty(study));
            Assert.IsFalse(String.IsNullOrEmpty(game));
            Assert.IsTrue(ContainsJSONExtension(fileName));
            
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
                (UnityWebRequest uwr) => {
                    if (callback != null)
                    {
                        DownloadHandler downloader = uwr.downloadHandler;
                        statusPanel.Status = "Downloading server response...";
                        callback(downloader.text, (int) uwr.responseCode);
                    }
                });
        }

        public static void GetCondition(string path, string apiKey, StatusPanel statusPanel, Action<string, int> callback=null)
        {
            statusPanel.Status = "Requesting condition...";
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