using System;
using BGC.Scripting;
using BGC.UI.Dialogs;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters
{
    [StringFieldDisplay("Output", displayTitle: "Output")]
    [ScriptFieldDisplay("Script", displayTitle: "Script", initial: DEFAULT_SCRIPT)]
    public class ControlledStringParameterTemplate : ControlledParameterTemplate, IStringParameterTemplate, IBescriptedPropertyGroup
    {
        [OutputField("Output")]
        public string Output { get; set; }

        [DisplayOutputFieldKey("Output")]
        public string OutputKey { get; set; }

        [DisplayInputField("Script")]
        public string Script { get; set; }

        private Script scriptObject;
        private ScriptRuntimeContext context;

        public ControlledStringParameterTemplate(IControlled controlledParameter)
            : base(controlledParameter)
        { }


        public override void FinalizeParameters(double thresholdStepValue)
        {
            try
            {
                Output = scriptObject.ExecuteFunction<string>("CalculateOutput", context, thresholdStepValue);
            }
            catch (ScriptRuntimeException excp)
            {
                UnityEngine.Debug.LogError($"Runtime Error: \"CalculateOutput\" failed with error: {excp.Message}.");

                Output = "";

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Runtime Error",
                    bodyText: $"Runtime Error: \"CalculateOutput\" failed with error: {excp.Message}.");
            }
            catch (Exception excp)
            {
                UnityEngine.Debug.LogError($"Error: \"CalculateOutput\" failed with error: {excp.Message}.");

                Output = "";

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Error: \"CalculateOutput\" failed with error: {excp.Message}.");
            }
        }

        public override void Initialize()
        {
            try
            {
                scriptObject.ExecuteFunction("Initialize", context);
            }
            catch (ScriptRuntimeException excp)
            {
                UnityEngine.Debug.LogError($"Runtime Error: \"Initialize\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Runtime Error",
                    bodyText: $"Runtime Error: \"Initialize\" failed with error: {excp.Message}.");
            }
            catch (Exception excp)
            {
                UnityEngine.Debug.LogError($"Error: \"Initialize\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Error: \"Initialize\" failed with error: {excp.Message}.");
            }
        }

        public override bool CouldStepTo(int stepNumber)
        {
            try
            {
                return scriptObject.ExecuteFunction<bool>("CouldStepTo", context, stepNumber);
            }
            catch (ScriptRuntimeException excp)
            {
                UnityEngine.Debug.LogError($"Runtime Error: \"CouldStepTo\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Runtime Error",
                    bodyText: $"Runtime Error: \"CouldStepTo\" failed with error: {excp.Message}.");
            }
            catch (Exception excp)
            {
                UnityEngine.Debug.LogError($"Error: \"CouldStepTo\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Error: \"CouldStepTo\" failed with error: {excp.Message}.");
            }

            return false;
        }

        #region IDoubleParameterTemplate

        string IStringParameterTemplate.GetValue(int stepNumber)
        {
            try
            {
                return scriptObject.ExecuteFunction<string>("GetValue", context, stepNumber);
            }
            catch (ScriptRuntimeException excp)
            {
                UnityEngine.Debug.LogError($"Runtime Error: \"GetValue\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Runtime Error",
                    bodyText: $"Runtime Error: \"GetValue\" failed with error: {excp.Message}.");
            }
            catch (Exception excp)
            {
                UnityEngine.Debug.LogError($"Error: \"GetValue\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Error: \"GetValue\" failed with error: {excp.Message}.");
            }

            return "";
        }

        string IStringParameterTemplate.GetOutput() => Output;

        #endregion IDoubleParameterTemplate
        #region IBescriptedPropertyGroup

        int IBescriptedPropertyGroup.InitPriority => 2;

        void IBescriptedPropertyGroup.Initialize(GlobalRuntimeContext globalContext)
        {
            scriptObject = ScriptParser.LexAndParseScript(
                   script: Script,
                   new FunctionSignature(
                       identifier: "Initialize",
                       returnType: typeof(void)),
                   new FunctionSignature(
                       identifier: "GetValue",
                       returnType: typeof(string),
                       arguments: new ArgumentData("stepNumber", typeof(int))),
                    new FunctionSignature(
                        identifier: "CouldStepTo",
                        returnType: typeof(bool),
                        arguments: new ArgumentData("stepNumber", typeof(int))),
                   new FunctionSignature(
                       identifier: "CalculateOutput",
                       returnType: typeof(string),
                       arguments: new ArgumentData("stepValue", typeof(double))));

            context = scriptObject.PrepareScript(globalContext);
        }

        void IBescriptedPropertyGroup.UpdateStateVarRectifier(InputRectificationContainer rectifier)
        {
            Script scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Initialize",
                    returnType: typeof(void)),
                new FunctionSignature(
                    identifier: "GetValue",
                    returnType: typeof(string),
                    arguments: new ArgumentData("stepNumber", typeof(int))),
                new FunctionSignature(
                    identifier: "CouldStepTo",
                    returnType: typeof(bool),
                    arguments: new ArgumentData("stepNumber", typeof(int))),
                new FunctionSignature(
                    identifier: "CalculateOutput",
                    returnType: typeof(string),
                    arguments: new ArgumentData("stepValue", typeof(double))));

            foreach (KeyInfo keyInfo in scriptObject.GetDeclarations())
            {
                //Mark output
                if (rectifier.unsatisfiedVariables.Contains(keyInfo.key))
                {
                    rectifier.unsatisfiedVariables.Remove(keyInfo.key);
                }

                if (rectifier.typeMapping.ContainsKey(keyInfo.key))
                {
                    if (keyInfo.valueType != rectifier.typeMapping[keyInfo.key].valueType)
                    {
                        throw new KeyMismatchException(
                            keyName: keyInfo.key,
                            keyPath: "Script",
                            desiredType: keyInfo.valueType,
                            encounteredType: rectifier.typeMapping[keyInfo.key].valueType,
                            message: $"Variable {keyInfo.key} of type {keyInfo.valueType.Name} mismatched existing variable of type {rectifier.typeMapping[keyInfo.key].valueType.Name}");
                    }
                }
                else
                {
                    rectifier.typeMapping.Add(keyInfo.key, keyInfo);
                }
            }

            foreach (KeyInfo keyInfo in scriptObject.GetDependencies())
            {
                //Mark requirements
                if (!rectifier.unsatisfiedVariables.Contains(keyInfo.key))
                {
                    rectifier.unsatisfiedVariables.Add(keyInfo.key);
                }

                if (rectifier.typeMapping.ContainsKey(keyInfo.key))
                {
                    if (keyInfo.valueType != rectifier.typeMapping[keyInfo.key].valueType)
                    {
                        throw new KeyMismatchException(
                            keyName: keyInfo.key,
                            keyPath: "Script",
                            desiredType: keyInfo.valueType,
                            encounteredType: rectifier.typeMapping[keyInfo.key].valueType,
                            message: $"Variable {keyInfo.key} of type {keyInfo.valueType.Name} mismatched existing variable of type {rectifier.typeMapping[keyInfo.key].valueType.Name}");
                    }
                }
                else
                {
                    rectifier.typeMapping.Add(keyInfo.key, keyInfo);
                }
            }
        }

        #endregion IBescriptedPropertyGroup
        #region Script Constant

        const string DEFAULT_SCRIPT =
@"List<string> stimuli = new List<string>()
{
    ""Easy.wav"",
    ""Medium.wav"",
    ""Hard.wav"",
    ""Impossible.wav""
};

//Initialize the Property
void Initialize() { }

//Determine the value
string GetValue(int stepNumber)
{
    return stimuli[Math.Clamp(stepNumber, 0, stimuli.Count - 1)];
}

//Determine if the stepNumber would be valid
bool CouldStepTo(int stepNumber)
{
    return stepNumber >= 0 && stepNumber < stimuli.Count;
}

//Calculate the output
string CalculateOutput(double stepValue)
{
    return stimuli[(int)Math.Floor(stepValue)];
}";

        #endregion
    }
}
