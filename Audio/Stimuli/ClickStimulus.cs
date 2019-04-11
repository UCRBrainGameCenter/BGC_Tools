using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Audio.Envelopes;

namespace BGC.Audio.Stimuli
{
    public static class ClickStimulus
    {
        public static IBGCStream ConstructClick(
            double gaussianWidth,
            double peakTime,
            ComplexCarrierTone carrierTone,
            bool peakAlign = true,
            double sigmaFloor = 3.0)
        {
            if (peakAlign)
            {
                carrierTone = carrierTone.WithNewPhase((Math.PI / 2.0) - 2.0 * Math.PI * carrierTone.frequency * peakTime);
            }

            return carrierTone.ToStream().ApplyEnvelope(
                new GaussianEnvelope(
                    gaussianWidth: gaussianWidth,
                    peakTime: peakTime,
                    sigmaFloor: sigmaFloor));
        }

        public static IBGCStream ConstructClick(
            double gaussianWidth,
            double peakTime,
            IEnumerable<ComplexCarrierTone> carrierTones,
            double timeShift = 0.0,
            double sigmaFloor = 3.0)
        {
            ComplexCarrierTone[] tonesCopy = carrierTones.ToArray();

            if (timeShift != 0.0)
            {
                for (int i = 0; i < tonesCopy.Length; i++)
                {
                    tonesCopy[i] = tonesCopy[i].TimeShift(timeShift);
                }
            }

            return carrierTones.ToStream().ApplyEnvelope(
                new GaussianEnvelope(
                    gaussianWidth: gaussianWidth,
                    peakTime: peakTime,
                    sigmaFloor: sigmaFloor));
        }

        public static IBGCStream ConstructClick(
            double gaussianWidth,
            double peakTime,
            IBGCStream carrierStream,
            double timeShift = 0.0,
            double sigmaFloor = 3.0)
        {
            if (timeShift != 0.0)
            {
                carrierStream = carrierStream.TimeShift(timeShift);
            }

            return carrierStream.ApplyEnvelope(
                new GaussianEnvelope(
                    gaussianWidth: gaussianWidth,
                    peakTime: peakTime,
                    sigmaFloor: sigmaFloor));
        }
    }
}
