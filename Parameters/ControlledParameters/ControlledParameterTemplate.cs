using System.Collections.Generic;
using LightJson;

namespace BGC.Parameters
{
    [ExtractPropertyGroupTitle]
    [ControlledExtraction("BaseValue", extractionFieldName: "BaseValue")]
    [ControlledExtraction("ConvergenceValue", extractionFieldName: "ConvergenceValue")]
    [ControlledExtraction("Max", extractionFieldName: "Max")]
    [ControlledExtraction("Min", extractionFieldName: "Min")]
    [ControlledExtraction("BaseStepSize", extractionFieldName: "BaseStepSize")]
    [ControlledExtraction("BaseMajorFactor", extractionFieldName: "BaseMajorFactor")]
    [ControlledExtraction("StepsToMajorFactor", extractionFieldName: "StepsToMajorFactor")]
    public abstract class ControlledParameterTemplate : StimulusPropertyGroup, IPropertyGroup
    {
        public readonly IControlled controlledParameter;
        public string Controller { get; set; }
        public int ControllerParameter { get; set; }

        public ControlledParameterTemplate(IControlled controlledParameter)
        {
            this.controlledParameter = controlledParameter;
        }

        public abstract void Initialize();

        public abstract void FinalizeParameters(double thresholdStepValue);

        /// <summary>
        /// Determine if a step would take parameter out of bounds
        /// </summary>
        public abstract bool CouldStepTo(int stepNumber);

        public StepStatus StepTo(int stepNumber) => controlledParameter.StepTo(stepNumber, this);

        protected double GetThresholdValue(double thresholdStepValue) =>
            controlledParameter.GetPartialStepValue(thresholdStepValue, this);

        JsonObject IPropertyGroup.Serialize()
        {
            JsonObject baseData = this.Internal_GetSerializedData();
            baseData.Add("Path", controlledParameter.GetGroupPath(true));
            baseData.Add("Controller", Controller);
            baseData.Add("ControllerParameter", ControllerParameter);

            return baseData;
        }

        public void DeserializeTemplate(JsonObject data)
        {
            Controller = data["Controller"];
            ControllerParameter = data["ControllerParameter"];
            this.Internal_RawDeserialize(data);
        }
    }


    [StringFieldDisplay("Threshold", displayTitle: "Output Threshold")]
    public class ControlledDoubleParameterTemplate : ControlledParameterTemplate, IDoubleParameterTemplate
    {
        [OutputField("Threshold")]
        public double Threshold { get; set; }

        [DisplayOutputFieldKey("Threshold")]
        public string ThresholdKey { get; set; }

        public ControlledDoubleParameterTemplate(IControlled controlledParameter)
            : base(controlledParameter)
        { }

        [AppendSelection(
            typeof(SimpleDoubleLinearSteps),
            typeof(SimpleDoubleExponentialSteps),
            typeof(SimpleDoubleListSteps),
            typeof(ScriptedDoubleSteps))]
        public ISimpleDoubleStepTemplate StepTemplate { get; set; }

        public override void FinalizeParameters(double thresholdStepValue)
        {
            Threshold = GetThresholdValue(thresholdStepValue);
        }

        public override void Initialize() => StepTemplate.Initialize();

        public override bool CouldStepTo(int stepNumber) => StepTemplate.CouldStepTo(stepNumber);

        #region IDoubleParameterTemplate

        double IDoubleParameterTemplate.GetValue(int stepNumber) => StepTemplate.GetValue(stepNumber);
        double IDoubleParameterTemplate.GetPartialValue(double thresholdValue) => StepTemplate.GetPartialValue(thresholdValue);
        double IDoubleParameterTemplate.GetThresholdEstimate() => Threshold;

        #endregion IDoubleParameterTemplate
    }

    [StringFieldDisplay("Threshold", displayTitle: "Output Threshold")]
    public class ControlledIntParameterTemplate : ControlledParameterTemplate, IIntParameterTemplate
    {
        [OutputField("Threshold")]
        public double Threshold { get; set; }

        [DisplayOutputFieldKey("Threshold")]
        public string ThresholdKey { get; set; }

        public ControlledIntParameterTemplate(IControlled controlledParameter)
            : base(controlledParameter)
        { }

        [AppendSelection(
            typeof(SimpleIntLinearSteps),
            typeof(SimpleIntListSteps),
            typeof(ScriptedIntSteps))]
        public ISimpleIntStepTemplate StepTemplate { get; set; }

        public override void FinalizeParameters(double thresholdStepValue) =>
            Threshold = GetThresholdValue(thresholdStepValue);

        public override void Initialize() => StepTemplate.Initialize();

        public override bool CouldStepTo(int stepNumber) => StepTemplate.CouldStepTo(stepNumber);

        #region IIntParameterTemplate

        int IIntParameterTemplate.GetValue(int stepNumber) => StepTemplate.GetValue(stepNumber);
        double IIntParameterTemplate.GetPartialValue(double stepValue) => StepTemplate.GetPartialValue(stepValue);
        double IIntParameterTemplate.GetThresholdEstimate() => Threshold;

        #endregion IIntParameterTemplate
    }
}
