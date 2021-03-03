using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Slows the playback speed by a factor between 1x and 0.5x without changing pitch
    /// </summary>
    public class PhaseVocoder : SimpleBGCFilter
    {
        private const int BASE_FFT_SIZE = 12;
        private const int EXPANDED_FFT_SIZE = BASE_FFT_SIZE + 1;
        private const int OVERLAP_FACTOR = 32;

        private readonly int halfFFTSamples;
        private readonly int baseFFTSamples;
        private readonly int expandedFFTSamples;

        private readonly int outputSamples;

        private readonly int stepSize;
        private readonly int overlap;
        private readonly int outputStep;

        private readonly double effectiveSpeed;
        private readonly double inputScalar;
        private readonly double outputScalar;

        private readonly TransformRMSBehavior rmsBehavior;

        /// <summary>
        /// Holds samples from underlying stream
        /// </summary>
        private readonly float[] localSampleBuffer;

        /// <summary>
        /// Store computed samples ready to deliver
        /// </summary>
        private readonly float[] cachedSampleBuffer;

        private readonly Complex64[] phasors;
        private readonly Complex64[] phasorDeltas;
        private readonly float[][] inputBuffers;
        private readonly Complex64[] fftBuffer;
        private readonly Complex64[] ifftBuffer;
        private readonly float[] outputAccumulation;

        private readonly double[] windowInput;
        private readonly double[] windowOutput;

        private int bufferIndex = 0;
        private int bufferCount = 0;

        public override int Channels => stream.Channels;

        public override int TotalSamples { get; }
        public override int ChannelSamples { get; }

        public PhaseVocoder(
            IBGCStream stream,
            double speed,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Passthrough)
            : base(stream)
        {
            baseFFTSamples = (int)Math.Pow(2, BASE_FFT_SIZE);
            expandedFFTSamples = 2 * baseFFTSamples;
            halfFFTSamples = baseFFTSamples / 2;

            stepSize = baseFFTSamples / OVERLAP_FACTOR;
            overlap = baseFFTSamples - stepSize;

            outputStep = ((int)(baseFFTSamples / speed)) / OVERLAP_FACTOR;
            outputSamples = outputStep * OVERLAP_FACTOR;

            effectiveSpeed = stepSize / (double)outputStep;

            localSampleBuffer = new float[Channels * stepSize];
            cachedSampleBuffer = new float[Channels * outputSamples];

            phasors = new Complex64[halfFFTSamples + 1];
            phasorDeltas = new Complex64[halfFFTSamples + 1];
            inputBuffers = new float[Channels][];
            outputAccumulation = new float[Channels * expandedFFTSamples];

            fftBuffer = new Complex64[baseFFTSamples];
            ifftBuffer = new Complex64[expandedFFTSamples];

            if (stream.ChannelSamples == int.MaxValue)
            {
                ChannelSamples = int.MaxValue;
                TotalSamples = int.MaxValue;
            }
            else
            {
                ChannelSamples = (int)(stream.ChannelSamples / effectiveSpeed);
                TotalSamples = Channels * ChannelSamples;
            }

            for (int i = 0; i < Channels; i++)
            {
                inputBuffers[i] = new float[baseFFTSamples];
            }

            int deltaSteps = outputStep - stepSize;
            for (int i = 0; i <= halfFFTSamples; i++)
            {
                //Initialize phasors to 2 so that it doubles the amplitudes on copy and rotation
                phasors[i] = 2f;
                phasorDeltas[i] = Complex64.FromPolarCoordinates(1.0, -i * deltaSteps / (double)baseFFTSamples);
            }

            //inputScalar = Math.Sqrt(baseFFTSamples);
            inputScalar = 1.0;
            outputScalar = 1.0 / OVERLAP_FACTOR;

            windowInput = new double[baseFFTSamples];
            windowOutput = new double[outputSamples];

            for (int i = 0; i < baseFFTSamples; i++)
            {
                //Hamming
                windowInput[i] = 0.54 - 0.46 * Math.Cos(2.0 * Math.PI * i / (baseFFTSamples - 1));
            }

            for (int i = 0; i < outputSamples; i++)
            {
                //Square
                windowOutput[i] = 1.0;
            }

            this.rmsBehavior = rmsBehavior;
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
                bufferCount = Channels * outputStep;

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
                    for (int i = 0; i < baseFFTSamples; i++)
                    {
                        fftBuffer[i] = inputScalar * inputBuffers[channel][i] * windowInput[i];
                    }

                    //FFT
                    Fourier.Forward(fftBuffer);

                    //Clear IFFT Buffer
                    Array.Clear(ifftBuffer, 0, ifftBuffer.Length);

                    //Copy values into IFFT Buffer
                    for (int i = 0; i <= halfFFTSamples; i++)
                    {
                        ifftBuffer[2 * i] = fftBuffer[i] * phasors[i];
                    }

                    //IFFT
                    Fourier.Inverse(ifftBuffer);

                    //Accumualte the window samples
                    for (int i = 0; i < outputSamples; i++)
                    {
                        outputAccumulation[Channels * i + channel] += (float)(outputScalar * windowOutput[i] * ifftBuffer[i].Real);
                    }
                }

                //Advance phasor
                for (int i = 0; i <= halfFFTSamples; i++)
                {
                    phasors[i] *= phasorDeltas[i];
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

            for (int i = 0; i <= halfFFTSamples; i++)
            {
                //Initialize phasors to 2 so that it doubles the amplitudes on copy and rotation
                phasors[i] = 2.0;
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
            stream.Seek((int)(position * effectiveSpeed));
        }

        private IEnumerable<double> _channelRMS = null;
        //Phase vocoder should preserve RMS
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
