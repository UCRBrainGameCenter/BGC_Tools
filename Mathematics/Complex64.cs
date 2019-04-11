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

namespace BGC.Mathematics
{
    /// <summary>
    /// Represents a complex number with double-precision floating point components
    /// </summary>
    public readonly struct Complex64 : IEquatable<Complex64>, IFormattable
    {
        /// <summary>
        /// The real component of the complex number.
        /// </summary>
        [DataMember(Order = 1)]
        private readonly double _real;

        /// <summary>
        /// The imaginary component of the complex number.
        /// </summary>
        [DataMember(Order = 2)]
        private readonly double _imag;


        /// <summary>
        /// Returns a new Complex64 instance with a real number equal to zero and an imaginary number equal to zero.
        /// </summary>
        public static readonly Complex64 Zero = new Complex64(0.0, 0.0);

        /// <summary>
        /// Returns a new Complex64 instance with a real number equal to one and an imaginary number equal to zero.
        /// </summary>
        public static readonly Complex64 One = new Complex64(1.0, 0.0);

        /// <summary>
        /// Returns a new Complex64 instance with a real number equal to zero and an imaginary number equal to one.
        /// </summary>
        public static readonly Complex64 ImaginaryOne = new Complex64(0.0, 1.0);

        /// <summary>
        /// Returns a new Complex64 instance with real and imaginary numbers positive infinite.
        /// </summary>
        public static readonly Complex64 PositiveInfinity = new Complex64(double.PositiveInfinity, double.PositiveInfinity);

        /// <summary>
        /// Returns a new Complex64 instance with real and imaginary numbers not a number.
        /// </summary>
        public static readonly Complex64 NaN = new Complex64(double.NaN, double.NaN);

        /// <summary>
        /// Initializes a new Complex64 structure using the specified real and imaginary values.
        /// </summary>
        public Complex64(double real, double imaginary)
        {
            _real = real;
            _imag = imaginary;
        }

        /// <summary>
        /// Gets the imaginary component of the current System.Numerics.Complex64 object.
        /// </summary>
        public double Imaginary => _imag;

        /// <summary>
        /// Gets the real component of the current System.Numerics.Complex64 object.
        /// </summary>
        public double Real => _real;

        /// <summary>
        /// Gets the magnitude (or absolute value) of a complex number.
        /// </summary>
        public double Magnitude
        {
            get
            {
                if (double.IsNaN(_real) || double.IsNaN(_imag))
                {
                    return double.NaN;
                }

                if (double.IsInfinity(_real) || double.IsInfinity(_imag))
                {
                    return double.PositiveInfinity;
                }

                double a = Math.Abs(_real);
                double b = Math.Abs(_imag);

                if (a > b)
                {
                    double tmp = b / a;
                    return a * Math.Sqrt(1.0 + tmp * tmp);
                }

                if (a == 0.0) // one can write a >= double.Epsilon here
                {
                    return b;
                }
                else
                {
                    double tmp = a / b;
                    return b * Math.Sqrt(1.0 + tmp * tmp);
                }
            }
        }

        /// <summary>
        /// Gets the phase of a complex number in radians.
        /// </summary>
        public double Phase => _imag == 0.0 && _real < 0.0 ? Math.PI : Math.Atan2(_imag, _real);

        /// <summary>
        /// Gets the absolute value (or magnitude) of a complex number.
        /// </summary>
        public static double Abs(in Complex64 value) => value.Magnitude;

        /// <summary>
        /// Trigonometric principal Arc Cosine of this Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc cosine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Acos(in Complex64 value)
        {
            if (value.Imaginary < 0.0 || value.Imaginary == 0.0 && value.Real > 0.0)
            {
                return Math.PI - Acos(-value);
            }

            return -ImaginaryOne * (value + (ImaginaryOne * (1.0 - value.Square()).SquareRoot())).NaturalLogarithm();
        }

        /// <summary>
        /// Returns the sum of the two Complex64 inputs
        /// </summary>
        public static Complex64 Add(in Complex64 left, in Complex64 right) => left + right;

        /// <summary>
        /// Trigonometric principal Arc Sine of this Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc sine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Asin(in Complex64 value)
        {
            if (value.Imaginary > 0.0 || value.Imaginary == 0.0 && value.Real < 0.0)
            {
                return -Asin(-value);
            }

            return -ImaginaryOne * ((1.0 - value.Square()).SquareRoot() + (ImaginaryOne * value)).NaturalLogarithm();
        }

        /// <summary>
        /// Trigonometric principal Arc Tangent of this Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The arc tangent of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Atan(in Complex64 value)
        {
            Complex64 iz = new Complex64(-value.Imaginary, value.Real); // I*this
            return new Complex64(0.0, 0.5) * ((1.0 - iz).NaturalLogarithm() - (1.0 + iz).NaturalLogarithm());
        }

        /// <summary>
        /// Computes the conjugate of a complex number and returns the result.
        /// </summary>
        public static Complex64 Conjugate(in Complex64 value) => value.Conjugate();

        /// <summary>
        /// Trigonometric Cosine of a Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The cosine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Cos(in Complex64 value)
        {
            if (value.IsReal())
            {
                return new Complex64(Math.Cos(value.Real), 0.0);
            }

            return new Complex64(
                Math.Cos(value.Real) * GeneralMath.Cosh(value.Imaginary),
                -Math.Sin(value.Real) * GeneralMath.Sinh(value.Imaginary));
        }

        /// <summary>
        /// Hyperbolic Cosine of a Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic cosine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Cosh(in Complex64 value)
        {
            if (value.IsReal())
            {
                return new Complex64(GeneralMath.Cosh(value.Real), 0.0);
            }

            // cosh(x + j*y) = cosh(x)*cos(y) + j*sinh(x)*sin(y)
            // if x > huge, cosh(x + j*y) = exp(|x|)/2*cos(y) + j*sign(x)*exp(|x|)/2*sin(y)

            if (Math.Abs(value.Real) >= 22.0) // Taken from the msun library in FreeBSD
            {
                double h = Math.Exp(Math.Abs(value.Real)) * 0.5;
                return new Complex64(
                    h * Math.Cos(value.Imaginary),
                    Math.Sign(value.Real) * h * Math.Sin(value.Imaginary));
            }

            return new Complex64(
                GeneralMath.Cosh(value.Real) * Math.Cos(value.Imaginary),
                GeneralMath.Sinh(value.Real) * Math.Sin(value.Imaginary));
        }

        /// <summary>
        /// Divides one complex number by another and returns the result.
        /// </summary>
        public static Complex64 Divide(in Complex64 dividend, in Complex64 divisor) => dividend / divisor;

        /// <summary>
        /// Returns e raised to the power specified by a complex number.
        /// </summary>
        public static Complex64 Exp(in Complex64 value) => value.Exponential();

        /// <summary>
        /// Creates a complex number from a point's polar coordinates.
        /// </summary>
        public static Complex64 FromPolarCoordinates(double magnitude, double phase) =>
            new Complex64(magnitude * Math.Cos(phase), magnitude * Math.Sin(phase));

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified complex number.
        /// </summary>
        public static Complex64 Log(in Complex64 value) => value.NaturalLogarithm();

        /// <summary>
        /// Returns the logarithm of a specified complex number in a specified base.
        /// </summary>
        public static Complex64 Log(in Complex64 value, double baseValue) => value.Logarithm(baseValue);

        /// <summary>
        /// Returns the base-10 logarithm of a specified complex number.
        /// </summary>
        public static Complex64 Log10(in Complex64 value) => value.CommonLogarithm();

        /// <summary>
        /// Returns the product of two complex numbers.
        /// </summary>
        public static Complex64 Multiply(in Complex64 left, in Complex64 right) => left * right;

        /// <summary>
        /// Returns the additive inverse of a specified complex number.
        /// </summary>
        public static Complex64 Negate(in Complex64 value) => -value;

        /// <summary>
        /// Returns a specified complex number raised to a power specified by a double-precision doubleing-point number.
        /// </summary>
        public static Complex64 Pow(in Complex64 value, double power) => value.Power(power);

        /// <summary>
        /// Returns a specified complex number raised to a power specified by a complex number.
        /// </summary>
        public static Complex64 Pow(in Complex64 value, in Complex64 power) => value.Power(power);

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        public static Complex64 Reciprocal(in Complex64 value) => value.Reciprocal();

        /// <summary>
        /// Trigonometric Sine of a Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The sine of the complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Sin(in Complex64 value)
        {
            if (value.IsReal())
            {
                return new Complex64(Math.Sin(value.Real), 0.0);
            }

            return new Complex64(
                Math.Sin(value.Real) * GeneralMath.Cosh(value.Imaginary),
                Math.Cos(value.Real) * GeneralMath.Sinh(value.Imaginary));
        }

        /// <summary>
        /// Hyperbolic Sine of a Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic sine of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Sinh(in Complex64 value)
        {
            if (value.IsReal())
            {
                return new Complex64(GeneralMath.Sinh(value.Real), 0.0);
            }

            // sinh(x + j y) = sinh(x)*cos(y) + j*cosh(x)*sin(y)
            // if x > huge, sinh(x + jy) = sign(x)*exp(|x|)/2*cos(y) + j*exp(|x|)/2*sin(y)

            if (Math.Abs(value.Real) >= 22.0) // Taken from the msun library in FreeBSD
            {
                double h = Math.Exp(Math.Abs(value.Real)) * 0.5;
                return new Complex64(
                    Math.Sign(value.Real) * h * Math.Cos(value.Imaginary),
                    h * Math.Sin(value.Imaginary));
            }

            return new Complex64(
                GeneralMath.Sinh(value.Real) * Math.Cos(value.Imaginary),
                GeneralMath.Cosh(value.Real) * Math.Sin(value.Imaginary));
        }

        /// <summary>
        /// The Square (power 2) of this Complex64
        /// </summary>
        /// <returns>
        /// The square of this complex number.
        /// </returns>
        public Complex64 Square()
        {
            if (IsReal())
            {
                return new Complex64(_real * _real, 0.0);
            }

            return new Complex64((_real * _real) - (_imag * _imag), 2 * _real * _imag);
        }

        /// <summary>
        /// The Square Root of a complex number
        /// </summary>
        // From Mathnet.Numerics
        public static Complex64 Sqrt(in Complex64 value) => value.SquareRoot();

        /// <summary>
        /// The difference between two Complex64 numbers;
        /// </summary>
        /// <returns>The complex difference.</returns>
        // From Mathnet.Numerics
        public static Complex64 Subtract(in Complex64 left, Complex64 right) => left - right;

        /// <summary>
        /// Trigonometric Tangent of a Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The tangent of the complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Tan(in Complex64 value)
        {
            if (value.IsReal())
            {
                return new Complex64(Math.Tan(value.Real), 0.0);
            }

            // tan(z) = - j*tanh(j*z)

            Complex64 z = Tanh(new Complex64(-value.Imaginary, value.Real));
            return new Complex64(z.Imaginary, -z.Real);
        }

        /// <summary>
        /// Hyperbolic Tangent of a Complex64 number.
        /// </summary>
        /// <param name="value">The complex value.</param>
        /// <returns>The hyperbolic tangent of a complex number.</returns>
        // From Mathnet.Numerics
        public static Complex64 Tanh(in Complex64 value)
        {
            if (value.IsReal())
            {
                return new Complex64(GeneralMath.Tanh(value.Real), 0.0);
            }

            // tanh(x + j*y) = (cosh(x)*sinh(x)/cos^2(y) + j*tan(y))/(1 + sinh^2(x)/cos^2(y))
            // if |x| > huge, tanh(z) = sign(x) + j*4*cos(y)*sin(y)*exp(-2*|x|)
            // if exp(-|x|) = 0, tanh(z) = sign(x)
            // if tan(y) = +/- oo or 1/cos^2(y) = 1 + tan^2(y) = oo, tanh(z) = cosh(x)/sinh(x)
            //
            // The algorithm is based on Kahan.

            if (Math.Abs(value.Real) >= 22) // Taken from the msun library in FreeBSD
            {
                double e = Math.Exp(-Math.Abs(value.Real));
                if (e == 0.0)
                {
                    return new Complex64(Math.Sign(value.Real), 0.0);
                }
                else
                {
                    return new Complex64(
                        Math.Sign(value.Real),
                        4.0 * Math.Cos(value.Imaginary) * Math.Sin(value.Imaginary) * e * e);
                }
            }

            double tani = Math.Tan(value.Imaginary);
            double beta = 1 + tani * tani; // beta = 1/cos^2(y) = 1 + t^2
            double sinhr = GeneralMath.Sinh(value.Real);
            double coshr = GeneralMath.Cosh(value.Real);

            if (double.IsInfinity(tani))
            {
                return new Complex64(coshr / sinhr, 0.0);
            }

            double denom = 1.0 + beta * sinhr * sinhr;
            return new Complex64(beta * coshr * sinhr / denom, tani / denom);
        }

        /// <summary>
        /// Retuns the Complex64 number, rotated by <paramref name="phase"/> radians, or exp(i*phase) 
        /// </summary>
        public Complex64 Rotation(double phase)
        {
            double cosPhase = Math.Cos(phase);
            double sinePhase = Math.Sin(phase);
            return new Complex64(
                _real * cosPhase - _imag * sinePhase,
                _real * sinePhase + _imag * cosPhase);
        }

        /// <summary>
        /// Retuns the real value of the Complex64 number after rotation by <paramref name="phase"/> radians, or exp(i*phase) 
        /// </summary>
        public double RealRotation(double phase) => _real * Math.Cos(phase) - _imag * Math.Sin(phase);

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified complex number have the same value.
        /// </summary>
        public bool Equals(in Complex64 value)
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
        bool IEquatable<Complex64>.Equals(Complex64 other) => Equals(other);

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified object have the same value.
        /// </summary>
        public override bool Equals(object obj) => (obj is Complex64) && Equals((Complex64)obj);

        /// <summary>
        /// Returns the hash code for the current Complex64 object.
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
        public static Complex64 operator +(in Complex64 left, in Complex64 right) => new Complex64(left._real + right._real, left._imag + right._imag);

        /// <summary>
        /// Returns the additive inverse of a specified complex number.
        /// </summary>
        public static Complex64 operator -(in Complex64 value) => new Complex64(-value._real, -value._imag);

        /// <summary>
        /// Subtracts a complex number from another complex number.
        /// </summary>
        public static Complex64 operator -(in Complex64 left, in Complex64 right) => new Complex64(left._real - right._real, left._imag - right._imag);

        /// <summary>
        /// Multiplies two specified complex numbers.
        /// </summary>
        public static Complex64 operator *(in Complex64 left, in Complex64 right) => new Complex64(
                (left._real * right._real) - (left._imag * right._imag),
                (left._real * right._imag) + (left._imag * right._real));

        /// <summary>
        /// Divides a specified complex number by another specified complex number.
        /// </summary>
        public static Complex64 operator /(in Complex64 dividend, in Complex64 divisor)
        {
            if (dividend.IsZero() && divisor.IsZero())
            {
                return NaN;
            }

            if (divisor.IsZero())
            {
                return PositiveInfinity;
            }

            double a = dividend.Real;
            double b = dividend.Imaginary;
            double c = divisor.Real;
            double d = divisor.Imaginary;
            if (Math.Abs(d) <= Math.Abs(c))
            {
                return InternalDiv(a, b, c, d, false);
            }

            return InternalDiv(b, a, d, c, true);
        }

        /// <summary>
        /// Returns a value that indicates whether two complex numbers are equal.
        /// </summary>
        public static bool operator ==(in Complex64 left, in Complex64 right) => left.Equals(right);

        /// <summary>
        /// Returns a value that indicates whether two complex numbers are not equal.
        /// </summary>
        public static bool operator !=(in Complex64 left, in Complex64 right) => !left.Equals(right);

        /// <summary>
        /// Defines an implicit conversion of an unsigned byte to a complex number.
        /// </summary>
        public static implicit operator Complex64(Complex32 value) => new Complex64(value.Real, value.Imaginary);

        /// <summary>
        /// Defines an implicit conversion of an unsigned byte to a complex number.
        /// </summary>
        public static implicit operator Complex64(byte value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a single-precision floating-point number to a complex number.
        /// </summary>
        public static implicit operator Complex64(float value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a double-precision floating-point number to a complex number.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Complex64(double value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a signed byte to a complex number.
        /// </summary>
        public static implicit operator Complex64(sbyte value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a 64-bit unsigned integer to a complex number.
        /// </summary>
        public static implicit operator Complex64(ulong value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a 32-bit unsigned integer to a complex number.
        /// </summary>
        public static implicit operator Complex64(uint value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a 16-bit unsigned integer to a complex number.
        /// </summary>
        public static implicit operator Complex64(ushort value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a 64-bit signed integer to a complex number.
        /// </summary>
        public static implicit operator Complex64(long value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a 32-bit signed integer to a complex number.
        /// </summary>
        public static implicit operator Complex64(int value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an implicit conversion of a 16-bit signed integer to a complex number.
        /// </summary>
        public static implicit operator Complex64(short value) => new Complex64(value, 0.0);

        /// <summary>
        /// Defines an explicit conversion of a System.Decimal value to a complex number.
        /// </summary>
        public static explicit operator Complex64(decimal value) => new Complex64((double)value, 0.0);

        /// <summary>
        /// Gets a value indicating whether the provided Complex64 is real.
        /// </summary>
        /// <returns>true if this instance is a real number; otherwise, false.</returns>
        public bool IsReal() => _imag == 0.0;

        /// <summary>
        /// Gets a value indicating whether the provided Complex64 is real and not negative, that is &gt;= 0.
        /// </summary>
        /// <returns>
        ///     true if this instance is real nonnegative number; otherwise, false.
        /// </returns>
        public bool IsRealNonNegative() => _imag == 0.0 && _real >= 0.0;

        /// <summary>
        /// Gets a value indicating whether the Complex64 is zero.
        /// </summary>
        /// <returns>true if this instance is zero; otherwise, false.</returns>
        public bool IsZero() => _real == 0.0 && _imag == 0.0;

        /// <summary>
        /// Gets a value indicating whether the Complex64 is one.
        /// </summary>
        /// <returns>true if this instance is one; otherwise, false.</returns>
        public bool IsOne() => _real == 1.0 && _imag == 0.0;

        /// <summary>
        /// Gets a value indicating whether the Complex64 is the imaginary unit.
        /// </summary>
        /// <returns>true if this instance is ImaginaryOne; otherwise, false.</returns>
        public bool IsImaginaryOne() => _real == 0.0 && _imag == 1.0;

        /// <summary>
        /// Gets a value indicating whether the provided Complex64 evaluates to an
        /// infinite value.
        /// </summary>
        /// <returns>
        ///     true if this instance is infinite; otherwise, false.
        /// </returns>
        /// <remarks>
        /// True if it either evaluates to a complex infinity
        /// or to a directed infinity.
        /// </remarks>
        public bool IsInfinity() => double.IsInfinity(_real) || double.IsInfinity(_imag);

        /// <summary>
        /// Gets a value indicating whether the provided Complex64evaluates
        /// to a value that is not a number.
        /// </summary>
        /// <returns>
        /// true if this instance is <see cref="NaN"/>; otherwise,
        /// false.
        /// </returns>
        public bool IsNaN() => double.IsNaN(_real) || double.IsNaN(_imag);

        /// <summary>
        /// Gets the squared magnitude (or squared absolute value) of a complex number.
        /// </summary>
        /// <returns>The squared magnitude of the current instance.</returns>
        public double MagnitudeSquared => (_real * _real) + (_imag * _imag);

        /// <summary>
        /// Raise this Complex64 to the given value.
        /// </summary>
        /// <param name="exponent">
        /// The exponent.
        /// </param>
        /// <returns>
        /// The complex number raised to the given exponent.
        /// </returns>
        public Complex64 Power(in Complex64 exponent)
        {
            if (IsZero())
            {
                if (exponent.IsZero())
                {
                    return One;
                }

                if (exponent.Real > 0.0)
                {
                    return Zero;
                }

                if (exponent.Real < 0.0)
                {
                    return exponent.Imaginary == 0.0
                        ? new Complex64(double.PositiveInfinity, 0.0)
                        : new Complex64(double.PositiveInfinity, double.PositiveInfinity);
                }

                return NaN;
            }

            return (exponent * NaturalLogarithm()).Exponential();
        }

        /// <summary>
        /// Natural Logarithm of this Complex64 (Base E).
        /// </summary>
        /// <returns>The natural logarithm of this complex number.</returns>
        public Complex64 NaturalLogarithm()
        {
            if (IsRealNonNegative())
            {
                return new Complex64(Math.Log(_real), 0.0);
            }

            return new Complex64(0.5 * Math.Log(MagnitudeSquared), Phase);
        }

        /// <summary>
        /// Common Logarithm of this Complex64 (Base 10).
        /// </summary>
        /// <returns>The common logarithm of this complex number.</returns>
        public Complex64 CommonLogarithm()
        {
            if (IsRealNonNegative())
            {
                return new Complex64(Math.Log10(_real), 0.0);
            }

            return new Complex64(0.5 * Math.Log10(MagnitudeSquared), Phase);
        }

        /// <summary>
        /// Logarithm of this Complex64 with custom base.
        /// </summary>
        /// <returns>The logarithm of this complex number.</returns>
        public Complex64 Logarithm(double baseValue) => NaturalLogarithm() / Math.Log(baseValue);

        /// <summary>
        /// Exponential of this Complex64 (exp(x), E^x).
        /// </summary>
        /// <returns>
        /// The exponential of this complex number.
        /// </returns>
        public Complex64 Exponential()
        {
            double exp = Math.Exp(_real);
            if (IsReal())
            {
                return new Complex64(exp, 0.0);
            }

            return new Complex64(exp * Math.Cos(_imag), exp * Math.Sin(_imag));
        }

        /// <summary>
        /// Returns the real component of the product of this Complex64 with other.
        /// </summary>
        /// <returns>
        /// The real component of the product.
        /// </returns>
        public double RealProduct(in Complex64 other) => _real * other._real - _imag * other._imag;

        /// <summary>
        /// Computes the conjugate of a complex number and returns the result.
        /// </summary>
        public Complex64 Conjugate() => new Complex64(_real, -_imag);

        /// <summary>
        /// Returns the multiplicative inverse of a complex number.
        /// </summary>
        public Complex64 Reciprocal() => IsZero() ? Zero : 1.0 / this;


        /// <summary>
        /// The Square Root (power 1/2) of this Complex64
        /// </summary>
        /// <returns>
        /// The square root of this complex number.
        /// </returns>
        public Complex64 SquareRoot()
        {
            if (IsRealNonNegative())
            {
                return new Complex64(Math.Sqrt(_real), 0.0);
            }

            Complex64 result;

            double absReal = Math.Abs(Real);
            double absImag = Math.Abs(Imaginary);
            double w;
            if (absReal >= absImag)
            {
                double ratio = Imaginary / Real;
                w = Math.Sqrt(absReal) * Math.Sqrt(0.5 * (1.0 + Math.Sqrt(1.0 + (ratio * ratio))));
            }
            else
            {
                double ratio = Real / Imaginary;
                w = Math.Sqrt(absImag) * Math.Sqrt(0.5 * (Math.Abs(ratio) + Math.Sqrt(1.0 + (ratio * ratio))));
            }

            if (Real >= 0.0)
            {
                result = new Complex64(w, (Imaginary / (2.0 * w)));
            }
            else if (Imaginary >= 0.0)
            {
                result = new Complex64((absImag / (2.0 * w)), w);
            }
            else
            {
                result = new Complex64((absImag / (2.0 * w)), -w);
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
        private static Complex64 InternalDiv(double a, double b, double c, double d, bool swapped)
        {
            double r = d / c;
            double t = 1 / (c + d * r);
            double e, f;

            if (r != 0.0) // one can use r >= double.Epsilon || r <= double.Epsilon instead
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

            return new Complex64(e, f);
        }
    }
}