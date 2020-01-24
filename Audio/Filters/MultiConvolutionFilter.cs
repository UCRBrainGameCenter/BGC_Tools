using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Performs a convolution on the underlying stream, generating one independent channel for each
    /// supplied filter.
    /// </summary>
    public class MultiConvolutionFilter : SimpleBGCFilter
    {
        const int FFT_EXP_MIN = 9;
        const int FFT_SIZE_BUMP = 1;

        private readonly float[] inputBuffer;
        private readonly Complex64[][] filterFD;
        private readonly Complex64[] fftBuffer;
        private readonly Complex64[] ifftBuffer;
        private readonly float[] outputAccumulation;

        private readonly int fftLength;
        private readonly int samplesPerOverlap;

        private readonly int filterLength;

        private int bufferIndex = 0;
        private int bufferCount = 0;
        private readonly TransformRMSBehavior rmsBehavior;

        public override int Channels { get; }

        public override int TotalSamples { get; }

        public override int ChannelSamples { get; }

        public MultiConvolutionFilter(
            IBGCStream stream,
            float[] filterL,
            float[] filterR,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"MultiConvolutionFilter expects a mono input stream. Input stream had {stream.Channels} channels.");
            }

            if (filterL.Length != filterR.Length)
            {
                throw new StreamCompositionException(
                    $"MultiConvolutionFilter expects filterL and filterR lengths match. Filter lengths: {filterL.Length} {filterR.Length}.");
            }

            filterLength = filterL.Length;
            Channels = 2;

            int fftSize = Math.Max(FFT_EXP_MIN, filterLength.ToNextExponentOf2() + FFT_SIZE_BUMP);
            fftLength = 1 << fftSize;
            samplesPerOverlap = fftLength - filterLength;

            //Protect against infinite streams
            if (stream.ChannelSamples == int.MaxValue)
            {
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                ChannelSamples = filterLength + stream.ChannelSamples - 1;
                TotalSamples = Channels * ChannelSamples;
            }

            inputBuffer = new float[samplesPerOverlap];
            outputAccumulation = new float[Channels * fftLength];

            fftBuffer = new Complex64[fftLength];
            ifftBuffer = new Complex64[fftLength];
            filterFD = new Complex64[Channels][];
            filterFD[0] = new Complex64[fftLength];
            filterFD[1] = new Complex64[fftLength];

            for (int i = 0; i < filterLength; i++)
            {
                filterFD[0][i] = filterL[i];
                filterFD[1][i] = filterR[i];
            }

            initialized = false;

            this.rmsBehavior = rmsBehavior;
        }

        public MultiConvolutionFilter(
            IBGCStream stream,
            double[] filterL,
            double[] filterR,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"MultiConvolutionFilter expects a mono input stream. Input stream had {stream.Channels} channels.");
            }

            if (filterL.Length != filterR.Length)
            {
                throw new StreamCompositionException(
                    $"MultiConvolutionFilter expects filterL and filterR lengths match. Filter lengths: {filterL.Length} {filterR.Length}.");
            }

            filterLength = filterL.Length;
            Channels = 2;

            int fftSize = Math.Max(FFT_EXP_MIN, filterLength.ToNextExponentOf2() + FFT_SIZE_BUMP);
            fftLength = 1 << fftSize;
            samplesPerOverlap = fftLength - filterLength;

            //Protect against infinite streams
            if (stream.ChannelSamples == int.MaxValue)
            {
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                ChannelSamples = filterLength + stream.ChannelSamples - 1;
                TotalSamples = Channels * ChannelSamples;
            }

            inputBuffer = new float[samplesPerOverlap];
            outputAccumulation = new float[Channels * fftLength];

            fftBuffer = new Complex64[fftLength];
            ifftBuffer = new Complex64[fftLength];
            filterFD = new Complex64[Channels][];
            filterFD[0] = new Complex64[fftLength];
            filterFD[1] = new Complex64[fftLength];

            for (int i = 0; i < filterLength; i++)
            {
                filterFD[0][i] = filterL[i];
                filterFD[1][i] = filterR[i];
            }

            initialized = false;

            this.rmsBehavior = rmsBehavior;
        }

        public MultiConvolutionFilter(
            IBGCStream stream,
            IBGCStream filter,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (stream.Channels != 1)
            {
                throw new StreamCompositionException(
                    $"MultiConvolutionFilter expects a mono input stream. Input stream had {stream.Channels} channels.");
            }

            filterLength = filter.ChannelSamples;
            Channels = filter.Channels;

            int fftSize = Math.Max(FFT_EXP_MIN, filterLength.ToNextExponentOf2() + FFT_SIZE_BUMP);
            fftLength = 1 << fftSize;
            samplesPerOverlap = fftLength - filterLength;

            //Protect against infinite streams
            if (stream.ChannelSamples == int.MaxValue)
            {
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                ChannelSamples = filterLength + stream.ChannelSamples - 1;
                TotalSamples = Channels * ChannelSamples;
            }

            inputBuffer = new float[samplesPerOverlap];
            outputAccumulation = new float[Channels * fftLength];

            fftBuffer = new Complex64[fftLength];
            ifftBuffer = new Complex64[fftLength];
            filterFD = new Complex64[Channels][];

            for (int i = 0; i < Channels; i++)
            {
                filterFD[i] = filter.ComplexSamples(i, fftLength);
            }

            initialized = false;

            this.rmsBehavior = rmsBehavior;
        }

        protected override void _Initialize()
        {
            double factor = fftLength;

            for (int i = 0; i < Channels; i++)
            {
                Fourier.Forward(filterFD[i]);

                for (int j = 0; j < fftLength; j++)
                {
                    filterFD[i][j] *= factor;
                }
            }
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
                int read = stream.Read(inputBuffer, 0, inputBuffer.Length);

                if (read <= 0)
                {
                    //Done, No samples left to work with
                    break;
                }

                //Slide output samples over to accumulate on the remainder
                Array.Copy(
                    sourceArray: outputAccumulation,
                    sourceIndex: bufferCount,
                    destinationArray: outputAccumulation,
                    destinationIndex: 0,
                    length: outputAccumulation.Length - bufferCount);

                Array.Clear(
                    array: outputAccumulation,
                    index: outputAccumulation.Length - bufferCount,
                    length: bufferCount);

                bufferIndex = 0;
                bufferCount = Channels * samplesPerOverlap;


                if (read < inputBuffer.Length)
                {
                    //Copy all the remaining samples
                    //We are guaranteed to have enough room because the output buffer's
                    //length is Channels * (inputBuffer.Length + fftLength)
                    bufferCount = Channels * (read + filterLength - 1);
                    //Set rest of inputBuffer to zero
                    Array.Clear(inputBuffer, read, inputBuffer.Length - read);
                }

                for (int i = 0; i < samplesPerOverlap; i++)
                {
                    fftBuffer[i] = inputBuffer[i];
                }

                Array.Clear(fftBuffer, samplesPerOverlap, fftLength - samplesPerOverlap);

                //FFT
                Fourier.Forward(fftBuffer);

                for (int channel = 0; channel < Channels; channel++)
                {
                    for (int i = 0; i < fftLength; i++)
                    {
                        ifftBuffer[i] = fftBuffer[i] * filterFD[channel][i];
                    }

                    //IFFT
                    Fourier.Inverse(ifftBuffer);

                    //Accumualte the window samples
                    for (int i = 0; i < fftLength; i++)
                    {
                        outputAccumulation[Channels * i + channel] += (float)ifftBuffer[i].Real;
                    }
                }

                samplesWritten += ReadBody(data, offset + samplesWritten, count - samplesWritten);
            }

            return samplesWritten;
        }

        private int ReadBody(float[] buffer, int offset, int count)
        {
            int samplesWritten = GeneralMath.Clamp(count, 0, bufferCount - bufferIndex);

            Array.Copy(
                sourceArray: outputAccumulation,
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

            Array.Clear(outputAccumulation, 0, outputAccumulation.Length);
            Array.Clear(inputBuffer, 0, inputBuffer.Length);
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
    }
}
