using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BGC.Audio.Filters
{
    /// <summary>
    /// Splits a 2 channel stream into separate streams for each channel
    /// </summary>
    public class StreamChannelSplitter : SimpleBGCFilter
    {
        private readonly Queue<float> bufferedSamples = new Queue<float>(2048);
        public readonly InternalStreamSplit splitStream;

        public override int Channels => 1;
        public override int TotalSamples => stream.ChannelSamples;
        public override int ChannelSamples => stream.ChannelSamples;

        public int position = 0;
        public int splitPosition = 0;

        private int impendingSeek = -1;
        private int impendingSplitSeek = -1;

        private const int BUFFER_SIZE = 512;
        private float[] buffer = new float[BUFFER_SIZE];

        public StreamChannelSplitter(IBGCStream stream)
            : base(stream)
        {
            Debug.Assert(stream.Channels == 2);
            splitStream = new InternalStreamSplit(this);
        }

        public StreamChannelSplitter(IBGCStream stream, out IBGCStream splitStream)
            : base(stream)
        {
            Debug.Assert(stream.Channels == 2);
            this.splitStream = new InternalStreamSplit(this);
            splitStream = this.splitStream;
        }

        public override int Read(float[] data, int offset, int count)
        {
            ForceImpendingActions();

            int samplesToRead = count;

            if (position < splitPosition)
            {
                int stashedRead = StashedRead(data, offset, samplesToRead);

                position += stashedRead;
                offset += stashedRead;
                samplesToRead -= stashedRead;
            }

            if (samplesToRead > 0)
            {
                int streamRead = StreamRead(data, offset, samplesToRead, true);

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

        private int StashedRead(float[] data, int offset, int count)
        {
            int stashRead = Math.Min(count, bufferedSamples.Count);

            for (int i = 0; i < stashRead; i++)
            {
                data[offset + i] = bufferedSamples.Dequeue();
            }

            return stashRead;
        }

        private int StreamRead(float[] data, int offset, int count, bool left)
        {
            int remainingSamples = 2 * count;

            int outputIndex = left ? 0 : 1;
            int cacheIndex = left ? 1 : 0;

            while (remainingSamples > 0)
            {
                int samplesToRead = Math.Min(remainingSamples, BUFFER_SIZE);

                int streamRead = stream.Read(buffer, 0, samplesToRead);

                if (streamRead == 0)
                {
                    //Out of samples
                    break;
                }

                for (int i = 0; i < streamRead / 2; i++)
                {
                    data[offset + i] = buffer[2 * i + outputIndex];
                    bufferedSamples.Enqueue(buffer[2 * i + cacheIndex]);
                }

                remainingSamples -= streamRead;
                offset += streamRead / 2;
            }

            return count - remainingSamples / 2;
        }

        private int FillBuffer(int count, bool left)
        {
            int samplesToCopy = 2 * count;

            int cacheIndex = left ? 0 : 1;

            while (samplesToCopy > 0)
            {
                int samplesRequested = Math.Min(BUFFER_SIZE, samplesToCopy);
                int copiedSamples = stream.Read(buffer, 0, samplesRequested);

                for (int i = 0; i < copiedSamples / 2; i++)
                {
                    bufferedSamples.Enqueue(buffer[2 * i + cacheIndex]);
                }

                samplesToCopy -= copiedSamples;
            }

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

        private IEnumerable<double> _channelRMS = null;
        public override IEnumerable<double> GetChannelRMS() =>
            _channelRMS ?? (_channelRMS = stream.GetChannelRMS().Take(1));

        private int SplitRead(float[] data, int offset, int count)
        {
            ForceImpendingSplitActions();

            int samplesToRead = count;

            if (splitPosition < position)
            {
                int stashedRead = StashedRead(data, offset, samplesToRead);

                splitPosition += stashedRead;
                offset += stashedRead;
                samplesToRead -= stashedRead;
            }

            if (samplesToRead > 0)
            {
                int streamRead = StreamRead(data, offset, samplesToRead, false);

                splitPosition += streamRead;
                samplesToRead -= streamRead;
            }

            return count - samplesToRead;
        }

        private void SplitReset()
        {
            impendingSplitSeek = 0;
            CheckMutualImpendingActions();
        }

        private void SplitSeek(int position)
        {
            impendingSplitSeek = position;
            CheckMutualImpendingActions();
        }

        private bool CheckMutualImpendingActions()
        {
            if (impendingSeek != -1 && impendingSplitSeek != -1)
            {
                //Both want to do something
                int min = Math.Min(impendingSeek, impendingSplitSeek);
                int max = Math.Max(impendingSeek, impendingSplitSeek);

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
                    FillBuffer(max - min, impendingSeek < impendingSplitSeek);
                }

                position = impendingSeek;
                splitPosition = impendingSplitSeek;

                impendingSeek = -1;
                impendingSplitSeek = -1;

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
                if (impendingSeek == splitPosition)
                {
                    //Seeking forward to the split position
                    bufferedSamples.Clear();
                }
                else if (impendingSeek > splitPosition)
                {
                    //Seeking forward past the split position

                    if (position < splitPosition)
                    {
                        //Was originally before it
                        //Clear the old samples and add the difference
                        bufferedSamples.Clear();
                        FillBuffer(impendingSeek - splitPosition, false);
                    }
                    else
                    {
                        //Was originally tied or past it
                        //Just add more samples
                        FillBuffer(impendingSeek - position, false);
                    }
                }
                else //impendingSeek < splitPosition
                {
                    //Seeking forward, but still behind the split position
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

                //If Only this stream wants to seek behind, seek and update impending splitseek

                if (impendingSeek == 0)
                {
                    stream.Reset();
                }
                else
                {
                    stream.Seek(impendingSeek);
                }

                impendingSplitSeek = splitPosition;
                position = impendingSeek;
                splitPosition = impendingSeek;
                impendingSeek = -1;
            }
        }

        private void ForceImpendingSplitActions()
        {
            //If this stream doesn't want to seek, we don't have to do anything
            if (impendingSplitSeek == -1)
            {
                return;
            }

            //Skip seeking to where we already are
            if (impendingSplitSeek == splitPosition)
            {
                impendingSplitSeek = -1;
                return;
            }

            //If Both streams want to seek, seek to earlier and then read-in for later
            if (CheckMutualImpendingActions())
            {
                return;
            }

            if (impendingSplitSeek > splitPosition)
            {
                //
                //Seeking Forward
                //

                //If Only this stream wants to seek ahead, deplete and then read into buffer...
                if (impendingSplitSeek == position)
                {
                    //Seeking forward to the stream position
                    bufferedSamples.Clear();
                }
                else if (impendingSplitSeek > position)
                {
                    //Seeking forward past the stream position

                    if (splitPosition < position)
                    {
                        //Was originally before it
                        //Clear the old samples and add the difference
                        bufferedSamples.Clear();
                        FillBuffer(impendingSplitSeek - position, true);
                    }
                    else
                    {
                        //Was originally tied or past it
                        //Just add more samples
                        FillBuffer(impendingSplitSeek - splitPosition, true);
                    }
                }
                else //impendingSplitSeek < position
                {
                    //Seeking forward, but still behind the split position
                    DepleteBuffer(impendingSplitSeek - splitPosition);
                }

                splitPosition = impendingSplitSeek;
                impendingSplitSeek = -1;

            }
            else
            {
                //
                //Seeking Backwards
                //

                //If Only this stream wants to seek behind, seek and update impendingSeek

                if (impendingSplitSeek == 0)
                {
                    stream.Reset();
                }
                else
                {
                    stream.Seek(impendingSplitSeek);
                }

                impendingSeek = position;
                position = impendingSplitSeek;
                splitPosition = impendingSplitSeek;
                impendingSplitSeek = -1;
            }
        }

        public class InternalStreamSplit : SimpleBGCFilter
        {
            public override int Channels => 1;
            public override int TotalSamples => stream.ChannelSamples;
            public override int ChannelSamples => stream.ChannelSamples;

            private readonly StreamChannelSplitter parent;

            public InternalStreamSplit(StreamChannelSplitter parent)
                : base(parent.stream)
            {
                this.parent = parent;
            }

            private IEnumerable<double> _channelRMS = null;
            public override IEnumerable<double> GetChannelRMS() =>
                _channelRMS ?? (_channelRMS = stream.GetChannelRMS().Skip(1).Take(1));

            public override int Read(float[] data, int offset, int count) =>
                parent.SplitRead(data, offset, count);

            public override void Reset() => parent.SplitReset();

            public override void Seek(int position) => parent.SplitSeek(position);
        }
    }
}
