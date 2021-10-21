using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using BGC.Scripting;
using BGC.UI.Dialogs;
using BGC.Parameters.Exceptions;

namespace BGC.Parameters.Algorithms.Scripted
{
    [PropertyChoiceTitle("MultiParam Scripted")]
    [EnumDropdownDisplay("StepScheme", displayTitle: "Step Scheme", initialValue: (int)StepScheme.Absolute, choiceListMethodName: nameof(GetStepSchemeChoiceList))]
    [EnumDropdownDisplay("ParameterCount", displayTitle: "Parameter Count", initialValue: 1, choiceListMethodName: nameof(GetParamCountChoiceList))]
    [ScriptFieldDisplay("Script", displayTitle: "Script", initial: DEFAULT_SCRIPT)]
    public class MultiParamScriptedAlgorithm : AlgorithmBase, IBinaryOutcomeAlgorithm, IBescriptedPropertyGroup
    {
        [DisplayInputField("StepScheme")]
        public StepScheme StepScheme { get; set; }

        [DisplayInputField("ParameterCount")]
        public int ParameterCount { get; set; }

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

        public static List<ValueNamePair> GetParamCountChoiceList()
        {
            return new List<ValueNamePair>
            {
                new ValueNamePair(1, "1 Parameter"),
                new ValueNamePair(2, "2 Parameters"),
                new ValueNamePair(3, "3 Parameters"),
                new ValueNamePair(4, "4 Parameters")
            };
        }

        #endregion Setup Methods
        #region IControlSource

        public override int GetSourceCount() => ParameterCount;

        public override string GetSourcePathDisplayName(int index)
        {
            if (index >= ParameterCount)
            {
                throw new ParameterizedCompositionException(
                    $"Unexpected Source index: {index}",
                    this.GetGroupPath());
            }

            return $"Scripted Parameter {index}";
        }

        #endregion IControlSource
        #region IBescriptedPropertyGroup

        private readonly FunctionSignature oldInitializeSignature = new FunctionSignature(
            identifier: "Initialize",
            returnType: typeof(List<int>),
            arguments: new VariableData("paramCount", typeof(int)));

        private readonly FunctionSignature newInitializeSignature = new FunctionSignature(
            identifier: "Initialize",
            returnType: typeof(List<int>),
            arguments: new VariableData("algorithmQuerier", typeof(IMultiParamScriptedAlgorithmQuerier)));

        void IBescriptedPropertyGroup.UpdateStateVarRectifier(InputRectificationContainer rectifier)
        {
            Script scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Step",
                    returnType: typeof(List<int>),
                    arguments: new VariableData("lastTrialCorrect", typeof(bool))),
                new FunctionSignature(
                    identifier: "End",
                    returnType: typeof(bool)),
                new FunctionSignature(
                    identifier: "CalculateThreshold",
                    returnType: typeof(List<double>)));

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
List<int> steps;
IMultiParamScriptedAlgorithmQuerier _algorithmQuerier;

//Initialize the algorithm and returns the starting step
List<int> Initialize(IMultiParamScriptedAlgorithmQuerier algorithmQuerier)
{
    _algorithmQuerier = algorithmQuerier;
    int paramCount = _algorithmQuerier.GetParamCount();

    steps = new List<int>();

    for (int i = 0; i < paramCount; i++)
    {
        steps.Add(0);
    }

    return steps;
}

//Determine how to step
List<int> Step(bool lastTrialCorrect)
{
    trialCount++;
    if (lastTrialCorrect)
    {
        correctCount++;
    }

    for (int i = 0; i < steps.Count; i++)
    {
        //Increment step values
        if (_algorithmQuerier.CouldStepTo(i, steps[i] + 1))
        {
            steps[i]++;
        }
    }

    return steps;
}

//Is the task finished?
bool End()
{
    return trialCount >= 10;
}

//Calculate the end threshold estimate
List<double> CalculateThreshold()
{
    List<double> thresholds = new List<double>();
    for (int i = 0; i < steps.Count; i++)
    {
        thresholds.Add(Math.Clamp(2 * (correctCount - 0.5 * trialCount), 0, trialCount));
    }
    return thresholds;
}";
        #endregion Script Constant
        #region Handler

