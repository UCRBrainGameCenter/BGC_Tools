using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public abstract class ChannelMidiEvent : MidiEvent
    {
        public enum ChannelEventType
        {
            Note = 0,
            Controller,
            Program,
            ChannelPressure,
            PitchBend,
            Mode
        }

        public abstract ChannelEventType EventType { get; }
        public override MidiMessageClass MessageClass => MidiMessageClass.ChannelEvent;

        public ChannelMidiEvent(int deltaTime, byte eventCode)
            : base(deltaTime, eventCode)
        {

        }

        public static ChannelMidiEvent ParseChannelEvent(
            Stream inputStream,
            int deltaTime,
            byte eventCode,
            byte nextByte)
        {
            switch (eventCode & 0b1111_0000)
            {
                case 0x80:
                case 0x90:
                case 0xA0:
                    return NoteMidiEvent.ParseNoteMidiEvent(deltaTime, eventCode, nextByte, (byte)inputStream.ReadByte());

                case 0xB0:
                    return ControllerMidiEvent.ParseControllerMidiEvent(deltaTime, eventCode, nextByte, (byte)inputStream.ReadByte());

                case 0xC0:
                    return ProgramMidiEvent.ParseProgramMidiEvent(deltaTime, eventCode, nextByte);

                case 0xD0:
                    return ChannelPressureMidiEvent.ParseChannelPressureMidiEvent(deltaTime, eventCode, nextByte);

                case 0xE0:
                    return PitchBendMidiEvent.ParsePitchBendMidiEvent(inputStream, deltaTime, eventCode, nextByte);

                default:
                    throw new MidiParsingException($"Encountered Unknown Channel Midi Event: {eventCode:X2}");
            }
        }

        protected override void Serialize(Stream outputStream) { }
    }
}
