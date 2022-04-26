using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Performs a convolution on the underlying stream, using Overlap-And-Add
    /// </summary>
    public class ConvolutionFilter : SimpleBGCFilter
    {
        const int FFT_EXP_MIN = 9;
        const int FFT_SIZE_BUMP = 1;

        private readonly float[] inputBuffer;
        private readonly Complex64[] filterFD;
        private readonly Complex64[] fftBuffer;
        private readonly float[] outputAccumulation;

        private readonly int fftLength;
        private readonly int samplesPerOverlap;

        private readonly int filterLength;

        private int bufferIndex = 0;
        private int bufferCount = 0;
        private readonly TransformRMSBehavior rmsBehavior;

        public override int Channels => stream.Channels;

        public override int TotalSamples { get; }

        public override int ChannelSamples { get; }

        public ConvolutionFilter(
            IBGCStream stream,
            float[] filter,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            filterLength = filter.Length;

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

            inputBuffer = new float[Channels * samplesPerOverlap];
            outputAccumulation = new float[Channels * fftLength];

            fftBuffer = new Complex64[fftLength];
            filterFD = new Complex64[fftLength];

            for (int i = 0; i < filterLength; i++)
            {
                filterFD[i] = filter[i];
            }

            initialized = false;

            this.rmsBehavior = rmsBehavior;
        }

        public ConvolutionFilter(
            IBGCStream stream,
            double[] filter,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            filterLength = filter.Length;

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

            inputBuffer = new float[Channels * samplesPerOverlap];
            outputAccumulation = new float[Channels * fftLength];

            fftBuffer = new Complex64[fftLength];
            filterFD = new Complex64[fftLength];

            for (int i = 0; i < filterLength; i++)
            {
                filterFD[i] = filter[i];
            }

            initialized = false;

            this.rmsBehavior = rmsBehavior;
        }

        public ConvolutionFilter(
            IBGCStream stream,
            IBGCStream filter,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            if (filter.Channels != 1)
            {
                throw new StreamCompositionException($"ConvolutionFilter expects a single-channel filter.");
            }

            filterLength = filter.ChannelSamples;

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

            inputBuffer = new float[Channels * samplesPerOverlap];
            outputAccumulation = new float[Channels * fftLength];

            fftBuffer = new Complex64[fftLength];
            filterFD = filter.ComplexSamples(fftLength);

            initialized = false;

            this.rmsBehavior = rmsBehavior;
        }

        protected override void _Initialize()
        {
            Fourier.Forward(filterFD);

            double factor = filterLength;

            for (int i = 0; i < filterLength; i++)
            {
                filterFD[i] *= factor;
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
                    bufferCount = read + Channels * (filterLength - 1);
                    //Set rest of inputBuffer to zero
                    Array.Clear(inputBuffer, read, inputBuffer.Length - read);
                }

                for (int channel = 0; channel < Channels; channel++)
                {
                    for (int i = 0; i < samplesPerOverlap; i++)
                    {
                        fftBuffer[i] = inputBuffer[Channels * i + channel];
                    }

                    Array.Clear(fftBuffer, samplesPerOverlap, fftLength - samplesPerOverlap);

                    //FFT
                    Fourier.Forward(fftBuffer);

                    for (int i = 0; i < fftLength; i++)
                    {
                        fftBuffer[i] *= filterFD[i];
                    }

                    //IFFT
                    Fourier.Inverse(fftBuffer);

                    //Accumualte the window samples
                    for (int i = 0; i < fftLength; i++)
                    {
                        outputAccumulation[Channels * i + channel] += (float)fftBuffer[i].Real;
                    }
                }

                samplesWritten += ReadBody(data, offset + samplesWritten, count - samplesWritten);
            }

            return samplesWritten;
        }

        private int ReadBody(float[] buffer, int offset, int count)
        {
            int samplesWritten = Math.Max(0, Math.Min(count, bufferCount - bufferIndex));

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

        private IEnumerable<double> channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (channelRMS == null)
            {
                switch (rmsBehavior)
                {
                    case TransformRMSBehavior.Recalculate:
                        channelRMS = this.CalculateRMS();
                        break;

                    case TransformRMSBehavior.Passthrough:
                        channelRMS = stream.GetChannelRMS();
                        break;

                    default:
                        throw new Exception($"Unexpected rmsBehavior: {rmsBehavior}");
                }
            }

            return channelRMS;
        }
    }
}
