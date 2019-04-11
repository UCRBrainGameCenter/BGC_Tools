using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class SequenceNumberMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"SequenceNumberEvent";
        public override int Length => base.Length + 3;
        public readonly short sequenceNumber;

        public SequenceNumberMetaMidiEvent(int deltaTime, short sequenceNumber)
            : base(deltaTime, 0x00)
        {
            this.sequenceNumber = sequenceNumber;
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteNumberPacket(sequenceNumber, 2);
        }

        public static SequenceNumberMetaMidiEvent ParseSequenceNumberMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            return new SequenceNumberMetaMidiEvent(deltaTime, (short)inputStream.ReadNumberPacket());
        }

        public override string ToString() => $"{base.ToString()} (0x02) 0x{sequenceNumber:X4}";

        public override void Integrate(MidiTrack track)
        {
            Debug.Assert(time == 0);
            track.SequenceNumber = sequenceNumber;
        }
    }
}
