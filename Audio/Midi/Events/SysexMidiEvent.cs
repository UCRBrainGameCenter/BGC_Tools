using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class SysexMidiEvent : MidiEvent
    {
        public enum SysexType : byte
        {
            Standard = 0xF0,
            Escape = 0xF7
        }

        public override string EventName => $"Sysex({sysexType})";
        public override MidiMessageClass MessageClass => MidiMessageClass.SysexEvent;

        public SysexType sysexType => (SysexType)eventCode;
        public readonly byte[] data;

        public override int Length => base.Length + data.GetPacketLength();

        public SysexMidiEvent(int deltaTime, byte eventCode, byte[] data)
            : base(deltaTime, eventCode)
        {
            this.data = data;

            switch (sysexType)
            {
                case SysexType.Standard:
                case SysexType.Escape:
                    //Do nothing
                    break;

                default:
                    Debug.LogError($"Unexpected messageType: {eventCode:X2}");
                    break;
            }
        }

        public static SysexMidiEvent ParseSysexEvent(
            Stream inputStream,
            int deltaTime,
            byte eventCode,
            byte nextByte)
        {
            return new SysexMidiEvent(
                deltaTime: deltaTime,
                eventCode: eventCode,
                data: inputStream.ReadDataPacket(nextByte));
        }

        protected override void Serialize(Stream outputStream)
        {
            outputStream.WritePacket(data);
        }

        public override void ExecuteEvent(MidiTrack track) { }

        public override string ToString() => $"{base.ToString()} {data.PacketToString()}";

    }
}
