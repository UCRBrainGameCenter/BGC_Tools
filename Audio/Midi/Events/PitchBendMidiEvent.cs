using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class PitchBendMidiEvent : ChannelMidiEvent
    {
        public override string EventName => $"PitchBendEvent";
        public override ChannelEventType EventType => ChannelEventType.PitchBend;
        public override int Length => base.Length + 2;

        public readonly byte channel;
        public readonly short value;

        public PitchBendMidiEvent(
            int deltaTime,
            byte channel,
            short value)
            : base(deltaTime, (byte)(0xE0 | channel))
        {
            this.channel = channel;
            this.value = value;
        }

        public static PitchBendMidiEvent ParsePitchBendMidiEvent(
            Stream inputStream,
            int deltaTime,
            byte typeCode,
            byte nextByte)
        {
            short value = (short)(nextByte | inputStream.ReadByte() << 8);
            return new PitchBendMidiEvent(deltaTime, (byte)(typeCode & 0b1111), value);
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteByte((byte)(value & 0b1111_1111));
            outputStream.WriteByte((byte)(value >> 8));
        }

        public override string ToString() => $"{base.ToString()} 0x{value:X4}";

        public override void ExecuteEvent(MidiTrack track) => track.ExecuteRunningEvent(this);

    }
}
