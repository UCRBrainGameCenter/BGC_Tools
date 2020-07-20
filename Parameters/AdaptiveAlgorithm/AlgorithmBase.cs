using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using BGC.Scripting;

namespace BGC.Parameters.Algorithms
{
    [PropertyGroupTitle("Algorithm")]
    public abstract class AlgorithmBase : IPropertyGroup, IControlSource
    {
        protected abstract void FinishInitialization();
        public abstract bool IsDone();

        #region IControlSource

        protected readonly HashSet<ControlledParameterTemplate> controlledParameters = new HashSet<ControlledParameterTemplate>();

        public abstract int GetSourceCount();
        public abstract string GetSourcePathDisplayName(int index);
        public void RegisterControlledParameters(IEnumerable<ControlledParameterTemplate> controlledTemplates)
        {
            foreach (ControlledParameterTemplate template in controlledTemplates)
            {
                controlledParameters.Add(template);
            }

            FinishInitialization();
        }

        public virtual void CleanupLinks() => controlledParameters.Clear();
        public abstract void PopulateScriptContext(GlobalRuntimeContext scriptContext);

        public StepStatus SetStepValue(int sourceParameter, int stepNumber)
        {
            bool valid = true;
            foreach (ControlledParameterTemplate template in controlledParameters.Where(x => x.ControllerParameter == sourceParameter))
            {
                //See if any steps fail
                valid &= template.CouldStepTo(stepNumber);
            }

            if (!valid)
            {
                //Report failure to step
                return StepStatus.OutOfBounds;
            }

            StepStatus valueStatus = StepStatus.Success;
            foreach (ControlledParameterTemplate template in controlledParameters.Where(x => x.ControllerParameter == sourceParameter))
            {
                valueStatus |= template.StepTo(stepNumber);
            }

            return valueStatus;
        }

        #endregion IControlSource
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
