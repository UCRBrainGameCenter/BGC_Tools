// <copyright file="Complex32.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2010 Math.NET
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
using System.Globalization;
using System.Runtime.Serialization;
using UnityEngine;

namespace BGC.Mathematics
{

    /// <summary>
    /// Represents a complex number with single-precision floating point components
    /// </summary>
    public readonly struct Complex32 : IEquatable<Complex32>, IFormattable
    {
        /// <summary>
        /// The real component of the complex number.
        /// </summary>
        [DataMember(Order = 1)]
        private readonly float _real;

        /// <summary>
        /// The imaginary component of the complex number.
        /// </summary>
        [DataMember(Order = 2)]
        private readonly float _imag;


        /// <summary>
        /// Returns a new Complex32 instance with a real number equal to zero and an imaginary number equal to zero.
        /// </summary>
        public static readonly Complex32 Zero = new Complex32(0f, 0f);

        /// <summary>
        /// Returns a new Complex32 instance with a real number equal to one and an imaginary number equal to zero.
        /// </summary>
        public static readonly Complex32 One = new Complex32(1f, 0f);

        /// <summary>
        /// Returns a new Complex32 instance with a real number equal to zero and an imaginary number equal to one.
        /// </summary>
        public static readonly Complex32 ImaginaryOne = new Complex32(0f, 1f);

        /// <summary>
        /// Returns a new Complex32 instance with real and imaginary numbers positive infinite.
        /// </summary>
        public static readonly Complex32 PositiveInfinity = new Complex32(float.PositiveInfinity, float.PositiveInfinity);

        /// <summary>
        /// Returns a new Complex32 instance with real and imaginary numbers not a number.
        /// </summary>
        public static readonly Complex32 NaN = new Complex32(float.NaN, float.NaN);

        /// <summary>
        /// Initializes a new Complex32 structure using the specified real and imaginary values.
        /// </summary>
        public Complex32(float real, float imaginary)
        {
            _real = real;
            _imag = imaginary;
        }

        /// <summary>
        /// Gets the imaginary component of the current System.Numerics.Complex32 object.
        /// </summary>
        public float Imaginary => _imag;

        /// <summary>
        /// Gets the real component of the current System.Numerics.Complex32 object.
        /// </summary>
        public float Real => _real;

        /// <summary>
        /// Gets the magnitude (or absolute value) of a complex number.
        /// </summary>
        public float Magnitude
        {
            get
            {
                if (float.IsNaN(_real) || float.IsNaN(_imag))
                {
                    return float.NaN;
                }

                if (float.IsInfinity(_real) || float.IsInfinity(_imag))
                {
                    return float.PositiveInfinity;
                }

                float a = Mathf.Abs(_real);
                float b = Mathf.Abs(_imag);

                if (a > b)
                {
                    float tmp = b / a;
                    return a * Mathf.Sqrt(1f + tmp * tmp);

                }

                if (a == 0f) // one can write a >= float.Epsilon here
                {
                    return b;
                }
                else
                {
                    float tmp = a / b;
                    return b * Mathf.Sqrt(1f + tmp * tmp);
                }
            }
        }

        /// <summary>
        /// Gets the phase of a complex number in radians.
        /// </summary>
        public float Phase => _imag == 0f && _real < 0f ? Mathf.PI : Mathf.Atan2(_imag, _real);

        /// <summary>
        /// Gets the absolute value (or magnitude) of a complex number.
        /// </summary>
        public static float Abs(in Complex32 value) => value.Magnitude;

        /// <summary>
        /// Trigonometric principal Arc Cosine of this Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc cosine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Acos(in Complex32 value)
        {
            if (value.Imaginary < 0f || value.Imaginary == 0f && value.Real > 0f)
            {
                return Mathf.PI - Acos(-value);
            }

            return -ImaginaryOne * (value + (ImaginaryOne * (1f - value.Square()).SquareRoot())).NaturalLogarithm();
        }

        /// <summary>
        /// Returns the sum of the two Complex32 inputs
        /// </summary>
        public static Complex32 Add(in Complex32 left, in Complex32 right) => left + right;

        /// <summary>
        /// Trigonometric principal Arc Sine of this Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc sine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Asin(in Complex32 value)
        {
            if (value.Imaginary > 0f || value.Imaginary == 0f && value.Real < 0f)
            {
                return -Asin(-value);
            }

            return -ImaginaryOne * ((1f - value.Square()).SquareRoot() + (ImaginaryOne * value)).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric principal Arc Tangent of this Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc tangent of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Atan(in Complex32 value)
        {
            Complex32 iz = new Complex32(-value.Imaginary, value.Real); // I*this
            return new Complex32(0f, 0.5f) * ((1f - iz).NaturalLogarithm() - (1f + iz).NaturalLogarithm());
        }

        /// <summary>
        /// Computes the conjugate of a complex number and returns the result.
        /// </summary>
        public static Complex32 Conjugate(in Complex32 value) => value.Conjugate();

        /// <summary>
        /// Trigonometric Cosine of a Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The cosine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Cos(in Complex32 value)
        {
            if (value.IsReal())
            {
                return new Complex32(Mathf.Cos(value.Real), 0f);
            }

            return new Complex32(
                Mathf.Cos(value.Real) * GeneralMath.Cosh(value.Imaginary),
                -Mathf.Sin(value.Real) * GeneralMath.Sinh(value.Imaginary));
        }

        /// <summary>
        /// Hyperbolic Cosine of a Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic cosine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Cosh(in Complex32 value)
        {
            if (value.IsReal())
            {
                return new Complex32(GeneralMath.Cosh(value.Real), 0f);
            }

            // cosh(x + j*y) = cosh(x)*cos(y) + j*sinh(x)*sin(y)
            // if x > huge, cosh(x + j*y) = exp(|x|)/2*cos(y) + j*sign(x)*exp(|x|)/2*sin(y)

            if (Mathf.Abs(value.Real) >= 22f) // Taken from the msun library in FreeBSD
            {
                float h = Mathf.Exp(Mathf.Abs(value.Real)) * 0.5f;
                return new Complex32(
                    h * Mathf.Cos(value.Imaginary),
                    Mathf.Sign(value.Real) * h * Mathf.Sin(value.Imaginary));
            }

            return new Complex32(
                GeneralMath.Cosh(value.Real) * Mathf.Cos(value.Imaginary),
                GeneralMath.Sinh(value.Real) * Mathf.Sin(value.Imaginary));
        }

        /// <summary>
        /// Divides one complex number by another and returns the result.
        /// </summary>
        public static Complex32 Divide(in Complex32 dividend, in Complex32 divisor) => dividend / divisor;

        /// <summary>
        /// Returns e raised to the power specified by a complex number.
        /// </summary>
        public static Complex32 Exp(in Complex32 value) => value.Exponential();

        /// <summary>
        /// Creates a complex number from a point's polar coordinates.
        /// </summary>
        public static Complex32 FromPolarCoordinates(float magnitude, float phase) =>
            new Complex32(magnitude * Mathf.Cos(phase), magnitude * Mathf.Sin(phase));

        /// <summary>
        /// Creates a complex number from a point's polar coordinates.
        /// </summary>
        public static Complex32 FromPolarCoordinates(double magnitude, double phase) =>
            new Complex32((float)(magnitude * Math.Cos(phase)), (float)(magnitude * Math.Sin(phase)));

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified complex number.
        /// </summary>
        public static Complex32 Log(in Complex32 value) => value.NaturalLogarithm();

        /// <summary>
        /// Returns the logarithm of a specified complex number in a specified base.
        /// </summary>
        public static Complex32 Log(in Complex32 value, float baseValue) => value.Logarithm(baseValue);

        /// <summary>
        /// Returns the base-10 logarithm of a specified complex number.
        /// </summary>
        public static Complex32 Log10(in Complex32 value) => value.CommonLogarithm();

        /// <summary>
        /// Returns the product of two complex numbers.
        /// </summary>
        public static Complex32 Multiply(in Complex32 left, in Complex32 right) => left * right;

        /// <summary>
        /// Returns the additive inverse of a specified complex number.
        /// </summary>
        public static Complex32 Negate(in Complex32 value) => -value;

        /// <summary>
        /// Returns a specified complex number raised to a power specified by a float-precision floating-point number.
        /// </summary>
        public static Complex32 Pow(in Complex32 value, float power) => value.Power(power);

        /// <summary>
        /// Returns a specified complex number raised to a power specified by a complex number.
        /// </summary>
        public static Complex32 Pow(in Complex32 value, in Complex32 power) => value.Power(power);

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        public static Complex32 Reciprocal(in Complex32 value) => value.Reciprocal();

        /// <summary>
        /// Trigonometric Sine of a Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The sine of the complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Sin(in Complex32 value)
        {
            if (value.IsReal())
            {
                return new Complex32(Mathf.Sin(value.Real), 0f);
            }

            return new Complex32(
                Mathf.Sin(value.Real) * GeneralMath.Cosh(value.Imaginary),
                Mathf.Cos(value.Real) * GeneralMath.Sinh(value.Imaginary));
        }

        /// <summary>
        /// Hyperbolic Sine of a Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic sine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Sinh(in Complex32 value)
        {
            if (value.IsReal())
            {
                return new Complex32(GeneralMath.Sinh(value.Real), 0f);
            }

            // sinh(x + j y) = sinh(x)*cos(y) + j*cosh(x)*sin(y)
            // if x > huge, sinh(x + jy) = sign(x)*exp(|x|)/2*cos(y) + j*exp(|x|)/2*sin(y)

            if (Mathf.Abs(value.Real) >= 22f) // Taken from the msun library in FreeBSD
            {
                float h = Mathf.Exp(Mathf.Abs(value.Real)) * 0.5f;
                return new Complex32(
                    Mathf.Sign(value.Real) * h * Mathf.Cos(value.Imaginary),
                    h * Mathf.Sin(value.Imaginary));
            }

            return new Complex32(
                GeneralMath.Sinh(value.Real) * Mathf.Cos(value.Imaginary),
                GeneralMath.Cosh(value.Real) * Mathf.Sin(value.Imaginary));
        }

        /// <summary>
        /// The Square (power 2) of this Complex32
        /// </summary>
        /// <returns>
        /// The square of this complex number.
        /// </returns>
        public Complex32 Square()
        {
            if (IsReal())
            {
                return new Complex32(_real * _real, 0.0f);
            }

            return new Complex32((_real * _real) - (_imag * _imag), 2 * _real * _imag);
        }

        /// <summary>
        /// The Square Root of a complex number
        /// </summary>
        // From Mathnet.Numerics
        public static Complex32 Sqrt(in Complex32 value) => value.SquareRoot();

        /// <summary>
        /// The difference between two Complex32 numbers;
        /// </summary>
        /// <returns>The complex difference.</returns>
        // From Mathnet.Numerics
        public static Complex32 Subtract(in Complex32 left, Complex32 right) => left - right;

        /// <summary>
        /// Trigonometric Tangent of a Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The tangent of the complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Tan(in Complex32 value)
        {
            if (value.IsReal())
            {
                return new Complex32(Mathf.Tan(value.Real), 0f);
            }

            // tan(z) = - j*tanh(j*z)

            Complex32 z = Tanh(new Complex32(-value.Imaginary, value.Real));
            return new Complex32(z.Imaginary, -z.Real);
        }

        /// <summary>
        /// Hyperbolic Tangent of a Complex32 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic tangent of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex32 Tanh(in Complex32 value)
        {
            if (value.IsReal())
            {
                return new Complex32(GeneralMath.Tanh(value.Real), 0f);
            }

            // tanh(x + j*y) = (cosh(x)*sinh(x)/cos^2(y) + j*tan(y))/(1 + sinh^2(x)/cos^2(y))
            // if |x| > huge, tanh(z) = sign(x) + j*4*cos(y)*sin(y)*exp(-2*|x|)
            // if exp(-|x|) = 0, tanh(z) = sign(x)
            // if tan(y) = +/- oo or 1/cos^2(y) = 1 + tan^2(y) = oo, tanh(z) = cosh(x)/sinh(x)
            //
            // The algorithm is based on Kahan.

            if (Math.Abs(value.Real) >= 22f) // Taken from the msun library in FreeBSD
            {
                float e = Mathf.Exp(-Mathf.Abs(value.Real));
                if (e == 0f)
                {
                    return new Complex32(Mathf.Sign(value.Real), 0f);
                }
                else
                {
                    return new Complex32(
                        Mathf.Sign(value.Real),
                        4f * Mathf.Cos(value.Imaginary) * Mathf.Sin(value.Imaginary) * e * e);
                }
            }

            float tani = Mathf.Tan(value.Imaginary);
            float beta = 1 + tani * tani; // beta = 1/cos^2(y) = 1 + t^2
            float sinhr = GeneralMath.Sinh(value.Real);
            float coshr = GeneralMath.Cosh(value.Real);

            if (float.IsInfinity(tani))
            {
                return new Complex32(coshr / sinhr, 0f);
            }

            float denom = 1f + beta * sinhr * sinhr;
            return new Complex32(beta * coshr * sinhr / denom, tani / denom);
        }

        /// <summary>
        /// Retuns the Complex32 number, rotated by <paramref name="phase"/> radians, or exp(i*phase) 
        /// </summary>
        public Complex32 Rotation(float phase)
        {
            float cosPhase = Mathf.Cos(phase);
            float sinePhase = Mathf.Sin(phase);
            return new Complex32(
                _real * cosPhase - _imag * sinePhase,
                _real * sinePhase + _imag * cosPhase);
        }

        /// <summary>
        /// Retuns the real value of the Complex32 number after rotation by <paramref name="phase"/> radians, or exp(i*phase) 
        /// </summary>
        public float RealRotation(float phase) => _real * Mathf.Cos(phase) - _imag * Mathf.Sin(phase);


        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified complex number have the same value.
        /// </summary>
        public bool Equals(in Complex32 value)
        {
            if (IsNaN() || value.IsNaN())
            {
                return false;
            }

            if (IsInfinity() && value.IsInfinity())
            {
                return true;
            }

            return GeneralMath.Approximately(_real, value._real) &&
                GeneralMath.Approximately(_imag, value._imag);
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified complex number have the same value.
        /// </summary>
        bool IEquatable<Complex32>.Equals(Complex32 other) => Equals(other);

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified object have the same value.
        /// </summary>
        public override bool Equals(object obj) => (obj is Complex32) && Equals((Complex32)obj);

        /// <summary>
        /// Returns the hash code for the current Complex32 object.
        /// </summary>
        public override int GetHashCode()
        {
            int hash = 27;
            hash = (13 * hash) + _real.GetHashCode();
            hash = (13 * hash) + _imag.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified format for its real and imaginary parts.
        /// </summary>
        public string ToString(string format) =>
            $"({_real.ToString(format, CultureInfo.CurrentCulture)}, {_imag.ToString(format, CultureInfo.CurrentCulture)})";

        /// <summary>
        /// Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified culture-specific formatting information.
        /// </summary>
        public string ToString(IFormatProvider provider) =>
            string.Format(provider, "({0}, {1})", _real, _imag);

        /// <summary>
        /// Converts the value of the current complex number to its equivalent string representation in Cartesian form by using the specified format and culture-specific format information for its real and imaginary parts.
        /// </summary>
        public string ToString(string format, IFormatProvider provider) =>
            string.Format(provider,
                "({0}, {1})",
                _real.ToString(format, provider),
                _imag.ToString(format, provider));

        /// <summary>
        /// Converts the value of the current complex number to its equivalent string representation in Cartesian form.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"({_real}, {_imag})";

        /// <summary>
        /// Adds two complex numbers.
        /// </summary>
        public static Complex32 operator +(in Complex32 left, in Complex32 right) => new Complex32(left._real + right._real, left._imag + right._imag);

        /// <summary>
        /// Returns the additive inverse of a specified complex number.
        /// </summary>
        public static Complex32 operator -(in Complex32 value) => new Complex32(-value._real, -value._imag);

        /// <summary>
        /// Subtracts a complex number from another complex number.
        /// </summary>
        public static Complex32 operator -(in Complex32 left, in Complex32 right) => new Complex32(left._real - right._real, left._imag - right._imag);

        /// <summary>
        /// Multiplies two specified complex numbers.
        /// </summary>
        public static Complex32 operator *(in Complex32 left, in Complex32 right) => new Complex32(
                (left._real * right._real) - (left._imag * right._imag),
                (left._real * right._imag) + (left._imag * right._real));

        /// <summary>
        /// Divides a specified complex number by another specified complex number.
        /// </summary>
        public static Complex32 operator /(in Complex32 dividend, in Complex32 divisor)
        {
            if (dividend.IsZero() && divisor.IsZero())
            {
                return NaN;
            }

            if (divisor.IsZero())
            {
                return PositiveInfinity;
            }

            float a = dividend.Real;
            float b = dividend.Imaginary;
            float c = divisor.Real;
            float d = divisor.Imaginary;
            if (Mathf.Abs(d) <= Mathf.Abs(c))
            {
                return InternalDiv(a, b, c, d, false);
            }

            return InternalDiv(b, a, d, c, true);
        }

        /// <summary>
        /// Returns a value that indicates whether two complex numbers are equal.
        /// </summary>
        public static bool operator ==(in Complex32 left, in Complex32 right) => left.Equals(right);

        /// <summary>
        /// Returns a value that indicates whether two complex numbers are not equal.
        /// </summary>
        public static bool operator !=(in Complex32 left, in Complex32 right) => !left.Equals(right);

        /// <summary>
        /// Defines an implicit conversion of an unsigned byte to a complex number.
        /// </summary>
        public static implicit operator Complex32(byte value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a single-precision floating-point number to a complex number.
        /// </summary>
        public static implicit operator Complex32(float value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a double-precision floating-point number to a complex number.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Complex32(double value) => new Complex32((float)value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a signed byte to a complex number.
        /// </summary>
        public static implicit operator Complex32(sbyte value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a 64-bit unsigned integer to a complex number.
        /// </summary>
        public static implicit operator Complex32(ulong value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a 32-bit unsigned integer to a complex number.
        /// </summary>
        public static implicit operator Complex32(uint value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a 16-bit unsigned integer to a complex number.
        /// </summary>
        public static implicit operator Complex32(ushort value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a 64-bit signed integer to a complex number.
        /// </summary>
        public static implicit operator Complex32(long value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a 32-bit signed integer to a complex number.
        /// </summary>
        public static implicit operator Complex32(int value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an implicit conversion of a 16-bit signed integer to a complex number.
        /// </summary>
        public static implicit operator Complex32(short value) => new Complex32(value, 0f);

        /// <summary>
        /// Defines an explicit conversion of a System.Decimal value to a complex number.
        /// </summary>
        public static explicit operator Complex32(decimal value) => new Complex32((float)value, 0f);

        public void Deconstruct(out float real, out float imag) => (real, imag) = (_real, _imag);

        /// <summary>
        /// Gets a value indicating whether the provided Complex32 is real.
        /// </summary>
        /// <returns>true if this instance is a real number; otherwise, false.</returns>
        public bool IsReal() => _imag == 0f;

        /// <summary>
        /// Gets a value indicating whether the provided Complex32 is real and not negative, that is &gt;= 0.
        /// </summary>
        /// <returns>
        ///     true if this instance is real nonnegative number; otherwise, false.
        /// </returns>
        public bool IsRealNonNegative() => _imag == 0f && _real >= 0f;

        /// <summary>
        /// Gets a value indicating whether the Complex32 is zero.
        /// </summary>
        /// <returns>true if this instance is zero; otherwise, false.</returns>
        public bool IsZero() => _real == 0f && _imag == 0f;

        /// <summary>
        /// Gets a value indicating whether the Complex32 is one.
        /// </summary>
        /// <returns>true if this instance is one; otherwise, false.</returns>
        public bool IsOne() => _real == 1f && _imag == 0f;

        /// <summary>
        /// Gets a value indicating whether the Complex32 is the imaginary unit.
        /// </summary>
        /// <returns>true if this instance is ImaginaryOne; otherwise, false.</returns>
        public bool IsImaginaryOne() => _real == 0f && _imag == 1f;

        /// <summary>
        /// Gets a value indicating whether the provided Complex32 evaluates to an
        /// infinite value.
        /// </summary>
        /// <returns>
        ///     true if this instance is infinite; otherwise, false.
        /// </returns>
        /// <remarks>
        /// True if it either evaluates to a complex infinity
        /// or to a directed infinity.
        /// </remarks>
        public bool IsInfinity() => float.IsInfinity(_real) || float.IsInfinity(_imag);

        /// <summary>
        /// Gets a value indicating whether the provided Complex32evaluates
        /// to a value that is not a number.
        /// </summary>
        /// <returns>
        /// true if this instance is <see cref="NaN"/>; otherwise,
        /// false.
        /// </returns>
        public bool IsNaN() => float.IsNaN(_real) || float.IsNaN(_imag);

        /// <summary>
        /// Gets the squared magnitude (or squared absolute value) of a complex number.
        /// </summary>
        /// <returns>The squared magnitude of the current instance.</returns>
        public float MagnitudeSquared => (_real * _real) + (_imag * _imag);

        /// <summary>
        /// Raise this Complex32 to the given value.
        /// </summary>
        /// <param name="exponent">
        /// The exponent.
        /// </param>
        /// <returns>
        /// The complex number raised to the given exponent.
        /// </returns>
        public Complex32 Power(in Complex32 exponent)
        {
            if (IsZero())
            {
                if (exponent.IsZero())
                {
                    return One;
                }

                if (exponent.Real > 0f)
                {
                    return Zero;
                }

                if (exponent.Real < 0f)
                {
                    return exponent.Imaginary == 0f
                        ? new Complex32(float.PositiveInfinity, 0f)
                        : new Complex32(float.PositiveInfinity, float.PositiveInfinity);
                }

                return NaN;
            }

            return (exponent * NaturalLogarithm()).Exponential();
        }

        /// <summary>
        /// Natural Logarithm of this Complex32 (Base E).
        /// </summary>
        /// <returns>The natural logarithm of this complex number.</returns>
        public Complex32 NaturalLogarithm()
        {
            if (IsRealNonNegative())
            {
                return new Complex32(Mathf.Log(_real), 0f);
            }

            return new Complex32(0.5f * Mathf.Log(MagnitudeSquared), Phase);
        }

        /// <summary>
        /// Common Logarithm of this Complex32 (Base 10).
        /// </summary>
        /// <returns>The common logarithm of this complex number.</returns>
        public Complex32 CommonLogarithm()
        {
            if (IsRealNonNegative())
            {
                return new Complex32(Mathf.Log10(_real), 0f);
            }

            return new Complex32(0.5f * Mathf.Log10(MagnitudeSquared), Phase);
        }

        /// <summary>
        /// Logarithm of this Complex32 with custom base.
        /// </summary>
        /// <returns>The logarithm of this complex number.</returns>
        public Complex32 Logarithm(float baseValue) => NaturalLogarithm() / Mathf.Log(baseValue);

        /// <summary>
        /// Exponential of this Complex32 (exp(x), E^x).
        /// </summary>
        /// <returns>
        /// The exponential of this complex number.
        /// </returns>
        public Complex32 Exponential()
        {
            float exp = Mathf.Exp(_real);
            if (IsReal())
            {
                return new Complex32(exp, 0f);
            }

            return new Complex32(exp * Mathf.Cos(_imag), exp * Mathf.Sin(_imag));
        }

        /// <summary>
        /// Returns the real component of the product of this Complex32 with other.
        /// </summary>
        /// <returns>
        /// The real component of the product.
        /// </returns>
        public float RealProduct(in Complex32 other) => _real * other._real - _imag * other._imag;

        /// <summary>
        /// Computes the conjugate of a complex number and returns the result.
        /// </summary>
        public Complex32 Conjugate() => new Complex32(_real, -_imag);

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        public Complex32 Reciprocal() => IsZero() ? Zero : 1f / this;


        /// <summary>
        /// The Square Root (power 1/2) of this Complex32
        /// </summary>
        /// <returns>
        /// The square root of this complex number.
        /// </returns>
        public Complex32 SquareRoot()
        {
            if (IsRealNonNegative())
            {
                return new Complex32(Mathf.Sqrt(_real), 0f);
            }

            Complex32 result;

            float absReal = Mathf.Abs(Real);
            float absImag = Mathf.Abs(Imaginary);
            float w;
            if (absReal >= absImag)
            {
                float ratio = Imaginary / Real;
                w = Mathf.Sqrt(absReal) * Mathf.Sqrt(0.5f * (1f + Mathf.Sqrt(1f + (ratio * ratio))));
            }
            else
            {
                float ratio = Real / Imaginary;
                w = Mathf.Sqrt(absImag) * Mathf.Sqrt(0.5f * (Mathf.Abs(ratio) + Mathf.Sqrt(1f + (ratio * ratio))));
            }

            if (Real >= 0f)
            {
                result = new Complex32(w, (Imaginary / (2f * w)));
            }
            else if (Imaginary >= 0f)
            {
                result = new Complex32((absImag / (2f * w)), w);
            }
            else
            {
                result = new Complex32((absImag / (2f * w)), -w);
            }

            return result;
        }

        /// <summary>
        /// Helper method for dividing.
        /// </summary>
        /// <param name="a">Re first</param>
        /// <param name="b">Im first</param>
        /// <param name="c">Re second</param>
        /// <param name="d">Im second</param>
        private static Complex32 InternalDiv(float a, float b, float c, float d, bool swapped)
        {
            float r = d / c;
            float t = 1 / (c + d * r);
            float e, f;

            if (r != 0f) // one can use r >= float.Epsilon || r <= float.Epsilon instead
            {
                e = (a + b * r) * t;
                f = (b - a * r) * t;
            }
            else
            {
                e = (a + d * (b / c)) * t;
                f = (b - d * (a / c)) * t;
            }

            if (swapped)
            {
                f = -f;
            }

            return new Complex32(e, f);
        }
    }
}