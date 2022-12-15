using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BGC.Parameters;

namespace BGC.AdaptiveDifficultyAlgorithm
{
    public abstract class AdaptiveDifficultyAlgorithmBase : CommonPropertyGroup, IAdaptiveDifficultyAlgorithm
    {
        public abstract int Difficulty { get; }
        public abstract bool IsDone { get; }
        public abstract int Threshold { get; }

        public abstract void Initialize();
        public abstract bool SubmitTrialResult(bool correct);
    }
}
