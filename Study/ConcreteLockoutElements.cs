using LightJson;
using System;
using System.Collections.Generic;

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
            if (sequenceTimes == null)
            {
                return false;
            }

            // Filter for sessions
            List<SequenceTime> sessions = new List<SequenceTime>();
            foreach (var st in sequenceTimes)
            {
                if (st.type == SequenceType.Session)
                {
                    sessions.Add(st);
                }
            }

            if (sessions.Count == 0)
            {
                return false;
            }

            if (TimeMinutes <= 0)
            {
                return false;
            }

            DateTime lastSessionTime = sessions[sessions.Count - 1].completedTime;
            TimeSpan timeSinceLastSession = currentTime - lastSessionTime;
            return timeSinceLastSession.TotalMinutes < TimeMinutes;
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

        public override bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes)
        {
            if (sequenceTimes == null)
            {
                return false;
            }

            // Filter for sessions
            List<SequenceTime> sessions = new List<SequenceTime>();
            foreach (var st in sequenceTimes)
            {
                if (st.type == SequenceType.Session)
                {
                    sessions.Add(st);
                }
            }

            if (sessions.Count == 0)
            {
                return false;
            }

            // 1. Check MinTime
            DateTime lastSessionTime = sessions[sessions.Count - 1].completedTime;
            TimeSpan timeSinceLastSession = currentTime - lastSessionTime;
            if (timeSinceLastSession.TotalMinutes < MinTimeMinutes)
            {
                return true;
            }

            // 2. Check Window
            if (WindowTimeMinutes > 0 && MaxSessions > 0)
            {
                // Simulate windows from the beginning of history to determine the current window state
                DateTime currentWindowStart = sessions[0].completedTime;
                int sessionsInCurrentWindow = 1;

                for (int i = 1; i < sessions.Count; i++)
                {
                    DateTime sessionTime = sessions[i].completedTime;

                    // Is this session inside the current window?
                    if ((sessionTime - currentWindowStart).TotalMinutes < WindowTimeMinutes)
                    {
                        sessionsInCurrentWindow++;
                    }
                    else
                    {
                        // Start a new window
                        currentWindowStart = sessionTime;
                        sessionsInCurrentWindow = 1;
                    }
                }

                // Now check against current time
                // If we are still inside the last established window...
                if ((currentTime - currentWindowStart).TotalMinutes < WindowTimeMinutes)
                {
                    // ...and we have hit the max sessions...
                    if (sessionsInCurrentWindow >= MaxSessions)
                    {
                        return true; // Locked
                    }
                }
            }

            return false;
        }
    }
}
