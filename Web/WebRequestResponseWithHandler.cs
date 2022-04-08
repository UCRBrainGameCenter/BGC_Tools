using System;
using UnityEngine.Networking;

namespace Plugins.BGC_Tools.Web
{
    /// <summary>
    /// Model for unity web request responses that can be used in async methods where disposal of web requests
    /// is an issue. Contains downloaded data. Intended for use with requests that use a standard unity DownloadHandler
    /// </summary>
    public class WebRequestResponseWithHandler
    {
        public long StatusCode { get; }
        public string Error { get; }

        public byte[] DownloadBytes { get; } = Array.Empty<byte>();

        /// <summary>Detailed error message</summary>
        public string DetailedErrorMessage { get; }

        public WebRequestResponseWithHandler(UnityWebRequest request)
        {
            StatusCode = request.responseCode;
            this.Error = string.IsNullOrEmpty(request.error)
                ? ""
                : request.error;

            this.DownloadBytes = request.downloadHandler?.data ?? Array.Empty<byte>();
            
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