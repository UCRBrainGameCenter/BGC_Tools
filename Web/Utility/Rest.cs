using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using UnityEngine.Networking;
using BGC.Utility;
using LightJson;

namespace BGC.Web.Utility
{
    public static class Rest
    {
        /// <summary>
        /// Send a get request
        /// </summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        public static void GetRequest(
            string url,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0)
        {
            // convert URL to HTTP-friendly URL
            url = Uri.EscapeUriString(url);
            CoroutineUtility.Mono.StartCoroutine(RunGet(
                url,
                callBack,
                timeout));
        }

        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        public static void PostRequest(
            string url,
            Dictionary<string, string> headers,
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
            int timeout = 0)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = timeout;
            yield return request.SendWebRequest();

            callBack?.Invoke(request, true);
        }

        /// <summary>
        /// Run post request
        /// </summary>
        private static IEnumerator RunPost(
            string url,
            Dictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack,
            int timeout = 0)
        {
            UnityWebRequest request = UnityWebRequest.Post(url, "");
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
}