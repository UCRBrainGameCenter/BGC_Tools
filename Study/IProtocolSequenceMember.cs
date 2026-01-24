using System;

namespace BGC.Study
{
    public interface IProtocolSequenceMember
    {
        int ID { get; }
        SequenceType Type { get; }

        /// <summary>
        /// Checks the status of this sequence member.
        /// Returns ProtocolStatus.Locked if blocked, 
        /// ProtocolStatus.SessionReady if ready to play (for sessions),
        /// or ProtocolStatus.StepCompleted if this step should be skipped/advanced (e.g. passed lockout).
        /// </summary>
        ProtocolStatus CheckStatus();

        /// <summary>
        /// Called when the sequence index first lands on this member.
        /// Useful for initializing timers or logging encounter times.
        /// </summary>
        void OnEncountered();

        /// <summary>
        /// Called when the sequence advances past this member.
        /// </summary>
        void OnCompleted();
    }
}
