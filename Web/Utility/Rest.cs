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
        private static int numActiveGets = 0;
        private static int numActivePosts = 0;
        private static int numActivePuts = 0;
        private const int MaxNumActiveGets = 20;

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

            var fileHandler = new DownloadHandlerFile(absoluteFilePath)
            {
                removeFileOnAbort = true
            };

            // convert URL to HTTP-friendly URL
            CoroutineUtility.Mono.StartCoroutine(RunGet(
                url: url,
                callBack: callBack,
                progressReporter: progressReporter,
                timeoutInSeconds: timeoutInSeconds,
                retries: retries,
                headers: headers,
                downloadHandler: fileHandler));
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
        [ItemCanBeNull]
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

            var fileHandler = new DownloadHandlerFile(absoluteFilePath)
            {
                removeFileOnAbort = true
            };

            if (abortToken.IsCancellationRequested)
            {
                fileHandler.Dispose();
                abortToken.ThrowIfCancellationRequested();
                return null;
            }

            WebRequestResponse resp = await RunGetAsync(
                url,
                headers,
                fileHandler,
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
            try
            {
                while (numActiveGets >= MaxNumActiveGets)
                {
                    // wait for other requests to wrap up to avoid port exhaustion.
                    yield return null;
                }

                numActiveGets++;
                using UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                yield return request.SendWebRequest();

                WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
                request.Dispose();
                callBack?.Invoke(resp);
            }
            finally
            {
                numActiveGets--;
            }
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

                numActiveGets++;
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
                numActiveGets--;
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
        [ItemCanBeNull]
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

                        await Task.Delay(5, abortToken);
                    }

                    numActiveGets++;

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        if (abortToken.IsCancellationRequested)
                        {
                            request.Abort();
                            return null;
                        }

                        progressReporter?.Report(operation.progress);
                        await Task.Delay(5, abortToken);
                    }

                    if (!string.IsNullOrEmpty(request.error) && retries > 0)
                    {
                        // retry if error and retries are specified
                        shouldRetry = true;
                        numRetries++;

                        if (numRetries > retries)
                        {
                            throw new WebException($"Unable to download {url}. Retries exceeded.");
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
                numActiveGets--;
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

                        await Task.Delay(5, abortToken);
                    }

                    numActiveGets++;

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        if (abortToken.IsCancellationRequested)
                        {
                            request.Abort();
                            return null;
                        }

                        progressReporter?.Report(operation.progress);
                        await Task.Delay(5, abortToken);
                    }

                    if (!string.IsNullOrEmpty(request.error) && retries > 0)
                    {
                        // retry if error and retries are specified
                        shouldRetry = true;
                        numRetries++;

                        if (numRetries > retries)
                        {
                            throw new WebException($"Unable to download {url}. Retries exceeded.");
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
                numActiveGets--;
            }
        }

        /// <summary>Run get request using a file handler.</summary>
        /// <param name="url">The URL to send request to.</param>
        /// <param name="callBack">Code to execute when request completes. Sends back response object.</param>
        /// <param name="headers">Headers to send in the request, if any.</param>
        /// <param name="downloadHandler">Download handler to use in the request.</param>
        /// <param name="progressReporter">Progress reporter for the download.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">
        /// The amount of time, in seconds, to wait before timing out. If set to 0, there is no timeout.
        /// </param>
        private static IEnumerator RunGet(
            string url,
            Action<WebRequestResponseWithHandler> callBack,
            IDictionary<string, string> headers,
            DownloadHandlerFile downloadHandler,
            IProgress<float> progressReporter = null,
            int retries = 0,
            int timeoutInSeconds = 0)
        {
            try
            {
                bool shouldRetry = true;
                int numRetries = 0;

                while (shouldRetry)
                {
                    shouldRetry = false;

                    using UnityWebRequest request = UnityWebRequest.Get(url);
                    request.timeout = timeoutInSeconds;

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
                        yield return new WaitForSeconds(0.005f); // wait for 5 ms
                    }

                    numActiveGets++;
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
                            numActiveGets--;
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

                            request.Dispose();
                            callBack?.Invoke(resp);
                        }
                    }
                    else
                    {
                        progressReporter?.Report(op.progress);

                        WebRequestResponseWithHandler resp = new WebRequestResponseWithHandler(request);
                        request.Dispose();

                        callBack?.Invoke(resp);
                    }
                }
            }
            finally
            {
                numActiveGets--;
            }
        }

        /// <summary>Run async GET request using a file handler.</summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="downloadHandler">The file download handler to use for the request.</param>
        /// <param name="retries">Max number of retries. If set to 0, then retries will not be used.</param>
        /// <param name="timeoutInSeconds">Optional timeout for the request. Default is 15 seconds.</param>
        /// <param name="progressReporter">
        /// Optional progress reporter for the request. Takes the progress of the web request as a float.
        /// </param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>The finished unity web request. Can be NULL if operation cancelled or error occurs.</returns>
        private static async Task<WebRequestResponse> RunGetAsync(
            string url,
            IDictionary<string, string> headers,
            DownloadHandlerFile downloadHandler,
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
                    await Task.Delay(5, abortToken);
                }

                abortToken.ThrowIfCancellationRequested();

                numActiveGets++;

                int numRetries = 0;
                bool shouldRetry = true;

                while (shouldRetry)
                {
                    shouldRetry = false;

                    using UnityWebRequest request = UnityWebRequest.Get(url);
                    request.timeout = timeoutInSeconds;
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
                        await Task.Delay(1, abortToken);
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

                            // retry if error and retries are specified
                            shouldRetry = true;
                            numRetries++;

                            if (numRetries > retries)
                            {
                                throw new WebException($"Unable to download {url}. Retries exceeded.");
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
                numActiveGets--;
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
            try
            {
                numActivePosts++;

                WebRequestResponseWithHandler resp;
                {
                    using UnityWebRequest request = CreatePostRequest(url, headers, body, contentType, timeoutInSeconds);

                    yield return request.SendWebRequest();

                    resp = new WebRequestResponseWithHandler(request);
                }

                callBack?.Invoke(resp);
            }
            finally
            {
                numActivePosts--;
            }
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
            try
            {
                numActivePosts++;

                using UnityWebRequest request = CreatePostRequest(url, headers, body, contentType, timeoutInSeconds);

                if (abortToken.IsCancellationRequested)
                {
                    return null;
                }

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                var tcs = new TaskCompletionSource<bool>();
                operation.completed += _ => tcs.SetResult(true);

                abortToken.Register(() =>
                {
                    request.Abort();
                    tcs.SetCanceled();
                });

                while (!tcs.Task.IsCompleted)
                {
                    progressReporter?.Report(operation.progress);

                    var delayTask = Task.Delay(500);
                    await Task.WhenAny(tcs.Task, delayTask);
                }

                if (tcs.Task.IsCanceled)
                {
                    return null;
                }

                // Update the progress one last time at the end
                progressReporter?.Report(operation.progress);

                return new WebRequestResponseWithHandler(request);
            }
            finally
            {
                numActivePosts--;
            }
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
            try
            {
                numActivePuts++;
                using UnityWebRequest request = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body));

                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                yield return request.SendWebRequest();

                WebRequestResponse resp = new WebRequestResponse(request);
                request.Dispose();
                callBack?.Invoke(resp);
            }
            finally
            {
                numActivePuts--;
            }
        }

        private static string WriteQueryParam(KeyValuePair<string, IConvertible> param) =>
            $"{param.Key}={param.Value.Encode()}";
    }
}