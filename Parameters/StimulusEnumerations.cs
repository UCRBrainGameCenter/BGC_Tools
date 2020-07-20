using System;
using System.Collections;
using System.Collections.Generic;
using BGC.Audio;
using BGC.Mathematics;

namespace BGC.Parameters
{
    public enum GenerationPhase
    {
        Instance = 0,
        Interval,
        Trial,
        Task,
        MAX
    }

    public enum AmplitudeDistribution
    {
        Unitary = 0,
        FlatRandom,
        Rayleigh,
        MAX
    }

    public enum AmplitudeFactor
    {
        Violet = 0,
        Blue,
        White,
        Pink,
        Brown,
        MAX
    }

    public enum FrequencyDistribution
    {
        Exponential = 0,
        Linear,
        MAX
    }

    public static class StimEnumExtensions
    {
        public static string ToDisplayName(this GenerationPhase phase)
        {
            switch (phase)
            {
                case GenerationPhase.Instance: return "Instance";
                case GenerationPhase.Interval: return "Interval";
                case GenerationPhase.Trial: return "Trial";
                case GenerationPhase.Task: return "Task";

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {phase}");
                    return "";
            }
        }

        public static string ToAltDisplayName(this GenerationPhase phase)
        {
            switch (phase)
            {
                case GenerationPhase.Instance: return "Instance";
                case GenerationPhase.Interval: return "Interval";
                case GenerationPhase.Trial: return "Block";
                case GenerationPhase.Task: return "Task";

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {phase}");
                    return "";
            }
        }

        public static string ToSerializationString(this GenerationPhase phase)
        {
            switch (phase)
            {
                case GenerationPhase.Instance: return "Instance";
                case GenerationPhase.Interval: return "Interval";
                case GenerationPhase.Trial: return "Trial";
                case GenerationPhase.Task: return "Task";

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {phase}");
                    return "";
            }
        }

        public static string ToDisplayName(this AmplitudeDistribution distribution)
        {
            switch (distribution)
            {
                case AmplitudeDistribution.Unitary: return "Unitary";
                case AmplitudeDistribution.FlatRandom: return "Random (Flat)";
                case AmplitudeDistribution.Rayleigh: return "Random (Rayleigh)";

                default:
                    UnityEngine.Debug.LogError($"Unexpected AmplitudeDistribution: {distribution}");
                    return "";
            }
        }

        public static string ToDisplayName(this AmplitudeFactor factor)
        {
            switch (factor)
            {
                case AmplitudeFactor.Violet: return "Frequency (+6 dB/Octave)";
                case AmplitudeFactor.Blue: return "Root Frequency (+3 dB/Octave)";
                case AmplitudeFactor.White: return "Unitary (0 dB/Octave)";
                case AmplitudeFactor.Pink: return "Inverse Root Frequency (-3 dB/Octave)";
                case AmplitudeFactor.Brown: return "Inverse Frequency (-6 dB/Octave)";

                default:
                    UnityEngine.Debug.LogError($"Unexpected AmplitudeFactor: {factor}");
                    return "";
            }
        }

        public static string ToDisplayName(this FrequencyDistribution distribution)
        {
            switch (distribution)
            {
                case FrequencyDistribution.Exponential: return "Exponential (-3 dB/Octave)";
                case FrequencyDistribution.Linear: return "Linear (White)";

                default:
                    UnityEngine.Debug.LogError($"Unexpected FrequencyDistribution: {distribution}");
                    return "";
            }
        }

        public static double GetFactor(this AmplitudeFactor amplitudeFactor, double frequency)
        {
            switch (amplitudeFactor)
            {
                case AmplitudeFactor.Violet: return frequency;
                case AmplitudeFactor.Blue: return Math.Sqrt(frequency);
                case AmplitudeFactor.White: return 1.0;
                case AmplitudeFactor.Pink: return 1.0 / Math.Sqrt(frequency);
                case AmplitudeFactor.Brown: return 1.0 / frequency;

                default:
                    UnityEngine.Debug.LogError($"Unexpected AmplitudeFactor: {amplitudeFactor}");
                    return 1.0;
            }
        }

        public static double GetFactor(this AmplitudeDistribution amplitudeDistribution, Random randomizer)
        {
            switch (amplitudeDistribution)
            {
                case AmplitudeDistribution.Unitary: return 1.0;
                case AmplitudeDistribution.FlatRandom: return randomizer.NextDouble();
                case AmplitudeDistribution.Rayleigh: return CustomRandom.RayleighDistribution(randomizer.NextDouble());

                default:
                    UnityEngine.Debug.LogError($"Unexpected AmplitudeDistribution: {amplitudeDistribution}");
                    return 1.0;
            }
        }

        public static string ToDisplayName(this Windowing.Function function)
        {
            switch (function)
            {
                case Windowing.Function.Hamming: return "Hamming";
                case Windowing.Function.Hann: return "Hann";
                case Windowing.Function.BlackmanHarris: return "Blackman-Harris";
                case Windowing.Function.Sine: return "Sine";
                case Windowing.Function.Linear: return "Triangular";
                case Windowing.Function.Square: return "Square";
                case Windowing.Function.Silence: return "Silence";

                default:
                    UnityEngine.Debug.LogError($"Unexpected Windowing.Function: {function}");
                    return "";
            }
        }

        public static string ToDisplayName(this PlotType plotType)
        {
            switch (plotType)
            {
                case PlotType.LimitedSpectrogram: return "Spectrogram (Limited)";
                case PlotType.Spectrogram: return "Spectrogram (Full)";
                case PlotType.Waveform: return "Waveform";
                case PlotType.Envelope: return "Envelope";
                case PlotType.PowerSpectralDensity: return "Power Spectrum";

                default:
                    UnityEngine.Debug.LogError($"Unexpected PlotType: {plotType}");
                    return "";
            }
        }

        public static string ToSimplePresentationName(this AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Left: return "Left";
                case AudioChannel.Right: return "Right";
                case AudioChannel.Both: return "Both";

                default:
                    UnityEngine.Debug.LogError($"Unexpected AudioChannel: {channel}");
                    return "";
            }
        }

        public static string ToPresentationName(this AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Left: return "Monaural Left";
                case AudioChannel.Right: return "Monaural Right";
                case AudioChannel.Both: return "Diotic";

                default:
                    UnityEngine.Debug.LogError($"Unexpected AudioChannel: {channel}");
                    return "";
            }
        }
    }
}
