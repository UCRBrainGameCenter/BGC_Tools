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
        #region Constructor

        public StimulusPropertyGroup()
        {
            //Initialize fields
            foreach (PropertyInfo property in this.GetInitializeableFieldProperties())
            {
                this.InitializeProperty(property);
            }
        }

        #endregion Constructor

        public virtual void InitiatePhase(GenerationPhase phase) { }
    }
}
