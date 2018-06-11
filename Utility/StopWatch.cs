using System.Collections.Generic;
using System;

namespace BGC.Utility
{
    public static class StopWatch
    {
        private static Dictionary<int, double> startTimes = new Dictionary<int, double>();
        private static Dictionary<int, double> endTimes = new Dictionary<int, double>();
        private static IdManager idManager = new IdManager();
        private static int recentId = -1;

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Get current time in milliseconds
        /// </summary>
        public static double AbsoluteTime
        {
            get
            {
                return (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
            }
        }

        /// <summary>
        /// Start a new timer
        /// </summary>
        /// <returns>id of the timer</returns>
        public static int StartTimer()
        {
            recentId = idManager.GetId();
            startTimes.Add(recentId, AbsoluteTime);

            return recentId;
        }

        /// <summary>
        /// Gets the time between when the timer started and when it was ended
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static double GetTime(int id)
        {
            if (startTimes.ContainsKey(id) == false)
            {
                throw new ArgumentException($"{id} did not have a timer started.");
            }

            if (endTimes.ContainsKey(id) == false)
            {
                throw new ArgumentException($"{id} did not have a timer ended.");
            }

            return endTimes[id] - startTimes[id];
        }

        /// <summary>
        /// Gets the time between when the timer started and when it was ended for
        /// the most recently created timer id
        /// </summary>
        /// <returns></returns>
        public static double GetTime()
        {
            return GetTime(recentId);
        }

        /// <summary>
        /// Ends the timer for the id given
        /// </summary>
        /// <param name="id"></param>
        public static void EndTimer(int id)
        {
            double endTime = AbsoluteTime;

            if (startTimes.ContainsKey(id) == false)
            {
                throw new ArgumentOutOfRangeException($"{id} did not have a timer started.");
            }
            else if (endTimes.ContainsKey(id))
            {
                throw new ArgumentException($"{id} has already had its timer ended.");
            }

            endTimes.Add(id, endTime);
        }

        /// <summary>
        /// Ends the timer for the most recent id created
        /// </summary>
        /// <returns></returns>
        public static void EndTimer()
        {
            EndTimer(recentId);
        }
    }
}