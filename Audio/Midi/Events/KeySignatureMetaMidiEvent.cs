using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class KeySignatureMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"KeySignatureEvent";
        public override int Length => base.Length + 3;

        public sbyte SharpFlatCount => (sbyte)data[0];
        public bool MajorKey => data[1] == 0x00;

        private readonly byte[] data;

        public KeySignatureMetaMidiEvent(
            int deltaTime,
            byte[] signature)
            : base(deltaTime, 0x59)
        {
            Debug.Assert(signature != null && signature.Length == 2);
            data = signature;
        }

        public KeySignatureMetaMidiEvent(
            int deltaTime,
            sbyte sharpFlatCount,
            bool majorKey)
            : base(deltaTime, 0x59)
        {
            data = new byte[]
            {
                (byte)sharpFlatCount,
                (majorKey ? (byte)0x00 : (byte)0x01)
            };
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WritePacket(data);
        }

        public static KeySignatureMetaMidiEvent ParseKeySignatureMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            return new KeySignatureMetaMidiEvent(deltaTime, inputStream.ReadDataPacket());
        }

        public override string ToString() => $"{base.ToString()} {data.PacketToString()}";

        public override void Integrate(MidiTrack track)
        {
            if (time == 0)
            {
                track.SharpFlatCount = SharpFlatCount;
                track.MajorKey = MajorKey;
            }
        }
    }
}
