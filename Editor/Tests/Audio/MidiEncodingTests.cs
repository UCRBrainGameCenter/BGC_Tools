using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using NUnit.Framework;
using BGC.IO;
using BGC.Audio;
using BGC.Audio.Midi;
using BGC.Audio.Filters;
using BGC.Audio.AnalyticStreams;
using BGC.Audio.Midi.Synth;
using BGC.Audio.Synthesis;
using BGC.Audio.Envelopes;

namespace BGC.Tests
{
    public class MidiEncodingTests
    {
        [Test]
        public void TestLoadSaveMidiFull()
        {
            string loadFile = DataManagement.PathForDataFile("Test", "MIDI_sample.mid");
            string saveFile = DataManagement.PathForDataFile("Test", "MIDI_sample_copy.mid");

            Assert.IsTrue(MidiEncoding.LoadFile(
                filePath: loadFile,
                midiFile: out MidiFile midiFile,
                retainAll: true));

            Debug.Log($"Format: {midiFile.headerInfo.format}");
            Debug.Log($"FramesPerSecond: {midiFile.headerInfo.framesPerSecond}");
            Debug.Log($"TicksPerFrame: {midiFile.headerInfo.ticksPerFrame}");
            Debug.Log($"TicksPerQuarter: {midiFile.headerInfo.ticksPerQuarter}");
            Debug.Log($"Tracks: {midiFile.headerInfo.tracks}");

            Debug.Log($"Samples Per Tick: {1E-6 * midiFile.tracks[0].Tempo * 44100 / midiFile.headerInfo.ticksPerQuarter}");

            Assert.IsTrue(MidiEncoding.SaveFile(
                filePath: saveFile,
                midiFile: midiFile,
                overwrite: true));

            Assert.IsTrue(File.Exists(saveFile));
        }

        [Test]
        public void TestRenderMidi()
        {
            Calibration.Initialize();

            string loadFile = DataManagement.PathForDataFile("Test", "MIDI_sample.mid");
            string saveFile = DataManagement.PathForDataFile("Test", "MIDI_sample.wav");

            Assert.IsTrue(MidiEncoding.LoadFile(
                filePath: loadFile,
                midiFile: out MidiFile midiFile,
                retainAll: true));

            Assert.IsTrue(WaveEncoding.SaveStream(
                filepath: saveFile,
                stream: new SlowRangeFitterFilter(new MidiFileStream(midiFile).SafeCache()),
                overwrite: true));


            Assert.IsTrue(File.Exists(saveFile));
        }

        [Test]
        public void TestRenderToccataMidi()
        {
            Calibration.Initialize();

            string loadFile = DataManagement.PathForDataFile("Test", "toccata1.mid");
            string saveFile = DataManagement.PathForDataFile("Test", "toccata1.wav");

            Assert.IsTrue(MidiEncoding.LoadFile(
                filePath: loadFile,
                midiFile: out MidiFile midiFile,
                retainAll: true));

            Assert.IsTrue(WaveEncoding.SaveStream(
                filepath: saveFile,
                stream: new SlowRangeFitterFilter(new MidiFileStream(midiFile).SafeCache()),
                overwrite: true));


            Assert.IsTrue(File.Exists(saveFile));
        }

        [Test]
        public void TestPulses()
        {
            Calibration.Initialize();

            double[] dutyCycles = new double[] { 0.1, 0.25, 0.5, 0.75, 0.9 };

            foreach (double dutyCycle in dutyCycles)
            {
                string saveFile = DataManagement.PathForDataFile("Test", $"squareWave({dutyCycle}).wav");

                Assert.IsTrue(WaveEncoding.SaveStream(
                    filepath: saveFile,
                    stream: new SquareWave(1.0, 400, dutyCycle: dutyCycle)
                        .Window(3.0),
                    overwrite: true));
            }
        }

        [Test]
        public void TestGuitar()
        {
            Calibration.Initialize();

            for (int octave = 2; octave <= 4; octave++)
            {
                string guitarFile = DataManagement.PathForDataFile("Test", $"guitarE{octave}.wav");

                Assert.IsTrue(WaveEncoding.SaveStream(
                    filepath: guitarFile,
                    stream: InstrumentLookup.GetNote(
                        set: ReservedSoundSet.ElectricGuitar_Jazz,
                        note: (byte)(12 * octave + 4),
                        velocity: 0xF7)
                        .SafeCache()
                        .SlowRangeFitter(),
                    overwrite: true));
            }
        }

        [Test]
        public void TestOrgan()
        {
            Calibration.Initialize();

            for (int octave = 2; octave <= 7; octave++)
            {
                string guitarFile = DataManagement.PathForDataFile("Test", $"organE{octave}.wav");

                Assert.IsTrue(WaveEncoding.SaveStream(
                    filepath: guitarFile,
                    stream: InstrumentLookup.GetNote(
                        set: ReservedSoundSet.CrutchOrgan,
                        note: (byte)(12 * octave + 4),
                        velocity: 0xF7)
                        .Window(4.0)
                        .SafeCache()
                        .SlowRangeFitter(),
                    overwrite: true));
            }
        }

