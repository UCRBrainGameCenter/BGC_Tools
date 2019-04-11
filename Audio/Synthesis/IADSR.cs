namespace BGC.Audio.Synthesis
{
    public interface IADSR
    {
        void TriggerRelease(bool immediate = false);
    }
}
