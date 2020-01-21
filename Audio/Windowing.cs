using System;
using UnityEngine;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// A collection of some common operations related to Windowing Audio.
    /// </summary>
    public static class Windowing
    {
        public enum Function
        {
            Hamming = 0,
            Hann,
            Sine,
            Linear,
            Square,
            Silence,
            MAX
        }

        public static float[] GetHalfWindow(
            Function function,
            int sampleCount)
        {
            switch (function)
            {
                case Function.Hamming: return HammingHalfWindow(sampleCount);
                case Function.Hann: return HannHalfWindow(sampleCount);
                case Function.Sine: return SineHalfWindow(sampleCount);
                case Function.Linear: return LinearHalfWindow(sampleCount);
                case Function.Square: return SquareHalfWindow(sampleCount);
                case Function.Silence: return SilenceHalfWindow(sampleCount);

                default:
                    Debug.LogError($"Unexpected Function: {function}");
                    goto case Function.Square;
            }
        }

        public static double[] GetHalfWindow64(
            Function function,
            int sampleCount)
        {
            switch (function)
            {
                case Function.Hamming: return HammingHalfWindow64(sampleCount);
                case Function.Hann: return HannHalfWindow64(sampleCount);
                case Function.Sine: return SineHalfWindow64(sampleCount);
                case Function.Linear: return LinearHalfWindow64(sampleCount);
                case Function.Square: return SquareHalfWindow64(sampleCount);
                case Function.Silence: return SilenceHalfWindow64(sampleCount);

                default:
                    Debug.LogError($"Unexpected Function: {function}");
                    goto case Function.Square;
            }
        }

        /// <summary>
        /// Applies a window to the passed-in samples
        /// </summary>
        /// <param name="startSample">First sample number (within, not across, channels)</param>
        public static void ApplyWindow(
            float[] samples,
            Function function,
            int startSample = -1,
            int windowWidth = -1,
            int smoothingSamples = 1000,
            int channels = 2)
        {
            switch (function)
            {
                case Function.Hamming:
                    Hamming(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Hann:
                    Hann(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Sine:
                    Sine(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Linear:
                    Linear(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Square:
                    //Do nothing
                    break;

                case Function.Silence:
                    Silence(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        channels: channels);
                    break;

                default:
                    Debug.LogError($"Unexpected Function: {function}");
                    break;
            }
        }

        /// <summary>
        /// Applies a window to the passed-in samples
        /// </summary>
        /// <param name="startSample">First sample number (within, not across, channels)</param>
        public static void ApplyWindow(
            float[] samples,
            Function function,
            int startSample = -1,
            int windowWidth = -1,
            double smoothingTime = 0.01,
            double samplingRate = 44100.0,
            int channels = 2)
        {
            int smoothingSamples = (int)(smoothingTime * samplingRate);

            switch (function)
            {
                case Function.Hamming:
                    Hamming(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Hann:
                    Hann(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Sine:
                    Sine(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Linear:
                    Linear(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        smoothingSamples: smoothingSamples,
                        channels: channels);
                    break;

                case Function.Square:
                    //Do nothing
                    break;

                case Function.Silence:
                    Silence(
                        samples: samples,
                        startSample: startSample,
                        windowWidth: windowWidth,
                        channels: channels);
                    break;

                default:
                    Debug.LogError($"Unexpected Function: {function}");
                    break;
            }
        }

        public static void Linear(
            float[] samples,
            int startSample = -1,
            int windowWidth = -1,
            int smoothingSamples = 1000,
            int channels = 2)
        {
            //Default value of startSample
            if (startSample == -1)
            {
                startSample = 0;
            }

            //Default value of windowWidth
            if (windowWidth == -1)
            {
                windowWidth = (samples.Length / channels) - startSample;
            }

            //Correct smoothingSamples for small windows
            if (2 * smoothingSamples > windowWidth)
            {
                smoothingSamples = windowWidth / 2;
            }

            int lastSample = startSample + windowWidth - 1;

            float count = smoothingSamples - 1;

            for (int i = 0; i < smoothingSamples; i++)
            {
                for (int chan = 0; chan < channels; chan++)
                {
                    samples[(startSample + i) * channels + chan] *= i / count;
                    samples[(lastSample - i) * channels + chan] *= i / count;
                }
            }
        }

        public static void Silence(
            float[] samples,
            int startSample = -1,
            int windowWidth = -1,
            int channels = 2)
        {
            //Default value of startSample
            if (startSample == -1)
            {
                startSample = 0;
            }

            //Default value of windowWidth
            if (windowWidth == -1)
            {
                windowWidth = (samples.Length / channels) - startSample;
            }

            for (int i = 0; i < windowWidth; i++)
            {
                for (int chan = 0; chan < channels; chan++)
                {
                    samples[(startSample + i) * channels + chan] = 0f;
                }
            }
        }

        public static void Hamming(
            float[] samples,
            int startSample = -1,
            int windowWidth = -1,
            int smoothingSamples = 1000,
            int channels = 2)
        {
            const float alpha = 0.54f;
            const float beta = 1f - alpha;

            //Default value of startSample
            if (startSample == -1)
            {
                startSample = 0;
            }

            //Default value of windowWidth
            if (windowWidth == -1)
            {
                windowWidth = (samples.Length / channels) - startSample;
            }

            //Correct smoothingSamples for small windows
            if (2 * smoothingSamples > windowWidth)
            {
                smoothingSamples = windowWidth / 2;
            }

            int lastSample = startSample + windowWidth - 1;

            double cosineArgument = Math.PI / (smoothingSamples - 1);

            for (int i = 0; i < smoothingSamples; i++)
            {
                float factor = alpha - beta * (float)Math.Cos(i * cosineArgument);

                for (int chan = 0; chan < channels; chan++)
                {
                    samples[(startSample + i) * channels + chan] *= factor;
                    samples[(lastSample - i) * channels + chan] *= factor;
                }
            }
        }

        public static void Hann(
            float[] samples,
            int startSample = -1,
            int windowWidth = -1,
            int smoothingSamples = 1000,
            int channels = 2)
        {
            const float alpha = 0.5f;
            const float beta = 1f - alpha;

            //Default value of startSample
            if (startSample == -1)
            {
                startSample = 0;
            }

            //Default value of windowWidth
            if (windowWidth == -1)
            {
                windowWidth = (samples.Length / channels) - startSample;
            }

            //Correct smoothingSamples for small windows
            if (2 * smoothingSamples > windowWidth)
            {
                smoothingSamples = windowWidth / 2;
            }

            int lastSample = startSample + windowWidth - 1;

            double cosineArgument = Math.PI / (smoothingSamples - 1);

            for (int i = 0; i < smoothingSamples; i++)
            {
                float factor = alpha - beta * (float)Math.Cos(i * cosineArgument);

                for (int chan = 0; chan < channels; chan++)
                {
                    samples[(startSample + i) * channels + chan] *= factor;
                    samples[(lastSample - i) * channels + chan] *= factor;
                }
            }
        }

        public static void Sine(
            float[] samples,
            int startSample = -1,
            int windowWidth = -1,
            int smoothingSamples = 1000,
            int channels = 2)
        {
            //Default value of startSample
            if (startSample == -1)
            {
                startSample = 0;
            }

            //Default value of windowWidth
            if (windowWidth == -1)
            {
                windowWidth = (samples.Length / channels) - startSample;
            }

            //Correct smoothingSamples for small windows
            if (2 * smoothingSamples > windowWidth)
            {
                smoothingSamples = windowWidth / 2;
            }

            int lastSample = startSample + windowWidth - 1;

            double sineArgument = Math.PI / (2 * smoothingSamples - 1);

            for (int i = 0; i < smoothingSamples; i++)
            {
                float factor = (float)Math.Sin(i * sineArgument);

                for (int chan = 0; chan < channels; chan++)
                {
                    samples[(startSample + i) * channels + chan] *= factor;
                    samples[(lastSample - i) * channels + chan] *= factor;
                }
            }
        }

        private static float[] HammingHalfWindow(int sampleCount)
        {
            const float alpha = 0.54f;
            const float beta = 1f - alpha;

            float[] window = new float[sampleCount];

            double cosineArgument = Math.PI / (sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = alpha - beta * (float)Math.Cos(i * cosineArgument);
            }

            return window;
        }

        private static float[] HannHalfWindow(int sampleCount)
        {
            const float alpha = 0.5f;
            const float beta = 1f - alpha;

            float[] window = new float[sampleCount];

            double cosineArgument = Math.PI / (sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = alpha - beta * (float)Math.Cos(i * cosineArgument);
            }

            return window;
        }

        private static float[] SineHalfWindow(int sampleCount)
        {
            float[] window = new float[sampleCount];

            double sineArgument = Math.PI / (2 * sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = (float)Math.Sin(i * sineArgument);
            }

            return window;
        }

        private static float[] LinearHalfWindow(int sampleCount)
        {
            float[] window = new float[sampleCount];

            float limit = sampleCount - 1;

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = i / limit;
            }

            return window;
        }

        private static float[] SquareHalfWindow(int sampleCount)
        {
            float[] window = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = 1f;
            }

            return window;
        }

        private static float[] SilenceHalfWindow(int sampleCount)
        {
            return new float[sampleCount];
        }

        private static double[] HammingHalfWindow64(int sampleCount)
        {
            const double alpha = 0.54;
            const double beta = 1.0 - alpha;

            double[] window = new double[sampleCount];

            double cosineArgument = Math.PI / (sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = alpha - beta * Math.Cos(i * cosineArgument);
            }

            return window;
        }

        private static double[] HannHalfWindow64(int sampleCount)
        {
            const double alpha = 0.5;
            const double beta = 1.0 - alpha;

            double[] window = new double[sampleCount];

            double cosineArgument = Math.PI / (sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = alpha - beta * Math.Cos(i * cosineArgument);
            }

            return window;
        }

        private static double[] SineHalfWindow64(int sampleCount)
        {
            double[] window = new double[sampleCount];

            double sineArgument = Math.PI / (2 * sampleCount - 1);

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = Math.Sin(i * sineArgument);
            }

            return window;
        }

        private static double[] LinearHalfWindow64(int sampleCount)
        {
            double[] window = new double[sampleCount];

            double limit = sampleCount - 1;

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = i / limit;
            }

            return window;
        }

        private static double[] SquareHalfWindow64(int sampleCount)
        {
            double[] window = new double[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                window[i] = 1.0;
            }

            return window;
        }

        private static double[] SilenceHalfWindow64(int sampleCount)
        {
            return new double[sampleCount];
        }
    }
}
