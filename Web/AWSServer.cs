using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Assertions;
using BGC.Web.Utility;
using UnityEngine;
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
        }

        public const string PathSeparator = "/";
        public const string ApiUrl = "https://84cje3rj4j.execute-api.us-east-1.amazonaws.com/Production/to-s3";
        public const string ApiKey = "3Hz6KpZXBb2aFY57wCo137fm9OWmmyOo9D7TJCsx";
        public const string CacheControl = "no-cache";
        public const string BGCExtension = ".bgc";
        public const string JSONExtension = ".json";

        private const char extensionSplitter = '.';

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
            Action<UnityWebRequest> callBack = null)
        {
            Assert.IsFalse(String.IsNullOrEmpty(fileName));
            Assert.IsFalse(String.IsNullOrEmpty(organization));
            Assert.IsFalse(String.IsNullOrEmpty(study));
            Assert.IsFalse(String.IsNullOrEmpty(game));
            Assert.IsTrue(ContainsJSONExtension(fileName));

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                { HeaderKeys.ApiKey, ApiKey },
                { "Content-Type", "application/x-www-form-urlencoded" }
            };

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
                ApiUrl,
                headers,
                body.ToString(),
                callBack);
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