using System;
using System.Collections.Generic;
using BGC.Audio;

using AudioChannel = BGC.Audio.AudioChannel;

namespace BGC.Parameters
{
    public static class SetupMethods
    {
        public static List<ValueNamePair> GenFreq0123()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)GenerationPhase.Instance, GenerationPhase.Instance.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Interval, GenerationPhase.Interval.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Trial, GenerationPhase.Trial.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Task, GenerationPhase.Task.ToDisplayName())
            };
        }
        public static List<ValueNamePair> GenFreq023()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)GenerationPhase.Instance, GenerationPhase.Instance.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Trial, GenerationPhase.Trial.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Task, GenerationPhase.Task.ToDisplayName())
            };
        }

        public static List<ValueNamePair> GenFreq123()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)GenerationPhase.Interval, GenerationPhase.Interval.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Trial, GenerationPhase.Trial.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Task, GenerationPhase.Task.ToDisplayName())
            };
        }

        public static List<ValueNamePair> GenFreq23()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)GenerationPhase.Trial, GenerationPhase.Trial.ToDisplayName()),
                new ValueNamePair((int)GenerationPhase.Task, GenerationPhase.Task.ToDisplayName())
            };
        }

        public static List<ValueNamePair> GenFreqBlock23()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)GenerationPhase.Trial, GenerationPhase.Trial.ToAltDisplayName()),
                new ValueNamePair((int)GenerationPhase.Task, GenerationPhase.Task.ToAltDisplayName())
            };
        }

        public static List<ValueNamePair> GenFreqBlock123()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)GenerationPhase.Interval, GenerationPhase.Interval.ToAltDisplayName()),
                new ValueNamePair((int)GenerationPhase.Trial, GenerationPhase.Trial.ToAltDisplayName()),
                new ValueNamePair((int)GenerationPhase.Task, GenerationPhase.Task.ToAltDisplayName())
            };
        }

        public static List<ValueNamePair> StandardWindowingChoices()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)Windowing.Function.Hamming, Windowing.Function.Hamming.ToDisplayName()),
                new ValueNamePair((int)Windowing.Function.Hann, Windowing.Function.Hann.ToDisplayName()),
                new ValueNamePair((int)Windowing.Function.BlackmanHarris, Windowing.Function.BlackmanHarris.ToDisplayName()),
                new ValueNamePair((int)Windowing.Function.Sine, Windowing.Function.Sine.ToDisplayName()),
                new ValueNamePair((int)Windowing.Function.Linear, Windowing.Function.Linear.ToDisplayName()),
                new ValueNamePair((int)Windowing.Function.Square, Windowing.Function.Square.ToDisplayName())
            };
        }

        public static List<ValueNamePair> FrequencyDistributionChoices()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)FrequencyDistribution.Exponential, FrequencyDistribution.Exponential.ToDisplayName()),
                new ValueNamePair((int)FrequencyDistribution.Linear, FrequencyDistribution.Linear.ToDisplayName())
            };
        }

        public static List<ValueNamePair> AmplitudeFactorChoices()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)AmplitudeFactor.Violet, AmplitudeFactor.Violet.ToDisplayName()),
                new ValueNamePair((int)AmplitudeFactor.Blue, AmplitudeFactor.Blue.ToDisplayName()),
                new ValueNamePair((int)AmplitudeFactor.White, AmplitudeFactor.White.ToDisplayName()),
                new ValueNamePair((int)AmplitudeFactor.Pink, AmplitudeFactor.Pink.ToDisplayName()),
                new ValueNamePair((int)AmplitudeFactor.Brown, AmplitudeFactor.Brown.ToDisplayName())
            };
        }

        public static List<ValueNamePair> AmplitudeDistributionChoices()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)AmplitudeDistribution.Unitary, AmplitudeDistribution.Unitary.ToDisplayName()),
                new ValueNamePair((int)AmplitudeDistribution.FlatRandom, AmplitudeDistribution.FlatRandom.ToDisplayName()),
                new ValueNamePair((int)AmplitudeDistribution.Rayleigh, AmplitudeDistribution.Rayleigh.ToDisplayName())
            };
        }

        public static List<ValueNamePair> AudioChannelPresentationNames()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)AudioChannel.Left, AudioChannel.Left.ToPresentationName()),
                new ValueNamePair((int)AudioChannel.Right, AudioChannel.Right.ToPresentationName()),
                new ValueNamePair((int)AudioChannel.Both, AudioChannel.Both.ToPresentationName())
            };
        }

        public static List<ValueNamePair> AudioChannelLRPresentationNames()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)AudioChannel.Left, AudioChannel.Left.ToSimplePresentationName()),
                new ValueNamePair((int)AudioChannel.Right, AudioChannel.Right.ToSimplePresentationName())
            };
        }

        public static List<ValueNamePair> AudioChannelLRBPresentationNames()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)AudioChannel.Left, AudioChannel.Left.ToSimplePresentationName()),
                new ValueNamePair((int)AudioChannel.Right, AudioChannel.Right.ToSimplePresentationName()),
                new ValueNamePair((int)AudioChannel.Both, AudioChannel.Both.ToSimplePresentationName())
            };
        }

        public static List<ValueNamePair> GetRMSBehaviorChoices()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)TransformRMSBehavior.Passthrough, "Passthrough"),
                new ValueNamePair((int)TransformRMSBehavior.Recalculate, "Recalculate")
            };
        }
    }
}
