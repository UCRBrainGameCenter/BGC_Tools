using System;
using System.Collections.Generic;
using UnityEngine;
using BGC.Mathematics;
using BGC.Audio.Synthesis;

namespace BGC.Audio.AnalyticStreams
{
    public class AnalyticADSREnvelope : SimpleAnalyticFilter, IADSR
    {
        private const double ENVELOPE_CUTOFF = 1E-6;
        //10 ms immediate decay
        private const double IMMEDIATE_DECAY_TIME = 0.01;

        private enum EnvelopeState
        {
            AttackUp = 0,
            AttackDown,
            Sustain,
            Released,
            ImmediateRelease,
            MAX
        }

        public override int Samples => stream.Samples;

        private readonly double sustainAmplitude;

        private readonly double sustainDecaySamples;
        private readonly double releaseDecaySamples;
        private readonly int attackUpSamples;
        private readonly int attackDownSamples;

        private readonly int attackUpEndSample;
        private readonly int attackDownEndSample;
        private readonly int sustainEndSample;

        private readonly double attackUpGrowthRate;
        private readonly double attackDownDecayRate;
        private readonly double sustainDecayRate;
        private readonly double releaseDecayRate;
        private readonly double immediateDecayRate;
        private double currentEnvelope;

        private EnvelopeState envelopeState = EnvelopeState.AttackUp;

        private int position = 0;