        private Script scriptObject;
        private ScriptRuntimeContext context;
        private StepScheme stepScheme;
        private List<int> currentSteps = null;

        private bool newInitializationScheme = true;

        int IBescriptedPropertyGroup.InitPriority => 1;

        void IBescriptedPropertyGroup.Initialize(GlobalRuntimeContext globalContext)
        {
            scriptObject = ScriptParser.LexAndParseScript(
                script: Script,
                new FunctionSignature(
                    identifier: "Step",
                    returnType: typeof(List<int>),
                    arguments: new VariableData("lastTrialCorrect", typeof(bool))),
                new FunctionSignature(
                    identifier: "End",
                    returnType: typeof(bool)),
                new FunctionSignature(
                    identifier: "CalculateThreshold",
                    returnType: typeof(List<double>)));

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
            currentSteps = null;

            try
            {
                if (newInitializationScheme)
                {
                    AlgorithmQuerier querier = new AlgorithmQuerier(this);

                    //Clone list so we don't risk modifying script memory
                    currentSteps = scriptObject.ExecuteFunction<List<int>>("Initialize", context, querier).ToList();
                }
                else
                {
                    //Clone list so we don't risk modifying script memory
                    currentSteps = scriptObject.ExecuteFunction<List<int>>("Initialize", context, ParameterCount).ToList();
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
            if (currentSteps is null)
            {
                currentSteps = new List<int>();
            }

            //Make sure all of the requisite parameter values are there
            while (currentSteps.Count < ParameterCount)
            {
                currentSteps.Add(0);
            }

            for (int i = 0; i < ParameterCount; i++)
            {
                SetStepValue(i, currentSteps[i], true);
            }
        }

        public void SubmitTrialResult(bool correct)
        {
            List<int> newSteps = null;

            try
            {
                newSteps = scriptObject.ExecuteFunction<List<int>>("Step", context, correct);
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

            for (int i = 0; i < ParameterCount; i++)
            {
                if (newSteps is null || i >= newSteps.Count)
                {
                    continue;
                }

                int newStep;

                switch (stepScheme)
                {
                    case StepScheme.Relative:
                        newStep = currentSteps[i] + newSteps[i];
                        break;

                    case StepScheme.Absolute:
                        newStep = newSteps[i];
                        break;

                    default:
                        UnityEngine.Debug.LogError($"Unexpected StepScheme: {stepScheme}");
                        goto case StepScheme.Relative;
                }

                if (currentSteps[i] != newStep)
                {
                    //Only call SetStepValue on change
                    SetStepValue(i, newStep, true);
                    currentSteps[i] = newStep;
                }
            }
        }

        public override void PopulateScriptContext(GlobalRuntimeContext scriptContext)
        {
            List<double> stepValues = null;

            try
            {
                stepValues = scriptObject.ExecuteFunction<List<double>>("CalculateThreshold", context);
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
                double stepValue;

                if (stepValues is null || stepValues.Count <= template.ControllerParameter)
                {
                    stepValue = 0.0;
                }
                else
                {
                    stepValue = stepValues[template.ControllerParameter];
                }

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

        private class AlgorithmQuerier : IMultiParamScriptedAlgorithmQuerier
        {
            private readonly MultiParamScriptedAlgorithm algorithm;

            public AlgorithmQuerier(MultiParamScriptedAlgorithm algorithm)
            {
                this.algorithm = algorithm;
            }

            public bool CouldStepBy(int parameter, int steps) =>
                algorithm.controlledParameters
                    .Where(x => x.ControllerParameter == parameter)
                    .All(x => x.CouldStepTo(algorithm.currentSteps[parameter] + steps));

            public bool CouldStepTo(int parameter, int stepNumber) =>
                algorithm.controlledParameters
                    .Where(x => x.ControllerParameter == parameter)
                    .All(x => x.CouldStepTo(stepNumber));

            public int GetParamCount() => algorithm.ParameterCount;
        }

        #endregion Handler
    }
}
