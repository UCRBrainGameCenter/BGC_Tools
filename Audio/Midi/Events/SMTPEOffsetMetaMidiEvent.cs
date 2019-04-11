using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class SMTPEOffsetMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"SMTPEOffsetEvent";
        public override int Length => base.Length + 6;

        public byte Hours => data[0];
        public byte Minutes => data[1];
        public byte Seconds => data[2];
        public byte Frames => data[3];
        public byte Fractions => data[4];

        private readonly byte[] data;

        public SMTPEOffsetMetaMidiEvent(
            int deltaTime,
            byte[] time)
            : base(deltaTime, 0x54)
        {
            Debug.Assert(time != null && time.Length == 5);
            data = time;
        }

        public SMTPEOffsetMetaMidiEvent(
            int deltaTime,
            byte hours,
            byte minutes,
            byte seconds,
            byte frames,
            byte fractions)
            : base(deltaTime, 0x54)
        {
            data = new byte[]
            {
                hours,
                minutes,
                seconds,
                frames,
                fractions
            };
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WritePacket(data);
        }

        public static SMTPEOffsetMetaMidiEvent ParseSMTPEOffsetMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            return new SMTPEOffsetMetaMidiEvent(deltaTime, inputStream.ReadDataPacket());
        }

        public override string ToString() => $"{base.ToString()} {data.PacketToString()}";

        public override void Integrate(MidiTrack track)
        {
            Debug.Assert(time == 0);
            throw new System.NotImplementedException();
        }
    }
}
