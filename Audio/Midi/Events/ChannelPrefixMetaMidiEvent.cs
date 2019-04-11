using System;
using System.IO;
using UnityEngine;

namespace BGC.Audio.Midi.Events
{
    public class ChannelPrefixMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => "ChannelPrefix";
        public override int Length => base.Length + 2;
        public readonly byte channel;

        public ChannelPrefixMetaMidiEvent(int deltaTime, byte channel)
            : base(deltaTime, 0x20)
        {
            this.channel = channel;

            Debug.Assert(channel <= 16);
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteNumberPacket(channel, 1);
        }

        public static ChannelPrefixMetaMidiEvent ParseChannelPrefixMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            return new ChannelPrefixMetaMidiEvent(deltaTime, (byte)inputStream.ReadNumberPacket());
        }

        public override string ToString() => $"{base.ToString()} (0x{1:X2}) 0x{channel:X2}";

        public override void Integrate(MidiTrack track)
        {
            //Do Nothing
        }
    }
}
