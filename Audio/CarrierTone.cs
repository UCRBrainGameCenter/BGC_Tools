using System;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Float-based description of a single carrier tone, for tone composition
    /// </summary>
    public readonly struct CarrierTone
    {
        public readonly float frequency;
        public readonly float amplitude;
        public readonly float phase;

        public CarrierTone(float frequency, float amplitude = 1f, float phase = 0f)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
            this.phase = phase;
        }

        public CarrierTone WithNewPhase(float newPhase) =>
            new CarrierTone(frequency, amplitude, newPhase);

        public CarrierTone RotatePhase(float rotator) =>
            new CarrierTone(frequency, amplitude, GeneralMath.Repeat(phase + rotator, -GeneralMath.fPI, GeneralMath.fPI));
    }
}
