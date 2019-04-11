using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class UnknownMidiEvent : MidiEvent
    {
        public override string EventName => $"UnknownEvent (0x{eventCode:X2})";
        public override bool Essential => false;

        public override MidiMessageClass MessageClass => MidiMessageClass.MAX;

        public readonly byte data;

        public UnknownMidiEvent(int deltaTime, byte eventCode, byte data) 
            : base(deltaTime, eventCode)
        {
            this.data = data;
        }

        public override string ToString() => $"{base.ToString()} 0x{data:X2}";

        public static UnknownMidiEvent ParseUnknownMidiEvent(
            int deltaTime,
            byte eventCode,
            byte nextByte)
        {
            Debug.Log($"Found Code 0x{eventCode:X2} event.  Skipping values: 0x{nextByte:X2}");
            return new UnknownMidiEvent(deltaTime, eventCode, nextByte);
        }

        protected override void Serialize(Stream outputStream)
        {
            outputStream.WriteByte(data);
        }

        public override void ExecuteEvent(MidiTrack track) { }
    }
}
