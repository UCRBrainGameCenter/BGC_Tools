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

        /// <summary>Detailed error message</summary>
        public string DetailedErrorMessage { get; }

        public WebRequestResponse(UnityWebRequest request)
        {
            StatusCode = request.responseCode;
            this.Error = string.IsNullOrEmpty(request.error)
                ? ""
                : request.error;

            this.DetailedErrorMessage = "";

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