using System;
using System.Collections.Generic;
using BGC.Mathematics;

namespace BGC.Audio.AnalyticStreams
{
    public class AnalyticStreamFork : SimpleAnalyticFilter
    {
        private readonly Queue<Complex64> bufferedSamples = new Queue<Complex64>(2048);
        public readonly InternalStreamFork forkedStream;

        public override int Samples => stream.Samples;

        public int position = 0;
        public int forkPosition = 0;

        private int impendingSeek = -1;
        private int impendingForkSeek = -1;

        private const int BUFFER_SIZE = 512;
        private readonly Complex64[] buffer = new Complex64[BUFFER_SIZE];

        public AnalyticStreamFork(IAnalyticStream stream)
            : base(stream)
        {
            forkedStream = new InternalStreamFork(this);
        }

        public AnalyticStreamFork(IAnalyticStream stream, out IAnalyticStream forkedStream)
            : base(stream)
        {
            this.forkedStream = new InternalStreamFork(this);
            forkedStream = this.forkedStream;
        }

        public override int Read(Complex64[] data, int offset, int count)
        {
            ForceImpendingActions();

            int samplesToRead = count;

            if (position < forkPosition)
            {
                int stashedRead = StashedRead(data, offset, samplesToRead);

                position += stashedRead;
                offset += stashedRead;
                samplesToRead -= stashedRead;
            }

            if (samplesToRead > 0)
            {
                int streamRead = StreamRead(data, offset, samplesToRead);

                position += streamRead;
                samplesToRead -= streamRead;
            }

            return count - samplesToRead;
        }

        public override void Reset()
        {
            impendingSeek = 0;
            CheckMutualImpendingActions();
        }

        public override void Seek(int position)
        {
            impendingSeek = position;
            CheckMutualImpendingActions();
        }

        private int StashedRead(Complex64[] data, int offset, int count)
        {
            int stashRead = Math.Min(count, bufferedSamples.Count);

            for (int i = 0; i < stashRead; i++)
            {
                data[offset + i] = bufferedSamples.Dequeue();
            }

            return stashRead;
        }

        private int StreamRead(Complex64[] data, int offset, int count)
        {
            int streamRead = stream.Read(data, offset, count);

            for (int i = 0; i < streamRead; i++)
            {
                bufferedSamples.Enqueue(data[offset + i]);
            }

            return streamRead;
        }

        private int FillBuffer(int count)
        {
            int samplesToCopy = count;
            int copiedSamples;

            do
            {
                int samplesRequested = Math.Min(BUFFER_SIZE, samplesToCopy);
                copiedSamples = stream.Read(buffer, 0, samplesRequested);

                for (int i = 0; i < copiedSamples; i++)
                {
                    bufferedSamples.Enqueue(buffer[i]);
                }

                samplesToCopy -= copiedSamples;

            }
            while (copiedSamples > 0 && samplesToCopy > 0);

            return count - samplesToCopy;
        }

        private int DepleteBuffer(int count)
        {
            int depletedSamples = Math.Min(bufferedSamples.Count, count);
            for (int i = 0; i < depletedSamples; i++)
            {
                bufferedSamples.Dequeue();
            }

            return depletedSamples;
        }

        public override double GetRMS() => stream.GetRMS();

        private int ForkRead(Complex64[] data, int offset, int count)
        {
            ForceImpendingForkActions();

            int samplesToRead = count;

            if (forkPosition < position)
            {
                int stashedRead = StashedRead(data, offset, samplesToRead);

                forkPosition += stashedRead;
                offset += stashedRead;
                samplesToRead -= stashedRead;
            }

            if (samplesToRead > 0)
            {
                int streamRead = StreamRead(data, offset, samplesToRead);

                forkPosition += streamRead;
                samplesToRead -= streamRead;
            }

            return count - samplesToRead;
        }

        private void ForkReset()
        {
            impendingForkSeek = 0;
            CheckMutualImpendingActions();
        }

        private void ForkSeek(int position)
        {
            impendingForkSeek = position;
            CheckMutualImpendingActions();
        }

        private bool CheckMutualImpendingActions()
        {
            if (impendingSeek != -1 && impendingForkSeek != -1)
            {
                //Both want to do something
                int min = Math.Min(impendingSeek, impendingForkSeek);
                int max = Math.Max(impendingSeek, impendingForkSeek);

                if (min == 0)
                {
                    //At least one wants a reset...
                    stream.Reset();
                }
                else
                {
                    stream.Seek(min);
                }

                bufferedSamples.Clear();

                if (min != max)
                {
                    FillBuffer(max - min);
                }

                position = impendingSeek;
                forkPosition = impendingForkSeek;

                impendingSeek = -1;
                impendingForkSeek = -1;

                return true;
            }

            return false;
        }

