using System;
using UnityEngine.Networking;

namespace Plugins.BGC_Tools.Web
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
                case 204:
                    this.DetailedErrorMessage = "";
                    break;
                default:
                    this.DetailedErrorMessage = request.downloadHandler?.text ?? "";
                    break;
            }
        }
    }
}