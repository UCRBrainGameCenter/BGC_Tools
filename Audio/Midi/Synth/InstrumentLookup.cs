using System;
using BGC.Audio.Synthesis;
using BGC.Audio.AnalyticStreams;
using BGC.Audio.Midi.Events;
using BGC.Audio.Filters;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Midi.Synth
{
    public enum Key
    {
        C = 0,
        CSharp,
        D,
        DSharp,
        E,
        F,
        FSharp,
        G,
        GSharp,
        A,
        ASharp,
        B,
        MAX
    }

    public static class InstrumentLookup
    {
        public static IBGCStream GetNote(NoteMidiEvent noteEvent, byte set)
        {
            switch ((ReservedChannels)noteEvent.channel)
            {
                case ReservedChannels.Percussion:
                    return GetPercussion(
                        (PercussionMap)noteEvent.note,
                        noteEvent.param);

                default:
                    return GetNote(
                        set: (ReservedSoundSet)set,
                        frequency: GetNoteFrequency(noteEvent.note),
                        velocity: noteEvent.param);
            }
        }

        public static IBGCStream GetNote(
            ReservedSoundSet set,
            double frequency,
            byte velocity)
        {
            double amplitude = GetNoteAmplitude(velocity);

            switch (set)
            {
                case ReservedSoundSet.AcousticGrandPiano:
                case ReservedSoundSet.BrightAcousticPiano:
                case ReservedSoundSet.ElectricGrandPiano:
                case ReservedSoundSet.HonkyTonkPiano:
                case ReservedSoundSet.Harpsichord:
                case ReservedSoundSet.Clavi:
                    return new AnalyticStreamAdder(
                        new AnalyticWave(amplitude, frequency),
                        new AnalyticWave(0.5 * amplitude, 3 * frequency),
                        new AnalyticWave(0.25 * amplitude, 5 * frequency),
                        new AnalyticWave(0.125 * amplitude, 7 * frequency))
                        .FrequencyModulation(15, 2.5)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.25, 0.8, 1.0, 0.05);

                case ReservedSoundSet.Xylophone:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(frequency / 2, 20)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.05, 1, 0.125, 0.05);

                case ReservedSoundSet.ElectricPiano1:
                case ReservedSoundSet.ElectricPiano2:
                    return new SquareWave(amplitude, frequency, 0.66)
                        .ADSR(0.0125, 0.25, 0.8, 1, 0.05);

                case ReservedSoundSet.DrawbarOrgan:
                case ReservedSoundSet.PercussiveOrgan:
                case ReservedSoundSet.RockOrgan:
                case ReservedSoundSet.CrutchOrgan:
                case ReservedSoundSet.ReedOrgan:
                case ReservedSoundSet.Accordion:
                case ReservedSoundSet.Harmonica:
                case ReservedSoundSet.TangoAccordion:
                    amplitude /= 5.0;
                    return new StreamAdder(
                        new TriangleWave(0.1 * amplitude, 0.5 * frequency),
                        new TriangleWave(amplitude, frequency),
                        new TriangleWave(amplitude, 1.5 * frequency),
                        new TriangleWave(amplitude, 2 * frequency),
                        new TriangleWave(amplitude, 4 * frequency),
                        new TriangleWave(amplitude, 8 * frequency))
                        .ADSR(0.0125, 0.25, 0.8, 100, 0.25);

                case ReservedSoundSet.AcousticGuitar_Nylon:
                case ReservedSoundSet.AcousticGuitar_Steel:
                case ReservedSoundSet.ElectricGuitar_Jazz:
                case ReservedSoundSet.ElectricGuitar_Clean:
                case ReservedSoundSet.ElectricGuitar_Muted:
                case ReservedSoundSet.OverdrivenGuitar:
                case ReservedSoundSet.DistortionGuitar:
                case ReservedSoundSet.GuitarHarmonics:
                case ReservedSoundSet.AcousticBass:
                case ReservedSoundSet.ElectricBass_Finger:
                case ReservedSoundSet.ElectricBasS_Pick:
                case ReservedSoundSet.FretlessBass:
                case ReservedSoundSet.SlapBass1:
                case ReservedSoundSet.SlapBass2:
                case ReservedSoundSet.SynthBass1:
                case ReservedSoundSet.SynthBass2:
                    return new SquareWave(amplitude, frequency, 0.33)
                        .ContinuousFilter(
                            envelopeStream: new LinearEnvelope(0.25, 1.5),
                            filterType: ContinuousFilter.FilterType.LowPass,
                            freqLB: 20 * frequency,
                            freqUB: 3 * frequency)
                        .ADSR(0.0125, 0.33, 0.2, 2.0, 0.05);


                case ReservedSoundSet.Piccolo:
                case ReservedSoundSet.Flute:
                case ReservedSoundSet.Recorder:
                case ReservedSoundSet.PanFlute:
                case ReservedSoundSet.BlownBottle:
                case ReservedSoundSet.Shakuhachi:
                case ReservedSoundSet.Whistle:
                case ReservedSoundSet.Ocarina:
                    return new StreamAdder(
                            new SquareWave(0.75 * amplitude, frequency, 0.6),
                            new AnalyticNoiseStream(0.0125, 20.0, 10000.0, 1000, AnalyticNoiseStream.AmplitudeDistribution.White)
                                .ToBGCStream())
                        .ContinuousFilter(
                            envelopeStream: new SineWave(1.0, 6.0),
                            filterType: ContinuousFilter.FilterType.LowPass,
                            freqLB: 2 * frequency,
                            freqUB: (2.125 + 0.125 * amplitude) * frequency,
                            qFactor: 0.0125)
                        .BiQuadHighpassFilter(200f)
                        .ADSR(0.125, 0.33, 0.8, 100.0, 0.125);


                case ReservedSoundSet.Lead1_Square:
                    return new SquareWave(amplitude, frequency, 0.5)
                        .ADSR(0.0125, 0.25, 0.8, 1, 0.05);

                case ReservedSoundSet.Lead2_Sawtooth:
                    return new SawtoothWave(amplitude, frequency)
                        .ADSR(0.0125, 0.25, 0.8, 1, 0.05);

                default:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(frequency / 3, 10)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.25, 0.8, 1, 0.05);
            }

        }

        public static IBGCStream GetNote(
            ReservedSoundSet set,
            byte note,
            byte velocity)
        {
            return GetNote(set, GetNoteFrequency(note), velocity);
        }

        public static IBGCStream GetPercussion(PercussionMap percussion, byte velocity)
        {
            double frequency = GetNoteFrequency((byte)percussion);
            double amplitude = GetNoteAmplitude(velocity);

            switch (percussion)
            {
                case PercussionMap.AcousticBassDrum:
                case PercussionMap.BassDrum1:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(30.0, 30.0)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.0125, 0.1, 1, 0.125, 0.05);

                case PercussionMap.AcousticSnare:
                case PercussionMap.ElectricSnare:
                    return new AnalyticStreamAdder(
                        new AnalyticWave(amplitude, frequency)
                            .FrequencyModulation(30.0, 30.0)
                            .ADSR(0.0125, 0.0125, 1, 0.05),
                        new AnalyticNoiseStream(
                                rms: amplitude * Math.Sqrt(0.25),
                                freqLB: frequency,
                                freqUB: 4 * frequency,
                                frequencyCount: 1000,
                                distribution: AnalyticNoiseStream.AmplitudeDistribution.White)
                            .ADSR(0.0125, 0.25, 1, 0.25))
                        .ToBGCStream()
                        .ADSR(0.0125, 0.0125, 0.15, 1, 0.125, 0.05);


                case PercussionMap.SideStick:
                case PercussionMap.HandClap:
                    return new AnalyticWave(amplitude, 4 * frequency)
                        .FrequencyModulation(30.0, 30.0)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.0125, 0.1, 1, 0.125, 0.05);

                case PercussionMap.LowFloorTom:
                case PercussionMap.HighFloorTom:
                case PercussionMap.LowTom:
                case PercussionMap.LowMidTom:
                case PercussionMap.HighTom:
                case PercussionMap.HiMidTom:
                case PercussionMap.HiBongo:
                case PercussionMap.LowBongo:
                case PercussionMap.MuteHiConga:
                case PercussionMap.OpenHiConga:
                case PercussionMap.LowConga:
                case PercussionMap.HighTimbale:
                case PercussionMap.LowTimbale:
                case PercussionMap.HighAgogo:
                case PercussionMap.LowAgogo:
                case PercussionMap.HiWoodBlock:
                case PercussionMap.LowWoodBlock:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(55.0, 15.0)
                        .ToBGCStream()
                        .ADSR(0.01, 0.125, 0.2, 1, 0.25, 0.05);

                case PercussionMap.ClosedHiHat:
                    return new NoiseAudioClip(
                        duration: 2f,
                        rms: amplitude,
                        freqLB: 1000f,
                        freqUB: 8000f,
                        frequencyCount: 1000,
                        distribution: NoiseAudioClip.AmplitudeDistribution.White)
                        .ADSR(0.02, 0.15, 0.05, 0.0125);

                case PercussionMap.PedalHiHat:
                    return new NoiseAudioClip(
                        duration: 2f,
                        rms: amplitude,
                        freqLB: 500f,
                        freqUB: 4000f,
                        frequencyCount: 1000,
                        distribution: NoiseAudioClip.AmplitudeDistribution.White)
                        .ADSR(0.02, 0.2, 0.1, 0.025);

                case PercussionMap.OpenHiHat:
                    return new NoiseAudioClip(
                        duration: 2f,
                        rms: amplitude,
                        freqLB: 1000f,
                        freqUB: 8000f,
                        frequencyCount: 1000,
                        distribution: NoiseAudioClip.AmplitudeDistribution.Blue)
                        .ADSR(0.02, 0.2, 0.1, 0.125);

                case PercussionMap.Tambourine:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(30.0, 30.0)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.0125, 0.4, 1, 0.5, 0.05);

                case PercussionMap.CrashCymbal1:
                case PercussionMap.CrashCymbal2:
                case PercussionMap.LongWhistle:
                case PercussionMap.OpenTriangle:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(30.0, 30.0)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.0125, 0.4, 1, 0.5, 0.05);

                case PercussionMap.RideCymbal1:
                case PercussionMap.RideCymbal2:
                case PercussionMap.ChineseCymbal:
                case PercussionMap.RideBell:
                case PercussionMap.SplashCymbal:
                case PercussionMap.Cowbell:
                case PercussionMap.ShortWhistle:
                case PercussionMap.MuteTriangle:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(30.0, 30.0)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.0125, 0.1, 1, 0.5, 0.05);

                case PercussionMap.Vibraslap:
                case PercussionMap.Cabasa:
                case PercussionMap.Maracas:
                case PercussionMap.ShortGuiro:
                case PercussionMap.Claves:
                case PercussionMap.LongGuiro:
                case PercussionMap.MuteCuica:
                case PercussionMap.OpenCuica:
                    return new AnalyticWave(amplitude, frequency)
                        .FrequencyModulation(30.0, 30.0)
                        .ToBGCStream()
                        .ADSR(0.0125, 0.0125, 0.1, 1, 0.5, 0.05);

                default:
                    break;
            }

            return null;
        }

        public static string GetNoteLabel(byte note) =>
            $"{GetKeyLabel(GetNoteKey(note))}{note / 12}";

        public static Key GetNoteKey(byte note)
        {
            switch (note % (int)Key.MAX)
            {
                case 0: return Key.C;
                case 1: return Key.CSharp;
                case 2: return Key.D;
                case 3: return Key.DSharp;
                case 4: return Key.E;
                case 5: return Key.F;
                case 6: return Key.FSharp;
                case 7: return Key.G;
                case 8: return Key.GSharp;
                case 9: return Key.A;
                case 10: return Key.ASharp;
                case 11: return Key.B;
            }

            return Key.MAX;
        }

        public static string GetKeyLabel(Key key)
        {
            switch (key)
            {
                case Key.C: return "C";
                case Key.CSharp: return "C#";
                case Key.D: return "D";
                case Key.DSharp: return "D#";
                case Key.E: return "E";
                case Key.F: return "F";
                case Key.FSharp: return "F#";
                case Key.G: return "G";
                case Key.GSharp: return "G#";
                case Key.A: return "A";
                case Key.ASharp: return "A#";
                case Key.B: return "B";
            }

            return "";
        }

        public static double GetNoteFrequency(byte note) =>
            GetOctaveFactor(note) * GetKeyCenterFrequency(note);

        public static double GetOctaveFactor(byte note)
        {
            int octave = (note / 12) - 5;

            if (octave == 0)
            {
                return 1.0;
            }

            return Math.Pow(2.0, octave);

        }

        public static double GetKeyCenterFrequency(Key key)
        {
            switch (key)
            {
                case Key.C: return 523.25;
                case Key.CSharp: return 554.37;
                case Key.D: return 587.33;
                case Key.DSharp: return 622.25;
                case Key.E: return 659.26;
                case Key.F: return 698.46;
                case Key.FSharp: return 739.99;
                case Key.G: return 783.99;
                case Key.GSharp: return 830.61;
                case Key.A: return 880.00;
                case Key.ASharp: return 932.33;
                case Key.B: return 987.77;
            }

            return 0.00;
        }

        public static double GetNoteAmplitude(byte velocity)
        {
            return velocity / (double)0x7F;
        }

        public static double GetKeyCenterFrequency(byte note) =>
            GetKeyCenterFrequency(GetNoteKey(note));
    }
}
