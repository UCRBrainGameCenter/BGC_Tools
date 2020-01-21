// <copyright file="ManagedFourierTransformProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
//
// Copyright (c) 2009-2018 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using BGC.Utility.Compute;


namespace BGC.Mathematics
{
    /// <summary>
    /// Support for Fourier Transforms.
    /// Borrows heavily from Mathnet.Numerics.
    /// </summary>
    public static class Fourier
    {
        public static void Forward(Complex32[] samples)
        {
            if (samples.Length.IsPowerOfTwo())
            {
                if (samples.Length >= 1024)
                {
                    Radix2ForwardParallel(samples);
                }
                else
                {
                    Radix2Forward(samples);
                }
            }
            else
            {
                BluesteinForward(samples);
            }

            Rescale(samples);
        }

        public static void Inverse(Complex32[] spectrum)
        {
            if (spectrum.Length.IsPowerOfTwo())
            {
                if (spectrum.Length >= 1024)
                {
                    Radix2InverseParallel(spectrum);
                }
                else
                {
                    Radix2Inverse(spectrum);
                }
            }
            else
            {
                BluesteinInverse(spectrum);
            }

            //Asymmetrical rescaling...
            //Rescale(spectrum);
        }

        public static void Forward(Complex64[] samples)
        {
            if (samples.Length.IsPowerOfTwo())
            {
                if (samples.Length >= 1024)
                {
                    Radix2ForwardParallel(samples);
                }
                else
                {
                    Radix2Forward(samples);
                }
            }
            else
            {
                BluesteinForward(samples);
            }

            Rescale(samples);
        }

