using System;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    public static class AnalyticStreamExtensions
    {
        /// <summary>Returns the AnalyticStream augmented with an BGCStreamConverter</summary>
        public static IBGCStream ToBGCStream(this IAnalyticStream stream) => 
            new BGCStreamConverter(stream);

        /// <summary>Returns the BGCStream augmented with an AnalyticStreamConverter</summary>
        public static IAnalyticStream ToAnalyticStream(this IBGCStream stream) => 
            new AnalyticStreamConverter(stream);

        /// <summary>Returns the BGCStream equivalent of the AnalyticStream's Envelope</summary>
        public static IBGCStream ToEnvelopeStream(this IAnalyticStream stream) =>
            new AnalyticStreamEnvelopeConverter(stream);

        /// <summary>Returns the AnalyticStream augmented with a FrequencyModulationFilter</summary>
        public static IAnalyticStream FrequencyModulation(
            this IAnalyticStream stream,
            double modulationRate,
            double modulationDepth)
        {
            return new AnalyticFrequencyModulationFilter(stream, modulationRate, modulationDepth);
        }

        public static IAnalyticStream FrequencyShift(
            this IAnalyticStream stream,
            double frequencyShift)
        {
            return new AnalyticStreamFrequencyShifter(stream, frequencyShift);
        }

        public static IAnalyticStream Window(
            this IAnalyticStream stream,
            double totalDuration,
            Windowing.Function function = Windowing.Function.Hamming,
            int smoothingSamples = 1000)
        {
            return new AnalyticStreamWindower(stream, function,
                totalDuration: totalDuration,
                smoothingSamples: smoothingSamples);
        }

        public static IAnalyticStream Window(
            this IAnalyticStream stream,
            Windowing.Function function = Windowing.Function.Hamming,
            int smoothingSamples = 1000)
        {
            return new AnalyticStreamWindower(stream, function,
                smoothingSamples: smoothingSamples);
        }

        public static IAnalyticStream Add(
            this IAnalyticStream stream,
            IAnalyticStream other)
        {
            return new AnalyticStreamAdder(stream, other);
        }

        public static IAnalyticStream Add(
            this IAnalyticStream stream,
            params IAnalyticStream[] others)
        {
            AnalyticStreamAdder adder = new AnalyticStreamAdder(stream);
            adder.AddStreams(others);
            return adder;
        }

        public static IAnalyticStream Fork(
            this IAnalyticStream stream,
            out IAnalyticStream forkedStream)
        {
            return new AnalyticStreamFork(stream, out forkedStream);
        }

        /// <summary>
        /// Returns an AnalyticStream at least <paramref name="minimumDuration"/> in duration, with the clip
        /// centered in it
        /// </summary>
        public static IAnalyticStream Center(
            this IAnalyticStream stream,
            double minimumDuration = 0.0)
        {
            return new AnalyticStreamCenterer(stream, Math.Max(stream.Duration(), minimumDuration));
        }

        public static IAnalyticStream Center(
            this IAnalyticStream stream,
            int preDelaySamples,
            int postDelaySamples)
        {
            return new AnalyticStreamCenterer(stream, preDelaySamples, postDelaySamples);
        }

        public static IAnalyticStream ADSR(
            this IAnalyticStream stream,
            double timeToPeak,
            double timeToSustain,
            double sustainAmplitude,
            double sustainDecayTime)
        {
            return new AnalyticADSREnvelope(
                stream: stream,
                timeToPeak: timeToPeak,
                timeToSustain: timeToSustain,
                sustainAmplitude: sustainAmplitude,
                sustainDecayTime: sustainDecayTime,
                releaseDecayTime: sustainDecayTime);
        }

        public static IAnalyticStream ADSR(
            this IAnalyticStream stream,
            double timeToPeak,
            double timeToSustain,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
        {
            return new AnalyticADSREnvelope(
                stream: stream,
                timeToPeak: timeToPeak,
                timeToSustain: timeToSustain,
                sustainAmplitude: sustainAmplitude,
                sustainDecayTime: sustainDecayTime,
                releaseDecayTime: releaseDecayTime);
        }

        public static IAnalyticStream ADSR(
            this IAnalyticStream stream,
            double timeToPeak,
            double timeToSustain,
            double timeToRelease,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
        {
            return new AnalyticADSREnvelope(
                stream: stream,
                timeToPeak: timeToPeak,
                timeToSustain: timeToSustain,
                timeToRelease: timeToRelease,
                sustainAmplitude: sustainAmplitude,
                sustainDecayTime: sustainDecayTime,
                releaseDecayTime: releaseDecayTime);
        }

        /// <summary> Calculates the effective Duration of the AnalyticStream </summary>
        public static double Duration(this IAnalyticStream stream) =>
            stream.Samples / stream.SamplingRate;


        /// <summary> Slowest Backup Alternative for calculating RMS </summary>
        public static double CalculateRMS(this IAnalyticStream stream)
        {
            if (stream.Samples == 0)
            {
                return 0.0;
            }

            if (stream.Samples == int.MaxValue)
            {
                return double.NaN;
            }

            double rms = 0.0;
            int readSamples;
            const int BUFFER_SIZE = 512;
            Complex64[] buffer = new Complex64[BUFFER_SIZE];

            stream.Reset();

            do
            {
                readSamples = stream.Read(buffer, 0, BUFFER_SIZE);

                for (int i = 0; i < readSamples; i++)
                {
                    rms += buffer[i].Real * buffer[i].Real;
                }

            }
            while (readSamples > 0);

            stream.Reset();

            return Math.Sqrt(rms / stream.Samples);
        }
    }
}
