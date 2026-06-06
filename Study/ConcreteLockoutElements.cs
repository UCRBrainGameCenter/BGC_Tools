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

        // Sequence index of the lockout instance most recently counted toward the
        // window, or -1 if none. Used by CheckLockout to count each instance once.
        private int ReadLastCountedIndex()
        {
            JsonObject obj = ProtocolManager.GetExtensionStateObject(StateKey);
            return obj != null && obj.ContainsKey("lastCountedIndex")
                ? obj["lastCountedIndex"].AsInteger
                : -1;
        }

        // Start time of the most recently completed session — the first session of the
        // window being opened. Anchoring windowStart here (rather than "now", the moment
        // this gate happens to be evaluated) makes "N sessions within T minutes" measure
        // from when that session actually began. It also self-corrects a stale gate: if
        // the app was quit between the session and this lockout, the recovered start may
        // already be more than WindowTime ago, so the window opens already-expired
        // instead of granting a fresh full window long after the session ran. Falls back
        // to <paramref name="fallback"/> when no session has been recorded yet.
        private static DateTime MostRecentSessionStart(
            IEnumerable<SequenceTime> sequenceTimes, DateTime fallback)
        {
            DateTime start = fallback;
            DateTime latestCompleted = DateTime.MinValue;

            if (sequenceTimes != null)
            {
                foreach (SequenceTime st in sequenceTimes)
                {
                    if (st.type == SequenceType.Session && st.completedTime >= latestCompleted)
                    {
                        latestCompleted = st.completedTime;
                        start = st.encounteredTime;
                    }
                }
            }

            return start;
        }

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
            // Admin skip: drop the current window and the min-time gate so this lockout
            // no longer blocks. Preserve lastCountedIndex so re-polling the *current*
            // position does not immediately re-count and re-lock — the next NEW
            // encounter (the following lockout instance) opens a fresh window.
            int lastCountedIndex = ReadLastCountedIndex();

            ProtocolManager.SetExtensionState(
                StateKey,
                new JsonValue(new JsonObject
                {
                    { "windowStart", DateTime.MinValue },
                    { "lastPassTime", DateTime.MinValue },
                    { "passCount", 0 },
                    { "lastCountedIndex", lastCountedIndex }
                }));
        }

        public override void OnLockoutCompleted(DateTime encounteredTime, DateTime completedTime)
        {
            if (WindowTimeMinutes <= 0 || MaxSessions <= 0)
            {
                return;
            }

            // The window count is maintained in CheckLockout (incremented once when each
            // lockout instance is first encountered). Completion only records when this
            // gate was passed, so MinTime can enforce a gap before the next session.
            JsonObject obj = ProtocolManager.GetExtensionStateObject(StateKey);

            DateTime windowStart = DateTime.MinValue;
            int passCount = 0;
            int lastCountedIndex = -1;

            if (obj != null)
            {
                windowStart = obj.ContainsKey("windowStart") ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                passCount = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;
                lastCountedIndex = obj.ContainsKey("lastCountedIndex") ? obj["lastCountedIndex"].AsInteger : -1;
            }

            ProtocolManager.SetExtensionState(
                StateKey,
                new JsonValue(new JsonObject
                {
                    { "windowStart", windowStart },
                    { "lastPassTime", completedTime },
                    { "passCount", passCount },
                    { "lastCountedIndex", lastCountedIndex }
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
            int count = 0;
            int lastCountedIndex = -1;

            if (obj != null)
            {
                windowStart = obj.ContainsKey("windowStart") ? (obj["windowStart"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                lastPassTime = obj.ContainsKey("lastPassTime") ? (obj["lastPassTime"].AsDateTime ?? DateTime.MinValue) : DateTime.MinValue;
                count = obj.ContainsKey("passCount") ? obj["passCount"].AsInteger : 0;
                lastCountedIndex = obj.ContainsKey("lastCountedIndex") ? obj["lastCountedIndex"].AsInteger : -1;
            }

            // Count this lockout instance ONCE, the first time it is encountered at this
            // sequence position. The same lockout element repeats at many positions
            // (between every pair of sessions); each position is one instance toward the
            // window's limit. CheckLockout is polled repeatedly within a single encounter
            // (status refreshes, hub re-renders), so dedupe on the active SequenceIndex.
            // When the window has expired (or never opened) a fresh window starts here and
            // the count resets — exactly the "window expires -> count resets to 0" rule.
            int currentIndex = ProtocolManager.SequenceIndex;
            if (currentIndex != lastCountedIndex)
            {
                bool windowExpired = windowStart == DateTime.MinValue
                    || currentTime >= windowStart.AddMinutes(WindowTimeMinutes);
                if (windowExpired)
                {
                    // Anchor the new window to the start of the session that opened it,
                    // not to "now" (when this gate is reached). See MostRecentSessionStart.
                    windowStart = MostRecentSessionStart(sequenceTimes, currentTime);
                    count = 0;
                }

                count++;
                lastCountedIndex = currentIndex;

                ProtocolManager.SetExtensionState(
                    StateKey,
                    new JsonValue(new JsonObject
                    {
                        { "windowStart", windowStart },
                        { "lastPassTime", lastPassTime },
                        { "passCount", count },
                        { "lastCountedIndex", lastCountedIndex }
                    }));
            }

            // Minimum time between consecutive sessions.
            if (lastPassTime != DateTime.MinValue && MinTimeMinutes > 0)
            {
                if ((currentTime - lastPassTime).TotalMinutes < MinTimeMinutes)
                {
                    return true;
                }
            }

            // Window limit: locked once this window has counted MaxSessions instances and
            // the window has not yet elapsed.
            if (windowStart != DateTime.MinValue
                && currentTime < windowStart.AddMinutes(WindowTimeMinutes)
                && count >= MaxSessions)
            {
                return true;
            }

            return false;
        }
    }
}
