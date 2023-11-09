using System;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace BGC.Web
{
    /// <summary>
    /// Model for unity web request responses that can be used in async methods where disposal of web requests
    /// is an issue.
    /// </summary>
    public class WebRequestResponse
    {
        public long StatusCode { get; }
        public string Error { get; }
        public string DetailedErrorMessage { get; }
        public UnityWebRequest.Result Result { get; }
        
        /// <summary>Returns TRUE, if the web request had an error.</summary>
        public bool HasError => !string.IsNullOrEmpty(Error) || !string.IsNullOrEmpty(DetailedErrorMessage);
        
        public WebRequestResponse(UnityWebRequest request)
        {
            StatusCode = request.responseCode;
            this.Error = string.IsNullOrEmpty(request.error)
                ? ""
                : request.error;

            this.DetailedErrorMessage = "";
            this.Result = request.result;

            switch (request.responseCode)
            {
                case 200:
                case 201:
                case 202:
                case 203:
                case 204:
                    this.DetailedErrorMessage = "";
                    break;
                default:
                    if (request.downloadHandler?.GetType() != typeof(DownloadHandlerFile))
                    {
                        this.DetailedErrorMessage = request.downloadHandler?.text ?? "";
                    }
                    else
                    {
                        DownloadHandlerFile downloadHandlerFile = (DownloadHandlerFile)request.downloadHandler;
                        this.DetailedErrorMessage = downloadHandlerFile?.error;
                    }

                    break;
            }
        }
    }
}