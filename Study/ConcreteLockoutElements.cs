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

        private string StateKey => $"BGC.Study.FixedTimeLockout:{id}";

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
        public override bool IsTimeBased => true;

        public override DateTime? GetLockoutExpiration()
        {
            if (TimeMinutes <= 0)
            {
                return null;
            }

            // Stored state — including DateTime.MinValue (the cleared-by-Skip
            // sentinel) — is authoritative. Falling back to "now + TimeMinutes"
            // when state.expiration == MinValue would surface a phantom future
            // lockout for a user whose lockout was already admin-skipped.
            JsonObject state = ProtocolManager.GetExtensionStateObject(StateKey);
            if (state != null && state.ContainsKey("expiration"))
            {
                return state["expiration"].AsDateTime ?? DateTime.MinValue;
            }

            // No state at all — first-encounter preview. Computed without
            // persisting; CheckLockout writes the real expiration on first call.
            return DateTime.Now.AddMinutes(TimeMinutes);
        }

        public override string GetLockoutMessage()
        {
            DateTime? expiration = GetLockoutExpiration();
            if (expiration.HasValue)
            {
                DateTime displayTime = RoundUpToNextMinute(expiration.Value);
                return $"Session is locked until {displayTime:g}.";
            }
            return "Session is locked.";
        }

        protected override void _PopulateJSONObject(JsonObject jsonObject)
        {
            jsonObject.Add(ProtocolKeys.LockoutElement.Time, TimeMinutes);
            if (!string.IsNullOrEmpty(BypassPassword))
            {
                jsonObject.Add(ProtocolKeys.LockoutElement.BypassPassword, BypassPassword);
            }
        }

        public override void OnLockoutCompleted(DateTime encounteredTime, DateTime completedTime)
        {
            // Clear stored state so this lockout starts fresh if encountered again
            ProtocolManager.RemoveExtensionState(StateKey);
        }

        public override void ClearLockout()
        {
            // Set expiration to the past so the lockout is no longer blocking
            ProtocolManager.SetExtensionState(StateKey, new JsonValue(new JsonObject
            {
                { "expiration", DateTime.MinValue }
            }));
        }

        public override void SeedExpiration(DateTime expiration)
        {
            ProtocolManager.SetExtensionState(StateKey, new JsonValue(new JsonObject
            {
                { "expiration", expiration }
            }));
        }

        public override bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes)
        {
            if (TimeMinutes <= 0)
            {
                return false;
            }

            JsonObject state = ProtocolManager.GetExtensionStateObject(StateKey);

            if (state != null && state.ContainsKey("expiration"))
            {
                // State exists — honor whatever's stored.  It can be:
                //   * a future expiration (still locked),
                //   * an already-elapsed expiration (naturally expired — not locked),
                //   * DateTime.MinValue, the sentinel ClearLockout writes when an
                //     admin Skip clears the lockout (not locked, and crucially must
                //     NOT trigger fresh re-initialisation — that's how the bug
                //     manifested where a Skipped time lockout re-appeared after
                //     the next CheckLockout call / app restart).
                // In every case we DO NOT recompute "now + TimeMinutes"; that's only
                // legitimate when there is no stored state at all (first encounter).
                DateTime storedExpiration = state["expiration"].AsDateTime ?? DateTime.MinValue;
                return currentTime < storedExpiration;
            }

            // No stored state — fresh encounter. Compute and persist the expiration.
            // Use currentTime rather than CurrentSequenceStartTime so the lockout
            // begins when first checked (i.e. after the session ends), not when the
            // previous session started.
            DateTime expiration = currentTime.AddMinutes(TimeMinutes);
            ProtocolManager.SetExtensionState(StateKey, new JsonValue(new JsonObject
            {
                { "expiration", expiration }
            }));

            return currentTime < expiration;
        }
    }

    public class PasswordLockout : LockoutElement
    {
        public override string ElementType => "Password";

        public string Password { get; private set; }

        private string StateKey => $"BGC.Study.PasswordLockout:{id}";

        public PasswordLockout(JsonObject data) : base(data)
        {
            if (data.ContainsKey(ProtocolKeys.LockoutElement.Password))
            {
                Password = data[ProtocolKeys.LockoutElement.Password].AsString;
            }
        }

        public override string GetPassword() => Password;

        public override string GetLockoutMessage()
        {
            return "Please enter the password to begin the session.";
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
            if (string.IsNullOrEmpty(Password))
            {
                return false;
            }

            // Per-element acknowledgement: once the UI has captured the correct
            // password for THIS element, it stops blocking even if other elements
            // in the same Lockout container are still locked.  Without this,
            // entering one password in a Lockout that contains multiple
            // PasswordLockouts (or a PasswordLockout alongside a FixedTimeLockout)
            // would advance the whole container — the symptom behind the bug
            // report that "only the first password is required."
            JsonObject state = ProtocolManager.GetExtensionStateObject(StateKey);
            if (state != null
                && state.ContainsKey("acknowledged")
                && state["acknowledged"].IsBoolean
                && state["acknowledged"].AsBoolean)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Marks this password lockout as satisfied for the current encounter.
        /// Persisted, so a multi-password sequence survives an app restart partway
        /// through (the user only has to enter passwords for elements that haven't
        /// been acknowledged yet). The flag is cleared by <see cref="OnLockoutCompleted"/>
        /// when the surrounding Lockout container advances, so re-encountering the
        /// same lockout (e.g. via an admin session-jump backwards) re-requires the
        /// password.
        /// </summary>
        public override void Acknowledge()
        {
            ProtocolManager.SetExtensionState(StateKey, new JsonValue(new JsonObject
            {
                { "acknowledged", true }
            }));
        }

        public override void ClearLockout()
        {
            // Same effect for Password lockouts — an admin "skip" should leave the
            // element non-blocking. Implemented in terms of Acknowledge for clarity.
            Acknowledge();
        }

        public override void OnLockoutCompleted(DateTime encounteredTime, DateTime completedTime)
        {
            // Drop the acknowledged flag so a later re-encounter (jump-back, dry
            // run replay, etc.) prompts for the password again.
            ProtocolManager.RemoveExtensionState(StateKey);
        }
    }

    public class WindowLockout : LockoutElement
    {
        public override string ElementType => "Window";

        public double WindowTimeMinutes { get; private set; }
        public double MinTimeMinutes { get; private set; }
        public int MaxSessions { get; private set; }
        public string BypassPassword { get; private set; }

        private string StateKey => $"BGC.Study.WindowLockout:{id}";

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
        public override bool IsTimeBased => true;

        public override DateTime? GetLockoutExpiration()
        {
            if (WindowTimeMinutes <= 0 || MaxSessions <= 0)
            {
                return null;
            }

            JsonObject obj = ProtocolManager.GetExtensionStateObject(StateKey);
            if (obj == null)
            {
                return null;
            }

            DateTime windowStart = obj.ContainsKey("windowStart") 
                ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue) 
                : DateTime.MinValue;
            
            if (windowStart == DateTime.MinValue)
            {
                return null;
            }

            int passCount = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;
            DateTime lastPassTime = obj.ContainsKey("lastPassTime") 
                ? (obj["lastPassTime"].AsDateTime ?? DateTime.MinValue) 
                : DateTime.MinValue;

            DateTime? expiration = null;

            // If at max sessions, locked until at least the window end
            if (passCount >= MaxSessions)
            {
                expiration = windowStart.AddMinutes(WindowTimeMinutes);
            }

            // Also check MinTime — CheckLockout enforces this before checking
            // window state, so it can extend the lockout beyond the window end.
            if (MinTimeMinutes > 0 && lastPassTime != DateTime.MinValue)
            {
                DateTime minTimeExpiration = lastPassTime.AddMinutes(MinTimeMinutes);
                if (minTimeExpiration > DateTime.Now)
                {
                    if (expiration == null || minTimeExpiration > expiration.Value)
                    {
                        expiration = minTimeExpiration;
                    }
                }
            }

            return expiration;
        }

        public override string GetLockoutMessage()
        {
            DateTime? expiration = GetLockoutExpiration();
            if (expiration.HasValue)
            {
                DateTime displayTime = RoundUpToNextMinute(expiration.Value);
                return $"Session limit reached. Next session available at {displayTime:g}.";
            }
            return "Session limit reached for this time window.";
        }

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

        public override void ClearLockout()
        {
            if (WindowTimeMinutes <= 0 || MaxSessions <= 0)
            {
                ProtocolManager.RemoveExtensionState(StateKey);
                return;
            }

            JsonObject obj = ProtocolManager.GetExtensionStateObject(StateKey);
            if (obj == null)
            {
                return;
            }

            DateTime windowStart = obj.ContainsKey("windowStart")
                ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue)
                : DateTime.MinValue;
            DateTime lastPassTime = obj.ContainsKey("lastPassTime")
                ? (obj["lastPassTime"].AsDateTime ?? DateTime.MinValue)
                : DateTime.MinValue;
            int passCount = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;

            if (windowStart == DateTime.MinValue)
            {
                // No active window — nothing to skip
                return;
            }

            DateTime now = DateTime.Now;
            TimeSpan timeToSkip = TimeSpan.Zero;

            // Determine how much time we need to skip to clear the MinTime lockout
            if (lastPassTime != DateTime.MinValue && MinTimeMinutes > 0)
            {
                DateTime minTimeExpiry = lastPassTime.AddMinutes(MinTimeMinutes);
                if (minTimeExpiry > now)
                {
                    timeToSkip = minTimeExpiry - now;
                }
            }

            // Determine how much time we need to skip to clear the MaxSessions lockout
            if (passCount >= MaxSessions)
            {
                DateTime windowExpiry = windowStart.AddMinutes(WindowTimeMinutes);
                if (windowExpiry > now)
                {
                    TimeSpan windowWait = windowExpiry - now;
                    if (windowWait > timeToSkip)
                    {
                        timeToSkip = windowWait;
                    }
                }
            }

            if (timeToSkip <= TimeSpan.Zero)
            {
                // Not currently locked — nothing to skip
                return;
            }

            // Shift all stored times backward by timeToSkip, simulating that
            // the minimum required time has actually elapsed.
            windowStart -= timeToSkip;

            if (lastPassTime != DateTime.MinValue)
            {
                lastPassTime -= timeToSkip;
            }

            ProtocolManager.SetExtensionState(
                StateKey,
                new JsonValue(new JsonObject
                {
                    { "windowStart", windowStart },
                    { "lastPassTime", lastPassTime },
                    { "passCount", passCount }
                }));
        }

        public override void OnLockoutCompleted(DateTime encounteredTime, DateTime completedTime)
        {
            if (WindowTimeMinutes <= 0 || MaxSessions <= 0)
            {
                return;
            }

            JsonObject obj = ProtocolManager.GetExtensionStateObject(StateKey);

            DateTime windowStart = DateTime.MinValue;
            DateTime lastPassTime = DateTime.MinValue;
            int passCount = 0;

            if (obj != null)
            {
                windowStart = obj.ContainsKey("windowStart") ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                lastPassTime = obj.ContainsKey("lastPassTime") ? (obj["lastPassTime"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                passCount = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;
            }

            // Check if the window has expired using completedTime (the actual current time)
            DateTime windowEnd = windowStart.AddMinutes(WindowTimeMinutes);
            if (windowStart == DateTime.MinValue || completedTime >= windowEnd)
            {
                // Use completedTime (DateTime.Now) rather than encounteredTime
                // (which comes from CurrentSequenceStartTime and may be stale).
                // This is consistent with CheckLockout() which uses currentTime.
                windowStart = completedTime;
                lastPassTime = DateTime.MinValue;
                passCount = 0;
            }

            int newPassCount = passCount + 1;

            ProtocolManager.SetExtensionState(
                StateKey,
                new JsonValue(new JsonObject
                {
                    { "windowStart", windowStart },
                    { "lastPassTime", completedTime },
                    { "passCount", newPassCount }
                }));
        }

        public override bool CheckLockout(DateTime currentTime, IEnumerable<SequenceTime> sequenceTimes)
        {
            if (WindowTimeMinutes <= 0 || MaxSessions <= 0)
            {
                return false;
            }

            JsonObject obj = ProtocolManager.GetExtensionStateObject(StateKey);

            DateTime windowStart = DateTime.MinValue;
            DateTime lastPassTime = DateTime.MinValue;
            int passCount = 0;

            if (obj != null)
            {
                windowStart = obj.ContainsKey("windowStart") ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                lastPassTime = obj.ContainsKey("lastPassTime") ? (obj["lastPassTime"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                passCount = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;
            }

            // Check if min time between sessions has passed (use currentTime)
            if (lastPassTime != DateTime.MinValue && MinTimeMinutes > 0)
            {
                double minutesSinceLastPass = (currentTime - lastPassTime).TotalMinutes;
                if (minutesSinceLastPass < MinTimeMinutes)
                {
                    return true;
                }
            }

            // Check if we have an active window using currentTime (not stale CurrentSequenceStartTime)
            bool hasActiveWindow = windowStart != DateTime.MinValue;
            if (hasActiveWindow)
            {
                DateTime windowEnd = windowStart.AddMinutes(WindowTimeMinutes);
                if (currentTime >= windowEnd)
                {
                    hasActiveWindow = false;
                }
            }

            // If no active window, start a new one and we're not locked
            if (!hasActiveWindow)
            {
                windowStart = currentTime;
                lastPassTime = DateTime.MinValue;
                passCount = 0;

                ProtocolManager.SetExtensionState(
                    StateKey,
                    new JsonValue(new JsonObject
                    {
                        { "windowStart", windowStart },
                        { "lastPassTime", lastPassTime },
                        { "passCount", passCount }
                    }));

                return false;
            }

            // Check if we've already reached the max sessions limit for this window.
            // passCount is incremented by OnLockoutCompleted each time this gate is
            // passed through, so it already reflects completed passes.
            if (passCount >= MaxSessions)
            {
                return true;
            }

            return false;
        }
    }
}
