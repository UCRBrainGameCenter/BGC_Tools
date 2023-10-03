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

        public StepStatus SetStepValue(
            int sourceParameter,
            int stepNumber,
            bool forceStep = false)
        {
            bool anyFailStep = controlledParameters
                .Where(x => x.ControllerParameter == sourceParameter)
                .Any(x => !x.CouldStepTo(stepNumber));

            if (anyFailStep && !forceStep)
            {
                //Report failure to step
                return StepStatus.OutOfBounds;
            }

            StepStatus valueStatus = StepStatus.Success;

            if (anyFailStep)
            {
                //Report error despite being forced
                valueStatus |= StepStatus.OutOfBounds;
            }

            foreach (ControlledParameterTemplate template in controlledParameters.Where(x => x.ControllerParameter == sourceParameter))
            {
                valueStatus |= template.StepTo(stepNumber);
            }

            return valueStatus;
        }

        public bool CouldStepTo(
            int sourceParameter,
            int stepNumber)
        {
            return controlledParameters
                .Where(x => x.ControllerParameter == sourceParameter)
                .All(x => x.CouldStepTo(stepNumber));
        }

        #endregion IControlSource
    }

}
