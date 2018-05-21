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
            Action<bool> callBack = null)
        {
            CoroutineUtility.Mono.StartCoroutine(runPost(
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
        private static IEnumerator runPost(
            string url, 
            Dictionary<string, string> headers, 
            string body,
            Action<bool> callBack)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            foreach (KeyValuePair<string, string> pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }

            yield return request.Send();
            callBack?.Invoke(request.isNetworkError);
        }
    }
}