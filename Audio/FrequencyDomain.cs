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

            if (!IsRenderable(buffer.Length, frequency))
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

            //An off-bin tone e^(i 2pi (bin + d) m / N) over one frame has the spectrum
            //  X[bin + n] = e^(i pi d) sin(pi d) / (pi (d - n)),
            //so the inverse FFT reproduces the tone at its frequency and phase, truncated to
            //sideFreqCount terms on each side
            amplitude *= (Math.Sin(Math.PI * normalizedDeviation) / Math.PI) *
                Complex64.FromPolarCoordinates(1.0, Math.PI * normalizedDeviation);

            for (int N = -sideFreqCount; N <= sideFreqCount; N++)
            {
                if (bin + N < 1 || bin + N > buffer.Length / 2)
                {
                    //Skip frequencies out of range
                    continue;
                }

                buffer[bin + N] += amplitude / (normalizedDeviation - N);
            }
        }

        /// <summary>
        /// Whether <see cref="Populate(Complex64[], double, Complex64, int)"/> renders a carrier of
        /// this frequency into a buffer of this size. Carriers below the first bin or above the
        /// Nyquist bin are skipped, so they must not count toward a claimed RMS either.
        /// </summary>
        public static bool IsRenderable(int bufferSize, double frequency)
        {
            int bin = GetComplexFrequencyBin(
                bufferSize: bufferSize,
                frequency: frequency);

            return bin >= 1 && bin <= bufferSize / 2;
        }

        public static double GetComplexSampleFrequency(int bufferSize, int sample) =>
            GetComplexSampleFrequency(bufferSize, sample, SamplingRate);

        public static double GetComplexSampleFrequency(int bufferSize, int sample, double samplingRate) =>
            sample * samplingRate / bufferSize;

        public static double GetComplexFrequencySample(int bufferSize, double frequency) =>
            GetComplexFrequencySample(bufferSize, frequency, SamplingRate);

        public static double GetComplexFrequencySample(int bufferSize, double frequency, double samplingRate) =>
            frequency * bufferSize / samplingRate;

        public static int GetComplexFrequencyBin(int bufferSize, double frequency) =>
           (int)GetComplexFrequencySample(bufferSize, frequency);

        /// <summary> The fractional bin offset d = f N / fs - bin, in [0, 1) </summary>
        private static double GetComplexNormalizedDeviation(int bufferSize, double frequency) =>
            GetComplexFrequencySample(bufferSize, frequency) - GetComplexFrequencyBin(bufferSize, frequency);
    }
}
