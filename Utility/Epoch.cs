using System;
using System.Diagnostics;

namespace BGC.Utility
{
    public static class Epoch
    {
        // Coarse, absolute epoch sample at startup
        private static readonly long _epochMillisAtStartup = DateTimeOffset
            .UtcNow
            .ToUnixTimeMilliseconds();

        // High-resolution tick count at the same instant
        private static readonly long _stopwatchTicksAtStartup = Stopwatch.GetTimestamp();

        // Conversion factor from ticks to milliseconds
        private static readonly double _ticksToMillis = 1000.0 / Stopwatch.Frequency;

        /// <summary>
        /// Current time in milliseconds since Unix epoch,
        /// with sub-millisecond precision courtesy of Stopwatch.
        /// </summary>
        public static double Time => _epochMillisAtStartup + (Stopwatch.GetTimestamp() - _stopwatchTicksAtStartup) * _ticksToMillis;
    }

}
