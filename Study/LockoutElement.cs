using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LightJson;

namespace BGC.Study
{
    public abstract class LockoutElement
    {
        private static int nextElementID = 1;

        public abstract string ElementType { get; }
        public readonly int id;

        public JsonObject envVals;

        //Deserialized lockoutElements are not added to the LockoutElement dictionary
        public LockoutElement(JsonObject data)
        {
            id = data[ProtocolKeys.LockoutElement.Id];
            if (nextElementID <= id)
            {
                nextElementID = id + 1;
            }

            if (data.ContainsKey(ProtocolKeys.LockoutElement.EnvironmentValues))
            {
                envVals = data[ProtocolKeys.LockoutElement.EnvironmentValues].AsJsonObject;
            }
            else
            {
                envVals = new JsonObject();
            }
        }

        //Explicitly created lockoutElements are added to the dictionary
        public LockoutElement()
        {
            id = nextElementID++;

            envVals = new JsonObject();

            ProtocolManager.lockoutElementDictionary.Add(id, this);
        }

        public JsonObject SerializeElement()
        {
            JsonObject lockoutElement = new JsonObject()
            {
                { ProtocolKeys.LockoutElement.Id, id },
                { ProtocolKeys.LockoutElement.Type, ElementType }
            };

            if (envVals.Count > 0)
            {
                lockoutElement.Add(ProtocolKeys.LockoutElement.EnvironmentValues, envVals);
            }

            _PopulateJSONObject(lockoutElement);

            return lockoutElement;
        }

        protected abstract void _PopulateJSONObject(JsonObject jsonObject);

        /// <summary>
        /// Checks if this lockout is currently blocking.
        /// </summary>
        public abstract bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes);

        /// <summary>
        /// Gets the bypass password for this lockout, if any.
        /// </summary>
        public virtual string GetBypassPassword() => null;


        /// <summary>
        /// Gets the required password for this lockout, if it's a password-based lockout.
        /// Returns null if this is not a password lockout.
        /// </summary>
        public virtual string GetPassword() => null;

        /// <summary>
        /// Returns true if this is a time-based lockout (has an expiration time).
        /// </summary>
        public virtual bool IsTimeBased => false;

        /// <summary>
        /// Gets when this lockout expires. Returns null if not time-based or cannot be calculated.
        /// </summary>
        public virtual DateTime? GetLockoutExpiration() => null;

        /// <summary>
        /// Rounds a time up to the next full minute for user-friendly display.
        /// Avoids confusion when showing "locked until 7:44 PM" at 7:44:30 PM.
        /// </summary>
        protected static DateTime RoundUpToNextMinute(DateTime time)
        {
            if (time.Second == 0 && time.Millisecond == 0)
            {
                return time;
            }
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0).AddMinutes(1);
        }

        /// <summary>
        /// Gets a user-friendly message describing why the session is locked.
        /// </summary>
        public virtual string GetLockoutMessage()
        {
            DateTime? expiration = GetLockoutExpiration();
            if (expiration.HasValue)
            {
                DateTime displayTime = RoundUpToNextMinute(expiration.Value);
                return $"Session is locked until {displayTime:g}.";
            }
            return "Session is locked.";
        }

        /// <summary>
        /// Called when the lockout is passed/completed.
        /// </summary>
        public virtual void OnLockoutCompleted(DateTime encounteredTime, DateTime completedTime)
        {
        }

        /// <summary>
        /// Clears/skips this lockout's stored state so it no longer blocks.
        /// Used by admin features to bypass time-based lockouts.
        /// </summary>
        public virtual void ClearLockout()
        {
            // Default implementation does nothing.
            // Override in concrete classes that have persistent state.
        }

        public static void HardClear()
        {
            nextElementID = 1;
        }
    }
}
