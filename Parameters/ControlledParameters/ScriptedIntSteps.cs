using System;
using BGC.Scripting;
using BGC.UI.Dialogs;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Scripted")]
    [ScriptFieldDisplay("Script", displayTitle: "Script", initial: DEFAULT_SCRIPT)]
    public class ScriptedIntSteps : StimulusPropertyGroup, ISimpleIntStepTemplate, IBescriptedPropertyGroup
    {
        [DisplayInputField("Script")]
        public string Script { get; set; }

        private Script scriptObject;
        private ScriptRuntimeContext context;

        void IBescriptedPropertyGroup.UpdateStateVarRectifier(InputRectificationContainer rectifier)
        {
            scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Initialize",
                    returnType: typeof(void)),
                new FunctionSignature(
                    identifier: "GetValue",
                    returnType: typeof(int),
                    arguments: new VariableData("stepNumber", typeof(int))),
                new FunctionSignature(
                    identifier: "CouldStepTo",
                    returnType: typeof(bool),
                    arguments: new VariableData("stepNumber", typeof(int))),
                new FunctionSignature(
                    identifier: "CalculateThreshold",
                    returnType: typeof(double),
                    arguments: new VariableData("stepValue", typeof(double))));


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

        bool ISimpleIntStepTemplate.CouldStepTo(int stepNumber)
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

        int ISimpleIntStepTemplate.GetValue(int stepNumber)
        {
            try
            {
                return scriptObject.ExecuteFunction<int>("GetValue", context, stepNumber);
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

            return 0;
        }

        double ISimpleIntStepTemplate.GetPartialValue(double stepNumber)
        {
            try
            {
                return scriptObject.ExecuteFunction<double>("CalculateThreshold", context, stepNumber);
            }
            catch (ScriptRuntimeException excp)
            {
                UnityEngine.Debug.LogError($"Runtime Error: \"CalculateThreshold\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Runtime Error",
                    bodyText: $"Runtime Error: \"CalculateThreshold\" failed with error: {excp.Message}.");
            }
            catch (Exception excp)
            {
                UnityEngine.Debug.LogError($"Error: \"CalculateThreshold\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Error: \"CalculateThreshold\" failed with error: {excp.Message}.");
            }

            return 0.0;
        }

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
                       returnType: typeof(int),
                       arguments: new VariableData("stepNumber", typeof(int))),
                    new FunctionSignature(
                        identifier: "CouldStepTo",
                        returnType: typeof(bool),
                        arguments: new VariableData("stepNumber", typeof(int))),
                   new FunctionSignature(
                       identifier: "CalculateThreshold",
                       returnType: typeof(double),
                       arguments: new VariableData("stepValue", typeof(double))));

            context = scriptObject.PrepareScript(globalContext);
        }

        void ISimpleIntStepTemplate.Initialize()
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

        #region Script Constant
        const string DEFAULT_SCRIPT =
@"const int baseValue = 0;
const int stepSize = 1;
const int minValue = 0;
const int maxValue = 10;

//Initialize the Property
void Initialize() { }

//Determine the value to step to
int GetValue(int stepNumber)
{
    return baseValue + stepSize * stepNumber;
}

//Determine if the stepNumber would be valid
bool CouldStepTo(int stepNumber)
{
    int potentialValue = GetValue(stepNumber);
    return potentialValue >= minValue && potentialValue <= maxValue;
}

//Calculate the end threshold estimate
double CalculateThreshold(double stepValue)
{
    return baseValue + stepSize * stepValue;
}";
        #endregion
    }
}
