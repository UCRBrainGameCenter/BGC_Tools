namespace BGC.Audio
{
    public abstract class BGCAudioClip : BGCStream
    {
        public float Duration => ChannelSamples / SamplingRate;

        public override float SamplingRate => 44100f;
    }
}
