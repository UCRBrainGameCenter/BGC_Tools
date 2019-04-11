namespace BGC.Audio.Extensions
{
    public static class AudioExtensions
    {
        public static AudioChannel Flip(this AudioChannel channel)
        {
            switch (channel)
            {
                case AudioChannel.Left: return AudioChannel.Right;
                case AudioChannel.Right: return AudioChannel.Left;
                case AudioChannel.Both: return AudioChannel.Both;

                default:
                    UnityEngine.Debug.LogError($"Unrecognized AudioChannel: {channel}");
                    return AudioChannel.Both;
            }
        }
    }
}
