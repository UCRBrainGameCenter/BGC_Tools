using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;
using BGC.Utility;
using BGC.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace BGC.Web.Utility
{
    public static class Rest
    {
        /// <summary>
        /// The amount of time, in milliseconds, that async GET requests poll the operation to check for progress.
        /// </summary>
        // private const int AsyncGetPollingTimeMs = 200;

        /// <summary>Send a get request.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        public static void GetRequest(
            string url,
            IDictionary<string, string> headers,
            Action<WebRequestResponseWithHandler> callBack = null,
            int timeoutInSeconds = 0,
            IDictionary<string, IConvertible> queryParams = default)
        {
            if (queryParams != default)
            {
                url += $"?{string.Join("&", queryParams.Select(WriteQueryParam))}";
            }

            // convert URL to HTTP-friendly URL
            CoroutineUtility.Mono.StartCoroutine(RunGet(
                url: url,
                callBack: callBack,
                timeoutInSeconds: timeoutInSeconds,
                headers: headers));
        }

        /// <summary>Send a get request with a body.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        public static void GetRequestWithBody(
            string url,
            string content,
            IDictionary<string, string> headers,
            Action<WebRequestResponseWithHandler> callBack = null,
            int timeoutInSeconds = 0,
            IDictionary<string, IConvertible> queryParams = default)
        {
            if (queryParams != default)
            {
                url += $"?{string.Join("&", queryParams.Select(WriteQueryParam))}";
            }

            // convert URL to HTTP-friendly URL
            CoroutineUtility.Mono.StartCoroutine(RunGetWithBody(
                url: url,
                content,
                callBack: callBack,
                timeoutInSeconds: timeoutInSeconds,
                headers: headers));
        }

        /// <summary>Send an async get request.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        /// <param name="progressReporter">Progress reporter for the request.</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        /// <param name="abortToken">Cancellation token to use for the request.</param>
        public static async Task<WebRequestResponseWithHandler> GetRequestAsync(
            string url,
            IDictionary<string, string> headers,
            int retries = 0,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            IDictionary<string, IConvertible> queryParams = default,
            CancellationToken abortToken = default)
        {
            if (queryParams != default)
            {
                url += $"?{string.Join("&", queryParams.Select(WriteQueryParam))}";
            }

            if (abortToken.IsCancellationRequested)
            {
                return null;
            }

            return await RunGetAsync(
                url,
                headers,
                retries,
                timeoutInSeconds,
                progressReporter,
                abortToken);
        }

        /// <summary>Send an async get request with a body.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        /// <param name="progressReporter">Progress reporter for the request.</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        /// <param name="abortToken">Cancellation token to use for the request.</param>
        public static async Task<WebRequestResponseWithHandler> GetRequestWithBodyAsync(
            string url,
            string content,
            IDictionary<string, string> headers,
            int retries = 0,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            IDictionary<string, IConvertible> queryParams = default,
            CancellationToken abortToken = default)
        {
            if (queryParams != default)
            {
                url += $"?{string.Join("&", queryParams.Select(WriteQueryParam))}";
            }

            if (abortToken.IsCancellationRequested)
            {
                return null;
            }

            return await RunGetWithBodyAsync(
                url,
                content,
                headers,
                retries,
                timeoutInSeconds,
                progressReporter,
                abortToken);
        }

        /// <summary>Send a get request using a file handler.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        /// <param name="callBack">Code to execute upon completion of request. Sends back a response object.</param>
        /// <param name="progressReporter">Progress reporter for the request.</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        /// <param name="absoluteFilePath">
        /// The absolute path to the file the data will be downloaded to. Must include filename and extension.
        /// </param>
        public static void GetRequest(
            string url,
            IDictionary<string, string> headers,
            string absoluteFilePath,
            IProgress<float> progressReporter = null,
            Action<WebRequestResponseWithHandler> callBack = null,
            int retries = 0,
            int timeoutInSeconds = 0,
            IDictionary<string, IConvertible> queryParams = default)
        {
            if (queryParams != default)
            {
                url += $"?{string.Join("&", queryParams.Select(WriteQueryParam))}";
            }

            // convert URL to HTTP-friendly URL
            CoroutineUtility.Mono.StartCoroutine(RunGetWithFileDownload(
                url: url,
                callBack: callBack,
                progressReporter: progressReporter,
                timeoutInSeconds: timeoutInSeconds,
                retries: retries,
                headers: headers,
                absoluteFilePath: absoluteFilePath));
        }

        /// <summary>Send an async GET request using a file handler.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        /// <param name="progressReporter">Progress reporter for the request.</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        /// <param name="absoluteFilePath">
        /// The absolute path to the file the data will be downloaded to. Must include filename and extension.
        /// </param>
        /// <param name="abortToken">Cancellation token for the request.</param>
        public static async Task<WebRequestResponse> GetRequestAsync(
            string url,
            IDictionary<string, string> headers,
            string absoluteFilePath,
            int retries = 0,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            IDictionary<string, IConvertible> queryParams = default,
            CancellationToken abortToken = default)
        {
            if (queryParams != default)
            {
                url += $"?{string.Join("&", queryParams.Select(WriteQueryParam))}";
            }

            if (abortToken.IsCancellationRequested)
            {
                abortToken.ThrowIfCancellationRequested();
                return null;
            }

            WebRequestResponse resp = await RunGetAsyncWithFileDownload(
                url,
                headers,
                absoluteFilePath,
                retries,
                timeoutInSeconds,
                progressReporter,
                abortToken);

            return resp;
        }

        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="body">Stringified body to send in the request.</param>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        public static void PostRequest(
            string url,
            IDictionary<string, string> headers,
            string body,
            string contentType,
            Action<WebRequestResponseWithHandler> callBack = null,
            int timeoutInSeconds = 0)
        {
            CoroutineUtility.Mono.StartCoroutine(RunPost(
                url,
                headers,
                body,
                contentType,
                callBack,
                timeoutInSeconds));
        }

        /// <summary>Send an async POST request</summary>
        [ItemCanBeNull]
        public static async Task<WebRequestResponseWithHandler> PostRequestAsync(
            string url,
            IDictionary<string, string> headers,
            string body,
            string contentType,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            int maxRetries = 0,
            CancellationToken abortToken = default)
        {
            return await RunPostAsync(
                url,
                headers,
                body,
                contentType,
                timeoutInSeconds,
                maxRetries,
                progressReporter,
                abortToken);
        }

        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="body">Stringified body to send in the request.</param>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        public static void PutRequest(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<WebRequestResponse> callBack = null,
            int timeoutInSeconds = 0)
        {
            CoroutineUtility.Mono.StartCoroutine(RunPut(
                url,
                headers,
                body,
                callBack,
                timeoutInSeconds));
        }

        /// <summary>Send an async PUT request</summary>
        [ItemCanBeNull]
        public static async Task<WebRequestResponse> PutRequestAsync(
            string url,
            IDictionary<string, string> headers,
            string body,
            int timeoutInSeconds = 0,
            CancellationToken abortToken = default)
        {
            return await RunPutAsync(
                url,
                headers,
                body,
                timeoutInSeconds,
                abortToken);
        }

        /// <summary>
        /// Run get request
        /// </summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="callBack">Code to execution when request completes. Sends back a response object.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        private static IEnumerator RunGet(
            string url,
            Action<WebRequestResponseWithHandler> callBack,
            IDictionary<string, string> headers,
            int timeoutInSeconds = 0)
        {
            yield return RestRequestThrottler.TrySubmitRequestCoroutine(HttpMethod.Get);

            using UnityWebRequest request = CreateGetRequest(
                url,
                timeoutInSeconds,
                headers: headers);

            yield return request.SendWebRequest();

            WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
            RestRequestThrottler.EndRequest(HttpMethod.Get);
            callBack?.Invoke(resp);
        }

        /// <summary>
        /// Run get request with body
        /// </summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="callBack">Code to execution when request completes. Sends back a response object.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        private static IEnumerator RunGetWithBody(
            string url,
            string content,
            Action<WebRequestResponseWithHandler> callBack,
            IDictionary<string, string> headers,
            int timeoutInSeconds = 0)
        {
            yield return RestRequestThrottler.TrySubmitRequestCoroutine(HttpMethod.Get);

            using UnityWebRequest request = CreateGetRequest(
                url,
                timeoutInSeconds,
                "",
                content,
                headers);

            yield return request.SendWebRequest();

            WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
            request.Dispose();
            RestRequestThrottler.EndRequest(HttpMethod.Get);
            callBack?.Invoke(resp);
        }

        /// <summary>Run async GET request using async/await C# pattern.</summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">Optional timeout for the request. Default is 15 seconds.</param>
        /// <param name="progressReporter">
        /// Optional progress reporter for the request. Takes the progress of the web request as a float.
        /// </param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>The finished unity web request. Can be NULL if operation cancelled or error occurs.</returns>
        private static async Task<WebRequestResponseWithHandler> RunGetAsync(
            string url,
            IDictionary<string, string> headers,
            int retries = 0,
            int timeoutInSeconds = 15,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
        {
            await WaitForAvailableRequestSlot(HttpMethod.Get, abortToken);
            
            try
            {
                using UnityWebRequest request = CreateGetRequest(
                    url,
                    timeoutInSeconds,
                    headers: headers);
               
                return await ExecuteWebRequestWithHandler(
                    request,
                    abortToken,
                    retries,
                    progressReporter);
                
                // int numRetries = 0;
                // bool shouldRetry = true;
                //
                // while (shouldRetry)
                // {
                //     shouldRetry = false;
                //
                //     UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                //
                //     while (!operation.isDone)
                //     {
                //         if (abortToken.IsCancellationRequested)
                //         {
                //             request.Abort();
                //             return new WebRequestResponseWithHandler(request);
                //         }
                //
                //         progressReporter?.Report(operation.progress);
                //         await Task.Yield();
                //         // await Task.Delay(AsyncGetPollingTimeMs, abortToken);
                //     }
                //
                //     if (!string.IsNullOrEmpty(request.error))
                //     {
                //         if (IsTransientError(request.responseCode) && numRetries < retries)
                //         {
                //             // retry if transient error and retry limit not reached.
                //             shouldRetry = true;
                //             numRetries++;
                //
                //             if (numRetries > retries)
                //             {
                //                 throw new WebException($"Unable to download {url}. Retries exceeded.");
                //             }
                //
                //             await BackoffHelper.WaitWithBackoffAsync(numRetries, abortToken);
                //         }
                //         else
                //         {
                //             if (numRetries > retries)
                //             {
                //                 throw new WebException($"Unable to download {url}. Retries exceeded.");
                //             }
                //             
                //             // Non-transient error or max retries reached.
                //             return new WebRequestResponseWithHandler(request);
                //         }
                //     }
                //     else
                //     {
                //         progressReporter?.Report(operation.progress);
                //     }
                // }
                //
                // return new WebRequestResponseWithHandler(request);
            }
            finally
            {
                RestRequestThrottler.EndRequest(HttpMethod.Get);
            }
        }

        /// <summary>Run async GET request with body using async/await C# pattern.</summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">Optional timeout for the request. Default is 15 seconds.</param>
        /// <param name="progressReporter">
        /// Optional progress reporter for the request. Takes the progress of the web request as a float.
        /// </param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>The finished unity web request. Can be NULL if operation cancelled or error occurs.</returns>
        [ItemCanBeNull]
        private static async Task<WebRequestResponseWithHandler> RunGetWithBodyAsync(
            string url,
            string content,
            IDictionary<string, string> headers,
            int retries = 0,
            int timeoutInSeconds = 15,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
        {
            await WaitForAvailableRequestSlot(HttpMethod.Get, abortToken);
            
            try
            {
                using UnityWebRequest request = CreateGetRequest(
                    url,
                    timeoutInSeconds,
                    null,
                    content,
                    headers);

                return await ExecuteWebRequestWithHandler(
                    request,
                    abortToken,
                    retries,
                    progressReporter);
                
                // int numRetries = 0;
                // bool shouldRetry = true;
                //
                // while (shouldRetry)
                // {
                //     shouldRetry = false;
                //
                //     UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                //
                //     while (!operation.isDone)
                //     {
                //         if (abortToken.IsCancellationRequested)
                //         {
                //             request.Abort();
                //             return new WebRequestResponseWithHandler(request);
                //         }
                //
                //         progressReporter?.Report(operation.progress);
                //         await Task.Yield();
                //     }
                //
                //     if (IsTransientError(request.responseCode) && numRetries < retries)
                //     {
                //         // retry if transient error and retry limit not reached.
                //         shouldRetry = true;
                //         numRetries++;
                //
                //         if (numRetries > retries)
                //         {
                //             throw new WebException($"Unable to download {url}. Retries exceeded.");
                //         }
                //
                //         await BackoffHelper.WaitWithBackoffAsync(numRetries, abortToken);
                //     }
                //     else
                //     {
                //         if (numRetries > retries)
                //         {
                //             throw new WebException($"Unable to download {url}. Retries exceeded.");
                //         }
                //             
                //         // Non-transient error or max retries reached.
                //         return new WebRequestResponseWithHandler(request);
                //     }
                // }
                //
                // return new WebRequestResponseWithHandler(request);
            }
            finally
            {
                RestRequestThrottler.EndRequest(HttpMethod.Get);
            }
        }

        /// <summary>Run get request using a file handler.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="callBack">Code to execute when request completes. Sends back response object.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="absoluteFilePath">
        /// The absolute path to the file the data will be downloaded to. Must include filename and extension.
        /// </param>
        /// <param name="progressReporter">Progress reporter for the download.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        private static IEnumerator RunGetWithFileDownload(
            string url,
            Action<WebRequestResponseWithHandler> callBack,
            IDictionary<string, string> headers,
            string absoluteFilePath,
            IProgress<float> progressReporter = null,
            int retries = 0,
            int timeoutInSeconds = 0)
        {
            yield return RestRequestThrottler.TrySubmitRequestCoroutine(HttpMethod.Get);
            
            bool shouldRetry = true;
            int numRetries = 0;

            while (shouldRetry)
            {
                shouldRetry = false;

                using UnityWebRequest request = CreateGetRequest(
                    url,
                    timeoutInSeconds,
                    absoluteFilePath,
                    null,
                    headers);
                
                AsyncOperation op = request.SendWebRequest();

                while (!op.isDone)
                {
                    yield return new WaitForSeconds(0.001f);
                    progressReporter?.Report(op.progress);
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    // check for cases where it's useless to retry, such as 404
                    if (request.responseCode != 404 && numRetries < retries)
                    {
                        // retry
                        shouldRetry = true;
                        numRetries++;

                        if (numRetries < retries)
                        {
                            float timeToWait =
                                (float) BackoffHelper.CalculateBackoffInterval(numRetries).TotalSeconds;
                            yield return new WaitForSeconds(timeToWait);
                        }
                    }
                    else
                    {
                        long statusCode = request.responseCode != 404 ? 504 : 404; // send timeout code if not 404
                        string err = request.responseCode != 404
                            ? "The download timed out and retries have been exceeded."
                            : request.error;

                        WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(
                            statusCode: statusCode,
                            error: err,
                            downloadBytes: Array.Empty<byte>(),
                            uploadBytes: Array.Empty<byte>());

                        RestRequestThrottler.EndRequest(HttpMethod.Get);
                        callBack?.Invoke(resp);
                    }
                }
                else
                {
                    progressReporter?.Report(op.progress);

                    WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
                    RestRequestThrottler.EndRequest(HttpMethod.Get);
                    callBack?.Invoke(resp);
                }
            }
        }

        /// <summary>Run async GET request using a file handler.</summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="absoluteFilePath">
        /// The absolute path to the file the data will be downloaded to. Must include filename and extension.
        /// </param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">Optional timeout for the request. Default is 15 seconds.</param>
        /// <param name="progressReporter">
        /// Optional progress reporter for the request. Takes the progress of the web request as a float.
        /// </param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>The finished unity web request. Can be NULL if operation cancelled or error occurs.</returns>
        private static async Task<WebRequestResponse> RunGetAsyncWithFileDownload(
            string url,
            IDictionary<string, string> headers,
            string absoluteFilePath,
            int retries = 0,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
        {
            await WaitForAvailableRequestSlot(HttpMethod.Get, abortToken);
            
            try
            {
                using UnityWebRequest request = CreateGetRequest(
                    url,
                    timeoutInSeconds,
                    absoluteFilePath,
                    null,
                    headers);
                
                return await ExecuteWebRequest(
                    request,
                    abortToken,
                    retries,
                    progressReporter);
            }
            finally
            {
                RestRequestThrottler.EndRequest(HttpMethod.Get);
            }
        }

        /// <summary>
        /// Run post request
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="body">Stringified body to send in the request.</param>
        /// <param name="callBack">Code to execute when request finishes. Sends back response object.</param>
        /// <param name="timeoutInSeconds">
        /// Optional timeout for the request. If set to 0, then there is no timeout behavior.
        /// </param>
        private static IEnumerator RunPost(
            string url,
            IDictionary<string, string> headers,
            string body,
            string contentType,
            Action<WebRequestResponseWithHandler> callBack,
            int timeoutInSeconds = 0)
        {
            yield return RestRequestThrottler.TrySubmitRequestCoroutine(HttpMethod.Post);
            
            using UnityWebRequest request = CreatePostRequest(url, headers, body, contentType, timeoutInSeconds);
            yield return request.SendWebRequest();
            var resp = new WebRequestResponseWithHandler(request);
            
            RestRequestThrottler.EndRequest(HttpMethod.Post);
            callBack?.Invoke(resp);
        }

        /// <summary>Run async POST request</summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="body">Stringified body to send in the request.</param>
        /// <param name="timeoutInSeconds">
        /// Optional timeout for the request. If set to 0, then there is no timeout behavior.
        /// </param>
        /// <param name="progressReporter">Progress reporter for the download.</param>
        /// <param name="abortToken">Cancellation token for the request.</param>
        private static async Task<WebRequestResponseWithHandler> RunPostAsync(
            string url,
            IDictionary<string, string> headers,
            string body,
            string contentType,
            int timeoutInSeconds = 0,
            int maxRetries = 0,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
        {
            await WaitForAvailableRequestSlot(HttpMethod.Post, abortToken);

            try
            {
                using UnityWebRequest request = CreatePostRequest(url, headers, body, contentType, timeoutInSeconds);

                return await ExecuteWebRequestWithHandler(request, abortToken, maxRetries, progressReporter);
            }
            finally
            {
                RestRequestThrottler.EndRequest(HttpMethod.Post);
            }
        }

        /// <summary>
        /// Run put request
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="body">Stringified body to send in the request.</param>
        /// <param name="callBack">Code to execute when request finishes. Sends back response object.</param>
        /// <param name="timeoutInSeconds">
        /// Optional timeout for the request. If set to 0, then there is no timeout behavior.
        /// </param>
        private static IEnumerator RunPut(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<WebRequestResponse> callBack,
            int timeoutInSeconds = 0)
        {
            yield return RestRequestThrottler.TrySubmitRequestCoroutine(HttpMethod.Put);
            using UnityWebRequest request = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body));

            request.timeout = timeoutInSeconds;

            foreach (KeyValuePair<string, string> pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }

            yield return request.SendWebRequest();

            WebRequestResponse resp = new WebRequestResponse(request);
            RestRequestThrottler.EndRequest(HttpMethod.Put);
            callBack?.Invoke(resp);
        }

        /// <summary>Run async POST request</summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="body">Stringified body to send in the request.</param>
        /// <param name="timeoutInSeconds">
        /// Optional timeout for the request. If set to 0, then there is no timeout behavior.
        /// </param>
        /// <param name="abortToken">Cancellation token for the request.</param>
        [ItemCanBeNull]
        private static async Task<WebRequestResponse> RunPutAsync(
            string url,
            IDictionary<string, string> headers,
            string body,
            int timeoutInSeconds = 0,
            CancellationToken abortToken = default)
        {
            await WaitForAvailableRequestSlot(HttpMethod.Put, abortToken);

            try
            {
                using UnityWebRequest request = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body));
                request.timeout = timeoutInSeconds;

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }
                }

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                var tcs = new TaskCompletionSource<bool>();
                operation.completed += _ => tcs.SetResult(true);

                // Perhaps a bit paranoid, but we should not be calling Register on an already-canceled token
                if (abortToken.IsCancellationRequested)
                {
                    request.Abort();
                    return new WebRequestResponse(request);
                }

                // Register a callback if the abort token is canceled which will cancel the request itself.
                CancellationTokenRegistration cancelRegistration = abortToken.Register(() =>
                {
                    request.Abort();
                    tcs.SetCanceled();
                });

                // Wait for the task to either complete or cancel
                await tcs.Task;

                // Unregister the callback as we no longer should be aborting the request if the token is canceled.
                await cancelRegistration.DisposeAsync();

                if (tcs.Task.IsCanceled)
                {
                    return new WebRequestResponse(request);
                }

                return new WebRequestResponse(request);
            }
            finally
            {
                RestRequestThrottler.EndRequest(HttpMethod.Put);
            }
        }

        #region Helpers

        /// <summary>
        /// Waits for an active slot for the requested HTTP method. This helps prevent port exhaustion for
        /// lots of concurrent requests.
        /// </summary>
        private static async Task WaitForAvailableRequestSlot(
            HttpMethod method,
            CancellationToken abortToken)
        {
            // Wait until a slot is available
            while (!await RestRequestThrottler.TrySubmitRequestAsync(method, abortToken))
            {
                // If no slot is available, wait for a bit before retrying
                await Task.Delay(10, abortToken);
            }
        }
        
        /// <summary>Helper method to determine if the error code is transient (i.e., a temporary error).</summary>
        /// <remarks>These errors are ones that should be retried.</remarks> 
        private static bool IsTransientError(long responseCode)
        {
            // List of transient HTTP response codes.
            var transientErrors = new HashSet<long>
            {
                // Add other status codes as deemed appropriate.
                408, // Request Timeout
                429, // Too Many Requests
                500, // Internal Server Error
                502, // Bad Gateway
                503, // Service Unavailable
                504, // Gateway Timeout
            };

            return transientErrors.Contains(responseCode);
        }
        
        private static string WriteQueryParam(KeyValuePair<string, IConvertible> param) =>
            $"{param.Key}={param.Value.Encode()}";
        
        private static UnityWebRequest CreatePostRequest(
            string url,
            IDictionary<string, string> headers,
            string body,
            string contentType,
            int timeoutInSeconds)
        {
#if UNITY_2022_2_OR_NEWER
            UnityWebRequest request = UnityWebRequest.Post(url, "", contentType);
#else
            UnityWebRequest request = UnityWebRequest.Post(url, "");
            request.SetRequestHeader("Content-Type", contentType);
#endif
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.disposeUploadHandlerOnDispose = true;
            request.timeout = timeoutInSeconds;

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }
            }
            
            return request;
        }

        /// <summary> Creates a new GET request object. </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="timeoutInSeconds">Amount of seconds before timeout occurs. 0 for no timeout.</param>
        /// <param name="absoluteFilePath">
        /// The absolute file path to the file on disc that the download handler will write to. If this is
        /// not specified, then no download handler is created. NOTE: Download handler, if attached, is disposed
        /// when the web request is disposed.
        /// </param>
        /// <param name="bodyContent">
        /// The content of the request body to send, if any. If not specified, no upload handler is attached. NOTE:
        /// the upload handler is disposed when the request is disposed.
        /// </param>
        /// <param name="headers"></param>
        /// <returns></returns>
        private static UnityWebRequest CreateGetRequest(
            string url,
            int timeoutInSeconds = 0,
            string absoluteFilePath = null,
            string bodyContent = null,
            IDictionary<string, string> headers = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = timeoutInSeconds;

            if (!string.IsNullOrEmpty(absoluteFilePath))
            {
                DownloadHandlerFile downloadHandler = new DownloadHandlerFile(absoluteFilePath);
                downloadHandler.removeFileOnAbort = true;
                request.downloadHandler = downloadHandler; // use file handler
                request.disposeDownloadHandlerOnDispose = true;
            }

            if (!string.IsNullOrEmpty(bodyContent))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyContent));
                request.disposeUploadHandlerOnDispose = true;
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }
            }

            return request;
        }

        /// <summary> Executes a web request with retries and backoff. </summary>
        /// <param name="request">The request to send</param>
        /// <param name="abortToken" />
        /// <param name="maxRetries">Max number of retries</param>
        /// <param name="progressReporter" />
        private static async Task<WebRequestResponseWithHandler> ExecuteWebRequestWithHandler(
            UnityWebRequest request,
            CancellationToken abortToken,
            int maxRetries = 0,
            IProgress<float> progressReporter = null)
        {
            int numRetries = 0;

            while (true)
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    if (abortToken.IsCancellationRequested)
                    {
                        request.Abort();
                        abortToken.ThrowIfCancellationRequested();
                    }

                    await Task.Yield();
                    progressReporter?.Report(operation.progress);
                }

                WebRequestResponseWithHandler response = new WebRequestResponseWithHandler(request);
                
                if (!response.HasError || numRetries >= maxRetries)
                    return response;

                if (response.StatusCode == 404 || !IsTransientError(response.StatusCode))
                    return response;

                numRetries++;
                if (abortToken.IsCancellationRequested)
                {
                    abortToken.ThrowIfCancellationRequested();
                }

                await BackoffHelper.WaitWithBackoffAsync(numRetries, abortToken);
            }
        }
        
        /// <summary> Executes a web request with retries and backoff. </summary>
        /// <param name="request">The request to send</param>
        /// <param name="abortToken" />
        /// <param name="maxRetries">Max number of retries</param>
        /// <param name="progressReporter" />
        private static async Task<WebRequestResponse> ExecuteWebRequest(
            UnityWebRequest request,
            CancellationToken abortToken,
            int maxRetries = 0,
            IProgress<float> progressReporter = null)
        {
            int numRetries = 0;
            
            while (true)
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    if (abortToken.IsCancellationRequested)
                    {
                        request.Abort();
                        abortToken.ThrowIfCancellationRequested();
                    }
                    
                    await Task.Yield();
                    progressReporter?.Report(operation.progress);
                }

                WebRequestResponse response = new WebRequestResponse(request);
                
                if (!response.HasError || numRetries >= maxRetries)
                    return response;

                if (response.StatusCode == 404 || !IsTransientError(response.StatusCode))
                    return response;

                numRetries++;
                if (abortToken.IsCancellationRequested)
                {
                    abortToken.ThrowIfCancellationRequested();
                }

                await BackoffHelper.WaitWithBackoffAsync(numRetries, abortToken);
            }
        }
        
        #endregion
       
    }
}