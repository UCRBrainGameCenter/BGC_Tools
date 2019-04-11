using System;
using System.IO;

namespace BGC.Audio.Midi.Events
{
    public class SetTempoMetaMidiEvent : MetaMidiEvent
    {
        public override string EventName => $"SetTempoEvent";
        public override int Length => base.Length + 4;

        public readonly int tempo;

        public SetTempoMetaMidiEvent(int deltaTime, int tempo)
            : base(deltaTime, 0x51)
        {
            this.tempo = tempo;
        }

        protected override void Serialize(Stream outputStream)
        {
            base.Serialize(outputStream);

            outputStream.WriteNumberPacket(tempo, 3);
        }

        public static SetTempoMetaMidiEvent ParseSetTempoMetaMidiEvent(
            Stream inputStream,
            int deltaTime)
        {
            return new SetTempoMetaMidiEvent(deltaTime, (int)inputStream.ReadNumberPacket());
        }

        public override string ToString() => $"{base.ToString()} (0x03) 0x{tempo:X6}";

        public override void Integrate(MidiTrack track)
        {
            if (time == 0)
            {
                track.Tempo = tempo;
            }
        }

        public override void ExecuteEvent(MidiTrack track)
        {
            track.SetTempo(tempo);
        }
    }
}
