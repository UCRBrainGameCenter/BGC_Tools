using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class TextMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"{MetaType}MetaEvent";
        public override bool Essential => false;
        public override int Length => base.Length + text.GetPacketLength();

        public readonly string text;

        public TextMetaMidiEvent(int deltaTime, byte metaTypeCode, string text)
            : base(deltaTime, metaTypeCode)
        {
            this.text = text;

            switch (MetaType)
            {
                case MidiMetaType.Text:
                case MidiMetaType.CopyrightNotice:
                case MidiMetaType.SeqTrackName:
                case MidiMetaType.InstrumentName:
                case MidiMetaType.Lyric:
                case MidiMetaType.Marker:
                case MidiMetaType.CuePoint:
                    //Do nothing
                    break;

                default:
                    Debug.LogError($"Encountered Unexpected Text Meta Midi Event: {metaTypeCode:X2}");
                    break;
            }
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WritePacket(text);
        }

        public static TextMetaMidiEvent ParseTextMetaMidiEvent(
            Stream inputStream,
            int deltaTime,
            byte metaTypeCode)
        {
            return new TextMetaMidiEvent(deltaTime, metaTypeCode, inputStream.ReadTextPacket());
        }

        public override string ToString() => $"{base.ToString()} (0x{text.Length.ToVarQuantityString()}) {text}";

        public override void Integrate(MidiTrack track)
        {
            switch (MetaType)
            {
                case MidiMetaType.SeqTrackName:
                    track.TrackName = text;
                    break;

                case MidiMetaType.InstrumentName:
                    track.InstrumentName = text;
                    break;

                case MidiMetaType.Text:
                case MidiMetaType.CopyrightNotice:
                case MidiMetaType.Lyric:
                case MidiMetaType.Marker:
                case MidiMetaType.CuePoint:
                    //Do nothing
                    break;
            }
        }
    }
}
