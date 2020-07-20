using System;
using System.Linq;
using BGC.Scripting;

namespace BGC.Parameters.Algorithms
{
    public interface IBinaryOutcomeAlgorithm : IAlgorithm, IPropertyGroup
    {
        void Initialize(double taskGuessRate);
        void SubmitTrialResult(bool correct);
    }
}
