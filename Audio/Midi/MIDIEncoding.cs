using System;
using System.IO;
using System.Text;
using UnityEngine;
using BGC.IO;

namespace BGC.Audio.Midi
{
    //Some documentation on the MIDI format is available here:
    // https://www.csie.ntu.edu.tw/~r92092/ref/midi/
    //Full documentation is here:
    // https://www.midi.org/specifications-old/item/the-midi-1-0-specification
    public static class MidiEncoding
    {
        public enum Format : ushort
        {
            SingleTrack = 0x0000,
            TempoMapped = 0x0001,
            SequenceContainer = 0x0002,
            UNDEFINED = 0xFFFF
        }

        public readonly struct HeaderInfo
        {
            public readonly Format format;
            public readonly int tracks;
            public readonly int ticksPerQuarter;
            public readonly int ticksPerFrame;
            public readonly int framesPerSecond;

            public HeaderInfo(byte[] buffer)
            {
                format = (Format)((buffer[0] << 8) | buffer[1]);
                tracks = (buffer[2] << 8) | buffer[3];

                if (buffer[4] >> 7 == 0)
                {
                    //Time Units Per QuarterNote format
                    ticksPerQuarter = (buffer[4] << 8) | buffer[5];
                    ticksPerFrame = -1;
                    framesPerSecond = -1;
                }
                else
                {
                    //Time Units Per Frame format
                    ticksPerQuarter = -1;
                    framesPerSecond = -1 * (sbyte)buffer[4];
                    ticksPerFrame = buffer[5];
                }
            }

            public byte[] Serialize()
            {
                byte[] output = new byte[6];
                output[0] = 0x00;
                output[1] = (byte)format;
                output[2] = (byte)(tracks >> 8);
                output[3] = (byte)(tracks & 0b1111_1111);
                if (ticksPerQuarter == -1)
                {
                    output[4] = (byte)(-framesPerSecond & 0b1111_1111);
                    output[5] = (byte)ticksPerFrame;
                }
                else
                {
                    output[4] = (byte)(ticksPerQuarter >> 8);
                    output[5] = (byte)(ticksPerQuarter & 0b1111_1111);
                }

                return output;
            }
        }

        /// <summary>
        /// Method to load a MIDI file an parse the data
        /// </summary>
        public static bool LoadFile(
            string filePath,
            out MidiFile midiFile,
            bool retainAll = false)
        {
            midiFile = null;

            if (!File.Exists(filePath))
            {
                Debug.LogError($"File {filePath} does not exist.");
                return false;
            }

            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    //Now Read Chunks
                    //Each chunk starts with an 8 byte header, so we probably reached padding if
                    //there is less than that remaining
                    while (fileStream.CanRead && (fileStream.Length - fileStream.Position) >= 8)
                    {
                        byte[] smallBuffer = new byte[8];
                        fileStream.Read(smallBuffer, 0, 8);
                        string chunkID = Encoding.UTF8.GetString(smallBuffer, 0, 4).Trim();
                        int chunkSize = 
                            smallBuffer[4] << 24 |
                            smallBuffer[5] << 16 |
                            smallBuffer[6] << 8 |
                            smallBuffer[7];

                        using (Stream chunkStream = new SubStream(fileStream, chunkSize, ownsStream: false))
                        {
                            switch (chunkID)
                            {
                                case MidiFile.HEADER_CHUNK_NAME:
                                    midiFile = new MidiFile(chunkStream, retainAll);
                                    break;

                                case MidiTrack.TRACK_CHUNK_NAME:
                                    if (midiFile == null)
                                    {
                                        throw new MidiParsingException($"\"{MidiFile.HEADER_CHUNK_NAME}\" chunk not found before \"{MidiTrack.TRACK_CHUNK_NAME}\".");
                                    }
                                    midiFile.ReadTrack(chunkStream);
                                    break;

                                default:
                                    Debug.Log($"Skipping unexpected Chunk in File {filePath}: {chunkID}.");
                                    //Do nothing with it
                                    break;
                            }
                        }
                    }

                    if (midiFile == null)
                    {
                        throw new MidiParsingException($"Finished parsing file without locating Header or Track chunks");
                    }

                    if (midiFile.tracks.Count == 0)
                    {
                        midiFile = null;
                        throw new MidiParsingException($"Finished parsing file without locating Track chunks");
                    }
                }
            }
            catch (MidiParsingException excp)
            {
                Debug.LogException(new MidiParsingException($"Error parsing Midi file \"{filePath}\"", excp));
                midiFile = null;
                return false;
            }

            return midiFile != null;
        }


        public static bool SaveFile(
            string filePath,
            MidiFile midiFile,
            bool overwrite = false)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    if (overwrite)
                    {
                        File.Delete(filePath);
                    }
                    else
                    {
                        filePath = DataManagement.NextAvailableFilePath(filePath);
                    }
                }

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    midiFile.Serialize(fileStream);
                }

                return true;
            }
            catch (IOException excp)
            {
                Debug.LogException(excp);
                return false;
            }
        }
    }
}
