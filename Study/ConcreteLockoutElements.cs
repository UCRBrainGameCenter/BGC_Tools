using LightJson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Study
{
    public class FixedTimeLockout : LockoutElement
    {
        public override string ElementType => "FixedTime";

        public double TimeMinutes { get; private set; }
        public string BypassPassword { get; private set; }

        public FixedTimeLockout(JsonObject data) : base(data)
        {
            if (data.ContainsKey(ProtocolKeys.LockoutElement.Time))
            {
                TimeMinutes = data[ProtocolKeys.LockoutElement.Time].AsNumber;
            }

            if (data.ContainsKey(ProtocolKeys.LockoutElement.BypassPassword))
            {
                BypassPassword = data[ProtocolKeys.LockoutElement.BypassPassword].AsString;
            }
        }

        public override string GetBypassPassword() => BypassPassword;

        protected override void _PopulateJSONObject(JsonObject jsonObject)
        {
            jsonObject.Add(ProtocolKeys.LockoutElement.Time, TimeMinutes);
            if (!string.IsNullOrEmpty(BypassPassword))
            {
                jsonObject.Add(ProtocolKeys.LockoutElement.BypassPassword, BypassPassword);
            }
        }

        public override bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes)
        {
            if (TimeMinutes <= 0)
            {
                return false;
            }

            DateTime lockoutStartTime = ProtocolManager.CurrentSequenceStartTime;
            if (lockoutStartTime == DateTime.MinValue)
            {
                return false;
            }

            TimeSpan timeSinceLockoutStart = currentTime - lockoutStartTime;
            return timeSinceLockoutStart.TotalMinutes < TimeMinutes;
        }
    }

    public class PasswordLockout : LockoutElement
    {
        public override string ElementType => "Password";

        public string Password { get; private set; }

        public PasswordLockout(JsonObject data) : base(data)
        {
            if (data.ContainsKey(ProtocolKeys.LockoutElement.Password))
            {
                Password = data[ProtocolKeys.LockoutElement.Password].AsString;
            }
        }

        protected override void _PopulateJSONObject(JsonObject jsonObject)
        {
            if (!string.IsNullOrEmpty(Password))
            {
                jsonObject.Add(ProtocolKeys.LockoutElement.Password, Password);
            }
        }

        public override bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes)
        {
            // Always locked if password exists. The UI handles unlocking via password entry.
            return !string.IsNullOrEmpty(Password);
        }
    }

    public class WindowLockout : LockoutElement
    {
        public override string ElementType => "Window";

        public double WindowTimeMinutes { get; private set; }
        public double MinTimeMinutes { get; private set; }
        public int MaxSessions { get; private set; }
        public string BypassPassword { get; private set; }

        public WindowLockout(JsonObject data) : base(data)
        {
            if (data.ContainsKey(ProtocolKeys.LockoutElement.WindowTime))
            {
                WindowTimeMinutes = data[ProtocolKeys.LockoutElement.WindowTime].AsNumber;
            }

            if (data.ContainsKey(ProtocolKeys.LockoutElement.MinTime))
            {
                MinTimeMinutes = data[ProtocolKeys.LockoutElement.MinTime].AsNumber;
            }

            if (data.ContainsKey(ProtocolKeys.LockoutElement.MaxSessions))
            {
                MaxSessions = data[ProtocolKeys.LockoutElement.MaxSessions].AsInteger;
            }

            if (data.ContainsKey(ProtocolKeys.LockoutElement.BypassPassword))
            {
                BypassPassword = data[ProtocolKeys.LockoutElement.BypassPassword].AsString;
            }
        }

        public override string GetBypassPassword() => BypassPassword;

        protected override void _PopulateJSONObject(JsonObject jsonObject)
        {
            jsonObject.Add(ProtocolKeys.LockoutElement.WindowTime, WindowTimeMinutes);
            jsonObject.Add(ProtocolKeys.LockoutElement.MinTime, MinTimeMinutes);
            jsonObject.Add(ProtocolKeys.LockoutElement.MaxSessions, MaxSessions);
            if (!string.IsNullOrEmpty(BypassPassword))
            {
                jsonObject.Add(ProtocolKeys.LockoutElement.BypassPassword, BypassPassword);
            }
        }

        public override void OnLockoutCompleted(DateTime encounteredTime, DateTime completedTime)
        {
            if (WindowTimeMinutes <= 0 || MaxSessions <= 0)
            {
                return;
            }

            // Ensure we're updating the active window that this attempt belongs to.
            string key = $"BGC.Study.WindowLockout:{id}";
            JsonObject obj = ProtocolManager.GetExtensionStateObject(key);

            DateTime windowStart = DateTime.MinValue;
            DateTime lastPassTime = DateTime.MinValue;
            int passCount = 0;

            if (obj != null)
            {
                windowStart = obj.ContainsKey("windowStart") ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                lastPassTime = obj.ContainsKey("lastPassTime") ? (obj["lastPassTime"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                passCount = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;
            }

            if (windowStart == DateTime.MinValue ||
                encounteredTime >= windowStart + TimeSpan.FromMinutes(WindowTimeMinutes))
            {
                windowStart = encounteredTime;
                lastPassTime = DateTime.MinValue;
                passCount = 0;
            }

            ProtocolManager.SetExtensionState(
                key,
                new JsonValue(new JsonObject
                {
                    { "windowStart", windowStart },
                    { "lastPassTime", completedTime },
                    { "passCount", passCount + 1 }
                }));
        }

        public override bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes)
        {
            DateTime attemptStartTime = ProtocolManager.CurrentSequenceStartTime;
            if (attemptStartTime == DateTime.MinValue)
            {
                return false;
            }

            if (WindowTimeMinutes <= 0 || MaxSessions <= 0)
            {
                return false;
            }

            string key = $"BGC.Study.WindowLockout:{id}";
            JsonObject obj = ProtocolManager.GetExtensionStateObject(key);

            DateTime windowStart = DateTime.MinValue;
            DateTime lastPassTime = DateTime.MinValue;
            int passCount = 0;

            if (obj != null)
            {
                windowStart = obj.ContainsKey("windowStart") ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                lastPassTime = obj.ContainsKey("lastPassTime") ? (obj["lastPassTime"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                passCount = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;
            }

            bool hasActiveWindow = windowStart != DateTime.MinValue;
            if (hasActiveWindow)
            {
                DateTime windowEnd = windowStart + TimeSpan.FromMinutes(WindowTimeMinutes);
                if (attemptStartTime >= windowEnd)
                {
                    hasActiveWindow = false;
                }
            }

            if (!hasActiveWindow)
            {
                windowStart = attemptStartTime;
                lastPassTime = DateTime.MinValue;
                passCount = 0;

                ProtocolManager.SetExtensionState(
                    key,
                    new JsonValue(new JsonObject
                    {
                        { "windowStart", windowStart },
                        { "lastPassTime", lastPassTime },
                        { "passCount", passCount }
                    }));
            }

            if (passCount >= MaxSessions)
            {
                return true;
            }

            if (lastPassTime != DateTime.MinValue && MinTimeMinutes > 0)
            {
                if ((attemptStartTime - lastPassTime).TotalMinutes < MinTimeMinutes)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
