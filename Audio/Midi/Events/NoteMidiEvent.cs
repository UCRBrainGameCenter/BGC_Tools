using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class NoteMidiEvent : ChannelMidiEvent
    {
        public enum NoteEventType : byte
        {
            NoteOff = 0x80,
            NoteOn = 0x90,
            Pressure = 0xA0
        }

        public override string EventName => $"Note({noteEventType})";

        public override ChannelEventType EventType => ChannelEventType.Note;

        public override int Length => base.Length + 2;

        public readonly NoteEventType noteEventType;
        public readonly byte channel;
        public readonly byte note;
        public readonly byte param;

        public NoteMidiEvent(
            int deltaTime,
            NoteEventType noteEventType,
            byte channel,
            byte note,
            byte param)
            : base(deltaTime, GetCode(noteEventType, channel))
        {
            this.noteEventType = noteEventType;
            this.channel = channel;
            this.note = note;
            this.param = param;
        }

        public NoteMidiEvent(
            int deltaTime,
            byte typeCode,
            byte note,
            byte param)
            : base(deltaTime, typeCode)
        {
            noteEventType = (NoteEventType)(typeCode & 0b1111_0000);
            channel = (byte)(typeCode & 0b1111);
            this.note = note;
            this.param = param;
        }


        public static NoteMidiEvent ParseNoteMidiEvent(
            int deltaTime,
            byte typeCode,
            byte note,
            byte param)
        {
            return new NoteMidiEvent(deltaTime, typeCode, note, param);
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteByte(note);
            outputStream.WriteByte(param);
        }

        private static byte GetCode(NoteEventType noteEventType, byte channel) =>
            (byte)((byte)noteEventType | channel);

        public override string ToString() => $"{base.ToString()} 0x{note:X2} 0x{param:X2}";

        public override void ExecuteEvent(MidiTrack track) => track.ExecuteRunningEvent(this);
    }
}
