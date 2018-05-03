using System.Collections.Generic;
using System.Collections;
using System;
using BGC.Utility;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

namespace BGC.Web.Utility
{
    public struct HttpResponse
    {
        public int StatusCode;
        public string Message;

        public HttpResponse(int status, string message)
        {
            StatusCode = status;
            Message    = message;
        }
    }

    public static class Rest
    {
        public static void PostRequest(
            string url, 
            Dictionary<string, string> headers, 
            string body, 
            Func<HttpResponse> callBack = null)
        {
            CoroutineUtility.Mono.StartCoroutine(runPost(
                url,
                headers,
                body,
                callBack));
        }

        private static IEnumerator runPost(
            string url, 
            Dictionary<string, string> headers, 
            string body,
            Func<HttpResponse> callBack)
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            foreach (KeyValuePair<string, string> pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }

            yield return request.Send();

            if (request.isNetworkError)
            {
                Debug.LogError("error");
            }
            else
            {
                Debug.Log("successfully uploaded this file!!!!");
            }
        }
    }
}