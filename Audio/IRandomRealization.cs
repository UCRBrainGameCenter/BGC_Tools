namespace BGC.Audio
{
    /// <summary>
    /// A stream whose samples are one realization of a random process, drawn as it is read
    /// (ContinuousWhiteNoiseStream, NoiseVocoder). Reset and Seek return to that realization's
    /// samples, as for every other stream, so a stream measured and then played plays what was
    /// measured. A host that wants fresh noise on a loop or a replay asks for it explicitly with
    /// <see cref="BGCStreamExtensions.NextRealization"/>; StreamRepeater does at every pass.
    /// </summary>
    public interface IRandomRealization
    {
        /// <summary> Begins a new realization, independent of the last, and rewinds to its start </summary>
        void NextRealization();
    }
}
