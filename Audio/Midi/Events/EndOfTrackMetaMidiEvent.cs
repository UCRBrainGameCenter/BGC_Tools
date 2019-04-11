using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class EndOfTrackMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"EndOfTrackEvent";
        public override int Length => base.Length + 1;

        public EndOfTrackMetaMidiEvent(int deltaTime)
            : base(deltaTime, 0x2F)
        {

        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteByte(0x00);
        }

        public static EndOfTrackMetaMidiEvent ParseEndOfTrackMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            //Skip length
            inputStream.ReadByte();
            return new EndOfTrackMetaMidiEvent(deltaTime);
        }

        public override string ToString() => $"{base.ToString()} (0x00)";

        public override void Integrate(MidiTrack track)
        {
            track.Length = time;
        }
    }
}
