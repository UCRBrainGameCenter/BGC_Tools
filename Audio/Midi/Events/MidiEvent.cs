using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public abstract class MidiEvent
    {
        public enum MidiMessageClass
        {
            ChannelEvent = 0,
            SysexEvent,
            SysCommEvent,
            SysRTEvent,
            MetaEvent,
            MAX
        }

        public int time;
        public int deltaTime;
        public readonly byte eventCode;

        public abstract string EventName { get; }
        public virtual bool Essential => true;

        public abstract MidiMessageClass MessageClass { get; }
        public virtual int Length => deltaTime.GetVarQuantitySize();

        public MidiEvent(int deltaTime, byte eventCode)
        {
            this.deltaTime = deltaTime;
            this.eventCode = eventCode;
        }

        public static MidiEvent ParseEvent(
            Stream inputStream,
            int deltaTime,
            byte eventCode,
            byte nextByte)
        {
            switch (eventCode)
            {
                case 0xF0:
                case 0xF7:
                    return SysexMidiEvent.ParseSysexEvent(
                        inputStream: inputStream,
                        deltaTime: deltaTime,
                        eventCode: eventCode,
                        nextByte: nextByte);

                case 0xFF:
                    return MetaMidiEvent.ParseMetaEvent(
                        inputStream: inputStream,
                        deltaTime: deltaTime,
                        metaTypeCode: nextByte);
            }

            //Check Channel Banks
            //Upper Nibble anywhere between 0x80 and 0xEF
            if (eventCode >= 0x80 && eventCode < 0xF0)
            {
                return ChannelMidiEvent.ParseChannelEvent(
                    inputStream: inputStream,
                    deltaTime: deltaTime,
                    eventCode: eventCode,
                    nextByte: nextByte);
            }


            return UnknownMidiEvent.ParseUnknownMidiEvent(
                deltaTime: deltaTime,
                eventCode: eventCode,
                nextByte: nextByte);
        }

        public void Serialize(Stream outputStream, bool running)
        {
            outputStream.WriteVarQuantity(deltaTime);

            if (!running)
            {
                outputStream.WriteByte(eventCode);
            }

            Serialize(outputStream);
        }

        protected abstract void Serialize(Stream outputStream);
        public abstract void ExecuteEvent(MidiTrack track);

        private const int VAR_1BYTE_MAX = 0b0111_1111;
        private const int VAR_2BYTE_MAX = (VAR_1BYTE_MAX << 7) | VAR_1BYTE_MAX;
        private const int VAR_3BYTE_MAX = (VAR_2BYTE_MAX << 7) | VAR_1BYTE_MAX;
        private const int VAR_4BYTE_MAX = (VAR_3BYTE_MAX << 7) | VAR_1BYTE_MAX;

        public override string ToString() => $"{EventName,-25} 0x{deltaTime.ToVarQuantityString(),-8} 0x{eventCode:X2}";
    }
}
