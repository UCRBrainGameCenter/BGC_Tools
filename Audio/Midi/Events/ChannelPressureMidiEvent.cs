using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class ChannelPressureMidiEvent : ChannelMidiEvent
    {
        public override string EventName => "ChannelPressure";
        public override ChannelEventType EventType => ChannelEventType.ChannelPressure;
        public override int Length => base.Length + 1;

        public readonly byte channel;
        public readonly byte value;

        public ChannelPressureMidiEvent(
            int deltaTime,
            byte channel,
            byte value)
            : base(deltaTime, (byte)(0xD0 | channel))
        {
            this.channel = channel;
            this.value = value;
        }

        public static ChannelPressureMidiEvent ParseChannelPressureMidiEvent(
            int deltaTime,
            byte typeCode,
            byte value)
        {
            return new ChannelPressureMidiEvent(deltaTime, (byte)(typeCode & 0b1111), value);
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
