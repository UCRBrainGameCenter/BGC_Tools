﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using BGC.Utility;
using BGC.Extensions;

namespace BGC.Web.Utility
{
    public static class Rest
    {
        private static int numActiveGets = 0;
        private static int numActivePosts = 0;
        private static int numActivePuts = 0;

        /// <summary>
        /// Get the number of GET requests which have not yet completed.
        /// </summary>
        /// <returns>The number of GET requests which have not yet completed</returns>
        public static int GetNumActiveGets() => numActiveGets;

        /// <summary>
        /// Get the number of POST requests which have not yet completed.
        /// </summary>
        /// <returns>The number of POST requests which have not yet completed</returns>
        public static int GetNumActivePosts() => numActivePosts;

        /// <summary>
        /// Get the number of PUT requests which have not yet completed.
        /// </summary>
        /// <returns>The number of PUT requests which have not yet completed</returns>
        public static int GetNumActivePuts() => numActivePuts;

        /// <summary>Send a get request</summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        public static void GetRequest(
            string url,
            IDictionary<string, string> headers,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0,
            IDictionary<string, IConvertible> queryParams = default)
        {
            if (queryParams != default)
            {
                url += $"?{string.Join("&", queryParams.Select(WriteQueryParam))}";
            }

            // convert URL to HTTP-friendly URL
            CoroutineUtility.Mono.StartCoroutine(RunGet(
                url: url,
                callBack: callBack,
                timeout: timeout,
                headers: headers));
        }

        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        public static void PostRequest(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0)
        {
            CoroutineUtility.Mono.StartCoroutine(RunPost(
                url,
                headers,
                body,
                callBack,
                timeout));
        }

        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        public static void PutRequest(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0)
        {
            CoroutineUtility.Mono.StartCoroutine(RunPut(
                url,
                headers,
                body,
                callBack,
                timeout));
        }

        /// <summary>
        /// Run get request
        /// </summary>
        private static IEnumerator RunGet(
            string url,
            Action<UnityWebRequest, bool> callBack,
            IDictionary<string, string> headers,
            int timeout = 0)
        {
            try
            {
                numActiveGets++;
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = timeout;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    yield return request.SendWebRequest();

                    callBack?.Invoke(request, true);
                }
            }
            finally
            {
                numActiveGets--;
            }
        }

        /// <summary>
        /// Run post request
        /// </summary>
        private static IEnumerator RunPost(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack,
            int timeout = 0)
        {
            try
            {
                numActivePosts++;
                using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                    request.timeout = timeout;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    yield return request.SendWebRequest();
                    callBack?.Invoke(request, true);
                }
            }
            finally
            {
                numActivePosts--;
            }
        }

        /// <summary>
        /// Run put request
        /// </summary>
        private static IEnumerator RunPut(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack,
            int timeout = 0)
        {
            try
            {
                numActivePuts++;
                using (UnityWebRequest request = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body)))
                {
                    request.timeout = timeout;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    yield return request.SendWebRequest();
                    callBack?.Invoke(request, true);
                }
            }
            finally
            {
                numActivePuts--;
            }
        }

        private static string WriteQueryParam(KeyValuePair<string, IConvertible> param) => $"{param.Key}={param.Value.Encode()}";
    }
}
