using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Mathematics;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// A Decorator class for BGCAudioClips that applies the a SinglePass Phase reencoding for
    /// introducing an effective ITD
    /// 
    /// This class is a bit wordy in its implementation, but I really wanted to make the Initialize
    /// method have certain work-skipping optimizations
    /// </summary>
    public class SinglePassPhaseReencoder : SimpleBGCFilter
    {
        public override int Channels { get; }

        public override int TotalSamples => Channels * ChannelSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public override float SamplingRate => stream.SamplingRate;

        private int RemainingSamples => ChannelSamples - Position;

        public int Position { get; private set; } = 0;
        public float[] Samples { get; private set; } = null;

        private readonly double leftTimeShift;
        private readonly double rightTimeShift;

        public SinglePassPhaseReencoder(
            IBGCStream stream,
            double timeShift)
            : base(stream)
        {
            if (stream.ChannelSamples == int.MaxValue)
            {
                throw new StreamCompositionException("Cannot Single-Pass Phase Reencode an infinite stream. " +
                    "Truncate first or use a Frame-based Reencoder.");
            }

            if (stream.Channels != 1)
            {
                throw new StreamCompositionException("Cannot Single-Pass Phase Reencode a stereo stream with " +
                    "only one argument. Use two arguments or Split first.");
            }

            Channels = 1;

            leftTimeShift = timeShift;
            rightTimeShift = 0.0;

            Samples = new float[TotalSamples];
        }

        public SinglePassPhaseReencoder(
            IBGCStream stream,
            double leftTimeShift,
            double rightTimeShift)
            : base(stream)
        {
            if (stream.ChannelSamples == int.MaxValue)
            {
                throw new StreamCompositionException("Cannot Single-Pass Phase Reencode an infinite stream. " +
                    "Truncate first or use a Frame-based Reencoder.");
            }

            if (stream.Channels > 2)
            {
                throw new StreamCompositionException("Cannot Single-Pass Phase Reencode a stream with more than " +
                    "2 channels.");
            }

            Channels = 2;

            this.leftTimeShift = leftTimeShift;
            this.rightTimeShift = rightTimeShift;

            Samples = new float[TotalSamples];
        }

        protected override void _Initialize()
        {
            if (Channels == 1)
            {
                Complex64[] samples = stream.ComplexSamples();
                int length = samples.Length;

                TimeShift(samples, leftTimeShift);

                for (int i = 0; i < length; i++)
                {
                    Samples[i] = (float)samples[i].Real;
                }
            }
            else if (stream.Channels == 2)
            {
                //Two Channels to Two Channels
                
                //Left Channel
                Complex64[] samples = stream.ComplexSamples(channelIndex: 0);
                int length = samples.Length;

                if (leftTimeShift != 0.0)
                {
                    //Skip this step if we're not timeshifting the left channel
                    TimeShift(samples, leftTimeShift);
                }

                for (int i = 0; i < length; i++)
                {
                    Samples[2 * i] = (float)samples[i].Real;
                }

                //Right Channel
                samples = stream.ComplexSamples(channelIndex: 1);

                if (rightTimeShift != 0.0)
                {
                    //Skip this step if we're not timeshifting the right channel
                    TimeShift(samples, rightTimeShift);
                }

                for (int i = 0; i < length; i++)
                {
                    Samples[2 * i + 1] = (float)samples[i].Real;
                }
            }
            else
            {
                //One Channel to Two Channels
                Complex64[] samples = stream.ComplexSamples();
                int length = samples.Length;

                //Copy samples before we destroy them
                //Left shiftless
                if (leftTimeShift == 0.0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        Samples[2 * i] = (float)samples[i].Real;
                    }
                }

                //Right shiftless
                if (rightTimeShift == 0.0)
                {
                    for (int i = 0; i < length; i++)
                    {
                        Samples[2 * i + 1] = (float)samples[i].Real;
                    }
                }

                //Destructive actions start here
                //Left shifting
                if (leftTimeShift != 0.0)
                {
                    TimeShift(samples, leftTimeShift);

                    for (int i = 0; i < length; i++)
                    {
                        Samples[2 * i] = (float)samples[i].Real;
                    }
                }

                //Right shifting
                if (rightTimeShift != 0.0)
                {
                    if (leftTimeShift != 0.0)
                    {
                        //Reacquire samples that were destroyed
                        samples = stream.ComplexSamples();
                    }

                    TimeShift(samples, rightTimeShift);

                    for (int i = 0; i < length; i++)
                    {
                        Samples[2 * i + 1] = (float)samples[i].Real;
                    }
                }
            }
        }

        private void TimeShift(Complex64[] samples, double timeShift)
        {
            int length = samples.Length;

            Fourier.Forward(samples);
            double offset = 2 * Math.PI * stream.SamplingRate * timeShift / length;

            for (int i = 1; i < (length / 2) - 1; i++)
            {
                //Double and rotate
                samples[i] *= Complex64.FromPolarCoordinates(2.0, -i * offset);
            }

            //Clear negative frequencies
            Array.Clear(
                array: samples,
                index: length / 2 + 1,
                length: length / 2 - 1);

            Fourier.Inverse(samples);
        }

        public override int Read(float[] data, int offset, int count)
        {
            if (!initialized)
            {
                Initialize();
            }

            int samplesToCopy = Math.Min(count, Channels * RemainingSamples);

            Array.Copy(
                sourceArray: Samples,
                sourceIndex: Channels * Position,
                destinationArray: data,
                destinationIndex: offset,
                length: samplesToCopy);

            Position += samplesToCopy / Channels;

            return samplesToCopy;
        }

        public override void Reset() => Position = 0;

        public override void Seek(int position) => 
            Position = GeneralMath.Clamp(position, 0, ChannelSamples);

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS()
        {
            if (_channelRMS == null)
            {
                if (Channels == stream.Channels)
                {
                    //1->1 or 2->2
                    _channelRMS = stream.GetChannelRMS();
                }
                else
                {
                    //Only 1->2
                    double rms = stream.GetChannelRMS().First();
                    _channelRMS = new double[] { rms, rms };
                }
            }

            return _channelRMS;
        }
    }

}
