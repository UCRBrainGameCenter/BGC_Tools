using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;
using BGC.Audio.Filters;

using static System.Math;

namespace BGC.Audio.AnalyticStreams
{
    /// <summary>
    /// Analytic signal generation based on 
    /// "An Efficient Analytic Signal Generator" by Clay S. Turner
    /// http://www.claysturner.com/dsp/asg.pdf
    /// </summary>
    public class AnalyticStreamConverter : IAnalyticStream
    {
        private const double A = 0.00125;
        private const double W_1 = 0.49875;
        private const double W_2 = 0.00125;
        private const int FILTER_LENGTH = 129;

        public double SamplingRate => stream.SamplingRate;

        public int Samples => stream.ChannelSamples;

        private readonly IBGCStream stream;
        private readonly IBGCStream convStream;

        private const int BUFFER_SIZE = 512;
        private readonly float[] buffer = new float[BUFFER_SIZE];

        public AnalyticStreamConverter(IBGCStream stream)
        {
            this.stream = stream;

            double[] realConvolutionFilter = new double[FILTER_LENGTH];
            double[] imagConvolutionFilter = new double[FILTER_LENGTH];

            const double twoPiSq = 2f * PI * PI;
            const double fourASq = 4f * A * A;
            const double piSq = PI * PI;
            const double piOv4 = PI / 4f;
            const double piOv4A = PI / (4f * A);
            double N_0 = (FILTER_LENGTH - 1.0) / 2.0;

            for (int i = 1; i < FILTER_LENGTH - 1; i++)
            {
                if (i == N_0)
                {
                    continue;
                }

                double t = 2 * PI * (i - N_0);
                double prefactor = twoPiSq * Cos(A * t) / (t * (fourASq * t * t - piSq));

                realConvolutionFilter[i] = prefactor * (Sin(W_1 * t + piOv4) - Sin(W_2 * t + piOv4));
            }

            realConvolutionFilter[0] = A * (Sin(piOv4A * (A - 2f * W_1)) - Sin(piOv4A * (A - 2f * W_2)));
            realConvolutionFilter[FILTER_LENGTH - 1] = A * (Sin(piOv4A * (A + 2f * W_2)) - Sin(piOv4A * (A + 2f * W_1)));

            if (FILTER_LENGTH % 2 == 1)
            {
                realConvolutionFilter[(int)N_0] = Sqrt(2f) * (W_2 - W_1);
            }

            for (int i = 0; i < FILTER_LENGTH; i++)
            {
                imagConvolutionFilter[i] = realConvolutionFilter[FILTER_LENGTH - 1 - i];
            }

            convStream = new MultiConvolutionFilter(stream, realConvolutionFilter, imagConvolutionFilter);
        }

        void IAnalyticStream.Initialize() => stream.Initialize();

        public int Read(Complex64[] data, int offset, int count)
        {
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                int maxReadCount = Min(2 * samplesRemaining, BUFFER_SIZE);

                int sampleReadCount = convStream.Read(buffer, 0, maxReadCount);

                if (sampleReadCount == 0)
                {
                    break;
                }

                sampleReadCount /= 2;

                for (int i = 0; i < sampleReadCount; i++)
                {
                    data[offset + i] = new Complex64(buffer[2 * i], buffer[2 * i + 1]);
                }

                samplesRemaining -= sampleReadCount;
                offset += sampleReadCount;
            }

            return count - samplesRemaining;
        }

        public void Reset()
        {
            convStream.Reset();
        }

        public void Seek(int position)
        {
            position = GeneralMath.Clamp(position, 0, Samples);
            convStream.Seek(position);
        }

        private double channelRMS = double.NaN;
        public double GetRMS() =>
            double.IsNaN(channelRMS) ? (channelRMS = stream.GetChannelRMS().First()) : channelRMS;

        public PresentationConstraints GetPresentationConstraints() =>
            stream.GetPresentationConstraints().First();

        public void Dispose()
        {
            stream?.Dispose();
            convStream?.Dispose();
        }
    }
}
