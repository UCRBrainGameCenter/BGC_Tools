using System;
using System.Collections.Generic;
using System.Reflection;
using LightJson;

namespace BGC.Parameters
{
    public abstract class CommonPropertyGroup : IPropertyGroup
    {
        public CommonPropertyGroup()
        {
            //InitializeProperties
            foreach (PropertyInfo property in this.GetInitializeableFieldProperties())
            {
                this.InitializeProperty(property);
            }
        }

        #region IPropertyGroup

        private IPropertyGroup _parentPropertyGroup;

        public virtual int __VERSION__ => 1; 
        
        IPropertyGroup IPropertyGroup.GetParent() => _parentPropertyGroup;
        void IPropertyGroup.SetParent(IPropertyGroup parent) => _parentPropertyGroup = parent;

        public virtual JsonObject Serialize(bool includeState = false) => this.Internal_GetSerializedData(includeState);
        public virtual void Deserialize(JsonObject data) => this.Internal_Deserialize(data);

        public virtual bool TryUpgradeVersion(JsonObject serializedData)
        {
            // By default, the base class has no upgrades. This would be implemented on a per-child basis, if needed.
            return true;
        }

        #endregion IPropertyGroup
    }
}