        public AnalyticADSREnvelope(
            IAnalyticStream stream,
            double timeToPeak,
            double timeToSustain,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
            : base(stream)
        {
            this.sustainAmplitude = sustainAmplitude;

            sustainDecaySamples = (int)(SamplingRate * sustainDecayTime);
            releaseDecaySamples = (int)(SamplingRate * releaseDecayTime);

            attackUpSamples = (int)(SamplingRate * timeToPeak);
            attackUpEndSample = attackUpSamples;

            attackDownSamples = (int)(SamplingRate * timeToSustain);
            attackDownEndSample = attackUpEndSample + attackDownSamples;
            sustainEndSample = int.MaxValue;

            attackUpGrowthRate = Math.Pow(1.0 / ENVELOPE_CUTOFF, 1.0 / attackUpSamples);
            attackDownDecayRate = Math.Pow(sustainAmplitude, 1.0 / attackDownSamples);
            sustainDecayRate = Math.Exp(-1.0 / sustainDecaySamples);
            releaseDecayRate = Math.Exp(-1.0 / releaseDecaySamples);

            immediateDecayRate = Math.Pow(ENVELOPE_CUTOFF, 1 / (SamplingRate * 0.01));
            //Start at a small envelope for exponential growth
            currentEnvelope = ENVELOPE_CUTOFF;

            envelopeState = EnvelopeState.AttackUp;
        }

        public AnalyticADSREnvelope(
            IAnalyticStream stream,
            double timeToPeak,
            double timeToSustain,
            double timeToRelease,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
            : base(stream)
        {
            this.sustainAmplitude = sustainAmplitude;

            sustainDecaySamples = (int)(SamplingRate * sustainDecayTime);
            releaseDecaySamples = (int)(SamplingRate * releaseDecayTime);

            attackUpSamples = (int)(SamplingRate * timeToPeak);
            attackUpEndSample = attackUpSamples;

            attackDownSamples = (int)(SamplingRate * timeToSustain);
            attackDownEndSample = attackUpEndSample + attackDownSamples;

            int sustainSamples = (int)(SamplingRate * timeToRelease);
            sustainEndSample = attackDownEndSample + sustainSamples;

            attackUpGrowthRate = Math.Pow(1.0 / ENVELOPE_CUTOFF, 1.0 / attackUpSamples);
            attackDownDecayRate = Math.Pow(sustainAmplitude, 1.0 / attackDownSamples);
            sustainDecayRate = Math.Exp(-1.0 / sustainDecaySamples);
            releaseDecayRate = Math.Exp(-1.0 / releaseDecaySamples);

            immediateDecayRate = Math.Pow(ENVELOPE_CUTOFF, 1 / (SamplingRate * 0.01));
            //Start at a small envelope for exponential growth
            currentEnvelope = ENVELOPE_CUTOFF;

            envelopeState = EnvelopeState.AttackUp;
        }

        public void TriggerRelease(bool immediate = false)
        {
            if (immediate)
            {
                envelopeState = EnvelopeState.ImmediateRelease;
            }
            else
            {
                if (envelopeState != EnvelopeState.ImmediateRelease)
                {
                    envelopeState = EnvelopeState.Released;
                }
            }
        }

        public override int Read(Complex64[] data, int offset, int count)
        {
            int samplesRemaining = count;

            while (samplesRemaining > 0)
            {
                if (currentEnvelope < ENVELOPE_CUTOFF)
                {
                    break;
                }

                int samplesToRead = Math.Min(StateEndSample(envelopeState) - position, samplesRemaining);
                int samplesRead = stream.Read(data, offset, samplesToRead);

                if (samplesRead == 0)
                {
                    break;
                }

                for (int i = 0; i < samplesRead; i++)
                {
                    currentEnvelope *= EnvelopeRate(envelopeState);
                    data[offset + i] *= currentEnvelope;
                }

                position += samplesRead;
                offset += samplesRead;
                samplesRemaining -= samplesRead;

                if (position == StateEndSample(envelopeState))
                {
                    //Advance EnvelopeState
                    envelopeState++;
                }
            }

            return count - samplesRemaining;
        }

        public override void Reset()
        {
            stream.Reset();
            position = 0;
        }

        public override void Seek(int position)
        {
            position = GeneralMath.Clamp(position, 0, Samples);

            stream.Seek(position);
            this.position = position;

            if (position < attackUpEndSample)
            {
                envelopeState = EnvelopeState.AttackUp;
                currentEnvelope = ENVELOPE_CUTOFF * Math.Pow(1.0 / ENVELOPE_CUTOFF, position / (double)attackUpSamples);
            }
            else if (position < attackDownEndSample)
            {
                envelopeState = EnvelopeState.AttackDown;
                currentEnvelope = Math.Pow(sustainAmplitude, (position - attackUpEndSample) / (double)attackDownSamples);
            }
            else if (position < sustainEndSample)
            {
                envelopeState = EnvelopeState.Sustain;
                currentEnvelope = sustainAmplitude * Math.Pow(sustainDecayRate, (position - attackDownEndSample));
            }
            else
            {
                envelopeState = EnvelopeState.Released;
                currentEnvelope = sustainAmplitude *
                    Math.Pow(sustainDecayRate, sustainEndSample - attackDownEndSample) *
                    Math.Pow(releaseDecayRate, position - sustainEndSample);
            }

        }

        public override double GetRMS() => stream.GetRMS();

        private double EnvelopeRate(EnvelopeState state)
        {
            switch (state)
            {
                case EnvelopeState.AttackUp: return attackUpGrowthRate;
                case EnvelopeState.AttackDown: return attackDownDecayRate;
                case EnvelopeState.Sustain: return sustainDecayRate;
                case EnvelopeState.Released: return releaseDecayRate;
                case EnvelopeState.ImmediateRelease: return immediateDecayRate;

                default:
                    Debug.LogError($"Unexpected EnvelopeState: {state}");
                    goto case EnvelopeState.ImmediateRelease;
            }
        }

        private int StateEndSample(EnvelopeState state)
        {
            switch (state)
            {
                case EnvelopeState.AttackUp: return attackUpEndSample;
                case EnvelopeState.AttackDown: return attackDownEndSample;
                case EnvelopeState.Sustain: return sustainEndSample;
                case EnvelopeState.Released: return int.MaxValue;
                case EnvelopeState.ImmediateRelease: return int.MaxValue;

                default:
                    Debug.LogError($"Unexpected EnvelopeState: {state}");
                    goto case EnvelopeState.ImmediateRelease;
            }
        }
    }
}
