using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Audio.Filters;
using BGC.Mathematics;

namespace BGC.Audio.Synthesis
{
    /// <summary>
    /// An Attack-Decay-Sustain-Release envelope with a triggerable release.
    /// </summary>
    public class ADSREnvelope : SimpleBGCFilter, IADSR
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

        public override int Channels => 1;

        public override int TotalSamples => ChannelSamples;
        public override int ChannelSamples { get; }

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

        public ADSREnvelope(
            IBGCStream stream,
            double timeToPeak,
            double timeToSustain,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
            : base(stream)
        {
            Debug.Assert(stream.Channels == 1);
            Debug.Assert(timeToPeak >= 0.0);
            Debug.Assert(timeToSustain >= 0.0);
            Debug.Assert(sustainDecayTime >= 0.0);
            Debug.Assert(releaseDecayTime >= 0.0);

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

            ChannelSamples = attackDownEndSample + (int)Math.Ceiling(
                Math.Log(ENVELOPE_CUTOFF / sustainAmplitude) / Math.Log(sustainDecayRate));
        }

        public ADSREnvelope(
            IBGCStream stream,
            double timeToPeak,
            double timeToSustain,
            double timeToRelease,
            double sustainAmplitude,
            double sustainDecayTime,
            double releaseDecayTime)
            : base(stream)
        {
            Debug.Assert(stream.Channels == 1);
            Debug.Assert(timeToPeak >= 0.0);
            Debug.Assert(timeToSustain >= 0.0);
            Debug.Assert(timeToRelease >= 0.0);
            Debug.Assert(sustainDecayTime >= 0.0);
            Debug.Assert(releaseDecayTime >= 0.0);

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

            double releaseBeginAmplitude = sustainAmplitude * Math.Pow(sustainDecayRate, sustainSamples);

            if (releaseBeginAmplitude < ENVELOPE_CUTOFF)
            {
                ChannelSamples = attackDownEndSample + (int)Math.Ceiling(
                    Math.Log(ENVELOPE_CUTOFF / sustainAmplitude) / Math.Log(sustainDecayRate));
            }
            else
            {
                ChannelSamples = sustainEndSample + (int)Math.Ceiling(
                    Math.Log(ENVELOPE_CUTOFF / releaseBeginAmplitude) / Math.Log(releaseDecayRate));
            }
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

        public override int Read(float[] data, int offset, int count)
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
                    data[offset + i] *= (float)currentEnvelope;
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
            currentEnvelope = ENVELOPE_CUTOFF;
        }

        public override void Seek(int position)
        {
            position = GeneralMath.Clamp(position, 0, ChannelSamples);

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

        private IEnumerable<double> channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (channelRMS == null)
            {
                channelRMS = stream.GetChannelRMS();

                //If the rms was previously unknowable
                if (channelRMS.Any(double.IsNaN))
                {
                    channelRMS = this.CalculateRMS();
                }
            }

            return channelRMS;
        }

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
