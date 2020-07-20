using System;
using System.Collections.Generic;
using BGC.Scripting;

namespace BGC.Parameters
{
    public enum ControlledBasis
    {
        FloatingPoint = 0,
        Integer,
        String,
        MAX
    }

    [System.Flags]
    public enum StepStatus
    {
        Success = 0,
        OutOfBounds = 1 << 0,
        TypeError = 1 << 1
    }

    public interface IControlled : IPropertyGroup
    {
        StepStatus StepTo(int stepNumber, ControlledParameterTemplate template);
        double GetPartialStepValue(double stepValue, ControlledParameterTemplate template);

        ControlledBasis ControlledBasis { get; }

        string GetValueString();
    }

    public interface IControlSource : IPropertyGroup
    {
        int GetSourceCount();
        string GetSourcePathDisplayName(int index);

        void RegisterControlledParameters(IEnumerable<ControlledParameterTemplate> controlledTemplates);
        void CleanupLinks();
        void PopulateScriptContext(GlobalRuntimeContext scriptContext);
    }
}
