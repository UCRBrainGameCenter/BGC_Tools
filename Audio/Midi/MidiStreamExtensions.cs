using System;
using System.Text;
using System.Linq;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi
{
    public static class MidiStreamExtensions
    {
        public static int ReadVarQuantity(this Stream inputStream)
        {
            int value = 0;
            int bytes;
            int maxRead = (int)Math.Min(4, inputStream.Length - inputStream.Position);

            for (bytes = 0; bytes < maxRead; bytes++)
            {
                byte read = (byte)inputStream.ReadByte();
                value |= read & 0b0111_1111;
                if ((read & 0b1000_0000) == 0)
                {
                    break;
                }
                else
                {
                    value <<= 7;
                }
            }

            if (bytes == maxRead)
            {
                if (bytes == 4)
                {
                    throw new MidiParsingException("Parsing Error!  Variable quantity consumed more than 4 bytes");
                }

                throw new MidiParsingException("Hit the end of the track while reading VariableLengthQuantity!");
            }

            return value;
        }

        public static int ReadVarQuantity(this Stream inputStream, byte firstByte)
        {
            int value = 0;
            int bytes;
            int maxRead = (int)Math.Min(4, inputStream.Length - inputStream.Position);

            for (bytes = 0; bytes < maxRead; bytes++)
            {
                byte read;
                if (bytes == 0)
                {
                    read = firstByte;
                }
                else
                {
                    read = (byte)inputStream.ReadByte();
                }

                value |= read & 0b0111_1111;
                if ((read & 0b1000_0000) == 0)
                {
                    break;
                }
                else
                {
                    value <<= 7;
                }
            }

            if (bytes == maxRead)
            {
                if (bytes == 4)
                {
                    throw new MidiParsingException("Parsing Error!  Variable quantity consumed more than 4 bytes");
                }

                throw new MidiParsingException("Hit the end of the track while reading VariableLengthQuantity!");
            }

            return value;
        }

        public static void WriteVarQuantity(this Stream outputStream, int value)
        {
            do
            {
                byte val = (byte)(value & 0b0111_1111);
                value >>= 7;
                if (value > 0)
                {
                    //Still going... turn on the ContinueReading bit (8)
                    val |= 0b1000_0000;
                }
                outputStream.WriteByte(val);
            }
            while (value > 0);
        }


        public static byte[] ReadDataPacket(this Stream inputStream, int length)
        {
            Debug.Assert(inputStream.Length >= inputStream.Position + length);

            byte[] dataPacket = new byte[length];
            inputStream.Read(dataPacket, 0, length);

            return dataPacket;
        }

        public static byte[] ReadDataPacket(this Stream inputStream)
        {
            int length = inputStream.ReadVarQuantity();

            Debug.Assert(inputStream.Length >= inputStream.Position + length);

            byte[] dataPacket = new byte[length];
            inputStream.Read(dataPacket, 0, length);

            return dataPacket;
        }

        public static byte[] ReadDataPacket(this Stream inputStream, byte firstByte)
        {
            int length = inputStream.ReadVarQuantity(firstByte);

            Debug.Assert(inputStream.Length >= inputStream.Position + length);

            byte[] dataPacket = new byte[length];
            inputStream.Read(dataPacket, 0, length);

            return dataPacket;
        }

        public static long ReadNumberPacket(this Stream inputStream)
        {
            int length = inputStream.ReadByte();

            long value = 0;

            for (int i = 0; i < length; i++)
            {
                value <<= 8;
                value += inputStream.ReadByte();
            }

            return value;
        }

        public static void WriteNumberPacket(this Stream outputStream, long value, byte length)
        {
            Debug.Assert(length <= 4);

            outputStream.WriteByte(length);

            for (int i = length - 1; i >= 0; i--)
            {
                outputStream.WriteByte((byte)((value >> 8 * i) & 0b1111_1111));
            }
        }

        public static byte[] ToVarQuantity(this int value)
        {
            int count = value.GetVarQuantitySize();
            byte[] output = new byte[count];

            for (int i = 0; i < count; i++)
            {
                output[i] = (byte)((value >> (7 * (count - i - 1))) & 0b0111_1111);
                if (i != count - 1)
                {
                    //Still going... turn on the ContinueReading bit (8)
                    output[i] |= 0b1000_0000;
                }
            }

            return output;
        }

        public static string ToVarQuantityString(this int value)
        {
            int count = value.GetVarQuantitySize();
            string output = "";

            for (int i = 0; i < count; i++)
            {
                byte writeNum = (byte)((value >> (7 * (count - i - 1))) & 0b0111_1111);
                if (i != count - 1)
                {
                    //Still going... turn on the ContinueReading bit (8)
                    writeNum |= 0b1000_0000;
                }
                output += writeNum.ToString("X2");
            }

            return output;
        }

        public static short ReadInt16(this Stream inputStream)
        {
            int value = (inputStream.ReadByte() << 8);
            value += inputStream.ReadByte();

            return (short)value;
        }

        public static int ReadInt32(this Stream inputStream)
        {
            int value = inputStream.ReadByte() << 24;
            value += inputStream.ReadByte() << 16;
            value += inputStream.ReadByte() << 8;
            value += inputStream.ReadByte();

            return value;
        }

        public static string ReadTextPacket(this Stream inputStream)
        {
            byte[] dataPacket = inputStream.ReadDataPacket();
            return Encoding.UTF8.GetString(dataPacket, 0, dataPacket.Length);
        }

        public static void WritePacket(this Stream outputStream, byte[] packet)
        {
            outputStream.WriteVarQuantity(packet.Length);
            outputStream.Write(packet, 0, packet.Length);
        }

        public static string PacketToString(this byte[] packet)
        {
            string length = packet.Length.ToVarQuantityString();
            string packetData = string.Join("", packet.Select(x => x.ToString("X2")));

            return $"(0x{length}) 0x{packetData}";
        }

        public static void WritePacket(this Stream outputStream, string text)
        {
            outputStream.WriteVarQuantity(text.Length);
            outputStream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
        }

        public static int GetPacketLength(this byte[] packet) =>
            packet.Length.GetVarQuantitySize() + packet.Length;

        public static int GetPacketLength(this string text) =>
            text.Length.GetVarQuantitySize() + text.Length;

        private const int VAR_1BYTE_CEILING = 1 << 7;
        private const int VAR_2BYTE_CEILING = 1 << 14;
        private const int VAR_3BYTE_CEILING = 1 << 21;

        //Comparing to constant values rather than calculating so this common operation is fast
        public static int GetVarQuantitySize(this int value)
        {
            //I expect most VarQuantities are 1 Byte, so lets just linearly test instead of
            //a binary search
            if (value < VAR_1BYTE_CEILING)
            {
                return 1;
            }

            if (value < VAR_2BYTE_CEILING)
            {
                return 2;
            }

            if (value < VAR_3BYTE_CEILING)
            {
                return 3;
            }

            return 4;
        }
    }
}
