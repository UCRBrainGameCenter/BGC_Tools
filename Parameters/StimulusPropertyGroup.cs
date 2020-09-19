using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LightJson;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("", "Default")]
    public abstract class StimulusPropertyGroup : CommonPropertyGroup
    {
        public virtual void InitiatePhase(GenerationPhase phase) { }
    }
}
