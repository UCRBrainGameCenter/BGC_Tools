using System;
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
        /// <summary>Send a get request</summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        public static void GetRequest(
            string url,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0,
            IDictionary<string, IConvertible> queryParams = default,
            IDictionary<string, string> headers = default)
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
        /// Run get request
        /// </summary>
        private static IEnumerator RunGet(
            string url,
            Action<UnityWebRequest, bool> callBack,
            IDictionary<string, string> headers,
            int timeout = 0)
        {
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

        private static string WriteQueryParam(KeyValuePair<string, IConvertible> param) => $"{param.Key}={param.Value.Encode()}";
    }
}
