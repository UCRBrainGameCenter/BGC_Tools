using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Performs continuous Noise Vocoding on the underlying stream
    /// </summary>
    public class NoiseVocoder : SimpleBGCFilter
    {
        public override int Channels => stream.Channels;
        public override int TotalSamples => stream.TotalSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        private readonly double noiseScalarA;
        private readonly double noiseScalarB;

        private readonly float[] inputBuffer;
        private readonly double[] noiseBuffer;
        private readonly double[] window;

        private readonly Complex64[] noiseFFTBuffer;
        private readonly Complex64[] signalFFTBuffer;

        private readonly Complex64[][] amplitudeBuffers;
        private readonly Complex64[][] noiseBandBuffers;

        private readonly double[] outputAccumulation;
        private readonly float[] cachedSampleBuffer;

        private readonly Random randomizer;

        private readonly double[] bandFrequencies;

        private readonly int fftSize;
        private readonly int overlapRatio;
        private readonly int stepSize;
        private readonly int overlapSize;
        private readonly double outputFactor;

        private int bufferIndex = 0;
        private int bufferCount = 0;
        private readonly TransformRMSBehavior rmsBehavior;

        private int frameLag = 0;
        private int samplesHandled = 0;

        public NoiseVocoder(
            IBGCStream stream,
            double freqLowerBound = 20.0,
            double freqUpperBound = 16000.0,
            int bandCount = 22,
            int fftSize = 4096,
            int overlapRatio = 4,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough,
            Random randomizer = null)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"Noise Vocoder requires a mono input stream. Input stream has {stream.Channels} channels.");
            }

            this.fftSize = fftSize;
            this.overlapRatio = overlapRatio;
            stepSize = fftSize / overlapRatio;
            overlapSize = fftSize - stepSize;

            inputBuffer = new float[fftSize];
            noiseBuffer = new double[fftSize];
            outputAccumulation = new double[fftSize];
            cachedSampleBuffer = new float[stepSize];

            noiseFFTBuffer = new Complex64[fftSize];
            signalFFTBuffer = new Complex64[fftSize];

            amplitudeBuffers = new Complex64[bandCount][];
            noiseBandBuffers = new Complex64[bandCount][];

            for (int i = 0; i < bandCount; i++)
            {
                amplitudeBuffers[i] = new Complex64[fftSize];
                noiseBandBuffers[i] = new Complex64[fftSize];
            }

            initialized = false;

            noiseScalarA = Math.Sqrt(1.0 / 3.0);
            noiseScalarB = 2.0 * noiseScalarA;

            double[] windowTemplate = Windowing.GetHalfWindow64(Windowing.Function.BlackmanHarris, fftSize / 2);

            window = new double[fftSize];
            for (int i = 0; i < fftSize / 2; i++)
            {
                window[i] = windowTemplate[i];
                window[fftSize - i - 1] = windowTemplate[i];
            }

            this.randomizer = randomizer ?? new Random(CustomRandom.Next());
            this.rmsBehavior = rmsBehavior;

            bandFrequencies = GetExponentialDistribution(freqLowerBound, freqUpperBound, bandCount).ToArray();

            outputFactor = 0.5 * Math.Sqrt(fftSize) / overlapRatio;
        }

        protected override void _Initialize()
        {
            frameLag = overlapRatio;
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int samplesWritten = ReadBody(data, offset, count);

            while (samplesWritten < count)
            {
                //Slide over noise samples
                Array.Copy(
                    sourceArray: noiseBuffer,
                    sourceIndex: stepSize,
                    destinationArray: noiseBuffer,
                    destinationIndex: 0,
                    length: overlapSize);

                int read = stream.Read(inputBuffer, overlapSize, stepSize);

                if (read <= 0 && samplesHandled <= 0)
                {
                    //Done, No samples left to work with
                    break;
                }
                else if (read <= 0)
                {
                    //We are in buffer-dumping window
                    //Set rest of inputBuffer to zero
                    Array.Clear(inputBuffer, overlapSize, stepSize);
                }
                else if (read < stepSize)
                {
                    //Near or at the end
                    //Set rest of inputBuffer to zero
                    Array.Clear(inputBuffer, overlapSize + read, inputBuffer.Length - overlapSize - read);
                }

                //Generate new noise
                for (int i = 0; i < stepSize; i++)
                {
                    noiseBuffer[overlapSize + i] = noiseScalarA - noiseScalarB * randomizer.NextDouble();
                }

                //Copy in the input data
                for (int i = 0; i < fftSize; i++)
                {
                    signalFFTBuffer[i] = inputBuffer[i] * window[i];
                    noiseFFTBuffer[i] = noiseBuffer[i];
                }

                //FFT
                Task.WaitAll(
                    Task.Run(() => Fourier.Forward(signalFFTBuffer)),
                    Task.Run(() => Fourier.Forward(noiseFFTBuffer)));

                //For each band...

                Parallel.For(
                    fromInclusive: 0,
                    toExclusive: bandFrequencies.Length - 1,
                    body: (int band) =>
                    {
                        int lowerBound = FrequencyDomain.GetComplexFrequencyBin(fftSize, bandFrequencies[band]);
                        int upperBound = FrequencyDomain.GetComplexFrequencyBin(fftSize, bandFrequencies[band + 1]);

                        Complex64[] amplitudeBuffer = amplitudeBuffers[band];
                        Complex64[] noiseBandBuffer = noiseBandBuffers[band];

                        //Copy over band just the relevant frequency band
                        for (int i = lowerBound; i < upperBound; i++)
                        {
                            amplitudeBuffer[i] = 2.0 * signalFFTBuffer[i];
                            noiseBandBuffer[i] = 2.0 * noiseFFTBuffer[i];
                        }

                        Complex64 zero = Complex64.Zero;


                        //Clear rest of buffers
                        for (int i = 0; i < lowerBound; i++)
                        {
                            amplitudeBuffer[i] = zero;
                            noiseBandBuffer[i] = zero;
                        }

                        for (int i = upperBound; i < amplitudeBuffer.Length; i++)
                        {
                            amplitudeBuffer[i] = zero;
                            noiseBandBuffer[i] = zero;
                        }

                        //IFFT
                        Task.WaitAll(
                            Task.Run(() => Fourier.Inverse(amplitudeBuffer)),
                            Task.Run(() => Fourier.Inverse(noiseBandBuffer)));

                        for (int i = 0; i < amplitudeBuffer.Length; i++)
                        {
                            outputAccumulation[i] += outputFactor * window[i] * noiseBandBuffer[i].Real * amplitudeBuffer[i].Magnitude;
                        }
                    });


                samplesHandled += read;

                if (--frameLag <= 0)
                {
                    bufferIndex = 0;
                    bufferCount = Math.Min(stepSize, samplesHandled);
                    samplesHandled -= bufferCount;

                    //Copy output samples to output buffer
                    for (int sample = 0; sample < bufferCount; sample++)
                    {
                        cachedSampleBuffer[sample] = (float)outputAccumulation[sample];
                    }
                }

                //Slide over input samples
                Array.Copy(
                    sourceArray: inputBuffer,
                    sourceIndex: stepSize,
                    destinationArray: inputBuffer,
                    destinationIndex: 0,
                    length: overlapSize);

                //Slide output samples
                Array.Copy(
                    sourceArray: outputAccumulation,
                    sourceIndex: stepSize,
                    destinationArray: outputAccumulation,
                    destinationIndex: 0,
                    length: overlapSize);

                //Clear empty output accumulation region
                Array.Clear(outputAccumulation, overlapSize, stepSize);

                samplesWritten += ReadBody(data, offset + samplesWritten, count - samplesWritten);
            }

            return samplesWritten;
        }

        private int ReadBody(float[] buffer, int offset, int count)
        {
            int samplesWritten = Math.Max(0, Math.Min(count, bufferCount - bufferIndex));

            Array.Copy(
                sourceArray: cachedSampleBuffer,
                sourceIndex: bufferIndex,
                destinationArray: buffer,
                destinationIndex: offset,
                length: samplesWritten);

            bufferIndex += samplesWritten;

            return samplesWritten;
        }

        private void ClearBuffers()
        {
            bufferIndex = 0;
            bufferCount = 0;
            samplesHandled = 0;
            frameLag = overlapRatio;

            Array.Clear(cachedSampleBuffer, 0, cachedSampleBuffer.Length);
            Array.Clear(outputAccumulation, 0, outputAccumulation.Length);
            Array.Clear(inputBuffer, 0, inputBuffer.Length);

            for (int i = 0; i < overlapSize; i++)
            {
                noiseBuffer[i] = noiseScalarA - noiseScalarB * randomizer.NextDouble();
            }
        }

        public override void Reset()
        {
            ClearBuffers();
            stream.Reset();
        }

        public override void Seek(int position)
        {
            ClearBuffers();
            stream.Seek(position);
        }

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

        private static IEnumerable<double> GetExponentialDistribution(
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
