using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;

using static System.Math;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Applies an ITD by adjusting the phase in the frequency domain, frame-based
    /// 
    /// This is close, but isn't ready for primetime yet.
    /// </summary>
    public class FramedPhaseReencoder : SimpleBGCFilter
    {
        private readonly int frameSize;
        private readonly int overlapFactor;

        private readonly int halfFrameSize;

        private readonly int stepSize;
        private readonly int overlap;

        private readonly double outputScalar;

        /// <summary>
        /// Holds samples from underlying stream
        /// </summary>
        private readonly float[] localSampleBuffer;

        /// <summary>
        /// Store computed samples ready to deliver
        /// </summary>
        private readonly float[] cachedSampleBuffer;

        private readonly Complex64[][] phasors;
        private readonly float[][] inputBuffers;
        private readonly Complex64[] fftBuffer;
        private readonly float[] outputAccumulation;

        private readonly double[] windowInput;

        private readonly double[] timeShifts;

        private int bufferIndex = 0;
        private int bufferCount = 0;

        public override int Channels { get; }

        public override int TotalSamples { get; }

        public override int ChannelSamples => stream.ChannelSamples;

        public FramedPhaseReencoder(
            IBGCStream stream,
            int frameSize,
            int overlapFactor,
            double timeShift)
            : base(stream)
        {
            UnityEngine.Debug.LogWarning("FramedPhaseReencoder isn't really ready... use at your own risk.");

            this.frameSize = frameSize;
            this.overlapFactor = overlapFactor;

            halfFrameSize = frameSize / 2;

            stepSize = frameSize / this.overlapFactor;
            overlap = frameSize - stepSize;

            Channels = stream.Channels;

            if (stream.ChannelSamples == int.MaxValue)
            {
                TotalSamples = int.MaxValue;
            }
            else
            {
                TotalSamples = Channels * stream.ChannelSamples;
            }

            localSampleBuffer = new float[Channels * stepSize];
            cachedSampleBuffer = new float[Channels * stepSize];

            phasors = new Complex64[Channels][];
            inputBuffers = new float[Channels][];
            outputAccumulation = new float[Channels * frameSize];

            fftBuffer = new Complex64[frameSize];

            timeShifts = Enumerable.Repeat(timeShift, Channels).ToArray();
            for (int i = 0; i < Channels; i++)
            {
                inputBuffers[i] = new float[frameSize];

                double rotationFactor = -2.0 * PI * SamplingRate * timeShifts[i] / frameSize;
                for (int j = 1; j < halfFrameSize; j++)
                {
                    //Initialize phasors to 2 so that it doubles the amplitudes on copy and rotation
                    phasors[i][j] = Complex64.FromPolarCoordinates(2.0, j * rotationFactor);
                }

                phasors[i][0] = 1.0;
            }

            outputScalar = 1.0 / (this.overlapFactor);

            windowInput = new double[frameSize];

            for (int i = 0; i < frameSize; i++)
            {
                //Hamming
                windowInput[i] = 0.54 - 0.46 * Cos(2.0 * PI * i / (frameSize - 1));
            }
        }

        public FramedPhaseReencoder(
            IBGCStream stream,
            int frameSize,
            int overlapFactor,
            double leftTimeShift,
            double rightTimeShift)
            : base(StereoifyStream(stream))
        {
            UnityEngine.Debug.LogWarning("FramedPhaseReencoder isn't really ready... use at your own risk.");

            this.frameSize = frameSize;
            this.overlapFactor = overlapFactor;

            halfFrameSize = frameSize / 2;

            stepSize = frameSize / this.overlapFactor;
            overlap = frameSize - stepSize;

            Channels = this.stream.Channels;

            if (this.stream.ChannelSamples == int.MaxValue)
            {
                TotalSamples = int.MaxValue;
            }
            else
            {
                TotalSamples = Channels * this.stream.ChannelSamples;
            }

            localSampleBuffer = new float[Channels * stepSize];
            cachedSampleBuffer = new float[Channels * stepSize];

            phasors = new Complex64[Channels][];
            inputBuffers = new float[Channels][];
            outputAccumulation = new float[Channels * frameSize];

            fftBuffer = new Complex64[frameSize];

            timeShifts = new double[] { leftTimeShift, rightTimeShift };
            for (int i = 0; i < Channels; i++)
            {
                inputBuffers[i] = new float[frameSize];

                double rotationFactor = -2.0 * PI * SamplingRate * timeShifts[i] / frameSize;
                for (int j = 1; j < halfFrameSize; j++)
                {
                    //Initialize phasors to 2 so that it doubles the amplitudes on copy and rotation
                    phasors[i][j] = Complex64.FromPolarCoordinates(2.0, j * rotationFactor);
                }

                phasors[i][0] = 1.0;
            }

            outputScalar = 2.0 / (this.overlapFactor * Sqrt(frameSize));

            windowInput = new double[frameSize];

            for (int i = 0; i < frameSize; i++)
            {
                //Hamming
                windowInput[i] = 0.54 - 0.46 * Cos(2.0 * PI * i / (frameSize - 1));
            }
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesWritten = ReadBody(data, offset, count);

            while (samplesWritten < count)
            {
                int read = stream.Read(localSampleBuffer, 0, localSampleBuffer.Length);

                if (read <= 0)
                {
                    //Done
                    break;
                }
                else if (read < localSampleBuffer.Length)
                {
                    //Set rest to zero
                    Array.Clear(localSampleBuffer, read, localSampleBuffer.Length - read);
                }

                bufferIndex = 0;
                bufferCount = Channels * stepSize;

                for (int channel = 0; channel < Channels; channel++)
                {
                    //Slide input samples over
                    Array.Copy(
                        sourceArray: inputBuffers[channel],
                        sourceIndex: stepSize,
                        destinationArray: inputBuffers[channel],
                        destinationIndex: 0,
                        length: overlap);

                    //Copy new samples into buffer
                    for (int i = 0; i < stepSize; i++)
                    {
                        inputBuffers[channel][overlap + i] = localSampleBuffer[Channels * i + channel];
                    }

                    //Copy and Window into fftbuffer
                    for (int i = 0; i < frameSize; i++)
                    {
                        fftBuffer[i] = inputBuffers[channel][i] * windowInput[i];
                    }

                    //FFT
                    Fourier.Forward(fftBuffer);

                    //Copy values into IFFT Buffer
                    for (int i = 0; i < halfFrameSize; i++)
                    {
                        fftBuffer[i] *= phasors[channel][i];
                    }

                    Array.Clear(
                        array: fftBuffer,
                        index: halfFrameSize + 1,
                        length: halfFrameSize - 1);

                    //IFFT
                    Fourier.Inverse(fftBuffer);

                    //Accumualte the window samples
                    for (int i = 0; i < frameSize; i++)
                    {
                        outputAccumulation[Channels * i + channel] += (float)(outputScalar * fftBuffer[i].Real);
                    }
                }

                //Copy output samples to output buffer
                Array.Copy(
                    sourceArray: outputAccumulation,
                    destinationArray: cachedSampleBuffer,
                    length: bufferCount);

                //Slide down output accumulation
                Array.Copy(
                    sourceArray: outputAccumulation,
                    sourceIndex: bufferCount,
                    destinationArray: outputAccumulation,
                    destinationIndex: 0,
                    length: outputAccumulation.Length - bufferCount);

                //Clear empty output accumulation region
                Array.Clear(outputAccumulation, outputAccumulation.Length - bufferCount, bufferCount);

                samplesWritten += ReadBody(data, offset + samplesWritten, count - samplesWritten);
            }

            return samplesWritten;
        }

        private int ReadBody(float[] buffer, int offset, int count)
        {
            int samplesWritten = Math.Min(count, bufferCount - bufferIndex);

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

            Array.Clear(outputAccumulation, 0, outputAccumulation.Length);

            for (int i = 0; i < Channels; i++)
            {
                Array.Clear(inputBuffers[i], 0, inputBuffers[i].Length);
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

        //Reencoder should preserve RMS
        public override IEnumerable<double> GetChannelRMS() => stream.GetChannelRMS();

        public static IBGCStream StereoifyStream(IBGCStream stream)
        {
            if (stream.Channels == 1)
            {
                return stream.UpChannel();
            }

            return stream;
        }
    }

}