        public static void Inverse(Complex64[] spectrum)
        {
            if (spectrum.Length.IsPowerOfTwo())
            {
                if (spectrum.Length >= 1024)
                {
                    Radix2InverseParallel(spectrum);
                }
                else
                {
                    Radix2Inverse(spectrum);
                }
            }
            else
            {
                BluesteinInverse(spectrum);
            }

            //Asymmetrical rescaling...
            //Rescale(spectrum);
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2ForwardParallel(Complex32[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                int size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, -1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2InverseParallel(Complex32[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                int size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, 1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Forward(Complex32[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (int k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, -1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Inverse(Complex32[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (int k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, 1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 Reorder Helper Method
        /// </summary>
        private static void Radix2Reorder(Complex32[] samples)
        {
            int j = 0;
            for (int i = 0; i < samples.Length - 1; i++)
            {
                if (i < j)
                {
                    Complex32 temp = samples[i];
                    samples[i] = samples[j];
                    samples[j] = temp;
                }

                int m = samples.Length;

                do
                {
                    m >>= 1;
                    j ^= m;
                }
                while ((j & m) == 0);
            }
        }

        /// <summary>
        /// Radix-2 Step Helper Method
        /// </summary>
        /// <param name="samples">Sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <param name="levelSize">Level Group Size.</param>
        /// <param name="k">Index inside of the level.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Radix2Step(Complex32[] samples, int exponentSign, int levelSize, int k)
        {
            // Twiddle Factor
            double exponent = exponentSign * k * GeneralMath.dPI / levelSize;
            Complex32 w = new Complex32(
                (float)Math.Cos(exponent),
                (float)Math.Sin(exponent));

            int step = levelSize << 1;
            for (int i = k; i < samples.Length; i += step)
            {
                Complex32 ai = samples[i];
                Complex32 t = w * samples[i + levelSize];
                samples[i] = ai + t;
                samples[i + levelSize] = ai - t;
            }
        }


        private static void BluesteinForward(Complex32[] samples)
        {
            int n = samples.Length;
            Complex32[] sequence = BluesteinSequence32(n);

            // Padding to power of two >= 2N–1 so we can apply Radix-2 FFT.
            int m = ((n << 1) - 1).CeilingToPowerOfTwo();
            Complex32[] b = new Complex32[m];
            Complex32[] a = new Complex32[m];

            CommonParallel.Invoke(
                () =>
                {
                    // Build and transform padded sequence b_k = exp(I*Pi*k^2/N)
                    for (int i = 0; i < n; i++)
                    {
                        b[i] = sequence[i];
                    }

                    for (int i = m - n + 1; i < b.Length; i++)
                    {
                        b[i] = sequence[m - i];
                    }

                    Radix2Forward(b);
                },
                () =>
                {
                    // Build and transform padded sequence a_k = x_k * exp(-I*Pi*k^2/N)
                    for (int i = 0; i < samples.Length; i++)
                    {
                        a[i] = sequence[i].Conjugate() * samples[i];
                    }

                    Radix2Forward(a);
                });

            for (int i = 0; i < a.Length; i++)
            {
                a[i] *= b[i];
            }

            Radix2InverseParallel(a);

            float nbinv = 1.0f / m;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = nbinv * sequence[i].Conjugate() * a[i];
            }
        }

        /// <summary>
        /// Bluestein generic FFT for arbitrary sized sample vectors.
        /// </summary>
        private static void BluesteinInverse(Complex32[] spectrum)
        {
            SwapRealImaginary(spectrum);
            BluesteinForward(spectrum);
            SwapRealImaginary(spectrum);
        }

        /// <summary>
        /// Sequences with length greater than Math.Sqrt(Int32.MaxValue) + 1
        /// will cause k*k in the Bluestein sequence to overflow (GH-286).
        /// </summary>
        const int BluesteinSequenceLengthThreshold = 46341;

        /// <summary>
        /// Generate the bluestein sequence for the provided problem size.
        /// </summary>
        /// <param name="n">Number of samples.</param>
        /// <returns>Bluestein sequence exp(I*Pi*k^2/N)</returns>
        private static Complex32[] BluesteinSequence32(int n)
        {
            double s = GeneralMath.dPI / n;
            Complex32[] sequence = new Complex32[n];

            // TODO: benchmark whether the second variation is significantly
            // faster than the former one. If not just use the former one always.
            if (n > BluesteinSequenceLengthThreshold)
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = (s * k) * k;
                    sequence[k] = new Complex32((float)Math.Cos(t), (float)Math.Sin(t));
                }
            }
            else
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = s * (k * k);
                    sequence[k] = new Complex32((float)Math.Cos(t), (float)Math.Sin(t));
                }
            }

            return sequence;
        }


        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2ForwardParallel(Complex64[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                int size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, -1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sample vectors (Parallel Version).
        /// </summary>
        private static void Radix2InverseParallel(Complex64[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                int size = levelSize;

                CommonParallel.For(0, size, 64, (u, v) =>
                {
                    for (int i = u; i < v; i++)
                    {
                        Radix2Step(data, 1, size, i);
                    }
                });
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Forward(Complex64[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (int k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, -1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 generic FFT for power-of-two sized sample vectors.
        /// </summary>
        private static void Radix2Inverse(Complex64[] data)
        {
            Radix2Reorder(data);
            for (int levelSize = 1; levelSize < data.Length; levelSize *= 2)
            {
                for (int k = 0; k < levelSize; k++)
                {
                    Radix2Step(data, 1, levelSize, k);
                }
            }
        }

        /// <summary>
        /// Radix-2 Reorder Helper Method
        /// </summary>
        private static void Radix2Reorder(Complex64[] samples)
        {
            int j = 0;
            for (int i = 0; i < samples.Length - 1; i++)
            {
                if (i < j)
                {
                    Complex64 temp = samples[i];
                    samples[i] = samples[j];
                    samples[j] = temp;
                }

                int m = samples.Length;

                do
                {
                    m >>= 1;
                    j ^= m;
                }
                while ((j & m) == 0);
            }
        }

        /// <summary>
        /// Radix-2 Step Helper Method
        /// </summary>
        /// <param name="samples">Sample vector.</param>
        /// <param name="exponentSign">Fourier series exponent sign.</param>
        /// <param name="levelSize">Level Group Size.</param>
        /// <param name="k">Index inside of the level.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Radix2Step(Complex64[] samples, int exponentSign, int levelSize, int k)
        {
            // Twiddle Factor
            double exponent = (exponentSign * k) * GeneralMath.dPI / levelSize;
            Complex64 w = new Complex64(Math.Cos(exponent), Math.Sin(exponent));

            int step = levelSize << 1;
            for (int i = k; i < samples.Length; i += step)
            {
                Complex64 ai = samples[i];
                Complex64 t = w * samples[i + levelSize];
                samples[i] = ai + t;
                samples[i + levelSize] = ai - t;
            }
        }

        private static void BluesteinForward(Complex64[] samples)
        {
            int n = samples.Length;
            Complex64[] sequence = BluesteinSequence64(n);

            // Padding to power of two >= 2N–1 so we can apply Radix-2 FFT.
            int m = ((n << 1) - 1).CeilingToPowerOfTwo();
            Complex64[] b = new Complex64[m];
            Complex64[] a = new Complex64[m];

            CommonParallel.Invoke(
                () =>
                {
                    // Build and transform padded sequence b_k = exp(I*Pi*k^2/N)
                    for (int i = 0; i < n; i++)
                    {
                        b[i] = sequence[i];
                    }

                    for (int i = m - n + 1; i < b.Length; i++)
                    {
                        b[i] = sequence[m - i];
                    }

                    Radix2Forward(b);
                },
                () =>
                {
                    // Build and transform padded sequence a_k = x_k * exp(-I*Pi*k^2/N)
                    for (int i = 0; i < samples.Length; i++)
                    {
                        a[i] = sequence[i].Conjugate() * samples[i];
                    }

                    Radix2Forward(a);
                });

            for (int i = 0; i < a.Length; i++)
            {
                a[i] *= b[i];
            }

            Radix2InverseParallel(a);

            double nbinv = 1.0 / m;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = nbinv * sequence[i].Conjugate() * a[i];
            }
        }

        /// <summary>
        /// Bluestein generic FFT for arbitrary sized sample vectors.
        /// </summary>
        private static void BluesteinInverse(Complex64[] spectrum)
        {
            SwapRealImaginary(spectrum);
            BluesteinForward(spectrum);
            SwapRealImaginary(spectrum);
        }

        /// <summary>
        /// Generate the bluestein sequence for the provided problem size.
        /// </summary>
        /// <param name="n">Number of samples.</param>
        /// <returns>Bluestein sequence exp(I*Pi*k^2/N)</returns>
        private static Complex64[] BluesteinSequence64(int n)
        {
            double s = GeneralMath.dPI / n;
            Complex64[] sequence = new Complex64[n];

            // TODO: benchmark whether the second variation is significantly
            // faster than the former one. If not just use the former one always.
            if (n > BluesteinSequenceLengthThreshold)
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = (s * k) * k;
                    sequence[k] = new Complex64(Math.Cos(t), Math.Sin(t));
                }
            }
            else
            {
                for (int k = 0; k < sequence.Length; k++)
                {
                    double t = s * (k * k);
                    sequence[k] = new Complex64(Math.Cos(t), Math.Sin(t));
                }
            }

            return sequence;
        }

        /// <summary>
        /// Fully rescale the FFT result.
        /// </summary>
        private static void Rescale(Complex32[] samples)
        {
            float scalingFactor = 1f / samples.Length;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Fully rescale the FFT result.
        /// </summary>
        private static void Rescale(Complex64[] samples)
        {
            double scalingFactor = 1.0 / samples.Length;
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scalingFactor;
            }
        }

        /// <summary>
        /// Swap the real and imaginary parts of each sample.
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        private static void SwapRealImaginary(Complex32[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new Complex32(samples[i].Imaginary, samples[i].Real);
            }
        }

        /// <summary>
        /// Swap the real and imaginary parts of each sample.
        /// </summary>
        /// <param name="samples">Sample Vector.</param>
        private static void SwapRealImaginary(Complex64[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new Complex64(samples[i].Imaginary, samples[i].Real);
            }
        }
    }

}
