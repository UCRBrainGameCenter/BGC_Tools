﻿using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BGC.Web.Utility
{
    /// <summary> Provides a set of methods to perform exponential backoff. </summary>
    /// <remarks> Generated by GPT 11/8/2023 </remarks>
    public static class BackoffHelper
    {
        private static readonly TimeSpan InitialBackoffInterval = TimeSpan.FromSeconds(1);
        private const double BackoffMultiplier = 2.0;
        private const int MaxBackoffIntervalSeconds = 60; // Max backoff interval to 60 seconds
        private const int MaxExponent = 31; // Max exponent used for backoff log2(Max Integer Value) = 31
        
        /// <summary> Asynchronously performs a delay based on an exponential backoff policy. </summary>
        /// <param name="retryAttempt">
        /// The current retry attempt count. This is used to calculate the backoff period.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of cancellation.
        /// </param>
        /// <returns>A Task that represents the asynchronous operation.</returns>
        /// <remarks> The delay increases exponentially with the number of retry attempts.</remarks>
        public static async Task WaitWithBackoffAsync(int retryAttempt, CancellationToken cancellationToken)
        {
            // Calculate the delay with an exponential backoff formula
            TimeSpan delay = CalculateBackoffInterval(retryAttempt);

            cancellationToken.ThrowIfCancellationRequested();
            
            // Wait for the delay duration
            await Task.Delay(delay, cancellationToken);
        }
        
        /// <summary>
        /// Calculates the backoff interval based on the given retry attempt.
        /// </summary>
        /// <param name="retryAttempt">The number of the current retry attempt.</param>
        /// <remarks>
        /// This method calculates the delay by applying an exponential backoff formula.
        /// Here is the pattern of delays for the first five retries, assuming a base delay of 1 second
        /// (2^(retryAttempt - 1)):
        /// <list type="bullet">
        /// <item>
        /// <description>Retry 1: delay = 2^(1 - 1) = 2^0 = 1 second</description>
        /// </item>
        /// <item>
        /// <description>Retry 2: delay = 2^(2 - 1) = 2^1 = 2 seconds</description>
        /// </item>
        /// <item>
        /// <description>Retry 3: delay = 2^(3 - 1) = 2^2 = 4 seconds</description>
        /// </item>
        /// <item>
        /// <description>Retry 4: delay = 2^(4 - 1) = 2^3 = 8 seconds</description>
        /// </item>
        /// <item>
        /// <description>Retry 5: delay = 2^(5 - 1) = 2^4 = 16 seconds</description>
        /// </item>
        /// </list>
        /// </remarks>
        /// <returns>The calculated backoff interval in seconds.</returns>
        public static TimeSpan CalculateBackoffInterval(int retryAttempt)
        {
            // Avoid overflow and use MaxValue if the retryAttempt is too high
            if (retryAttempt > MaxExponent) 
            {
                return TimeSpan.FromSeconds(MaxBackoffIntervalSeconds);
            }

            // Calculate the exponential backoff interval
            var backoffInterval = (int)(Math.Pow(BackoffMultiplier, retryAttempt) * InitialBackoffInterval.TotalSeconds);
            backoffInterval = Math.Min(backoffInterval, MaxBackoffIntervalSeconds);

            return TimeSpan.FromSeconds(backoffInterval);
        }
    }

}