using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public abstract class MetaMidiEvent : MidiEvent
    {
        public enum MidiMetaType : byte
        {
            SequenceNumber = 0x00,
            Text = 0x01,
            CopyrightNotice = 0x02,
            SeqTrackName = 0x03,
            InstrumentName = 0x04,
            Lyric = 0x05,
            Marker = 0x06,
            CuePoint = 0x07,
            ChannelPrefix = 0x20,
            EndOfTrack = 0x2F,
            SetTempo = 0x51,
            SMTPEOffset = 0x54,
            TimeSignature = 0x58,
            KeySignature = 0x59,
            SequencerSpecific = 0x7F
        }

        public override MidiMessageClass MessageClass => MidiMessageClass.MetaEvent;
        public readonly byte metaTypeCode;

        public MidiMetaType MetaType => (MidiMetaType)metaTypeCode;

        public override int Length => base.Length + 1;

        public MetaMidiEvent(int deltaTime, byte metaTypeCode)
            : base(deltaTime, 0xFF)
        {
            this.metaTypeCode = metaTypeCode;
        }

        public static MetaMidiEvent ParseMetaEvent(Stream inputStream, int deltaTime, byte metaTypeCode)
        {
            MidiMetaType metaType = (MidiMetaType)metaTypeCode;

            switch (metaType)
            {
                case MidiMetaType.SequenceNumber:
                    return SequenceNumberMetaMidiEvent.ParseSequenceNumberMetaMidiEvent(inputStream, deltaTime);

                case MidiMetaType.Text:
                case MidiMetaType.CopyrightNotice:
                case MidiMetaType.SeqTrackName:
                case MidiMetaType.InstrumentName:
                case MidiMetaType.Lyric:
                case MidiMetaType.Marker:
                case MidiMetaType.CuePoint:
                    return TextMetaMidiEvent.ParseTextMetaMidiEvent(inputStream, deltaTime, metaTypeCode);

                case MidiMetaType.ChannelPrefix:
                    return ChannelPrefixMetaMidiEvent.ParseChannelPrefixMetaMidiEvent(inputStream, deltaTime);

                case MidiMetaType.EndOfTrack:
                    return EndOfTrackMetaMidiEvent.ParseEndOfTrackMetaMidiEvent(inputStream, deltaTime);

                case MidiMetaType.SetTempo:
                    return SetTempoMetaMidiEvent.ParseSetTempoMetaMidiEvent(inputStream, deltaTime);

                case MidiMetaType.SMTPEOffset:
                    return SMTPEOffsetMetaMidiEvent.ParseSMTPEOffsetMetaMidiEvent(inputStream, deltaTime);

                case MidiMetaType.TimeSignature:
                    return TimeSignatureMetaMidiEvent.ParseTimeSignatureMetaMidiEvent(inputStream, deltaTime);

                case MidiMetaType.KeySignature:
                    return KeySignatureMetaMidiEvent.ParseKeySignatureMetaMidiEvent(inputStream, deltaTime);

                case MidiMetaType.SequencerSpecific:
                    return SequencerSpecificMetaMidiEvent.ParseSequencerSpecificMetaMidiEvent(inputStream, deltaTime);

                default:
                    Debug.Log($"Encountered Unknown Meta Midi Event: {metaTypeCode:X2}");
                    return UnknownMetaMidiEvent.ParseUnknownMetaMidiEvent(inputStream, deltaTime, metaTypeCode);
            }
        }

        protected override void Serialize(Stream outputStream)
        {
            outputStream.WriteByte(metaTypeCode);
        }

        public override string ToString() => $"{base.ToString()} 0x{metaTypeCode:X2}";

        public abstract void Integrate(MidiTrack track);

        public override void ExecuteEvent(MidiTrack track) { }

    }
}
