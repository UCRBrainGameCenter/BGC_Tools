using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using BGC.Utility;
using System.Text;
using System;

namespace BGC.Web.Utility
{
    public static class Rest
    {
        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="callBack">true means there was an error</param>
        public static void PostRequest(
            string url,
            Dictionary<string, string> headers, 
            string body, 
            Action<UnityWebRequest> callBack = null)
        {
            CoroutineUtility.Mono.StartCoroutine(RunPost(
                url,
                headers,
                body,
                callBack));
        }

        /// <summary>
        /// Run post request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="body"></param>
        /// <param name="callBack">boolean true means there was an error</param>
        /// <returns></returns>
        private static IEnumerator RunPost(
            string url, 
            Dictionary<string, string> headers, 
            string body,
            Action<UnityWebRequest> callBack)
        {
            UnityWebRequest request = UnityWebRequest.Post(url, "");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));

            foreach (KeyValuePair<string, string> pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }

            yield return request.SendWebRequest();
            callBack?.Invoke(request);
        }
    }
}