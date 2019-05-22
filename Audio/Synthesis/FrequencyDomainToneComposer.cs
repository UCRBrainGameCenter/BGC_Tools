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
    public class FrequencyDomainToneComposer : BGCStream
    {
        public override int Channels => 1;
        public override float SamplingRate => 44100f;

        public override int TotalSamples => int.MaxValue;
        public override int ChannelSamples => int.MaxValue;

        private readonly ComplexCarrierTone[] carrierTones;

        private const int FRAME_SIZE = 2048;
        private const int OVERLAP_FACTOR = 32;
        private const int WINDOWING_SAMPLES = FRAME_SIZE / 2;

        private const int STEP_SIZE = FRAME_SIZE / OVERLAP_FACTOR;
        private const int OVERLAP = FRAME_SIZE - STEP_SIZE;

        private readonly double timePerFrame;
        private readonly double outputScalar;

        private readonly Complex64[] ifftBuffer = new Complex64[FRAME_SIZE];
        private readonly float[] outputAccumulation = new float[FRAME_SIZE];
        private readonly float[] cachedSampleBuffer = new float[STEP_SIZE];
        private readonly double[] window = new double[FRAME_SIZE];

        private int leadingFrames = 0;
        private int frame = 0;

        private int bufferIndex = 0;
        private int bufferCount = 0;

        public FrequencyDomainToneComposer(IEnumerable<ComplexCarrierTone> carrierTones)
        {
            this.carrierTones = carrierTones.ToArray();

            timePerFrame = STEP_SIZE / (double)SamplingRate;

            //outputScalar = 1.0 / OVERLAP_FACTOR;
            outputScalar = 2.0 / (OVERLAP_FACTOR * Sqrt(FRAME_SIZE));

            for (int i = 0; i < WINDOWING_SAMPLES; i++)
            {
                window[i] = 0.54 - 0.46 * Cos(i * PI / WINDOWING_SAMPLES);
                window[FRAME_SIZE - i - 1] = window[i];
            }

            for (int i = WINDOWING_SAMPLES; i < FRAME_SIZE - WINDOWING_SAMPLES; i++)
            {
                window[i] = 1.0;
            }

            leadingFrames = -(OVERLAP_FACTOR - 1);
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
                bufferCount = STEP_SIZE;

                //Clear IFFT Buffer
                Array.Clear(ifftBuffer, 0, ifftBuffer.Length);

                //If there are any leading frames...
                double timeShiftDeltaT = (frame + leadingFrames) * timePerFrame;
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
                for (int i = 0; i < FRAME_SIZE; i++)
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
                    length: STEP_SIZE);

                //Slide down output accumulation
                Array.Copy(
                    sourceArray: outputAccumulation,
                    sourceIndex: STEP_SIZE,
                    destinationArray: outputAccumulation,
                    destinationIndex: 0,
                    length: OVERLAP);

                //Clear empty output accumulation region
                Array.Clear(
                    array: outputAccumulation,
                    index: OVERLAP,
                    length: STEP_SIZE);

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
            leadingFrames = -(OVERLAP_FACTOR - 1);

            Array.Clear(outputAccumulation, 0, FRAME_SIZE);
        }

        public override void Reset()
        {
            ClearBuffers();
            frame = 0;
        }

        public override void Seek(int position)
        {
            ClearBuffers();
            frame = position / STEP_SIZE;
            int scanSamples = position - STEP_SIZE * frame;
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
