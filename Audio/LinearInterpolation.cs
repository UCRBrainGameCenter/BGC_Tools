using System;

namespace BGC.Audio
{
    /// <summary>
    /// Resampling methods
    /// </summary>
    public static class LinearInterpolation
    {
        public static (float[] leftSamples, float[] rightSamples) Resample(
            (float[] leftSamples, float[] rightSamples) samples,
            double oldSamplingRate,
            double newSamplingRate,
            int channels)
        {
            float[] newLeftSamples = Resample(samples.leftSamples, oldSamplingRate, newSamplingRate, 1);
            float[] newRightSamples = Resample(samples.rightSamples, oldSamplingRate, newSamplingRate, 1);

            return (newLeftSamples, newRightSamples);
        }


        public static float[] Resample(
            float[] samples,
            double oldSamplingRate,
            double newSamplingRate,
            int channels)
        {
            //Smaller than 1 when downsampling
            double rateRatio = newSamplingRate / oldSamplingRate;
            //Larger than 1 when downsampling
            double invRateRatio = oldSamplingRate / newSamplingRate;

            int inputSampleCount = samples.Length / channels;
            int outputSampleCount = (int)Math.Ceiling(inputSampleCount * rateRatio);
            float[] outputSamples = new float[outputSampleCount * channels];

            double i;
            int i0;
            int i1;

            for (int j = 0; j < outputSampleCount; j++)
            {
                i = j * invRateRatio;
                i0 = (int)i;
                i1 = i0 + 1;

                outputSamples[j] = (float)((i1 - i) * samples[i0] + (i - i0) * samples[i1]);
            }

            return outputSamples;
        }

        public static short[] Resample(
            short[] samples,
            double oldSamplingRate,
            double newSamplingRate,
            int channels)
        {
            //Smaller than 1 when downsampling
            double rateRatio = newSamplingRate / oldSamplingRate;
            //Larger than 1 when downsampling
            double invRateRatio = oldSamplingRate / newSamplingRate;

            int inputSampleCount = samples.Length / channels;
            int outputSampleCount = (int)Math.Ceiling(inputSampleCount * rateRatio);
            short[] outputSamples = new short[outputSampleCount * channels];

            double i;
            int i0;
            int i1;

            for (int j = 0; j < outputSampleCount; j++)
            {
                i = j * invRateRatio;
                i0 = (int)i;
                i1 = i0 + 1;

                outputSamples[j] = (short)Math.Round((i1 - i) * samples[i0] + (i - i0) * samples[i1]);
            }

            return outputSamples;
        }
    }
}