        [Test]
        public void TestFlute()
        {
            Calibration.Initialize();

            for (int octave = 2; octave <= 8; octave++)
            {
                string guitarFile = DataManagement.PathForDataFile("Test", $"fluteE{octave}.wav");

                Assert.IsTrue(WaveEncoding.SaveStream(
                    filepath: guitarFile,
                    stream: InstrumentLookup.GetNote(
                        set: ReservedSoundSet.Flute,
                        note: (byte)(12 * octave + 4),
                        velocity: 0xF7)
                        .Window(4.0)
                        .SafeCache()
                        .SlowRangeFitter(),
                    overwrite: true));
            }
        }

        [Test]
        public void TestContinuousFilter()
        {
            Calibration.Initialize();

            IBGCStream noiseStream = new NoiseAudioClip(
                duration: 4,
                rms: 1.0,
                freqLB: 20.0,
                freqUB: 10000.0,
                frequencyCount: 10000,
                distribution: NoiseAudioClip.AmplitudeDistribution.Pink);


            for (ContinuousFilter.FilterType filter = 0; filter < ContinuousFilter.FilterType.MAX; filter++)
            {
                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"Sigmoid{filter}FilteredNoise.wav"),
                    stream: new ContinuousFilter(
                        stream: noiseStream,
                        filterEnvelope: new SigmoidEnvelope(4.0, 1.0),
                        filterType: filter,
                        freqLB: 20,
                        freqUB: 10000)
                        .Normalize(80),
                    overwrite: true);

                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"Linear{filter}FilteredNoise.wav"),
                    stream: new ContinuousFilter(
                        stream: noiseStream,
                        filterEnvelope: new LinearEnvelope(4.0),
                        filterType: filter,
                        freqLB: 20,
                        freqUB: 10000)
                        .Normalize(80),
                    overwrite: true);

                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"SlowSine{filter}FilteredNoise.wav"),
                    stream: new ContinuousFilter(
                        stream: noiseStream,
                        filterEnvelope: new SineWave(1.0, 1.0),
                        filterType: filter,
                        freqLB: 20,
                        freqUB: 10000)
                        .Normalize(80),
                    overwrite: true);

                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"FastSine{filter}FilteredNoise.wav"),
                    stream: new ContinuousFilter(
                        stream: noiseStream,
                        filterEnvelope: new SineWave(1.0, 50.0),
                        filterType: filter,
                        freqLB: 20,
                        freqUB: 10000)
                        .Normalize(80),
                    overwrite: true);

                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"SlowTriangle{filter}FilteredNoise.wav"),
                    stream: new ContinuousFilter(
                        stream: noiseStream,
                        filterEnvelope: new TriangleWave(1.0, 1.0),
                        filterType: filter,
                        freqLB: 20,
                        freqUB: 10000)
                        .Normalize(80),
                    overwrite: true);

                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", $"FastTriangle{filter}FilteredNoise.wav"),
                    stream: new ContinuousFilter(
                        stream: noiseStream,
                        filterEnvelope: new TriangleWave(1.0, 50.0),
                        filterType: filter,
                        freqLB: 20,
                        freqUB: 10000)
                        .Normalize(80),
                    overwrite: true);

            }
        }

        [Test]
        public void TestLoadSaveMidiBare()
        {
            string loadFile = DataManagement.PathForDataFile("Test", "MIDI_sample.mid");
            string saveFile = DataManagement.PathForDataFile("Test", "MIDI_sample_barecopy.mid");

            Assert.IsTrue(MidiEncoding.LoadFile(
                filePath: loadFile,
                midiFile: out MidiFile midiFile,
                retainAll: false));

            Assert.IsTrue(MidiEncoding.SaveFile(
                filePath: saveFile,
                midiFile: midiFile,
                overwrite: true));


            Assert.IsTrue(File.Exists(saveFile));
        }


        [Test]
        public void TestSnare()
        {
            string saveFile = DataManagement.PathForDataFile("Test", "SnareTest.wav");

            Assert.IsTrue(WaveEncoding.SaveStream(
                filepath: saveFile,
                stream: InstrumentLookup.GetPercussion(PercussionMap.AcousticSnare, 0xF7)
                    .Normalize(80f)
                    .Window(1f)
                    .Center(1.5f),
                overwrite: true));
        }


        [Test]
        public void TestHiHat()
        {
            string closedFile = DataManagement.PathForDataFile("Test", "ClosedHiHat.wav");

            Assert.IsTrue(WaveEncoding.SaveStream(
                filepath: closedFile,
                stream: InstrumentLookup.GetPercussion(PercussionMap.ClosedHiHat, 0xF7)
                    .SafeCache()
                    .SlowRangeFitter(),
                overwrite: true));

            string pedalFile = DataManagement.PathForDataFile("Test", "PedalHiHat.wav");

            Assert.IsTrue(WaveEncoding.SaveStream(
                filepath: pedalFile,
                stream: InstrumentLookup.GetPercussion(PercussionMap.PedalHiHat, 0xF7)
                    .SafeCache()
                    .SlowRangeFitter(),
                overwrite: true));

            string openFile = DataManagement.PathForDataFile("Test", "OpenHiHat.wav");

            Assert.IsTrue(WaveEncoding.SaveStream(
                filepath: openFile,
                stream: InstrumentLookup.GetPercussion(PercussionMap.OpenHiHat, 0xF7)
                    .SafeCache()
                    .SlowRangeFitter(),
                overwrite: true));
        }


    }
}