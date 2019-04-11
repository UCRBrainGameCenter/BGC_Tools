using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class SystemRealTimeMidiEvent : MidiEvent
    {
        public enum SystemRealTimeType : byte
        {
            TimingClock = 0xF8,
            Undefined_01 = 0xF9,
            Start = 0xFA,
            Continue = 0xFB,
            Stop = 0xFC,
            Undefined_02 = 0xFD,
            ActiveSensing = 0xFE,
            SystemReset = 0xFF
        }

        public override string EventName => $"SysRT({systemRealTimeType})";
        public override MidiMessageClass MessageClass => MidiMessageClass.SysRTEvent;

        public SystemRealTimeType systemRealTimeType => (SystemRealTimeType)eventCode;

        public override int Length => base.Length;

        public SystemRealTimeMidiEvent(int deltaTime, byte eventCode)
            : base(deltaTime, eventCode)
        {
        }

        public static SystemRealTimeMidiEvent ParseSystemRealTimeEvent(
            int deltaTime,
            byte eventCode)
        {
            return new SystemRealTimeMidiEvent(
                deltaTime: deltaTime,
                eventCode: eventCode);
        }

        protected override void Serialize(Stream outputStream) { }

        public override string ToString() => base.ToString();

        public override void ExecuteEvent(MidiTrack track) { }
    }
}
