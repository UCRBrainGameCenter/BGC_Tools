using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// A Decorator class for BGCAudioClips that applies the Carlile noise-generation technique
    /// </summary>
    public class CarlileShuffler : SimpleBGCFilter
    {
        public override int Channels => 1;

        public override int TotalSamples => stream.TotalSamples;

        public override int ChannelSamples => stream.ChannelSamples;

        public override float SamplingRate => stream.SamplingRate;

        private int RemainingSamples => ChannelSamples - Position;

        public int Position { get; private set; } = 0;
        public float[] Samples { get; private set; } = null;

        private readonly TransformRMSBehavior rmsBehavior;

        private readonly Random randomizer;

        private readonly IEnumerable<double> frequencyDistribution;

        public CarlileShuffler(
            IBGCStream stream,
            double freqLowerBound = 20.0,
            double freqUpperBound = 16000.0,
            int bandCount = 22,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough,
            Random randomizer = null)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"Carlile Shuffler requires a mono input stream. Input stream has {stream.Channels} channels.");
            }

            if (stream.ChannelSamples == int.MaxValue)
            {
                throw new StreamCompositionException(
                    $"Carlile Shuffler cannot be performed on a stream of infinite duration. Try truncating first.");
            }

            this.randomizer = randomizer ?? new Random(CustomRandom.Next());
            this.rmsBehavior = rmsBehavior;

            frequencyDistribution = GetExponentialDistribution(freqLowerBound, freqUpperBound, bandCount);
        }

        public CarlileShuffler(
            IBGCStream stream,
            IEnumerable<double> frequencyDistribution,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough,
            Random randomizer = null)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"Carlile Shuffler requires a mono input stream. Input stream has {stream.Channels} channels.");
            }

            if (stream.ChannelSamples == int.MaxValue)
            {
                throw new StreamCompositionException(
                    $"Carlile Shuffler cannot be performed on a stream of infinite duration. Try truncating first.");
            }

            this.randomizer = randomizer ?? new Random(CustomRandom.Next());
            this.rmsBehavior = rmsBehavior;

            this.frequencyDistribution = frequencyDistribution;
        }

        protected override void _Initialize()
        {
            Complex64[] samples = stream.ComplexSamples();
            int bufferLength = samples.Length;

            Fourier.Forward(samples);

            IEnumerator<double> distribution = frequencyDistribution.GetEnumerator();
            distribution.MoveNext();

            int lowerBound = FrequencyDomain.GetComplexFrequencyBin(bufferLength, distribution.Current);

            while (distribution.MoveNext())
            {
                int upperBound = FrequencyDomain.GetComplexFrequencyBin(bufferLength, distribution.Current);

                //Generate random offset for the range
                double offset = 2 * Math.PI * randomizer.NextDouble();

                for (int i = lowerBound; i < upperBound; i++)
                {
                    samples[i] *= Complex64.FromPolarCoordinates(2.0, i * offset);
                }

                lowerBound = upperBound;
            }

            for (int i = bufferLength / 2; i < bufferLength; i++)
            {
                samples[i] = 0.0;
            }

            Fourier.Inverse(samples);

            Samples = new float[bufferLength];

            for (int i = 0; i < bufferLength; i++)
            {
                Samples[i] = (float)samples[i].Real;
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int samplesToCopy = Math.Min(count, RemainingSamples);

            Array.Copy(
                sourceArray: Samples,
                sourceIndex: Position,
                destinationArray: data,
                destinationIndex: offset,
                length: samplesToCopy);

            Position += samplesToCopy;

            return samplesToCopy;
        }

        public override void Reset() => Position = 0;

        public override void Seek(int position) => Position = position;

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        _channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        _channelRMS = stream.GetChannelRMS();
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return _channelRMS;
        }

        #region Helper Generator

        private IEnumerable<double> GetExponentialDistribution(
            double freqLowerBound,
            double freqUpperBound, 
            int bandCount)
        {
            double freqRatio = Math.Pow((freqUpperBound / freqLowerBound), 1.0 / bandCount);
            if (double.IsNaN(freqRatio) || double.IsInfinity(freqRatio))
            {
                freqRatio = 1.0;
            }

            double freq = freqLowerBound;

            for (int carrierTone = 0; carrierTone < bandCount + 1; carrierTone++)
            {
                yield return freq;

                freq *= freqRatio;
            }
        }

        #endregion Helper Generator
    }
}
