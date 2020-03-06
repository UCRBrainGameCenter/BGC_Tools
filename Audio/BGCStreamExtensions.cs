﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Audio.Envelopes;
using BGC.Audio.Filters;
using BGC.Audio.Synthesis;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Contains convenience extensions for BGCStreams, to support linear
    /// concatenation of filters
    /// </summary>
    public static class BGCStreamExtensions
    {
        /// <summary>
        /// Slowest Backup Alternative for calculating RMS
        /// </summary>
        public static IEnumerable<double> CalculateRMS(this IBGCStream stream)
        {
            double[] rms = new double[stream.Channels];

            if (stream.ChannelSamples == 0)
            {
                //Default to 0 for empty streams
                return rms;
            }

            if (stream.ChannelSamples == int.MaxValue)
            {
                //Can't calculate the RMS of an infinite stream
                for (int i = 0; i < rms.Length; i++)
                {
                    rms[i] = double.NaN;
                }

                return rms;
            }

            int readSamples;

            const int BUFFER_SIZE = 512;
            float[] buffer = new float[BUFFER_SIZE];

            stream.Reset();
            do
            {
                readSamples = stream.Read(buffer, 0, BUFFER_SIZE);

                for (int i = 0; i < readSamples; i++)
                {
                    rms[i % stream.Channels] += buffer[i] * buffer[i];
                }

            }
            while (readSamples > 0);

            stream.Reset();

            for (int i = 0; i < stream.Channels; i++)
            {
                rms[i] = Math.Sqrt(rms[i] / stream.ChannelSamples);
            }

            return rms;
        }

        /// <summary> Calculates the effective Duration of the stream </summary>
        public static double Duration(this IBGCStream stream) =>
            stream.ChannelSamples / (double)stream.SamplingRate;

        /// <summary>
        /// Lengthen and center each BGCStream in <paramref name="streams"/> so they are all the
        /// same duration
        /// </summary>
        /// <param name="minimumDuration">The minimum duration of each stream after lengthening</param>
        /// <returns>Each BGCStream, wrapped in a ClipCenterer</returns>
        public static IEnumerable<IBGCStream> EqualizeAndCenter(
            this IEnumerable<IBGCStream> streams,
            double minimumDuration = 0.0)
        {
            double length = Math.Max(streams.Max(x => x.Duration()), minimumDuration);

            foreach (IBGCStream stream in streams)
            {
                yield return new StreamCenterer(stream, length);
            }
        }

        public static IBGCStream StreamCache(this IBGCStream stream)
        {
            return new StreamCacher(stream);
        }

        /// <summary>
        /// Returns a stream at least <paramref name="minimumDuration"/> in duration, with the clip
        /// centered in it
        /// </summary>
        public static IBGCStream Center(
            this IBGCStream stream,
            double minimumDuration = 0.0)
        {
            return new StreamCenterer(stream, Math.Max(stream.Duration(), minimumDuration));
        }

        public static IBGCStream Center(
            this IBGCStream stream,
            int preDelaySamples,
            int postDelaySamples)
        {
            return new StreamCenterer(stream, preDelaySamples, postDelaySamples);
        }

        public static IBGCStream ContinuousFilter(
            this IBGCStream stream,
            IBGCEnvelopeStream envelopeStream,
            ContinuousFilter.FilterType filterType,
            double freqLB,
            double freqUB,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate)
        {
            return new ContinuousFilter(
                stream: stream,
                filterEnvelope: envelopeStream,
                filterType: filterType,
                freqLB: freqLB,
                freqUB: freqUB,
                qFactor: qFactor,
                rmsBehavior: rmsBehavior);
        }

        public static IBGCStream ADSR(
            this IBGCStream stream,
            double timeToPeak,
            double timeToSustain,
            double sustainAmplitude,
            double sustainDecayTime)
        {
            return new ADSREnvelope(
                stream: stream,
                timeToPeak: timeToPeak,
                timeToSustain: timeToSustain,
                sustainAmplitude: sustainAmplitude,
                sustainDecayTime: sustainDecayTime,
                releaseDecayTime: sustainDecayTime);
        }

        public static IBGCStream ADSR(
            this IBGCStream stream,
            double timeToPeak,
            double timeToSustain,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
        {
            return new ADSREnvelope(
                stream: stream,
                timeToPeak: timeToPeak,
                timeToSustain: timeToSustain,
                sustainAmplitude: sustainAmplitude,
                sustainDecayTime: sustainDecayTime,
                releaseDecayTime: releaseDecayTime);
        }

        public static IBGCStream ADSR(
            this IBGCStream stream,
            double timeToPeak,
            double timeToSustain,
            double timeToRelease,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
        {
            return new ADSREnvelope(
                stream: stream,
                timeToPeak: timeToPeak,
                timeToSustain: timeToSustain,
                timeToRelease: timeToRelease,
                sustainAmplitude: sustainAmplitude,
                sustainDecayTime: sustainDecayTime,
                releaseDecayTime: releaseDecayTime);
        }

        public static IBGCStream AddWith(
            this IBGCStream stream,
            IBGCStream other)
        {
            return new StreamAdder(stream, other);
        }

        public static IBGCStream AddWith(
            this IBGCStream stream,
            params IBGCStream[] others)
        {
            if (others.Length == 0)
            {
                return stream;
            }

            StreamAdder adder = new StreamAdder(stream);
            adder.AddStreams(others);
            return adder;
        }

        public static IBGCStream AddTogether(
            this IEnumerable<IBGCStream> streams)
        {
            if (streams.Count() == 1)
            {
                return streams.First();
            }

            return new StreamAdder(streams);
        }

        public static IBGCStream Window(
            this IBGCStream stream,
            double totalDuration,
            Windowing.Function function = Windowing.Function.Hamming,
            int smoothingSamples = 1000)
        {
            return new StreamWindower(
                stream: stream,
                function: function,
                totalDuration: totalDuration,
                smoothingSamples: smoothingSamples);
        }

        public static IBGCStream Window(
            this IBGCStream stream,
            Windowing.Function function = Windowing.Function.Hamming,
            int smoothingSamples = 1000)
        {
            return new StreamWindower(
                stream: stream,
                function: function,
                smoothingSamples: smoothingSamples);
        }

        public static IBGCStream Window(
            this IBGCStream stream,
            Windowing.Function openingFunction,
            Windowing.Function closingFunction,
            int openingSmoothingSamples = 1000,
            int closingSmoothingSamples = 1000,
            int sampleShift = 0,
            int totalChannelSamples = -1)
        {
            return new StreamWindower(
                stream: stream,
                openingFunction: openingFunction,
                closingFunction: closingFunction,
                openingSmoothingSamples: openingSmoothingSamples,
                closingSmoothingSamples: closingSmoothingSamples,
                sampleShift: sampleShift,
                totalChannelSamples: totalChannelSamples);
        }

        public static IBGCStream Truncate(
            this IBGCStream stream,
            double totalDuration,
            int offset = 0,
            TransformRMSBehavior transformRMSBehavior = TransformRMSBehavior.Passthrough)
        {
            return new StreamTruncator(stream, totalDuration, offset, transformRMSBehavior);
        }

        public static IBGCStream Truncate(
            this IBGCStream stream,
            int totalChannelSamples,
            int offset = 0,
            TransformRMSBehavior transformRMSBehavior = TransformRMSBehavior.Passthrough)
        {
            return new StreamTruncator(stream, totalChannelSamples, offset, transformRMSBehavior);
        }

        public static IBGCStream IsolateChannel(
            this IBGCStream stream,
            int channelIndex)
        {
            return new ChannelIsolaterFilter(stream, channelIndex);
        }

        public static IBGCStream Spatialize(
            this IBGCStream stream,
            double angle)
        {
            return new MultiConvolutionFilter(stream, Spatial.GetFilter(angle), TransformRMSBehavior.Passthrough);
        }

        public static IBGCStream MultiConvolve(
            this IBGCStream stream,
            IBGCStream filter,
            TransformRMSBehavior transformRMSBehavior = TransformRMSBehavior.Passthrough)
        {
            return new MultiConvolutionFilter(stream, filter, transformRMSBehavior);
        }

        public static IBGCStream MultiConvolve(
            this IBGCStream stream,
            float[] filter1,
            float[] filter2,
            TransformRMSBehavior transformRMSBehavior = TransformRMSBehavior.Passthrough)
        {
            return new MultiConvolutionFilter(stream, filter1, filter2, transformRMSBehavior);
        }

        public static IBGCStream MultiConvolve(
            this IBGCStream stream,
            double[] filter1,
            double[] filter2,
            TransformRMSBehavior transformRMSBehavior = TransformRMSBehavior.Passthrough)
        {
            return new MultiConvolutionFilter(stream, filter1, filter2, transformRMSBehavior);
        }

        public static IBGCStream Convolve(
            this IBGCStream stream,
            double[] filter,
            TransformRMSBehavior transformRMSBehavior = TransformRMSBehavior.Passthrough)
        {
            return new ConvolutionFilter(stream, filter, transformRMSBehavior);
        }

        public static IBGCStream Normalize(
            this IBGCStream stream,
            double presentationLevel,
            bool safetyLimit = true)
        {
            if (stream.Channels == 1)
            {
                return new NormalizerMonoFilter(stream, presentationLevel, safetyLimit);
            }
            else if (stream.Channels == 2)
            {
                return new NormalizerFilter(stream, presentationLevel, safetyLimit);
            }

            throw new StreamCompositionException("Cannot normalize stream of more than 2 channels");
        }

        public static IBGCStream Normalize(
            this IBGCStream stream,
            (double levelL, double levelR) presentationLevels,
            bool safetyLimit = true)
        {
            if (stream.Channels == 1)
            {
                return new NormalizerMonoFilter(stream, presentationLevels, safetyLimit);
            }
            else if (stream.Channels == 2)
            {
                return new NormalizerFilter(stream, presentationLevels, safetyLimit);
            }

            throw new StreamCompositionException("Cannot normalize stream of more than 2 channels");
        }

        public static IBGCStream SlowRangeFitter(
            this IBGCStream stream)
        {
            return new SlowRangeFitterFilter(stream);
        }

        public static IBGCStream UpChannel(
            this IBGCStream stream,
            int channelCount = 2)
        {
            return new UpChannelMonoFilter(stream, channelCount);
        }

        public static IBGCStream HardClip(
            this IBGCStream stream)
        {
            return new HardClipFilter(stream);
        }

        public static IBGCStream SafeStereoUpChannel(
            this IBGCStream stream)
        {
            switch (stream.Channels)
            {
                case 1: return new UpChannelMonoFilter(stream, 2);
                case 2: return stream;

                default:
                    throw new StreamCompositionException($"Cannot Stereo upchannel a stream with {stream.Channels} channels");
            }
        }

        public static IBGCStream SelectiveUpChannel(
            this IBGCStream stream,
            AudioChannel channels)
        {
            return new StreamSelectiveUpChanneler(stream, channels);
        }

        public static IBGCStream PhaseVocode(
            this IBGCStream stream,
            double speed,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
        {
            return new PhaseVocoder(stream, speed, rmsBehavior);
        }

        public static IBGCStream CarlileShuffle(
            this IBGCStream stream,
            double freqLowerBound = 20.0,
            double freqUpperBound = 16000.0,
            int bandCount = 22,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough,
            System.Random randomizer = null)
        {
            return new CarlileShuffler(
                stream: stream,
                freqLowerBound: freqLowerBound,
                freqUpperBound: freqUpperBound,
                bandCount: bandCount,
                rmsBehavior: rmsBehavior,
                randomizer: randomizer);
        }

        public static IBGCStream CarlileShuffle(
            this IBGCStream stream,
            IEnumerable<double> frequencyDistribution,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough,
            System.Random randomizer = null)
        {
            return new CarlileShuffler(
                stream: stream,
                frequencyDistribution: frequencyDistribution,
                rmsBehavior: rmsBehavior,
                randomizer: randomizer);
        }

        public static IBGCStream AllPass(
            this IBGCStream stream,
            in Complex64 coefficient,
            int delay,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
        {
            return new AllPassFilter(stream, coefficient, delay, rmsBehavior);
        }

        public static IBGCStream FrequencyModulation(
            this IBGCStream stream,
            double modulationRate,
            double modulationDepth)
        {
            return new FrequencyModulationFilter(stream, modulationRate, modulationDepth);
        }

        public static IBGCStream StandardizeRMS(
            this IBGCStream stream,
            double rms = (1.0 / 128.0))
        {
            return new StreamRMSStandardizer(stream, rms);
        }

        public static IBGCStream Fork(
            this IBGCStream stream,
            out IBGCStream forkedStream)
        {
            return new StreamFork(stream, out forkedStream);
        }
        public static IBGCStream Split(
            this IBGCStream stream,
            out IBGCStream splitStream)
        {
            return new StreamChannelSplitter(stream, out splitStream);
        }

        public static IBGCStream ParallelInitialize(
            this IBGCStream stream)
        {
            return new ParallelInitializer(stream);
        }

        public static IBGCStream ApplyEnvelope(
            this IBGCStream stream,
            IBGCEnvelopeStream envelope,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
        {
            return new StreamEnveloper(stream, envelope, rmsBehavior);
        }

        public static IBGCStream TimeShift(
            this IBGCStream stream,
            double timeShift)
        {
            return new StreamTimeShiftFilter(stream, timeShift);
        }

        public static IBGCStream SinglePassPhaseReencode(
            this IBGCStream stream,
            double leftTimeShift,
            double rightTimeShift)
        {
            return new SinglePassPhaseReencoder(stream, leftTimeShift, rightTimeShift);
        }

        public static IBGCStream Loop(this IBGCStream stream)
        {
            return new StreamRepeater(stream);
        }

        public static IBGCStream ToWaveStream(
            this ComplexCarrierTone carrierTone)
        {
            return new SineWave(carrierTone);
        }

        public static IBGCStream ToStream(
            this IEnumerable<ComplexCarrierTone> carrierTones)
        {
            int carrierToneCount = carrierTones.Count();

            if (carrierToneCount == 0)
            {
                return new PerpetualSilence();
            }

            if (carrierToneCount == 1)
            {
                return new SineWave(carrierTones.First());
            }

            if (carrierToneCount < 20)
            {
                return new StreamAdder(carrierTones.Select(ToWaveStream));
            }

            return new ContinuousFrequencyDomainToneComposer(
                carrierTones: carrierTones,
                frameSize: 1 << 11,
                overlapFactor: 8);
        }

        /// <summary>
        /// Immediately collect all of the underlying samples and return as a Complex array
        /// </summary>
        public static Complex64[] ComplexSamples(
            this IBGCStream stream,
            int sampleCount = -1)
        {
            Debug.Assert(stream.Channels == 1);

            if (sampleCount == -1)
            {
                sampleCount = stream.ChannelSamples;
            }

            const int BUFFER_SIZE = 512;
            float[] buffer = new float[BUFFER_SIZE];
            Complex64[] complexSamples = new Complex64[sampleCount];
            int readSamples;
            int offset = 0;

            stream.Reset();

            do
            {
                int samplesToRead = Math.Min(BUFFER_SIZE, sampleCount - offset);
                readSamples = stream.Read(buffer, 0, samplesToRead);

                for (int i = 0; i < readSamples; i++)
                {
                    complexSamples[offset + i] = buffer[i];
                }

                offset += readSamples;

            }
            while (readSamples > 0);

            stream.Reset();

            return complexSamples;
        }

        /// <summary>
        /// Immediately collect all of the underlying samples and return as a Complex array
        /// </summary>
        public static Complex64[] ComplexSamples(
            this IBGCStream stream,
            int channelIndex,
            int sampleCount = -1)
        {
            Debug.Assert(stream.Channels > channelIndex);

            if (sampleCount == -1)
            {
                sampleCount = stream.ChannelSamples;
            }

            const int BUFFER_SIZE = 512;
            float[] buffer = new float[BUFFER_SIZE];
            Complex64[] complexSamples = new Complex64[sampleCount];
            int readSamples;
            int offset = 0;

            stream.Reset();

            do
            {
                int samplesToRead = Math.Min(BUFFER_SIZE, stream.Channels * (sampleCount - offset));
                readSamples = stream.Read(buffer, 0, samplesToRead);
                int readChannelSamples = readSamples / stream.Channels;

                for (int i = 0; i < readChannelSamples; i++)
                {
                    complexSamples[offset + i] = buffer[stream.Channels * i + channelIndex];
                }

                offset += readChannelSamples;

            }
            while (readSamples > 0);

            stream.Reset();

            return complexSamples;
        }

        /// <summary>
        /// Immediately collect all of the underlying samples and return a SimpleAudioClip
        /// that contains them all
        /// </summary>
        /// <returns>New cached audio clip</returns>
        public static SimpleAudioClip Cache(this IBGCStream stream)
        {
            if (stream.ChannelSamples == int.MaxValue)
            {
                Debug.LogError($"Tried to cache infinite stream. Truncating to 10 seconds");

                return stream.Truncate(10.0).Cache();
            }

            stream.Reset();
            float[] samples = new float[stream.TotalSamples];
            stream.Read(samples, 0, stream.TotalSamples);
            stream.Reset();

            return new SimpleAudioClip(samples, stream.Channels);
        }

        /// <summary>
        /// Immediately collect all of the underlying samples and return a SimpleAudioClip
        /// that contains them all
        /// </summary>
        /// <returns>New cached audio clip</returns>
        public static SimpleAudioClip SafeCache(this IBGCStream stream)
        {
            List<float> samples = new List<float>();

            const int BUFFER_SIZE = 512;
            float[] buffer = new float[BUFFER_SIZE];
            int readSamples;

            stream.Reset();
            do
            {
                readSamples = stream.Read(buffer, 0, BUFFER_SIZE);
                for (int i = 0; i < readSamples; i++)
                {
                    samples.Add(buffer[i]);
                }

            }
            while (readSamples > 0);

            stream.Reset();

            return new SimpleAudioClip(samples.ToArray(), stream.Channels);
        }
    }
}
