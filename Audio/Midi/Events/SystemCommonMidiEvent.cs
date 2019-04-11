using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class SystemCommonMidiEvent : MidiEvent
    {
        public enum SystemCommonType : byte
        {
            TimeCodeMessage = 0xF1,
            SongPositionPointer = 0xF2,
            SongSelect = 0xF3,
            Undefined_01 = 0xF4,
            Undefined_02 = 0xF5,
            TuneRequest = 0xF6,
            EndOfSystemExclusive = 0xF7
        }

        public override string EventName => $"SysComm({systemCommonType})";
        public override MidiMessageClass MessageClass => MidiMessageClass.SysCommEvent;

        public SystemCommonType systemCommonType => (SystemCommonType)eventCode;
        public readonly byte dataA;
        public readonly byte dataB;

        public override int Length => base.Length + DataByteCount(systemCommonType);

        public SystemCommonMidiEvent(int deltaTime, byte eventCode, byte dataA = 0, byte dataB = 0)
            : base(deltaTime, eventCode)
        {
            this.dataA = dataA;
            this.dataB = dataB;
        }

        public static SystemCommonMidiEvent ParseSystemCommonEvent(
            Stream inputStream,
            int deltaTime,
            byte eventCode)
        {
            switch (DataByteCount((SystemCommonType)eventCode))
            {
                case 0:
                    return new SystemCommonMidiEvent(
                        deltaTime: deltaTime,
                        eventCode: eventCode);

                case 1:
                    return new SystemCommonMidiEvent(
                        deltaTime: deltaTime,
                        eventCode: eventCode,
                        dataA: (byte)inputStream.ReadByte());

                case 2:
                    return new SystemCommonMidiEvent(
                        deltaTime: deltaTime,
                        eventCode: eventCode,
                        dataA: (byte)inputStream.ReadByte(),
                        dataB: (byte)inputStream.ReadByte());

                default:
                    throw new System.Exception($"Unexpected DataByteCount: {DataByteCount((SystemCommonType)eventCode)}");
            }

        }

        protected override void Serialize(Stream outputStream)
        {
            switch (DataByteCount(systemCommonType))
            {
                case 0:
                    break;

                case 1:
                    outputStream.WriteByte(dataA);
                    break;

                case 2:
                    outputStream.WriteByte(dataA);
                    outputStream.WriteByte(dataB);
                    break;

                default:
                    throw new System.Exception($"Unexpected DataByteCount: {DataByteCount((SystemCommonType)eventCode)}");
            }
        }

        public override string ToString() => $"{base.ToString()}{DataString()}";

        private string DataString()
        {
            switch (DataByteCount(systemCommonType))
            {
                case 0: return "";
                case 1: return $" {dataA:X2}";
                case 2: return $" {dataA:X2}{dataB:X2}";
                default: goto case 0;
            }
        }


        private static int DataByteCount(SystemCommonType type)
        {
            switch (type)
            {
                case SystemCommonType.TimeCodeMessage:
                case SystemCommonType.SongSelect:
                    return 1;

                case SystemCommonType.SongPositionPointer:
                    return 2;

                case SystemCommonType.Undefined_01:
                case SystemCommonType.Undefined_02:
                case SystemCommonType.TuneRequest:
                case SystemCommonType.EndOfSystemExclusive:
                default:
                    return 0;
            }
        }

        public override void ExecuteEvent(MidiTrack track) { }
    }
}
