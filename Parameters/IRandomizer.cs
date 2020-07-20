using System;
using System.Collections;
using System.Collections.Generic;

namespace BGC.Parameters
{
    public interface IRandomizer
    {
        void AssignRandomizer(Func<Random> randomizerGetter);
    }
}
