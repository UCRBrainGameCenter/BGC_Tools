using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private static int numActiveGets;
        private static int numActivePosts;
        private static int numActivePuts;
        private const int MaxNumActiveGets = 20;

        /// <summary>
        /// The amount of time, in milliseconds, that async GET requests poll the operation to check for progress.
        /// </summary>
        // private const int AsyncGetPollingTimeMs = 200;

        /// <summary>
        /// Get the number of GET requests which have not yet completed.
        /// </summary>
        /// <returns>The number of GET requests which have not yet completed</returns>
        public static int GetNumActiveGets() => numActiveGets;

        /// <summary>
        /// Get the number of POST requests which have not yet completed.
        /// </summary>
        /// <returns>The number of POST requests which have not yet completed</returns>
        public static int GetNumActivePosts() => numActivePosts;

        /// <summary>
        /// Get the number of PUT requests which have not yet completed.
        /// </summary>
        /// <returns>The number of PUT requests which have not yet completed</returns>
        public static int GetNumActivePuts() => numActivePuts;

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
            CancellationToken abortToken = default)
        {
            return await RunPostAsync(
                url,
                headers,
                body,
                contentType,
                timeoutInSeconds,
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
            while (numActiveGets >= MaxNumActiveGets)
            {
                // wait for other requests to wrap up to avoid port exhaustion.
                yield return null;
            }

            Interlocked.Increment(ref numActiveGets);
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = timeoutInSeconds;

            foreach (KeyValuePair<string, string> pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }

            yield return request.SendWebRequest();

            WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
            request.Dispose();
            Interlocked.Decrement(ref numActiveGets);
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
            try
            {
                while (numActiveGets >= MaxNumActiveGets)
                {
                    // wait for other requests to wrap up to avoid port exhaustion.
                    yield return null;
                }

                Interlocked.Increment(ref numActiveGets);
                using UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(content));

                yield return request.SendWebRequest();

                WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
                request.Dispose();
                callBack?.Invoke(resp);
            }
            finally
            {
                Interlocked.Decrement(ref numActiveGets);
            }
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
            try
            {
                if (abortToken.IsCancellationRequested)
                {
                    return null;
                }

                using UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                int numRetries = 0;
                bool shouldRetry = true;

                while (shouldRetry)
                {
                    shouldRetry = false;

                    while (numActiveGets >= MaxNumActiveGets)
                    {
                        // wait for other requests to wrap up to avoid port exhaustion.

                        if (abortToken.IsCancellationRequested)
                        {
                            return null;
                        }

                        await Task.Delay(10, abortToken);
                    }

                    Interlocked.Increment(ref numActiveGets);

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        if (abortToken.IsCancellationRequested)
                        {
                            request.Abort();
                            return null;
                        }

                        progressReporter?.Report(operation.progress);
                        await Task.Yield();
                        // await Task.Delay(AsyncGetPollingTimeMs, abortToken);
                    }

                    if (!string.IsNullOrEmpty(request.error))
                    {
                        if (IsTransientError(request.responseCode) && numRetries < retries)
                        {
                            // retry if transient error and retry limit not reached.
                            shouldRetry = true;
                            numRetries++;

                            if (numRetries > retries)
                            {
                                throw new WebException($"Unable to download {url}. Retries exceeded.");
                            }

                            await BackoffHelper.WaitWithBackoffAsync(numRetries, abortToken);
                        }
                        else
                        {
                            if (numRetries > retries)
                            {
                                throw new WebException($"Unable to download {url}. Retries exceeded.");
                            }
                            
                            // Non-transient error or max retries reached.
                            return new WebRequestResponseWithHandler(request);
                        }
                    }
                    else
                    {
                        progressReporter?.Report(operation.progress);
                    }
                }

                return new WebRequestResponseWithHandler(request);
            }
            finally
            {
                Interlocked.Decrement(ref numActiveGets);
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
            try
            {
                if (abortToken.IsCancellationRequested)
                {
                    return null;
                }

                using UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(content));

                int numRetries = 0;
                bool shouldRetry = true;

                while (shouldRetry)
                {
                    shouldRetry = false;

                    while (numActiveGets >= MaxNumActiveGets)
                    {
                        // wait for other requests to wrap up to avoid port exhaustion.

                        if (abortToken.IsCancellationRequested)
                        {
                            return null;
                        }

                        await Task.Delay(10, abortToken);
                    }

                    Interlocked.Increment(ref numActiveGets);

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        if (abortToken.IsCancellationRequested)
                        {
                            request.Abort();
                            return null;
                        }

                        progressReporter?.Report(operation.progress);
                        await Task.Yield();
                    }

                    if (IsTransientError(request.responseCode) && numRetries < retries)
                    {
                        // retry if transient error and retry limit not reached.
                        shouldRetry = true;
                        numRetries++;

                        if (numRetries > retries)
                        {
                            throw new WebException($"Unable to download {url}. Retries exceeded.");
                        }

                        await BackoffHelper.WaitWithBackoffAsync(numRetries, abortToken);
                    }
                    else
                    {
                        if (numRetries > retries)
                        {
                            throw new WebException($"Unable to download {url}. Retries exceeded.");
                        }
                            
                        // Non-transient error or max retries reached.
                        return new WebRequestResponseWithHandler(request);
                    }
                }

                return new WebRequestResponseWithHandler(request);
            }
            finally
            {
                Interlocked.Decrement(ref numActiveGets);
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
            bool shouldRetry = true;
            int numRetries = 0;

            while (shouldRetry)
            {
                shouldRetry = false;

                using UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = timeoutInSeconds;
                using var downloadHandler = new DownloadHandlerFile(absoluteFilePath);
                downloadHandler.removeFileOnAbort = true;
                
                // use file handler
                request.downloadHandler = downloadHandler;
                request.disposeDownloadHandlerOnDispose = true;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                while (numActiveGets >= MaxNumActiveGets)
                {
                    // wait for other requests to wrap up to avoid port exhaustion.
                    yield return new WaitForSeconds(0.100f); // wait for 100 ms
                }

                Interlocked.Increment(ref numActiveGets);
                AsyncOperation op = request.SendWebRequest();

                while (!op.isDone)
                {
                    yield return null;
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

                        Interlocked.Decrement(ref numActiveGets);
                        callBack?.Invoke(resp);
                    }
                }
                else
                {
                    progressReporter?.Report(op.progress);

                    WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
                    Interlocked.Decrement(ref numActiveGets);
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
            try
            {
                while (numActiveGets >= MaxNumActiveGets)
                {
                    abortToken.ThrowIfCancellationRequested();

                    // wait for other requests to wrap up to avoid port exhaustion.
                    await Task.Delay(10, abortToken);
                }

                abortToken.ThrowIfCancellationRequested();

                Interlocked.Increment(ref numActiveGets);

                int numRetries = 0;
                bool shouldRetry = true;

                while (shouldRetry)
                {
                    shouldRetry = false;

                    using UnityWebRequest request = UnityWebRequest.Get(url);
                    request.timeout = timeoutInSeconds;
                    
                    using var downloadHandler = new DownloadHandlerFile(absoluteFilePath);
                    downloadHandler.removeFileOnAbort = true;
                    request.downloadHandler = downloadHandler; // use file handler
                    request.disposeDownloadHandlerOnDispose = true;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                    
                    while (!operation.isDone)
                    {
                        if (abortToken.IsCancellationRequested)
                        {
                            request.Abort();
                            abortToken.ThrowIfCancellationRequested();
                            return null;
                        }

                        // spin lock while waiting for response. Since method is async, it doesn't block main thread.
                        progressReporter?.Report(operation.progress);
                        await Task.Yield();
                    }

                    if (!string.IsNullOrEmpty(request.error) && retries > 0)
                    {
                        if (request.responseCode != 404)
                        {
                            if (abortToken.IsCancellationRequested)
                            {
                                request.Abort();
                                abortToken.ThrowIfCancellationRequested();
                                return null;
                            }

                            if (IsTransientError(request.responseCode) && numRetries < retries)
                            {
                                // retry if transient error and retry limit not reached.
                                shouldRetry = true;
                                numRetries++;

                                if (numRetries > retries)
                                {
                                    throw new WebException($"Unable to download {url}. Retries exceeded.");
                                }

                                await BackoffHelper.WaitWithBackoffAsync(numRetries, abortToken);
                            }
                            else
                            {
                                if (numRetries > retries)
                                {
                                    throw new WebException($"Unable to download {url}. Retries exceeded.");
                                }
                            
                                // Non-transient error or max retries reached.
                                return new WebRequestResponse(request);
                            }
                        }
                        else
                        {
                            progressReporter?.Report(operation.progress);
                            return new WebRequestResponse(request);
                        }
                    }
                    else
                    {
                        progressReporter?.Report(operation.progress);

                        return new WebRequestResponse(request);
                    }
                }

                return null;
            }
            finally
            {
                Interlocked.Decrement(ref numActiveGets);
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
            using UnityWebRequest request = CreatePostRequest(url, headers, body, contentType, timeoutInSeconds);
            Interlocked.Increment(ref numActivePosts);
            yield return request.SendWebRequest();
            var resp = new WebRequestResponseWithHandler(request);
            
            Interlocked.Decrement(ref numActivePosts);
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
        [ItemCanBeNull]
        private static async Task<WebRequestResponseWithHandler> RunPostAsync(
            string url,
            IDictionary<string, string> headers,
            string body,
            string contentType,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
        {
            using UnityWebRequest request = CreatePostRequest(url, headers, body, contentType, timeoutInSeconds);

            if (abortToken.IsCancellationRequested)
            {
                return null;
            }

            Interlocked.Increment(ref numActivePosts);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            var tcs = new TaskCompletionSource<bool>();
            operation.completed += _ => tcs.SetResult(true);

            // Perhaps a bit paranoid, but we should not be calling Register on an already-canceled token
            if (abortToken.IsCancellationRequested)
            {
                Interlocked.Decrement(ref numActivePosts);
                request.Abort();
                return null;
            }

            // Register a callback if the abort token is canceled which will cancel the request itself.
            CancellationTokenRegistration cancelRegistration = abortToken.Register(() =>
            {
                Interlocked.Decrement(ref numActivePosts);
                request.Abort();
                tcs.SetCanceled();
            });

            // Wait for the task to either complete or cancel
            // Also every 500ms, update the progress
            while (!tcs.Task.IsCompleted)
            {
                progressReporter?.Report(operation.progress);

                Task delayTask = Task.Delay(500, abortToken);
                await Task.WhenAny(tcs.Task, delayTask);
            }

            // Unregister the callback as we no longer should be aborting the request if the token is canceled.
            await cancelRegistration.DisposeAsync();

            if (tcs.Task.IsCanceled)
            {
                Interlocked.Decrement(ref numActivePosts);
                return null;
            }

            // Update the progress one last time at the end
            progressReporter?.Report(operation.progress);

            Interlocked.Decrement(ref numActivePosts);
            return new WebRequestResponseWithHandler(request);
        }


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
                using UnityWebRequest request = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body));

                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                Interlocked.Increment(ref numActivePuts);
                yield return request.SendWebRequest();

                WebRequestResponse resp = new WebRequestResponse(request);
                Interlocked.Decrement(ref numActivePuts);
                request.Dispose();
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
            using UnityWebRequest request = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body));
            request.timeout = timeoutInSeconds;

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }
            }

            if (abortToken.IsCancellationRequested)
            {
                return null;
            }

            Interlocked.Increment(ref numActivePuts);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            var tcs = new TaskCompletionSource<bool>();
            operation.completed += _ => tcs.SetResult(true);

            // Perhaps a bit paranoid, but we should not be calling Register on an already-canceled token
            if (abortToken.IsCancellationRequested)
            {
                Interlocked.Decrement(ref numActivePuts);
                request.Abort();
                return null;
            }

            // Register a callback if the abort token is canceled which will cancel the request itself.
            CancellationTokenRegistration cancelRegistration = abortToken.Register(() =>
            {
                Interlocked.Decrement(ref numActivePuts);
                request.Abort();
                tcs.SetCanceled();
            });

            // Wait for the task to either complete or cancel
            await tcs.Task;

            // Unregister the callback as we no longer should be aborting the request if the token is canceled.
            await cancelRegistration.DisposeAsync();

            if (tcs.Task.IsCanceled)
            {
                Interlocked.Decrement(ref numActivePuts);
                return null;
            }

            Interlocked.Decrement(ref numActivePuts);
            return new WebRequestResponse(request);
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
    }
}