        private void ForceImpendingActions()
        {
            //If this stream doesn't want to seek, we don't have to do anything
            if (impendingSeek == -1)
            {
                return;
            }

            //Skip seeking to where we already are
            if (impendingSeek == position)
            {
                impendingSeek = -1;
                return;
            }

            //If Both streams want to seek, seek to earlier and then read-in for later
            if (CheckMutualImpendingActions())
            {
                return;
            }

            if (impendingSeek > position)
            {
                //
                //Seeking Forward
                //

                //If Only this stream wants to seek ahead, deplete and then read into buffer...
                if (impendingSeek == forkPosition)
                {
                    //Seeking forward to the forked position
                    bufferedSamples.Clear();
                }
                else if (impendingSeek > forkPosition)
                {
                    //Seeking forward past the fork position

                    if (position < forkPosition)
                    {
                        //Was originally before it
                        //Clear the old samples and add the difference
                        bufferedSamples.Clear();
                        FillBuffer(impendingSeek - forkPosition);
                    }
                    else
                    {
                        //Was originally tied or past it
                        //Just add more samples
                        FillBuffer(impendingSeek - position);
                    }
                }
                else //impendingSeek < forkPosition
                {
                    //Seeking forward, but still behind the fork position
                    DepleteBuffer(impendingSeek - position);
                }


                position = impendingSeek;
                impendingSeek = -1;

            }
            else
            {
                //
                //Seeking Backwards
                //

                //If Only this stream wants to seek behind, seek and update impending forkseek

                if (impendingSeek == 0)
                {
                    stream.Reset();
                }
                else
                {
                    stream.Seek(impendingSeek);
                }

                impendingForkSeek = forkPosition;
                position = impendingSeek;
                forkPosition = impendingSeek;
                impendingSeek = -1;
            }
        }

        private void ForceImpendingForkActions()
        {
            //If this stream doesn't want to seek, we don't have to do anything
            if (impendingForkSeek == -1)
            {
                return;
            }

            //Skip seeking to where we already are
            if (impendingForkSeek == forkPosition)
            {
                impendingForkSeek = -1;
                return;
            }

            //If Both streams want to seek, seek to earlier and then read-in for later
            if (CheckMutualImpendingActions())
            {
                return;
            }

            if (impendingForkSeek > forkPosition)
            {
                //
                //Seeking Forward
                //

                //If Only this stream wants to seek ahead, deplete and then read into buffer...
                if (impendingForkSeek == position)
                {
                    //Seeking forward to the stream position
                    bufferedSamples.Clear();
                }
                else if (impendingForkSeek > position)
                {
                    //Seeking forward past the stream position

                    if (forkPosition < position)
                    {
                        //Was originally before it
                        //Clear the old samples and add the difference
                        bufferedSamples.Clear();
                        FillBuffer(impendingForkSeek - position);
                    }
                    else
                    {
                        //Was originally tied or past it
                        //Just add more samples
                        FillBuffer(impendingForkSeek - forkPosition);
                    }
                }
                else //impendingForkSeek < position
                {
                    //Seeking forward, but still behind the fork position
                    DepleteBuffer(impendingForkSeek - forkPosition);
                }


                forkPosition = impendingForkSeek;
                impendingForkSeek = -1;

            }
            else
            {
                //
                //Seeking Backwards
                //

                //If Only this stream wants to seek behind, seek and update impendingSeek

                if (impendingForkSeek == 0)
                {
                    stream.Reset();
                }
                else
                {
                    stream.Seek(impendingForkSeek);
                }

                impendingSeek = position;
                position = impendingForkSeek;
                forkPosition = impendingForkSeek;
                impendingForkSeek = -1;
            }
        }

        public class InternalStreamFork : SimpleAnalyticFilter
        {
            public override int Samples => stream.Samples;

            private readonly AnalyticStreamFork parent;

            public InternalStreamFork(AnalyticStreamFork parent)
                : base(parent.stream)
            {
                this.parent = parent;
            }

            public override double GetRMS() => stream.GetRMS();

            public override int Read(Complex64[] data, int offset, int count) =>
                parent.ForkRead(data, offset, count);

            public override void Reset() => parent.ForkReset();

            public override void Seek(int position) => parent.ForkSeek(position);
        }
    }
}
