using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using LightJson;
using BGC.Scripting;

namespace BGC.Parameters.Algorithms
{
    public interface IResponseCollectionAlgorithm : IAlgorithm, IPropertyGroup
    {
        void Initialize();
        void SubmitTrialResult(int step);

        double GetOutputStepValue();
    }
}
