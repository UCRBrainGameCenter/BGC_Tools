using BGC.Parameters;

namespace BGC.AdaptiveDifficultyAlgorithm
{
    [PropertyGroupTitle("Adaptive Difficulty Algorithm")]
    public interface IAdaptiveDifficultyAlgorithm : IPropertyGroup
    {
        int Difficulty { get; }
        bool IsDone { get; }
        int Threshold { get; }

        void Initialize();
        bool SubmitTrialResult(bool correct);
    }
}
