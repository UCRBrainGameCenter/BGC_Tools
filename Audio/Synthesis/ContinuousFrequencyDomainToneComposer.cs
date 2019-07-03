using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;

using static System.Math;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// Suboptimal attempt to do frame-based rendering of a set of carrier tones. 
    /// </summary>
    public class ContinuousFrequencyDomainToneComposer : BGCStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        private readonly ComplexCarrierTone[] carrierTones;

        private readonly int frameSize;
        private readonly int windowingSamples;

        private readonly int overlapFactor;
        private readonly int stepSize;
        private readonly int overlap;

        private readonly double timePerStep;
        private readonly double outputScalar;

        private readonly Complex64[] ifftBuffer;
        private readonly float[] outputAccumulation;
        private readonly float[] cachedSampleBuffer;
        private readonly double[] window;

        private int leadingFrames = 0;
        private int frame = 0;

        private int bufferIndex = 0;
        private int bufferCount = 0;

        public ContinuousFrequencyDomainToneComposer(
            IEnumerable<ComplexCarrierTone> carrierTones,
            int frameSize = (1 << 11),
            int overlapFactor = 8)
        {
            this.carrierTones = carrierTones.ToArray();

            this.frameSize = frameSize;
            this.overlapFactor = overlapFactor;

            windowingSamples = frameSize / 2;
            stepSize = frameSize / overlapFactor;
            overlap = frameSize - stepSize;

            timePerStep = stepSize / (double)SamplingRate;

            //outputScalar = 1.0 / OVERLAP_FACTOR;
            outputScalar = 2.0 / (overlapFactor * Sqrt(frameSize));

            ifftBuffer = new Complex64[frameSize];
            outputAccumulation = new float[frameSize];
            cachedSampleBuffer = new float[stepSize];
            window = new double[frameSize];


            for (int i = 0; i < windowingSamples; i++)
            {
                window[i] = 0.54 - 0.46 * Cos(i * PI / windowingSamples);
                window[frameSize - i - 1] = window[i];
            }

            for (int i = windowingSamples; i < frameSize - windowingSamples; i++)
            {
                window[i] = 1.0;
            }

            leadingFrames = -(overlapFactor - 1);
        }

        private int ReadBody(float[] buffer, int offset, int count)
        {
            int samplesWritten = Min(count, bufferCount - bufferIndex);

            Array.Copy(
                sourceArray: cachedSampleBuffer,
                sourceIndex: bufferIndex,
                destinationArray: buffer,
                destinationIndex: offset,
                length: samplesWritten);

            bufferIndex += samplesWritten;

            return samplesWritten;
        }

        public override int Read(float[] data, int offset, int count)
        {
            int samplesWritten = ReadBody(data, offset, count);

            while (samplesWritten < count)
            {
                bufferIndex = 0;
                bufferCount = stepSize;

                //Clear IFFT Buffer
                Array.Clear(ifftBuffer, 0, ifftBuffer.Length);

                //If there are any leading frames...
                double timeShiftDeltaT = (frame + leadingFrames) * timePerStep;
                bool isLeadingFrame = leadingFrames < 0;

                foreach (ComplexCarrierTone carrierTone in carrierTones)
                {
                    FrequencyDomain.Populate(
                        buffer: ifftBuffer,
                        carrierTone: carrierTone.TimeShift(timeShiftDeltaT));
                }

                //IFFT
                Fourier.Inverse(ifftBuffer);

                //Accumualte the window samples
                for (int i = 0; i < frameSize; i++)
                {
                    outputAccumulation[i] += (float)(outputScalar * window[i] * ifftBuffer[i].Real);
                }

                //Advance Frame
                if (isLeadingFrame)
                {
                    leadingFrames++;
                }
                else
                {
                    frame++;
                }

                //Copy output samples to output buffer
                Array.Copy(
                    sourceArray: outputAccumulation,
                    destinationArray: cachedSampleBuffer,
                    length: stepSize);

                //Slide down output accumulation
                Array.Copy(
                    sourceArray: outputAccumulation,
                    sourceIndex: stepSize,
                    destinationArray: outputAccumulation,
                    destinationIndex: 0,
                    length: overlap);

                //Clear empty output accumulation region
                Array.Clear(
                    array: outputAccumulation,
                    index: overlap,
                    length: stepSize);

                if (!isLeadingFrame)
                {
                    samplesWritten += ReadBody(data, offset + samplesWritten, count - samplesWritten);
                }
            }

            return count;
        }

        private void ClearBuffers()
        {
            bufferIndex = 0;
            bufferCount = 0;
            leadingFrames = -(overlapFactor - 1);

            Array.Clear(outputAccumulation, 0, frameSize);
        }

        public override void Reset()
        {
            ClearBuffers();
            frame = 0;
        }

        public override void Seek(int position)
        {
            ClearBuffers();
            frame = position / stepSize;
            int scanSamples = position - stepSize * frame;
            float[] temp = new float[scanSamples];

            Read(temp, 0, scanSamples);
        }

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS is null)
            {
                double rms = carrierTones.Select(x => 0.5 * x.amplitude.MagnitudeSquared).Sum();
                _channelRMS = new double[] { Sqrt(rms) };
            }

            return _channelRMS;
        }
    }
}
