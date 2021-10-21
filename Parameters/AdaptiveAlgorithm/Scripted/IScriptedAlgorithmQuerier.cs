using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGC.Parameters.Algorithms.Scripted
{
    public interface IScriptedAlgorithmQuerier
    {
        bool CouldStepTo(int stepNumber);
        bool CouldStepBy(int steps);
    }

    public interface IMultiParamScriptedAlgorithmQuerier
    {
        int GetParamCount();
        bool CouldStepTo(int parameter, int stepNumber);
        bool CouldStepBy(int parameter, int steps);
    }
}
