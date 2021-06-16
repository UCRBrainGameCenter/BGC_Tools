using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using BGC.Scripting;

namespace BGC.Parameters.Algorithms
{
    [PropertyGroupTitle("Algorithm")]
    public abstract class AlgorithmBase : CommonPropertyGroup, IControlSource, IAlgorithm
    {
        protected abstract void FinishInitialization();
        public abstract bool IsDone();

        #region IAlgorithm

        public virtual ControlledParameterTemplate BuildTemplate(IControlled controlledParameter)
        {
            switch (controlledParameter.ControlledBasis)
            {
                case ControlledBasis.FloatingPoint:
                    return new ControlledDoubleParameterTemplate(controlledParameter);

                case ControlledBasis.Integer:
                    return new ControlledIntParameterTemplate(controlledParameter);

                case ControlledBasis.String:
                    return new ControlledStringParameterTemplate(controlledParameter);

                default:
                    UnityEngine.Debug.LogError($"Unexpected ControlledBasis: {controlledParameter.ControlledBasis}");
                    return null;
            }
        }

        public virtual JsonObject GetTrialMetaData() => new JsonObject();

        #endregion IAlgorithm
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
    }
}
