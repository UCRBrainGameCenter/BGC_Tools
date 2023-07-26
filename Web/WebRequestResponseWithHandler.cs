using System;
using System.IO;
using LightJson;
using LightJson.Serialization;
using UnityEngine.Networking;

namespace BGC.Web
{
    /// <summary>
    /// Model for unity web request responses that can be used in async methods where disposal of web requests
    /// is an issue. Contains downloaded data. Intended for use with requests that use a standard unity DownloadHandler
    /// </summary>
    public class WebRequestResponseWithHandler
    {
        public long StatusCode { get; }
        public string Error { get; }

        public byte[] DownloadBytes { get; }
        public byte[] UploadBytes { get; }

        public string DownloadAsString
        {
            get
            {
                using MemoryStream ms = new MemoryStream(DownloadBytes);
                using StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }

        public JsonValue DownloadAsJsonValue
        {
            get
            {
                using MemoryStream ms = new MemoryStream(DownloadBytes);
                using StreamReader sr = new StreamReader(ms);
                return JsonReader.Parse(sr);
            }
        }

        public string UploadAsString
        {
            get
            {
                using MemoryStream ms = new MemoryStream(UploadBytes);
                using StreamReader sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }
        }

        public JsonValue UploadAsJsonValue
        {
            get
            {
                using MemoryStream ms = new MemoryStream(UploadBytes);
                using StreamReader sr = new StreamReader(ms);
                return JsonReader.Parse(sr);
            }
        }

        /// <summary>Detailed error message</summary>
        public string DetailedErrorMessage { get; }

        public UnityWebRequest.Result Result { get; }

        /// <summary>Returns TRUE, if the web request had an error.</summary>
        public bool HasError => !string.IsNullOrEmpty(Error) || !string.IsNullOrEmpty(DetailedErrorMessage);

        public WebRequestResponseWithHandler(UnityWebRequest request)
        {
            StatusCode = request.responseCode;
            this.Error = string.IsNullOrEmpty(request.error)
                ? ""
                : request.error;

            this.DownloadBytes = request.downloadHandler?.data ?? Array.Empty<byte>();
            this.UploadBytes = request.uploadHandler?.data ?? Array.Empty<byte>();
            this.Result = request.result;
            
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

        public WebRequestResponseWithHandler(
            long statusCode,
            string error,
            byte[] downloadBytes,
            byte[] uploadBytes)
        {
            this.StatusCode = statusCode;
            this.Error = error;
            this.DownloadBytes = downloadBytes;
            this.UploadBytes = uploadBytes;
            this.Result = UnityWebRequest.Result.ConnectionError;
        }

        /// <summary>Constructor that can be used when you don't have a unity web request to send back.</summary>
        public WebRequestResponseWithHandler(
            long statusCode,
            string error,
            UnityWebRequest.Result result)
        {
            this.StatusCode = statusCode;
            this.Error = error;
            this.DownloadBytes = Array.Empty<byte>();
            this.UploadBytes = Array.Empty<byte>();
            this.Result = result;
        }
    }
}