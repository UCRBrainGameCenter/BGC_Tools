using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using NUnit.Framework;
using BGC.IO;
using BGC.Audio;
using BGC.Audio.Filters;
using BGC.Audio.Synthesis;
using BGC.Audio.AnalyticStreams;
using BGC.Mathematics;

namespace BGC.Tests
{
    public class SynthesisTests
    {
        //5 second sound files
        const float DURATION = 5f;
        //Normalize to 70dB
        const float LEVEL = 70f;

        [Test]
        public void TestSineWave()
        {
            Calibration.Initialize();

            IBGCStream stream =
                new SineWave(1f, 440f)
                .Window(DURATION)
                .Normalize(LEVEL);

            bool success = WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "sineWave.wav"),
                stream: stream,
                overwrite: true);

            Assert.IsTrue(success);
        }

        [Test]
        public void TestSquareWave()
        {
            Calibration.Initialize();

            IBGCStream stream =
                new SquareWave(1f, 400f)
                .Window(DURATION)
                .Normalize(LEVEL);

            bool success = WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "squareWave.wav"),
                stream: stream,
                overwrite: true);

            Assert.IsTrue(success);
        }

        [Test]
        public void TestSawtoothWave()
        {
            Calibration.Initialize();

            IBGCStream stream =
                new SawtoothWave(1f, 200f)
                .Window(DURATION)
                .Normalize(LEVEL);

            bool success = WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "sawtoothWave.wav"),
                stream: stream,
                overwrite: true);

            Assert.IsTrue(success);
        }

        [Test]
        public void TestTriangleWave()
        {
            Calibration.Initialize();

            IBGCStream stream =
                new TriangleWave(1f, 440f)
                .Window(DURATION)
                .Normalize(LEVEL);

            bool success = WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "triangleWave.wav"),
                stream: stream,
                overwrite: true);

            Assert.IsTrue(success);
        }

        [Test]
        public void TestBiQuadFilters()
        {
            Calibration.Initialize();

            {
                //Bandpass Filtered Sine Wave, Matched Frequency
                IBGCStream stream =
                    new SquareWave(1f, 400f)
                    .BiQuadBandpassFilter(400f)
                    .Window(DURATION)
                    .Normalize(LEVEL);

                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "BiQuadMostThroughBandWave.wav"),
                    stream: stream,
                    overwrite: true);

                Assert.IsTrue(success);
            }

            {
                //Bandpass Filtered Sine Wave, 2x Frequency
                IBGCStream stream =
                    new SquareWave(1f, 400f)
                    .BiQuadBandpassFilter(800f)
                    .Window(DURATION)
                    .Normalize(LEVEL);

                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "BiQuadTooHighBandWave.wav"),
                    stream: stream,
                    overwrite: true);

                Assert.IsTrue(success);
            }
            {
                //Bandpass Filtered Sine Wave, Half Frequency
                IBGCStream stream =
                    new SquareWave(1f, 400f)
                    .BiQuadBandpassFilter(200f)
                    .Window(DURATION)
                    .Normalize(LEVEL);

                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "BiQuadTooLowBandWave.wav"),
                    stream: stream,
                    overwrite: true);

                Assert.IsTrue(success);
            }

            {
                //Notch Filtered Square Wave, Matched Frequency
                IBGCStream stream =
                    new SquareWave(1f, 400f)
                    .BiQuadNotchFilter(400f)
                    .Window(DURATION)
                    .Normalize(LEVEL);

                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "BiQuadNotchFilteredSquareWave.wav"),
                    stream: stream,
                    overwrite: true);

                Assert.IsTrue(success);
            }

            {
                //Notch Filtered Sine Wave, Matched Frequency
                IBGCStream stream =
                    new SineWave(1f, 400f)
                    .BiQuadNotchFilter(400f)
                    .Window(DURATION)
                    .Normalize(LEVEL);

                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "BiQuadNotchFilteredSineWave.wav"),
                    stream: stream,
                    overwrite: true);

                Assert.IsTrue(success);
            }

            {
                //Notch Filtered Sine Wave, +10 Hz Mismatch
                IBGCStream stream =
                    new SineWave(1f, 400f)
                    .BiQuadNotchFilter(410f)
                    .Window(DURATION)
                    .Normalize(LEVEL);

                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "BiQuadNotchFiltered410SineWave.wav"),
                    stream: stream,
                    overwrite: true);

                Assert.IsTrue(success);
            }
        }
        [Test]
        public void TestCarrierModifiedFakeVoices()
        {
            Calibration.Initialize();

            // F2: 500
            // F3: 1000
            // F4: 2000

            double qFactor = 200;

            Func<IBGCStream> makeCarrierA = () =>
                new AnalyticNoiseStream(
                        rms: 1.0,
                        freqLB: 20,
                        freqUB: 10000,
                        frequencyCount: 10000,
                        distribution: AnalyticNoiseStream.AmplitudeDistribution.Brown)
                        .ToBGCStream();

            Func<IBGCStream> makeCarrierB = () =>
                new SawtoothWave(
                        amplitude: 1.0,
                        frequency: 120);

            Func<IBGCStream>[] carrierFuncs = new Func<IBGCStream>[]
            {
                makeCarrierA,
                makeCarrierB
            };

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    WaveEncoding.SaveStream(
                        filepath: DataManagement.PathForDataFile("Test", $"testVoice{i}{j}.wav"),
                        stream: new StreamAdder(
                            carrierFuncs[i]()
                                .BiQuadBandpassFilter(
                                    criticalFrequency: 500,
                                    qFactor: qFactor),
                            carrierFuncs[j]()
                                .BiQuadBandpassFilter(
                                    criticalFrequency: 1500,
                                    qFactor: qFactor))
                        .Window(0.5)
                        .SlowRangeFitter(),
                        overwrite: true);
                }
            }
        }

        [Test]
        public void TestFunFakeVoice()
        {
            Calibration.Initialize();

            // F2: 500
            // F3: 1000
            // F4: 2000

            double qFactor = 200;

            //IBGCStream f2 = new AnalyticNoiseStream(
            //    rms: 1.0,
            //    freqLB: 20,
            //    freqUB: 10000,
            //    frequencyCount: 10000,
            //    distribution: AnalyticNoiseStream.AmplitudeDistribution.Pink)
            //    .ToBGCStream()
            //    .ContinuousFilter(
            //        envelopeStream: new Audio.Envelopes.SigmoidEnvelope(2.0),
            //        filterType: ContinuousFilter.FilterType.BandPass,
            //        freqLB: 500,
            //        freqUB: 540,
            //        qFactor: 100.0);


            //Func<IBGCStream> makeCarrier = () =>
            //new AnalyticNoiseStream(
            //        rms: 1.0,
            //        freqLB: 20,
            //        freqUB: 10000,
            //        frequencyCount: 10000,
            //        distribution: AnalyticNoiseStream.AmplitudeDistribution.Brown)
            //    .ToBGCStream();

            //Func<IBGCStream> makeCarrier = () =>
            //    new SawtoothWave(
            //            amplitude: 1.0,
            //            frequency: 120);

            Func<IBGCStream> makeCarrier = () =>
                new StreamAdder(
                    new SawtoothWave(
                            amplitude: 1.0,
                            frequency: 120),
                    new AnalyticNoiseStream(
                        rms: 0.2,
                        freqLB: 20,
                        freqUB: 10000,
                        frequencyCount: 10000,
                        distribution: AnalyticNoiseStream.AmplitudeDistribution.Brown)
                        .ToBGCStream());

            //Func<IBGCStream> makeCarrier = () =>
            //    new SquareWave(
            //        1.0,
            //        280.0,
            //        0.1);

            //IBGCStream f2 = makeCarrier()
            //    .ContinuousFilter(
            //        envelopeStream: new SineWave(1.0, 4),
            //        filterType: ContinuousFilter.FilterType.BandPass,
            //        freqLB: 1500,
            //        freqUB: 1000,
            //        qFactor: qFactor);

            {
                IBGCStream f1 = makeCarrier()
                    .BiQuadBandpassFilter(
                        criticalFrequency: 500,
                        qFactor: qFactor);

                IBGCStream f2 = makeCarrier()
                    .BiQuadBandpassFilter(
                        criticalFrequency: 1500,
                        qFactor: qFactor);

                IBGCStream fakeVoice = new StreamAdder(f1, f2)
                    .Window(0.2)
                    .Center(0.5)
                    .SlowRangeFitter();

                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "testVoiceA.wav"),
                    stream: fakeVoice,
                    overwrite: true);
            }

            {
                IBGCStream f1 = makeCarrier()
                    .BiQuadBandpassFilter(
                        criticalFrequency: 750,
                        qFactor: qFactor);

                IBGCStream f2 = makeCarrier()
                    .BiQuadBandpassFilter(
                        criticalFrequency: 2000,
                        qFactor: qFactor);

                IBGCStream fakeVoice = new StreamAdder(f1, f2)
                    .Window(0.2)
                    .Center(0.5)
                    .SlowRangeFitter();

                WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "testVoiceB.wav"),
                    stream: fakeVoice,
                    overwrite: true);
            }
        }

        [Test]
        public void TestFakeVoice()
        {
            Calibration.Initialize();

            // F2: 500
            // F3: 1000
            // F4: 2000

            double qFactor = 100;

            //IBGCStream f2 = new AnalyticNoiseStream(
            //    rms: 1.0,
            //    freqLB: 20,
            //    freqUB: 10000,
            //    frequencyCount: 10000,
            //    distribution: AnalyticNoiseStream.AmplitudeDistribution.Pink)
            //    .ToBGCStream()
            //    .ContinuousFilter(
            //        envelopeStream: new Audio.Envelopes.SigmoidEnvelope(2.0),
            //        filterType: ContinuousFilter.FilterType.BandPass,
            //        freqLB: 500,
            //        freqUB: 540,
            //        qFactor: 100.0);


            //Func<IBGCStream> makeCarrier = () =>
            //new AnalyticNoiseStream(
            //        rms: 1.0,
            //        freqLB: 20,
            //        freqUB: 10000,
            //        frequencyCount: 10000,
            //        distribution: AnalyticNoiseStream.AmplitudeDistribution.Brown)
            //    .ToBGCStream();

            //Func<IBGCStream> makeCarrier = () =>
            //    new SawtoothWave(
            //            amplitude: 1.0,
            //            frequency: 120);

            Func<IBGCStream> makeCarrier = () =>
                new StreamAdder(
                    new SawtoothWave(
                            amplitude: 1.0,
                            frequency: 120),
                    new AnalyticNoiseStream(
                        rms: 0.2,
                        freqLB: 20,
                        freqUB: 10000,
                        frequencyCount: 10000,
                        distribution: AnalyticNoiseStream.AmplitudeDistribution.Brown)
                        .ToBGCStream());

            //Func<IBGCStream> makeCarrier = () =>
            //    new SquareWave(
            //        1.0,
            //        280.0,
            //        0.1);

            IBGCStream f2 = makeCarrier()
                .ContinuousFilter(
                    envelopeStream: new Audio.Envelopes.EnvelopeConcatenator(
                        new Audio.Envelopes.SigmoidEnvelope(0.1),
                        new Audio.Envelopes.ConstantEnvelope(1.0, 0.1)),
                    filterType: ContinuousFilter.FilterType.BandPass,
                    freqLB: 400,
                    freqUB: 700,
                    qFactor: qFactor);

            IBGCStream f3 = makeCarrier()
                .ContinuousFilter(
                    envelopeStream: new Audio.Envelopes.LinearEnvelope(0.05, 0.15),
                    filterType: ContinuousFilter.FilterType.BandPass,
                    freqLB: 1500,
                    freqUB: 1000,
                    qFactor: qFactor);

            IBGCStream f4 = makeCarrier()
                .ContinuousFilter(
                    envelopeStream: new Audio.Envelopes.ConstantEnvelope(1.0, 0.2),
                    filterType: ContinuousFilter.FilterType.BandPass,
                    freqLB: 2000,
                    freqUB: 2000,
                    qFactor: qFactor);

            IBGCStream fakeVoice = new StreamAdder(f2, f3, f4)
                .Window(.2)
                .Center(1)
                .SlowRangeFitter();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "testVoice.wav"),
                stream: fakeVoice);
        }


        [Test]
        public void TestAllPassFilterSynth()
        {
            Calibration.Initialize();

            IBGCStream[] synthStreams = new IBGCStream[]
            {
                new SineWave(1f, 440f).Window(DURATION),
                new SquareWave(1f, 440f).Window(DURATION),
                new SawtoothWave(1f, 440f).Window(DURATION),
                new TriangleWave(1f, 440f).Window(DURATION)
            };

            foreach (IBGCStream stream in synthStreams)
            {
                bool success = TryAllpassFilters(
                    stream: stream,
                    baseName: stream.GetType().Name.Substring(0, 4));

                Assert.IsTrue(success);
            }
        }

        [Test]
        public void TestAllPassFilterSpeech()
        {
            Calibration.Initialize();

            string speechFile = DataManagement.PathForDataFile("Test", "000000.wav");
            if (!File.Exists(speechFile))
            {
                throw new Exception($"Test utilizes CRM missing sentence: {speechFile}");
            }

            WaveEncoding.LoadBGCStream(
                filepath: speechFile,
                stream: out IBGCStream speechStream);

            speechStream = speechStream.Center(speechStream.Duration() + 0.4f);

            bool success = TryAllpassFilters(
                stream: speechStream,
                baseName: "CRMS");

            Assert.IsTrue(success);
        }

        private bool TryAllpassFilters(IBGCStream stream, string baseName)
        {
            List<Complex64> testCoefficients = new List<Complex64>();

            double[] magnitudes = new double[]
            {
                0.1,
                0.2,
                0.5,
                0.8,
                0.9,
                1.0
            };

            double[] angles = new double[]
            {
                0.0,
                Math.PI / 3.0,
                Math.PI / 2.0,
                2.0 * Math.PI / 3.0,
                Math.PI
            };

            foreach (double m in magnitudes)
            {
                foreach (double angle in angles)
                {
                    testCoefficients.Add(Complex64.FromPolarCoordinates(m, angle));
                }
            }

            List<int> testDelays = new List<int>()
            {
                1,
                44,   //1 ms
                441,  //10 ms
                4410, //100 ms
            };

            foreach (Complex64 coeff in testCoefficients)
            {
                string coeffPart = $"(m{coeff.Magnitude:0.00}p{(coeff.Phase >= 0 ? coeff.Phase : 2 * Math.PI - coeff.Phase):0.00})";
                foreach (int delay in testDelays)
                {
                    if (!TryAllPassFilter(stream, coeff, delay, $"{baseName}{coeffPart}{delay:0000}"))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool TryAllPassFilter(IBGCStream stream, in Complex64 coeff, int delay, string name)
        {
            IBGCStream filteredStream = stream
                .AllPass(coeff, delay)
                .Normalize(LEVEL);

            bool success = WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"AllPass_{name}.wav"),
                stream: filteredStream,
                overwrite: true);

            return success;
        }

        [Test]
        public void TestFDComposer()
        {
            Calibration.Initialize();

            System.Random randomizer = new System.Random();

            IEnumerable<double> frequencies = GetExpFrequencies(500, 10000, 10000);

            IEnumerable<ComplexCarrierTone> carriers = frequencies.Select(freq =>
                new ComplexCarrierTone(
                    frequency: freq,
                    amplitude: Complex64.FromPolarCoordinates(
                        magnitude: CustomRandom.RayleighDistribution(randomizer.NextDouble()),
                        phase: 2.0 * Math.PI * randomizer.NextDouble()))).ToArray();


            for (int frameExp = 10; frameExp < 16; frameExp++)
            {
                for (int overlapExp = 1; overlapExp < 6; overlapExp++)
                {
                    int frameSize = 1 << frameExp;
                    int overlap = 1 << overlapExp;

                    IBGCStream composed = new ContinuousFrequencyDomainToneComposer(
                            carrierTones: carriers,
                            frameSize: frameSize,
                            overlapFactor: overlap)
                        .Truncate(totalDuration: 1.0)
                        .StandardizeRMS(0.008)
                        .Cache();

                    Debug.Log($"(FrameSize, Overlap): ({frameSize},{overlap}). " +
                        $"RMSExp: 0.008  " +
                        $"RMSActual: {composed.GetChannelRMS().First()}");

                    WaveEncoding.SaveStream(
                       filepath: DataManagement.PathForDataFile("Test", $"FDNoise_{frameExp:D2}_{overlapExp}.wav"),
                       stream: composed,
                       overwrite: true);
                }
            }
        }

        [Test]
        public void TestFDComposerPure()
        {
            Calibration.Initialize();

            System.Random randomizer = new System.Random();

            //Maximally inharmonic
            double frequency = 1 / ((1 / 441.0) + 0.5 / 44100.0);

            IEnumerable<ComplexCarrierTone> carriers = new ComplexCarrierTone[]
            {
                new ComplexCarrierTone(
                    frequency: frequency,
                    amplitude: Complex64.FromPolarCoordinates(
                        magnitude: 1.0,
                        phase: 2.0 * Math.PI * randomizer.NextDouble()))
            };

            for (int frameExp = 10; frameExp < 16; frameExp++)
            {
                for (int overlapExp = 1; overlapExp < 6; overlapExp++)
                {
                    int frameSize = 1 << frameExp;
                    int overlap = 1 << overlapExp;

                    IBGCStream composed = new ContinuousFrequencyDomainToneComposer(
                            carrierTones: carriers,
                            frameSize: frameSize,
                            overlapFactor: overlap)
                        .Truncate(totalDuration: 1.0)
                        .StandardizeRMS(0.008)
                        .Cache();

                    Debug.Log($"(FrameSize, Overlap): ({frameSize},{overlap}). " +
                        $"RMSExp: 0.008 " +
                        $"RMSActual: {composed.GetChannelRMS().First()}");

                    WaveEncoding.SaveStream(
                       filepath: DataManagement.PathForDataFile("Test", $"FDClean_{frameExp:D2}_{overlapExp}.wav"),
                       stream: composed.StandardizeRMS(),
                       overwrite: true);
                }
            }
        }

        private IEnumerable<double> GetExpFrequencies(double freqLB, double freqUB, int freqCount)
        {
            double ratio = freqUB / freqLB;

            for (int i = 0; i < freqCount; i++)
            {
                yield return freqLB * Math.Pow(ratio, i / (double)(freqCount - 1));
            }
        }

        [Test]
        public void TestPhaseReEncoder()
        {
            Calibration.Initialize();

            IBGCStream baseStream = new STMAudioClip(
                    duration: 4.0,
                    freqLB: 20,
                    freqUB: 10000,
                    frequencyCount: 10000,
                    modulationDepth: 20,
                    spectralModulationRate: 2,
                    temporalModulationRate: 4,
                    rippleDirection: STMAudioClip.RippleDirection.Up,
                    distribution: STMAudioClip.AmplitudeDistribution.Pink)
                .StandardizeRMS(0.008)
                .Cache();

            Debug.Log($"Base RMS: {baseStream.CalculateRMS().First()}");

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"ReEncoding_Initial.wav"),
                stream: baseStream
                    .Truncate(totalDuration: 1.0)
                    .Window(),
                overwrite: true);

            IBGCStream fullReencodedStream = baseStream.SinglePassPhaseReencode(0.0, 0.000_100);

            Debug.Log($"After 100us Full ReEncoding: ({string.Join(",", fullReencodedStream.CalculateRMS().Select(x => x.ToString()))})");

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncoding_0100us_Single.wav"),
               stream: fullReencodedStream
                    .Truncate(totalDuration: 1.0)
                    .Window(),
               overwrite: true);

            IBGCStream frameReencodedStream = new FramedPhaseReencoder(baseStream, 0.0, 0.000_100);

            Debug.Log($"After 100us Frame ReEncoding: ({string.Join(",", frameReencodedStream.CalculateRMS().Select(x => x.ToString()))})");

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"ReEncoding_0100us_Frame.wav"),
                stream: frameReencodedStream
                    .Truncate(totalDuration: 1.0)
                    .Window(),
                overwrite: true);

            fullReencodedStream = baseStream.SinglePassPhaseReencode(0.0, 0.001_000);

            Debug.Log($"After 1000us Full ReEncoding: ({string.Join(",", fullReencodedStream.CalculateRMS().Select(x => x.ToString()))})");

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncoding_1000us_Single.wav"),
               stream: fullReencodedStream
                    .Truncate(totalDuration: 1.0)
                    .Window(),
               overwrite: true);

            frameReencodedStream = new FramedPhaseReencoder(baseStream, 0.0, 0.001_000);

            Debug.Log($"After 1000us Frame ReEncoding: ({string.Join(",", frameReencodedStream.CalculateRMS().Select(x => x.ToString()))})");

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"ReEncoding_1000us_Frame.wav"),
                stream: frameReencodedStream
                    .Truncate(totalDuration: 1.0)
                    .Window(),
                overwrite: true);

            fullReencodedStream = baseStream.SinglePassPhaseReencode(0.0, 0.000_010);

            Debug.Log($"After 10us Full ReEncoding: ({string.Join(",", fullReencodedStream.CalculateRMS().Select(x => x.ToString()))})");

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncoding_0010us_Single.wav"),
               stream: fullReencodedStream
                    .Truncate(totalDuration: 1.0)
                    .Window(),
               overwrite: true);

            frameReencodedStream = new FramedPhaseReencoder(baseStream, 0.0, 0.000_010);

            Debug.Log($"After 10us Frame ReEncoding: ({string.Join(",", frameReencodedStream.CalculateRMS().Select(x => x.ToString()))})");

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"ReEncoding_0010us_Frame.wav"),
                stream: frameReencodedStream
                    .Truncate(totalDuration: 1.0)
                    .Window(),
                overwrite: true);

            WaveEncoding.LoadBGCStream(
                filepath: DataManagement.PathForDataFile("Test", "000000.wav"),
                stream: out baseStream);

            baseStream = baseStream.StandardizeRMS();

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncodingCRM_initial.wav"),
               stream: baseStream,
               overwrite: true);

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncodingCRM_1000us_Single.wav"),
               stream: baseStream.SinglePassPhaseReencode(0.0, 0.001_000),
               overwrite: true);

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncodingCRM_0100us_Single.wav"),
               stream: baseStream.SinglePassPhaseReencode(0.0, 0.000_100),
               overwrite: true);

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncodingCRM_0010us_Single.wav"),
               stream: baseStream.SinglePassPhaseReencode(0.0, 0.000_010),
               overwrite: true);

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncodingCRM_1000us_Frame.wav"),
               stream: new FramedPhaseReencoder(baseStream, 0.0, 0.001_000),
               overwrite: true);

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncodingCRM_0100us_Frame.wav"),
               stream: new FramedPhaseReencoder(baseStream, 0.0, 0.000_100),
               overwrite: true);

            WaveEncoding.SaveStream(
               filepath: DataManagement.PathForDataFile("Test", $"ReEncodingCRM_0010us_Frame.wav"),
               stream: new FramedPhaseReencoder(baseStream, 0.0, 0.000_010),
               overwrite: true);
        }

        [Test]
        public void TestSTM()
        {
            Calibration.Initialize();

            WaveEncoding.SaveStream(
            filepath: DataManagement.PathForDataFile("Test", $"Long_STM.wav"),
            stream: new STMAudioClip(
                duration: 95.0,
                freqLB: 20,
                freqUB: 10000,
                frequencyCount: 10000,
                modulationDepth: 20,
                spectralModulationRate: 2,
                temporalModulationRate: 4,
                rippleDirection: STMAudioClip.RippleDirection.Up,
                distribution: STMAudioClip.AmplitudeDistribution.Pink).SlowRangeFitter(),
            overwrite: true);
        }

        [Test]
        public void SmallSineFMTest()
        {
            Calibration.Initialize();

            {
                //Sine -> FM 2, 20
                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "SineFM_2Hz_p20Hz.wav"),
                    stream: new SineWave(1f, 440f)
                        .Window(DURATION)
                        .FrequencyModulation(2, 20)
                        .Normalize(LEVEL),
                    overwrite: true);

                Assert.IsTrue(success);
            }

            {
                //Sine -> FM 2, -20
                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "SineFM_2Hz_n20Hz.wav"),
                    stream: new SineWave(1f, 440f)
                        .Window(DURATION)
                        .FrequencyModulation(2, -20)
                        .Normalize(LEVEL),
                    overwrite: true);

                Assert.IsTrue(success);
            }

            {
                //Sine -> FM 2, 0 (Should be unmodified)
                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "SineFM_2Hz_000Hz.wav"),
                    stream: new SineWave(1f, 440f)
                        .Window(DURATION)
                        .FrequencyModulation(2, 0)
                        .Normalize(LEVEL),
                    overwrite: true);

                Assert.IsTrue(success);
            }

            {
                //FM Inverted
                //Sine -> Left(FM 2, 20) + Right(FM 2, -20)
                IBGCStream leftStream = new SineWave(1f, 440f)
                    .Window(DURATION)
                    .Fork(out IBGCStream rightStream);

                leftStream = leftStream.FrequencyModulation(2, 20);
                rightStream = rightStream.FrequencyModulation(2, -20);

                bool success = WaveEncoding.SaveStream(
                    filepath: DataManagement.PathForDataFile("Test", "SineFM_2Hz_p20Hz_INV.wav"),
                    stream: new StreamMergeFilter(leftStream, rightStream).Normalize(LEVEL),
                    overwrite: true);

                Assert.IsTrue(success);
            }
        }

        [Test]
        public void SmallAnalyticFMTest()
        {
            Calibration.Initialize();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"SmallFM_20Hz.wav"),
                stream: new AnalyticWave(1f, 440f)
                    .FrequencyModulation(2, 20)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"SmallFM_00Hz.wav"),
                stream: new AnalyticWave(1f, 440f)
                    .FrequencyModulation(2, 0)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            IAnalyticStream leftStream = new AnalyticWave(1f, 440f)
                .Window(DURATION)
                .Fork(out IAnalyticStream rightStream);

            leftStream = leftStream.FrequencyModulation(2, 20);
            rightStream = rightStream.FrequencyModulation(2, -20);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"SmallFM_Invr.wav"),
                stream: new StreamMergeFilter(leftStream.ToBGCStream(), rightStream.ToBGCStream()).Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void FMTestBell()
        {
            Calibration.Initialize();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestBell.wav"),
                stream: new AnalyticWave(1f, 200f)
                    .FrequencyModulation(280, 10)
                    .ToBGCStream()
                    .ADSR(0.01f, 1.0f, 0.9f, 0.4f)
                    .Window(DURATION, smoothingSamples: 10)
                    .Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void FMTestDrum()
        {
            Calibration.Initialize();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestDrum.wav"),
                stream: new AnalyticWave(1f, 80f)
                    .FrequencyModulation(55, 15)
                    .ToBGCStream()
                    .ADSR(0.01f, 0.125f, 1.0f, 0.05f)
                    .Window(1f, smoothingSamples: 10)
                    .Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void FMTestBrass()
        {
            Calibration.Initialize();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestBrass.wav"),
                stream: new AnalyticWave(1f, 660)
                    .FrequencyModulation(10, 220)
                    .ToBGCStream()
                    .ADSR(0.25f, 0.25f, 0.8f, 1000f)
                    .Window(DURATION, smoothingSamples: 10)
                    .Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void FMTestPiano()
        {
            Calibration.Initialize();

            double baseFreq = 440.0;

            IAnalyticStream noteStream = new AnalyticStreamAdder(
                new AnalyticWave(1.0, baseFreq),
                new AnalyticWave(0.5, 3 * baseFreq),
                new AnalyticWave(0.25, 5 * baseFreq),
                new AnalyticWave(0.125, 7 * baseFreq));

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestPiano.wav"),
                stream: noteStream
                    .FrequencyModulation(15, 2.5)
                    .ToBGCStream()
                    .ADSR(0.25f, 0.25f, 0.8f, 1f)
                    .Window(DURATION, smoothingSamples: 10)
                    .Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void FMTestTrianglePiano()
        {
            Calibration.Initialize();

            float baseFreq = 440.0f;

            IBGCStream noteStream = new StreamAdder(
                new TriangleWave(1.0f, baseFreq));

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestTrianglePiano.wav"),
                stream: noteStream
                    .FrequencyModulation(15, 2.5)
                    .ADSR(0.25f, 0.25f, 0.8f, 1f)
                    .Window(DURATION, smoothingSamples: 10)
                    .Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void TestFrequencyShifter()
        {
            Calibration.Initialize();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FS450Sine.wav"),
                stream: new AnalyticWave(1.0, 450.0)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FS500Sine.wav"),
                stream: new AnalyticWave(1.0, 500.0)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FS440Sine.wav"),
                stream: new AnalyticWave(1.0, 440.0)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FS450ShiftedSine.wav"),
                stream: new AnalyticWave(1.0, 440.0)
                    .FrequencyShift(10.0)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FS500ShiftedSine.wav"),
                stream: new AnalyticWave(1.0, 440.0)
                    .FrequencyShift(60.0)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FS450Tri.wav"),
                stream: new TriangleWave(1f, 450f)
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FS450ShiftedTri.wav"),
                stream: new TriangleWave(1f, 440f)
                    .ToAnalyticStream()
                    .FrequencyShift(10.0)
                    .ToBGCStream()
                    .Window(DURATION)
                    .Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void SynthTestSnare()
        {
            IBGCStream shiftedTriA =
                new TriangleWave(1f, 111f)
                .ToAnalyticStream()
                .Fork(out IAnalyticStream triOscB)
                .FrequencyShift(175.0)
                .ToBGCStream();

            IBGCStream shiftedTriB = triOscB.FrequencyShift(224.0)
                .ToBGCStream();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "SnareDrum.wav"),
                stream:
                    new NormalizerMonoFilter(
                        new StreamAdder(
                            shiftedTriA,
                            shiftedTriB,
                            new SineWave(0.5, 180.0),
                            new SineWave(0.5, 330.0))
                            .ADSR(0.01f, 0.125f, 1.0f, 0.05f)
                            .Window(0.5f, smoothingSamples: 10),
                        LEVEL, false)
                    .Center(1f),
                overwrite: true);
        }

        [Test]
        public void FMTestOvertones()
        {
            Calibration.Initialize();

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestOvertones_010.wav"),
                stream: new SineWave(1f, 440)
                    .Window(DURATION)
                    .FrequencyModulation(1320, 10)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestOvertones_020.wav"),
                stream: new SineWave(1f, 440)
                    .Window(DURATION)
                    .FrequencyModulation(1320, 20)
                    .Normalize(LEVEL),
                overwrite: true);

            WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", "FMTestOvertones_060.wav"),
                stream: new SineWave(1f, 440)
                    .Window(DURATION)
                    .FrequencyModulation(1320, 60)
                    .Normalize(LEVEL),
                overwrite: true);
        }

        [Test]
        public void TestFMFilterSynth()
        {
            Calibration.Initialize();

            IBGCStream[] synthStreams = new IBGCStream[]
            {
                new SineWave(1f, 440f).Window(DURATION),
                new SquareWave(1f, 440f).Window(DURATION),
                new SawtoothWave(1f, 440f).Window(DURATION),
                new TriangleWave(1f, 440f).Window(DURATION)
            };

            foreach (IBGCStream stream in synthStreams)
            {
                bool success = TryFMParams(
                    stream: stream,
                    baseName: stream.GetType().Name.Substring(0, 4));

                Assert.IsTrue(success);
            }
        }

        [Test]
        public void TestFMFilterVoice()
        {
            Calibration.Initialize();

            string speechFile = DataManagement.PathForDataFile("Test", "000000.wav");
            if (!File.Exists(speechFile))
            {
                throw new Exception($"Test utilizes CRM missing sentence: {speechFile}");
            }

            WaveEncoding.LoadBGCStream(
                filepath: speechFile,
                stream: out IBGCStream speechStream);

            speechStream = speechStream.Center(speechStream.Duration() + 0.4f);

            bool success = TryFMParams(
                stream: speechStream,
                baseName: "CRMS");

            Assert.IsTrue(success);
        }

        private bool TryFMParams(IBGCStream stream, string baseName)
        {
            float[] rates = new float[]
            {
                2f,
                4f,
                8f,
                16f,
                32f,
                64f
            };

            float[] depths = new float[]
            {
                10f,
                20f,
                40f,
                60f,
                80f
            };

            foreach (float rate in rates)
            {
                foreach (float depth in depths)
                {
                    if (!TryFMFilter(
                        stream: stream,
                        rate: rate,
                        depth: depth,
                        name: $"{baseName}_{rate:000}_{depth:00}"))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool TryFMFilter(IBGCStream stream, float rate, float depth, string name)
        {
            IBGCStream filteredStream = stream
                .FrequencyModulation(rate, depth)
                .Normalize(LEVEL);

            bool success = WaveEncoding.SaveStream(
                filepath: DataManagement.PathForDataFile("Test", $"FM_{name}.wav"),
                stream: filteredStream,
                overwrite: true);

            return success;
        }

        [Test]
        public void GenerateButterworthCoeff()
        {
            double[] coeffs = Mathematics.FIRFilters.FIRButterworth.BandpassCoefficients(
                order: 510,
                f1: 1682,
                f2: 2378,
                samplingRate: 24414);

            File.WriteAllLines(
                path: DataManagement.PathForDataFile("Test", "ButterWorth_510_2k.txt"),
                contents: coeffs.Select(x => x.ToString()));

            double[] coeffs2 = Mathematics.FIRFilters.FIRButterworth.BandpassCoefficients(
                order: 511,
                f1: 1682,
                f2: 2378,
                samplingRate: 24414);

            File.WriteAllLines(
                path: DataManagement.PathForDataFile("Test", "ButterWorth_511_2k.txt"),
                contents: coeffs2.Select(x => x.ToString()));
        }
    }
}
