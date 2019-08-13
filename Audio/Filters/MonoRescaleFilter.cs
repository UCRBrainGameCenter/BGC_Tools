using System;
using System.Linq;
using System.Collections.Generic;
using BGC.Audio;
using BGC.Audio.Filters;

/// <summary>
/// Scales an underlying stream to the desired level (dB SPL)
/// </summary>
public class MonoRescaleFilter : SimpleBGCFilter
{
    public override int TotalSamples => stream.TotalSamples;
    public override int ChannelSamples => stream.ChannelSamples;

    public override int Channels => 1;

    private readonly float factor;

    public MonoRescaleFilter(
        IBGCStream stream,
        double deltaLevel)
        : base(stream)
    {
        if (stream.Channels != 1)
        {
            throw new ArgumentException("MonoRescaleFilter inner stream but have one channel.");
        }

        factor = (float)Math.Pow(10, deltaLevel/20);
    }

    public override int Read(float[] data, int offset, int count)
    {
        if (!initialized)
        {
            Initialize();
        }

        int samplesRead = stream.Read(data, offset, count);

        for (int i = 0; i < samplesRead; i++)
        {
            data[offset + i] *= factor;
        }

        return samplesRead;
    }

    private IEnumerable<double> _channelRMS = null;
    public override IEnumerable<double> GetChannelRMS()
    {
        if (_channelRMS == null)
        {
            double innerRMS = stream.GetChannelRMS().First();

            _channelRMS = new double[1] { factor * innerRMS };
        }

        return _channelRMS;
    }
}

public static class CustomClipFilters
{
    public static IBGCStream Rescale(this IBGCStream stream, float deltaLevel) =>
        new MonoRescaleFilter(stream, deltaLevel);
}
