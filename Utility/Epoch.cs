using System;
using System.Collections.Generic;

namespace BGC.Utility
{
    public static class Epoch
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        /// <summary> Get current Epoch time in milliseconds </summary>
        public static double Time => (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }
}
