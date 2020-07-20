using System;
using BGC.Scripting;
using BGC.UI.Dialogs;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.FixedCollection
{
    [PropertyChoiceTitle("Scripted Collection")]
    [ScriptFieldDisplay("Script", displayTitle: "Script", initial: DEFAULT_SCRIPT)]
    public class ScriptedCollectionAlgorithm : AlgorithmBase, IResponseCollectionAlgorithm, IBescriptedPropertyGroup
    {
        [DisplayInputField("Script")]
        public string Script { get; set; }


        #region IControlSource

        public override int GetSourceCount() => 0;

        public override string GetSourcePathDisplayName(int index)
        {
            throw new ParameterizedCompositionException(
                $"Unexpected Source index: {index}",
                this.GetGroupPath());
        }

        #endregion IControlSource
        #region Handler

        private Script scriptObject;
        private ScriptRuntimeContext context;

        int IBescriptedPropertyGroup.InitPriority => 1;

        void IBescriptedPropertyGroup.Initialize(GlobalRuntimeContext globalContext)
        {
            scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Initialize",
                    returnType: typeof(void)),
                new FunctionSignature(
                    identifier: "SubmitResult",
                    returnType: typeof(void),
                    arguments: new VariableData("stepValue", typeof(int))),
                new FunctionSignature(
                    identifier: "End",
                    returnType: typeof(bool)),
                new FunctionSignature(
                    identifier: "CalculateThreshold",
                    returnType: typeof(double)));

            context = scriptObject.PrepareScript(globalContext);
        }

        public void Initialize()
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

        protected override void FinishInitialization()
        {
            SetStepValue(0, 0);
        }

        public void SubmitTrialResult(int stepValue)
        {
            try
            {
                scriptObject.ExecuteFunction("SubmitResult", context, stepValue);
            }
            catch (ScriptRuntimeException excp)
            {
                UnityEngine.Debug.LogError($"Runtime Error: \"Step\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Runtime Error",
                    bodyText: $"Runtime Error: \"Step\" failed with error: {excp.Message}.");
            }
            catch (Exception excp)
            {
                UnityEngine.Debug.LogError($"Error: \"Step\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Error: \"Step\" failed with error: {excp.Message}.");
            }
        }

        public override bool IsDone()
        {
            try
            {
                return scriptObject.ExecuteFunction<bool>("End", context);
            }
            catch (ScriptRuntimeException excp)
            {
                UnityEngine.Debug.LogError($"Runtime Error: \"End\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Runtime Error",
                    bodyText: $"Runtime Error: \"End\" failed with error: {excp.Message}.");
            }
            catch (Exception excp)
            {
                UnityEngine.Debug.LogError($"Error: \"End\" failed with error: {excp.Message}.");

                ModalDialog.ShowSimpleModal(ModalDialog.Mode.Accept,
                    headerText: "Error",
                    bodyText: $"Error: \"End\" failed with error: {excp.Message}.");
            }

            return true;
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(0);
                template.PopulateScriptContextOutputs(scriptContext);
            }
        }

        public double GetOutputStepValue()
        {
            try
            {
                return scriptObject.ExecuteFunction<double>("CalculateThreshold", context);
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

        void IBescriptedPropertyGroup.UpdateStateVarRectifier(InputRectificationContainer rectifier)
        {
            Script scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Initialize",
                    returnType: typeof(void)),
                new FunctionSignature(
                    identifier: "SubmitResult",
                    returnType: typeof(void),
                    arguments: new VariableData("stepValue", typeof(int))),
                new FunctionSignature(
                    identifier: "End",
                    returnType: typeof(bool)),
                new FunctionSignature(
                    identifier: "CalculateThreshold",
                    returnType: typeof(double)));


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

        #endregion Handler
        #region Script Constant

        const string DEFAULT_SCRIPT =
@"//Default Collection Algorithm script that collects 3 responses and returns the average
const int COLLECTION_COUNT = 3;

double cumulativeValue = 0.0;
int trialCount = 0;

//Initialize the algorithm
void Initialize()
{
    
}

//Collects the submitted result
void SubmitResult(int stepValue)
{
    trialCount++;
    cumulativeValue += stepValue;
}

//Is the task finished?
bool End()
{
    return trialCount >= COLLECTION_COUNT;
}

//Calculate the end threshold estimate
double CalculateThreshold()
{
    return cumulativeValue / trialCount;
}";

        #endregion Script Constant
    }
}
