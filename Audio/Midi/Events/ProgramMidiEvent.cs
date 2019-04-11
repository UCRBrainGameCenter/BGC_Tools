using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    /// <summary>
    /// Used to specify the type of instrument on the channel
    /// </summary>
    public class ProgramMidiEvent : ChannelMidiEvent
    {
        public override string EventName => $"ProgramEvent";
        public override ChannelEventType EventType => ChannelEventType.Program;
        public override int Length => base.Length + 1;

        public readonly byte channel;
        public readonly byte value;

        public ProgramMidiEvent(
            int deltaTime,
            byte channel,
            byte value)
            : base(deltaTime, (byte)(0xC0 | channel))
        {
            this.channel = channel;
            this.value = value;
        }

        public static ProgramMidiEvent ParseProgramMidiEvent(
            int deltaTime,
            byte typeCode,
            byte value)
        {
            return new ProgramMidiEvent(deltaTime, (byte)(typeCode & 0b1111), value);
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteByte(value);
        }

        public override string ToString() => $"{base.ToString()} 0x{value:X2}";

        public override void ExecuteEvent(MidiTrack track) => track.ExecuteRunningEvent(this);
    }
}
