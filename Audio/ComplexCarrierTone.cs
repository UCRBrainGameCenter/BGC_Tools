using System;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Double, Complex64-based description of a single carrier tone, for tone
    /// composition.
    /// </summary>
    public readonly struct ComplexCarrierTone
    {
        public readonly double frequency;
        public readonly Complex64 amplitude;

        public ComplexCarrierTone(double frequency, Complex64 amplitude)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
        }

        public ComplexCarrierTone(double frequency)
        {
            this.frequency = frequency;
            amplitude = 1.0;
        }

        public ComplexCarrierTone WithNewPhase(double newPhase) => new ComplexCarrierTone(
            frequency: frequency,
            amplitude: Complex64.FromPolarCoordinates(
                magnitude: amplitude.Magnitude,
                phase: newPhase));

        public ComplexCarrierTone RotatePhase(double rotator) => new ComplexCarrierTone(
            frequency: frequency,
            amplitude: amplitude.Rotation(rotator));

        public ComplexCarrierTone TimeShift(double deltaT) => RotatePhase(2.0 * Math.PI * frequency * deltaT);
    }
}
