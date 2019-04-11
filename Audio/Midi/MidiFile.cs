using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BGC.IO;
using BGC.IO.Extensions;

using HeaderInfo = BGC.Audio.Midi.MidiEncoding.HeaderInfo;
using Format = BGC.Audio.Midi.MidiEncoding.Format;

namespace BGC.Audio.Midi
{
    public class MidiFile
    {
        public const string HEADER_CHUNK_NAME = "MThd";
        public readonly List<MidiTrack> tracks = new List<MidiTrack>();

        private readonly bool retainAll;
        public readonly HeaderInfo headerInfo;

        public MidiFile(
            in HeaderInfo headerInfo,
            bool retainAll = false)
        {
            this.headerInfo = headerInfo;
            this.retainAll = retainAll;
        }

        /// <summary> Deserialization Constructor </summary>
        public MidiFile(
            Stream headerStream,
            bool retainAll = false)
        {
            headerInfo = new HeaderInfo(headerStream.ReadRemainder());
            this.retainAll = retainAll;

            switch (headerInfo.format)
            {
                case Format.SingleTrack:
                case Format.TempoMapped:
                case Format.SequenceContainer:
                    //Do nothing - this is fine
                    break;

                case Format.UNDEFINED:
                default:
                    throw new MidiParsingException($"Unexpected Format: {headerInfo.format}");
            }
        }

        public void Add(MidiTrack track)
        {
            tracks.Add(track);
        }

        public void AddRange(IEnumerable<MidiTrack> tracks)
        {
            foreach (MidiTrack track in tracks)
            {
                Add(track);
            }
        }

        public void ReadTrack(Stream trackStream) =>
            Add(new MidiTrack(
                headerInfo: headerInfo,
                sequenceNumber: (short)tracks.Count,
                trackStream: trackStream,
                retainAll: retainAll));

        public void Serialize(Stream outputStream)
        {
            byte[] header = headerInfo.Serialize();

            outputStream.Write(Encoding.UTF8.GetBytes(HEADER_CHUNK_NAME), 0, 4);
            outputStream.WriteByte((byte)(header.Length >> 24));
            outputStream.WriteByte((byte)((header.Length >> 16) & 0b1111_1111));
            outputStream.WriteByte((byte)((header.Length >> 8) & 0b1111_1111));
            outputStream.WriteByte((byte)(header.Length & 0b1111_1111));
            outputStream.Write(header, 0, header.Length);

            tracks.ForEach(track => track.Serialize(outputStream));
        }
    }
}
