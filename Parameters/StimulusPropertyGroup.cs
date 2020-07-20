using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using LightJson;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("", "Default")]
    public abstract class StimulusPropertyGroup : IPropertyGroup
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

        #region IPropertyGroup

        private IPropertyGroup _parentPropertyGroup;

        IPropertyGroup IPropertyGroup.GetParent() => _parentPropertyGroup;
        void IPropertyGroup.SetParent(IPropertyGroup parent)
        {
            _parentPropertyGroup = parent;
            foreach (PropertyInfo property in this.GetInitializeableFieldProperties())
            {
                this.InitializeProperty(property);
            }
        }

        JsonObject IPropertyGroup.Serialize() => this.Internal_GetSerializedData();
        void IPropertyGroup.Deserialize(JsonObject data) => this.Internal_Deserialize(data);

        #endregion IPropertyGroup
    }
}
