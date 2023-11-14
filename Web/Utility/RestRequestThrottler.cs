using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BGC.Web.Utility
{
    /// <summary>
    /// Keeps track of the number of active requests happening per HTTP Method type to avoid port exhaustion.
    /// </summary>
    public static class RestRequestThrottler
    {
        private const int DefaultMaxValueForMethod = 100;
        private static readonly ConcurrentDictionary<HttpMethod, SemaphoreSlim> Semaphores = new ConcurrentDictionary<HttpMethod, SemaphoreSlim>();

        private static readonly Dictionary<HttpMethod, int> MaxActiveRequests = new Dictionary<HttpMethod, int>()
        {
            [HttpMethod.Get] = 20,
            [HttpMethod.Post] = DefaultMaxValueForMethod,
            [HttpMethod.Put] = DefaultMaxValueForMethod,
            [HttpMethod.Delete] = DefaultMaxValueForMethod,
            [HttpMethod.Head] = DefaultMaxValueForMethod,
            [HttpMethod.Options] = DefaultMaxValueForMethod,
            [HttpMethod.Trace] = DefaultMaxValueForMethod,
        };
        
        static RestRequestThrottler()
        {
            InitializeSemaphores();
        }

        private static void InitializeSemaphores()
        {
            // semaphores will be used for tracking maximum concurrency of requests.
            HttpMethod[] methods = new[]
            {
                HttpMethod.Get,
                HttpMethod.Post,
                HttpMethod.Put,
                HttpMethod.Delete,
                HttpMethod.Head,
                HttpMethod.Options,
                HttpMethod.Trace
            };

            foreach (HttpMethod method in methods)
            {
                int maxConcurrent = MaxActiveRequests[method];
                Semaphores[method] = new SemaphoreSlim(maxConcurrent, maxConcurrent);
            }
        }
        
        /// <summary>Attempts to enter a new request into the system.</summary>
        /// <param name="method">The HTTP method to run. GET, POST, etc.</param>
        /// <param name="abortToken" />
        /// <returns>TRUE if a slot is available, FALSE otherwise.</returns>
        public static async Task<bool> TrySubmitRequestAsync(HttpMethod method, CancellationToken abortToken)
        {
            if (!Semaphores.TryGetValue(method, out SemaphoreSlim semaphore))
            {
                // Optionally handle the case where the method is not supported.
                throw new InvalidOperationException($"Semaphore not found for HTTP method: {method}");
            }

            Debug.Log($"Requesting semaphore for {method}. Current count = {semaphore.CurrentCount}");
            return await semaphore.WaitAsync(0, abortToken);
        }

        public static IEnumerator TrySubmitRequestCoroutine(HttpMethod method)
        {
            if (!Semaphores.TryGetValue(method, out SemaphoreSlim semaphore))
            {
                throw new InvalidOperationException($"Semaphore not found for HTTP method: {method}");
            }

            Debug.Log($"Requesting semaphore for {method}. Current count = {semaphore.CurrentCount}");
            while (!semaphore.Wait(0))
            {
                // Yield return null will wait for one frame before continuing the while loop
                yield return null;
            }
            Debug.Log($"Semaphore retrieved. New Count = {semaphore.CurrentCount}");
        }
        
        /// <summary> Signals that a request has completed. </summary>
        /// <param name="method">The HTTP method to end. GET, POST, etc.</param>
        public static void EndRequest(HttpMethod method)
        {
            if (!Semaphores.TryGetValue(method, out SemaphoreSlim semaphore))
            {
                // Optionally handle the case where the method is not supported.
                throw new InvalidOperationException($"Semaphore not found for HTTP method: {method}");
            }
            
            semaphore.Release();
            Debug.Log($"Returning semaphore for {method}. New count = {semaphore.CurrentCount}");
        }
    }
}