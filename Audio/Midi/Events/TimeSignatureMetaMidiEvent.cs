using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class TimeSignatureMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => "TimeSignatureEvent";
        public override int Length => base.Length + 5;

        public int Numerator => data[0];
        public byte DenominatorPower => data[1];
        public int Denominator => 1 << data[1];
        /// <summary> MIDI Clocks per metronome tick </summary>
        public int ClockTicks => data[2];
        /// <summary> The number of 1/32 notes per 24 Midi clock ticks (8 is standard) </summary>
        public int NoteRate => data[3];

        private readonly byte[] data;

        public TimeSignatureMetaMidiEvent(
            int deltaTime,
            byte[] signature)
            : base(deltaTime, 0x58)
        {
            Debug.Assert(signature != null && signature.Length == 4);
            data = signature;
        }

        public TimeSignatureMetaMidiEvent(
            int deltaTime,
            byte numerator,
            byte denominatorPower,
            byte clockTicks,
            byte noteRate)
            : base(deltaTime, 0x58)
        {
            data = new byte[]
            {
                numerator,
                denominatorPower,
                clockTicks,
                noteRate
            };
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WritePacket(data);
        }

        public static TimeSignatureMetaMidiEvent ParseTimeSignatureMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            return new TimeSignatureMetaMidiEvent(deltaTime, inputStream.ReadDataPacket());
        }

        public override string ToString() => $"{base.ToString()} {data.PacketToString()}";

        public override void Integrate(MidiTrack track)
        {
            if (time == 0)
            {
                track.SignatureNumerator = Numerator;
                track.SignatureDenominator = Denominator;
                track.SignatureClockTicks = ClockTicks;
                track.SignatureNoteRate = NoteRate;
            }
        }
    }
}
