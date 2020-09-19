using System;
using System.Collections.Generic;
using System.Reflection;
using LightJson;

namespace BGC.Parameters
{
    public abstract class CommonPropertyGroup : IPropertyGroup
    {
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

        public JsonObject Serialize() => this.Internal_GetSerializedData();
        public void Deserialize(JsonObject data) => this.Internal_Deserialize(data);

        #endregion IPropertyGroup
    }
}
