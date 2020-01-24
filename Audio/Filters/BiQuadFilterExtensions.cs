using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGC.Audio.Filters
{
    public static class SpecializedFilterExtensions
    {
        public static IBGCStream BiQuadBandpassFilter(
            this IBGCStream stream,
            double criticalFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate) =>
            BiQuadFilter.BandpassFilter(stream, criticalFrequency, qFactor, rmsBehavior);

        public static IBGCStream BiQuadHighpassFilter(
            this IBGCStream stream,
            double criticalFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate) =>
            BiQuadFilter.HighpassFilter(stream, criticalFrequency, qFactor, rmsBehavior);

        public static IBGCStream BiQuadLowpassFilter(
            this IBGCStream stream,
            double criticalFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate) =>
            BiQuadFilter.LowpassFilter(stream, criticalFrequency, qFactor, rmsBehavior);

        public static IBGCStream BiQuadNotchFilter(
            this IBGCStream stream,
            double criticalFrequency,
            double qFactor = double.NaN,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate) =>
            BiQuadFilter.NotchFilter(stream, criticalFrequency, qFactor, rmsBehavior);

        public static IBGCStream BiQuadLowShelfFilter(
            this IBGCStream stream,
            double criticalFrequency,
            double dbGain,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate) =>
            BiQuadFilter.LowShelfFilter(stream, criticalFrequency, dbGain, rmsBehavior);

        public static IBGCStream BiQuadHighShelfFilter(
            this IBGCStream stream,
            double criticalFrequency,
            double dbGain,
            TransformRMSBehavior rmsBehavior = TransformRMSBehavior.Recalculate) =>
            BiQuadFilter.HighShelfFilter(stream, criticalFrequency, dbGain, rmsBehavior);
    }
}
