using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Scripting;
using BGC.UI.Dialogs;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.Scripted
{
    [PropertyChoiceTitle("Scripted")]
    [EnumDropdownDisplay("StepScheme", displayTitle: "Step Scheme", initialValue: 0, choiceListMethodName: nameof(GetStepSchemeChoiceList))]
    [ScriptFieldDisplay("Script", displayTitle: "Script", initial: DEFAULT_SCRIPT)]
    public class ScriptedAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm, IBescriptedPropertyGroup
    {
        [DisplayInputField("StepScheme")]
        public StepScheme StepScheme { get; set; }

        [DisplayInputField("Script")]
        public string Script { get; set; }

        #region Setup Methods

        public static List<ValueNamePair> GetStepSchemeChoiceList()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair((int)StepScheme.Relative, StepScheme.Relative.ToDisplayName()),
                new ValueNamePair((int)StepScheme.Absolute, StepScheme.Absolute.ToDisplayName())
            };
        }

        #endregion Setup Methods
        #region IControlSource

        public override int GetSourceCount() => 1;

        public override string GetSourcePathDisplayName(int index)
        {
            if (index != 0)
            {
                throw new ParameterizedCompositionException(
                    $"Unexpected Source index: {index}",
                    this.GetGroupPath());
            }

            return "Scripted Parameter";
        }

        #endregion IControlSource
        #region IBescriptedPropertyGroup

        private readonly FunctionSignature oldInitializeSignature = new FunctionSignature(
            identifier: "Initialize",
            returnType: typeof(int));

        private readonly FunctionSignature newInitializeSignature = new FunctionSignature(
            identifier: "Initialize",
            returnType: typeof(int),
            arguments: new VariableData("algorithmQuerier", typeof(IScriptedAlgorithmQuerier)));

        void IBescriptedPropertyGroup.UpdateStateVarRectifier(InputRectificationContainer rectifier)
        {
            Script scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Step",
                    returnType: typeof(int),
                    arguments: new VariableData("lastTrialCorrect", typeof(bool))),
                new FunctionSignature(
                    identifier: "End",
                    returnType: typeof(bool)),
                new FunctionSignature(
                    identifier: "CalculateThreshold",
                    returnType: typeof(double)));

            //Check if it has either initialize method
            if (!(scriptObject.HasFunction(newInitializeSignature) || scriptObject.HasFunction(oldInitializeSignature)))
            {
                //Throw exception
                if (scriptObject.HasFunction("Initialize"))
                {
                    FunctionSignature matchingFunction = scriptObject.GetFunctionSignature("Initialize");
                    //Mismatched Signature
                    throw new ScriptParsingException(
                        source: matchingFunction.identifierToken,
                        message: $"Expected Function: {newInitializeSignature}  Found Function: {matchingFunction}");
                }
                else
                {
                    //Missing Initialize
                    throw new ScriptParsingException(
                        source: new EOFToken(0, 0),
                        message: $"Expected Function not found: {newInitializeSignature}");
                }
            }


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
@"int trialCount = 0;
int correctCount = 0;
IScriptedAlgorithmQuerier _algorithmQuerier;
bool endFlag = false;

//Initialize the algorithm and returns the starting step
int Initialize(IScriptedAlgorithmQuerier algorithmQuerier)
{
    _algorithmQuerier = algorithmQuerier;
    return 0;
}

//Determine how to step
int Step(bool lastTrialCorrect)
{
    trialCount++;
    if (lastTrialCorrect)
    {
        correctCount++;
    }

    if (!_algorithmQuerier.CouldStepBy(1))
    {
        endFlag = true;
        return 0;
    }

    return 1;
}

//Is the task finished?
bool End()
{
    return endFlag || trialCount >= 10;
}

//Calculate the end threshold estimate
double CalculateThreshold()
{
    return Math.Clamp(2 * (correctCount - 0.5 * trialCount), 0, trialCount);
}";
        #endregion Script Constant
        #region Handler

        private Script scriptObject;
        private ScriptRuntimeContext context;
        private StepScheme stepScheme;
        private int currentStep = 0;

        private bool newInitializationScheme = true;

        int IBescriptedPropertyGroup.InitPriority => 1;

        void IBescriptedPropertyGroup.Initialize(GlobalRuntimeContext globalContext)
        {
            scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Step",
                    returnType: typeof(int),
                    arguments: new VariableData("lastTrialCorrect", typeof(bool))),
                new FunctionSignature(
                    identifier: "End",
                    returnType: typeof(bool)),
                new FunctionSignature(
                    identifier: "CalculateThreshold",
                    returnType: typeof(double)));

            //Check which initialize method it has
            if (scriptObject.HasFunction(newInitializeSignature))
            {
                newInitializationScheme = true;
            }
            else if (scriptObject.HasFunction(oldInitializeSignature))
            {
                newInitializationScheme = false;
            }
            else
            {
                //Throw exception
                if (scriptObject.HasFunction("Initialize"))
                {
                    FunctionSignature matchingFunction = scriptObject.GetFunctionSignature("Initialize");
                    //Mismatched Signature
                    throw new ScriptParsingException(
                        source: matchingFunction.identifierToken,
                        message: $"Expected Function: {newInitializeSignature}  Found Function: {matchingFunction}");
                }
                else
                {
                    //Missing Initialize
                    throw new ScriptParsingException(
                        source: new EOFToken(0, 0),
                        message: $"Expected Function not found: {newInitializeSignature}");
                }
            }

            context = scriptObject.PrepareScript(globalContext);
        }

        public void Initialize(double taskGuessRate)
        {
            stepScheme = StepScheme;

            try
            {
                if (newInitializationScheme)
                {
                    AlgorithmQuerier querier = new AlgorithmQuerier(this);

                    currentStep = scriptObject.ExecuteFunction<int>("Initialize", context, querier);
                }
                else
                {
                    currentStep = scriptObject.ExecuteFunction<int>("Initialize", context);
                }
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
            SetStepValue(0, currentStep, true);
        }

        public void SubmitTrialResult(bool correct)
        {
            int step = 0;

            try
            {
                step = scriptObject.ExecuteFunction<int>("Step", context, correct);
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

            int oldStep = currentStep;
            switch (stepScheme)
            {
                case StepScheme.Relative:
                    currentStep += step;
                    break;

                case StepScheme.Absolute:
                    currentStep = step;
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected StepScheme: {stepScheme}");
                    goto case StepScheme.Relative;
            }

            if (oldStep != currentStep)
            {
                //Only call SetStepValue on change
                SetStepValue(0, currentStep, true);
            }
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            double stepValue = 0.0;

            try
            {
                stepValue = scriptObject.ExecuteFunction<double>("CalculateThreshold", context);
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

            foreach (ControlledParameterTemplate template in controlledParameters)
            {
                template.FinalizeParameters(stepValue);
                template.PopulateScriptContextOutputs(scriptContext);
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

        private class AlgorithmQuerier : IScriptedAlgorithmQuerier
        {
            private readonly ScriptedAlgorithm algorithm;

            public AlgorithmQuerier(ScriptedAlgorithm algorithm)
            {
                this.algorithm = algorithm;
            }

            public bool CouldStepBy(int steps) =>
                algorithm.controlledParameters
                    .Where(x => x.ControllerParameter == 0)
                    .All(x => x.CouldStepTo(algorithm.currentStep + steps));

            public bool CouldStepTo(int stepNumber) =>
                algorithm.controlledParameters
                    .Where(x => x.ControllerParameter == 0)
                    .All(x => x.CouldStepTo(stepNumber));
        }

        #endregion Handler
    }
}
