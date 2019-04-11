using System;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Collection of common mathematical processes used in the Frequency-Domain.
    /// </summary>
    public static class FrequencyDomain
    {
        public static double SamplingRate { get; set; } = 44100.0;

        public static void Populate(
            Complex64[] buffer,
            in ComplexCarrierTone carrierTone,
            double amplitudeFactor = 1.0,
            int sideFreqCount = 20)
        {
            Populate(
                buffer: buffer,
                frequency: carrierTone.frequency,
                amplitude: amplitudeFactor * carrierTone.amplitude,
                sideFreqCount: sideFreqCount);
        }

        public static void Populate(
            Complex64[] buffer,
            double frequency,
            Complex64 amplitude,
            int sideFreqCount = 20)
        {
            int bin = GetComplexFrequencyBin(
                bufferSize: buffer.Length,
                frequency: frequency);

            if (bin < 1 || bin > buffer.Length / 2)
            {
                //Skipping frequency as it's out of range
                return;
            }

            double normalizedDeviation = GetComplexNormalizedDeviation(
                bufferSize: buffer.Length,
                frequency: frequency);

            amplitude *= Math.Sqrt(buffer.Length);

            if (normalizedDeviation == 0.0)
            {
                //Perfect - No side terms
                buffer[bin] += amplitude;
                return;
            }

            amplitude *= (Math.Sin(Math.PI * normalizedDeviation) / Math.PI) *
                Complex64.FromPolarCoordinates(1.0, Math.PI * normalizedDeviation);

            for (int N = -sideFreqCount; N <= sideFreqCount; N++)
            {
                if (bin + N < 1 || bin + N > buffer.Length / 2)
                {
                    //Skip frequencies out of range
                    continue;
                }

                buffer[bin + N] += amplitude / (N - normalizedDeviation);
            }
        }

        public static double GetComplexSampleFrequency(int bufferSize, int sample) =>
            sample * SamplingRate / bufferSize;

        public static double GetComplexFrequencySample(int bufferSize, double frequency) =>
            frequency * bufferSize / SamplingRate;

        public static int GetComplexFrequencyBin(int bufferSize, double frequency) =>
           (int)GetComplexFrequencySample(bufferSize, frequency);

        /// <summary> Get f_Delta * T </summary>
        private static double GetComplexNormalizedDeviation(int bufferSize, double frequency) =>
            0.5 * (GetComplexFrequencySample(bufferSize, frequency) - GetComplexFrequencyBin(bufferSize, frequency));
    }
}
