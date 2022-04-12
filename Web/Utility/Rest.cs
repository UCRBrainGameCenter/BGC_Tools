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
using Plugins.BGC_Tools.Web;

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
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        public static void GetRequest(
            string url,
            IDictionary<string, string> headers,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0,
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
                timeout: timeout,
                headers: headers));
        }

        /// <summary>Send an async get request.</summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        [ItemCanBeNull]
        public static async Task<WebRequestResponse> GetRequestAsync(
            string url,
            IDictionary<string, string> headers,
            int retries = 0,
            int timeout = 0,
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
                timeout,
                progressReporter,
                abortToken);
        }

        /// <summary>Send a get request using a file handler.</summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        /// <param name="absoluteFilePath">
        /// The absolute path to the file the data will be downloaded to. Must include filename and extension.
        /// </param>
        public static void GetRequest(
            string url,
            IDictionary<string, string> headers,
            string absoluteFilePath,
            Action<UnityWebRequest, bool> callBack = null,
            int retries = 0,
            int timeout = 0,
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
                timeout: timeout,
                retries: retries,
                headers: headers,
                downloadHandler: fileHandler));
        }

        /// <summary>Send an async GET request using a file handler.</summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        /// <param name="queryParams">Dictionary of key names hashed to their values of any type</param>
        /// <param name="absoluteFilePath">
        /// The absolute path to the file the data will be downloaded to. Must include filename and extension.
        /// </param>
        [ItemCanBeNull]
        public static async Task<WebRequestResponse> GetRequestAsync(
            string url,
            IDictionary<string, string> headers,
            string absoluteFilePath,
            int retries = 0,
            int timeout = 0,
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
                return null;
            }

            WebRequestResponse resp = await RunGetAsync(
                url,
                headers,
                fileHandler,
                retries,
                timeout,
                progressReporter,
                abortToken);

            return resp;
        }

        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        public static void PostRequest(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0)
        {
            CoroutineUtility.Mono.StartCoroutine(RunPost(
                url,
                headers,
                body,
                callBack,
                timeout));
        }

        /// <summary>Send an async POST request</summary>
        [ItemCanBeNull]
        public static async Task<WebRequestResponseWithHandler> PostRequestAsync(
            string url,
            IDictionary<string, string> headers,
            string body,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
        {
            return await RunPostAsync(
                url,
                headers,
                body,
                timeoutInSeconds,
                progressReporter,
                abortToken);
        }

        /// <summary>
        /// Send a post request
        /// </summary>
        /// <param name="callBack">false means there was a local parsing error</param>
        public static void PutRequest(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack = null,
            int timeout = 0)
        {
            CoroutineUtility.Mono.StartCoroutine(RunPut(
                url,
                headers,
                body,
                callBack,
                timeout));
        }

        /// <summary>
        /// Run get request
        /// </summary>
        private static IEnumerator RunGet(
            string url,
            Action<UnityWebRequest, bool> callBack,
            IDictionary<string, string> headers,
            int timeout = 0)
        {
            try
            {
                while (numActiveGets >= MaxNumActiveGets)
                {
                    // wait for other requests to wrap up to avoid port exhaustion.
                    yield return null;
                }

                numActiveGets++;
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = timeout;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    yield return request.SendWebRequest();

                    callBack?.Invoke(request, true);
                }
            }
            finally
            {
                numActiveGets--;
            }
        }

        /// <summary>Run async GET request using async/await C# pattern.</summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="headers">The headers to attach to the request.</param>
        /// <param name="timeoutInSeconds">Optional timeout for the request. Default is 15 seconds.</param>
        /// <param name="progressReporter">
        /// Optional progress reporter for the request. Takes the progress of the web request as a float.
        /// </param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>The finished unity web request. Can be NULL if operation cancelled or error occurs.</returns>
        [ItemCanBeNull]
        private static async Task<WebRequestResponse> RunGetAsync(
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
                        await Task.Delay(1, abortToken);
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
                }

                return new WebRequestResponse(request);
            }
            finally
            {
                numActiveGets--;
            }
        }

        /// <summary>Run get request using a file handler.</summary>
        private static IEnumerator RunGet(
            string url,
            Action<UnityWebRequest, bool> callBack,
            IDictionary<string, string> headers,
            DownloadHandlerFile downloadHandler,
            int retries = 0,
            int timeout = 0)
        {
            try
            {
                bool shouldRetry = true;
                int numRetries = 0;

                while (shouldRetry)
                {
                    shouldRetry = false;
                    
                    using UnityWebRequest request = UnityWebRequest.Get(url);
                    request.timeout = timeout;

                    // use file handler
                    request.downloadHandler = downloadHandler;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    while (numActiveGets >= MaxNumActiveGets)
                    {
                        // wait for other requests to wrap up to avoid port exhaustion.
                        yield return null;
                    }

                    numActiveGets++;
                    yield return request.SendWebRequest();

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
                    }
                    else
                    {
                        downloadHandler.Dispose();
                        callBack?.Invoke(request, true);
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
        /// <param name="timeoutInSeconds">Optional timeout for the request. Default is 15 seconds.</param>
        /// <param name="progressReporter">
        /// Optional progress reporter for the request. Takes the progress of the web request as a float.
        /// </param>
        /// <param name="abortToken">Optional cancellation token.</param>
        /// <returns>The finished unity web request. Can be NULL if operation cancelled or error occurs.</returns>
        [ItemCanBeNull]
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
                    if (abortToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    // wait for other requests to wrap up to avoid port exhaustion.
                    await Task.Delay(5, abortToken);
                }

                if (abortToken.IsCancellationRequested)
                {
                    return null;
                }

                numActiveGets++;
                using UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                // use file handler
                request.downloadHandler = downloadHandler;

                int numRetries = 0;
                bool shouldRetry = true;

                while (shouldRetry)
                {
                    shouldRetry = false;
                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                    while (!operation.isDone)
                    {
                        if (abortToken.IsCancellationRequested)
                        {
                            request.Abort();
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
                            // retry if error and retries are specified
                            shouldRetry = true;
                            numRetries++;

                            if (numRetries > retries)
                            {
                                throw new WebException($"Unable to download {url}. Retries exceeded.");
                            }
                        }
                    }
                }

                return new WebRequestResponse(request);
            }
            finally
            {
                numActiveGets--;
            }
        }

        /// <summary>
        /// Run post request
        /// </summary>
        private static IEnumerator RunPost(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack,
            int timeout = 0)
        {
            try
            {
                numActivePosts++;
                using (UnityWebRequest request = UnityWebRequest.Post(url, ""))
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                    request.timeout = timeout;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    yield return request.SendWebRequest();
                    callBack?.Invoke(request, true);
                }
            }
            finally
            {
                numActivePosts--;
            }
        }

        /// <summary>Run async POST request</summary>
        [ItemCanBeNull]
        private static async Task<WebRequestResponseWithHandler> RunPostAsync(
            string url,
            IDictionary<string, string> headers,
            string body,
            int timeoutInSeconds = 0,
            IProgress<float> progressReporter = null,
            CancellationToken abortToken = default)
        {
            try
            {
                numActivePosts++;

                using UnityWebRequest request = UnityWebRequest.Post(url, "");
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.timeout = timeoutInSeconds;

                foreach (KeyValuePair<string, string> pair in headers)
                {
                    request.SetRequestHeader(pair.Key, pair.Value);
                }

                if (abortToken.IsCancellationRequested)
                {
                    return null;
                }

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    if (abortToken.IsCancellationRequested)
                    {
                        request.Abort();
                        return null;
                    }

                    // spin lock while waiting for response. Since method is async, it doesn't block main thread.
                    progressReporter?.Report(operation.progress);
                    await Task.Delay(1, abortToken);
                }

                return new WebRequestResponseWithHandler(request);
            }
            finally
            {
                numActivePosts--;
            }
        }

        /// <summary>
        /// Run put request
        /// </summary>
        private static IEnumerator RunPut(
            string url,
            IDictionary<string, string> headers,
            string body,
            Action<UnityWebRequest, bool> callBack,
            int timeout = 0)
        {
            try
            {
                numActivePuts++;
                using (UnityWebRequest request = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body)))
                {
                    request.timeout = timeout;

                    foreach (KeyValuePair<string, string> pair in headers)
                    {
                        request.SetRequestHeader(pair.Key, pair.Value);
                    }

                    yield return request.SendWebRequest();
                    callBack?.Invoke(request, true);
                }
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