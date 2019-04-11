using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class SequencerSpecificMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"SequencerSpecificEvent";
        public override bool Essential => false;
        public override int Length => base.Length + data.GetPacketLength();

        public readonly byte[] data;

        public SequencerSpecificMetaMidiEvent(int deltaTime, byte[] data)
            : base(deltaTime, 0x7F)
        {
            this.data = data;
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WritePacket(data);
        }

        public static SequencerSpecificMetaMidiEvent ParseSequencerSpecificMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            return new SequencerSpecificMetaMidiEvent(deltaTime, inputStream.ReadDataPacket());
        }

        public override string ToString() => $"{base.ToString()} {data.PacketToString()}";

        public override void Integrate(MidiTrack track)
        {
            UnityEngine.Debug.Log($"Skipping Integration: {this}");
        }
    }
}
