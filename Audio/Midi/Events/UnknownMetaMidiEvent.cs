using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class UnknownMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"UnknownMetaEvent (0x{metaTypeCode:X2})";
        public override bool Essential => false;
        public override int Length => base.Length + data.GetPacketLength();

        public readonly byte[] data;

        public UnknownMetaMidiEvent(int deltaTime, byte metaTypeCode, byte[] data)
            : base(deltaTime, metaTypeCode)
        {
            this.data = data;
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WritePacket(data);
        }

        public static UnknownMetaMidiEvent ParseUnknownMetaMidiEvent(
            Stream inputStream,
            int deltaTime,
            byte metaTypeCode)
        {
            return new UnknownMetaMidiEvent(deltaTime, metaTypeCode, inputStream.ReadDataPacket());
        }

        public override string ToString() => $"{base.ToString()} {data.PacketToString()}";

        public override void Integrate(MidiTrack track)
        {
            //Do Nothing
        }
    }
}
