using System;

namespace BGC.Audio
{
    /// <summary>
    /// What a level regulator does when a stimulus would play above the safety limit
    /// (<see cref="Normalization.dbSafetyLimit"/>: 90 dB SPL, or 90 dB HL for audiometric
    /// regulation): refuse it (a StreamCompositionException on the first read, the historical
    /// behavior), clamp it (play it at the limit), or allow it (play it as requested). Every
    /// exceedance is reported through <see cref="SafetyLimit.EventReporter"/> whatever the mode, so
    /// the host application can log it; with no reporter installed nothing is logged and Off costs
    /// no measurement.
    /// </summary>
    public enum SafetyLimitMode
    {
        Refuse = 0,
        Clamp = 1,
        Off = 2
    }

    /// <summary> One safety-limit decision: a level that would have exceeded the limit, and what was done about it </summary>
    public sealed class SafetyLimitEvent
    {
        /// <summary> "Request" (the requested level), "Delivered" (the samples' level re the claim) or "Trial" (a mix) </summary>
        public string Check;

        /// <summary> "dB" (SPL) or "dB HL" </summary>
        public string Unit;

        public double Limit;

        public double RequestedLevel;

        /// <summary> The level that would have played </summary>
        public double DeliveredLevel;

        /// <summary> The level that plays: the limit under Clamp, DeliveredLevel under Off, NaN when refused </summary>
        public double AppliedLevel;

        public SafetyLimitMode Mode;

        /// <summary> 0 left, 1 right, -1 every channel </summary>
        public int Channel;

        public string Message;

        /// <summary> How far the stimulus was turned down; NaN when refused </summary>
        public double AttenuationDB => double.IsNaN(AppliedLevel) ? double.NaN : DeliveredLevel - AppliedLevel;

        public bool Refused => Mode == SafetyLimitMode.Refuse;
    }

    public static class SafetyLimit
    {
        /// <summary> Installed by the host to record every exceedance; null records nothing </summary>
        public static Action<SafetyLimitEvent> EventReporter { get; set; }

        /// <summary> The mode the historical bool meant: true refuses, false disengages the limit </summary>
        public static SafetyLimitMode ModeFor(bool safetyLimit) => safetyLimit ? SafetyLimitMode.Refuse : SafetyLimitMode.Off;

        /// <summary> True when the limit changes what plays (Refuse or Clamp) </summary>
        public static bool Engaged(SafetyLimitMode mode) => mode != SafetyLimitMode.Off;

        /// <summary> Whether a delivered level is worth measuring: when the limit is engaged, or to report it under Off </summary>
        public static bool WantsMeasurement(SafetyLimitMode mode) => Engaged(mode) || EventReporter != null;

        /// <summary>
        /// Decides what a level that may exceed the limit becomes. Returns
        /// <paramref name="deliveredLevel"/> unchanged when it is within the limit plus
        /// <paramref name="tolerance"/>, or under Off; the limit under Clamp; and throws a
        /// StreamCompositionException under Refuse. Every exceedance is reported first.
        /// </summary>
        /// <param name="check">"Request", "Delivered" or "Trial"</param>
        /// <param name="channel">0 left, 1 right, -1 every channel (names the ear in the message)</param>
        /// <param name="detail">Appended to the message, e.g. the request and the claim</param>
        public static double Resolve(
            SafetyLimitMode mode,
            string check,
            string unit,
            double limit,
            double tolerance,
            double requestedLevel,
            double deliveredLevel,
            int channel,
            string detail)
        {
            if (!(deliveredLevel > limit + tolerance))
            {
                return deliveredLevel;
            }

            double applied = mode == SafetyLimitMode.Clamp ? limit : mode == SafetyLimitMode.Off ? deliveredLevel : double.NaN;
            SafetyLimitEvent reported = Report(mode, check, unit, limit, requestedLevel, deliveredLevel, applied, channel, detail);

            if (mode == SafetyLimitMode.Refuse)
            {
                throw new StreamCompositionException(reported.Message);
            }

            return applied;
        }

        /// <summary>
        /// Reports one exceedance with the level that will actually play (<paramref name="appliedLevel"/>:
        /// NaN when refused), for callers that decide the attenuation themselves, e.g. a trial whose
        /// ears share one scale. Builds the message, invokes the reporter, returns the event.
        /// </summary>
        public static SafetyLimitEvent Report(
            SafetyLimitMode mode,
            string check,
            string unit,
            double limit,
            double requestedLevel,
            double deliveredLevel,
            double appliedLevel,
            int channel,
            string detail)
        {
            string ear = channel == 0 ? " in the left ear" : channel == 1 ? " in the right ear" : "";
            string message;

            switch (mode)
            {
                case SafetyLimitMode.Clamp:
                    message = $"Safety limit: {check} check, the stimulus would deliver {deliveredLevel:0.0} {unit}{ear}; " +
                        $"turned down to {appliedLevel:0.0} {unit} ({deliveredLevel - appliedLevel:0.0} dB attenuation). {detail}";
                    break;

                case SafetyLimitMode.Off:
                    message = $"Safety limit disengaged: {check} check, the stimulus plays at {deliveredLevel:0.0} {unit}{ear}, " +
                        $"above the {limit} {unit} limit. {detail}";
                    break;

                default:
                    message = $"Tried to exceed safety limit of {limit}{unit} without disengaging safety. {detail}";
                    break;
            }

            SafetyLimitEvent limitEvent = new SafetyLimitEvent
            {
                Check = check,
                Unit = unit,
                Limit = limit,
                RequestedLevel = requestedLevel,
                DeliveredLevel = deliveredLevel,
                AppliedLevel = appliedLevel,
                Mode = mode,
                Channel = channel,
                Message = message
            };

            EventReporter?.Invoke(limitEvent);
            return limitEvent;
        }
    }
}
