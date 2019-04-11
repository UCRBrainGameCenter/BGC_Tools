using System;
using System.Collections.Generic;
using BGC.Audio.Midi.Events;

namespace BGC.Audio.Midi
{
    /// <summary>
    /// Renders an underlying MidiFile
    /// </summary>
    public class MidiFileStream : SynthStream
    {
        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;

        public override int ChannelSamples { get; }

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        private readonly MidiFile midiFile;
        private readonly Dictionary<byte, byte> programLookup = new Dictionary<byte, byte>();

        public MidiFileStream(MidiFile midiFile)
        {
            this.midiFile = midiFile;

            midiFile.tracks.ForEach(x => x.Initialize(this));

            ChannelSamples = midiFile.tracks[0].SampleEstimate();

            //Handle tempo mapping:

            midiFile.tracks[0].events.ForEach(x =>
            {
                if (x is SetTempoMetaMidiEvent tempoEvent)
                {
                    InsertTempoEvent(tempoEvent);
                }
            });
        }

        public void InsertTempoEvent(SetTempoMetaMidiEvent tempoEvent)
        {
            for (int i = 1; i < midiFile.tracks.Count; i++)
            {
                midiFile.tracks[i].Insert(new SetTempoMetaMidiEvent(0, tempoEvent.tempo), tempoEvent.time);
            }
        }

        public override IEnumerable<double> GetChannelRMS()
        {
            yield return Math.Sqrt(0.5);
        }

        protected override void _Initialize()
        {
        }

        public override int Read(float[] data, int offset, int count)
        {
            int minRemainingSamples = count;

            Array.Clear(data, offset, count);

            foreach (MidiTrack track in midiFile.tracks)
            {
                int trackRemainingSamples = count;
                int trackOffset = offset;

                while (trackRemainingSamples > 0)
                {
                    int maxRead = Math.Min(BUFFER_SIZE, trackRemainingSamples);
                    int trackReadSamples = track.Read(buffer, 0, maxRead);

                    if (trackReadSamples == 0)
                    {
                        //Done with this track
                        break;
                    }

                    for (int i = 0; i < trackReadSamples; i++)
                    {
                        data[trackOffset + i] += buffer[i];
                    }

                    trackOffset += trackReadSamples;
                    trackRemainingSamples -= trackReadSamples;
                }

                minRemainingSamples = Math.Min(minRemainingSamples, trackRemainingSamples);
            }

            return count - minRemainingSamples;

        }

        public override void Reset() => midiFile.tracks.ForEach(track => track.Reset());

        public override void Seek(int position) => midiFile.tracks.ForEach(track => track.Seek(position));

        public void ExecuteRunningEvent (ProgramMidiEvent programEvent)
        {
            if (programLookup.ContainsKey(programEvent.channel))
            {
                programLookup[programEvent.channel] = programEvent.value;
            }
            else
            {
                programLookup.Add(programEvent.channel, programEvent.value);
            }
        }

        public byte GetChannelProgram(byte channel)
        {
            if (programLookup.ContainsKey(channel))
            {
                return programLookup[channel];
            }

            return (byte)ReservedSoundSet.CrutchOrgan;
        }
    }
}
