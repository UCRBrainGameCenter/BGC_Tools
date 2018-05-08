using System.Collections.Generic;
using BGC.Web.Utility;
using BGC.Extensions;
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
            public const string ApiKey       = "x-api-key";
            public const string CacheControl = "Cache-Control";
        }

        public static class BodyKeys
        {
            public const string Bucket  = "bucket";
            public const string Path    = "key";
            public const string Content = "content";
        }

        public const string ApiUrl = "https://kukp0cevff.execute-api.us-east-2.amazonaws.com/prod/post_file_to_s3";
        public const string ApiKey = "eQtoU42BkJ697U9t74dQH73fjxdfcy7p70Rvs1Ft";
        public const string CacheControl = "no-cache";
        public const string BGCExtension = ".bgc";

        private const char extensionSplitter = '.';

        public static void PostFileToAWS(
            string filePath,
            string bucket,
            string serverPath,
            Action<bool> callBack = null)
        {
            if (File.Exists(filePath) == false)
            {
                Debug.LogError(filePath + " is not a file.");

                if (callBack != null)
                {
                    callBack(true);
                }

                return;
            }

            if (containsBGCExtension(filePath) == false)
            {
                Debug.LogError("file " + filePath + " must have the bgc extension.");

                if (callBack != null)
                {
                    callBack(true);
                }

                return;
            }

            StreamReader reader = new StreamReader(filePath);
            string content = reader.ReadToEnd();
            reader.Close();

            PostToAWS(
                content,
                bucket,
                serverPath,
                callBack);
        }

        /// <summary>
        /// Post a string to Aws
        /// </summary>
        /// <param name="url"></param>
        /// <param name="bucket"></param>
        /// <param name="serverPath"></param>
        /// <param name="fileContents"></param>
        /// <param name="callBack">True means there was an error</param>
        public static void PostToAWS(
            string fileContents,
            string bucket,
            string serverPath,
            Action<bool> callBack = null)
        {
            if (containsBGCExtension(serverPath) == false)
            {
                Debug.LogError("server path " + serverPath + " must have a bgc extension \".bgc\"");

                if (callBack != null)
                {
                    callBack(true);
                }

                return;
            }

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add(HeaderKeys.ApiKey, ApiKey);
            headers.Add(HeaderKeys.CacheControl, CacheControl);

            JsonObject body = new JsonObject();
            body.Add(BodyKeys.Bucket, bucket);
            body.Add(BodyKeys.Path, serverPath);
            body.Add(BodyKeys.Content, fileContents);

            Rest.PostRequest(
                ApiUrl,
                headers,
                body.ToString(),
                callBack);
        }

        private static bool containsBGCExtension(string path)
        {
            return Path.HasExtension(BGCExtension);
        }
    }
}