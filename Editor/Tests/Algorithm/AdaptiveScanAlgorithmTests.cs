using BGC.Extensions.Linq;
using BGC.Parameters;
using BGC.Parameters.Algorithms.AdaptiveScan;
using BGC.Scripting;
using LightJson;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BGC.Tests
{
    public class AdaptiveScanAlgorithmTests
    {
        private class ControlledValue : IControlled
        {
            public int stepNumber;
            public double stepValue;
            public ControlledBasis ControlledBasis => ControlledBasis.FloatingPoint;

            public void Deserialize(JsonObject data)
            {
                throw new System.NotImplementedException();
            }

            public IPropertyGroup GetParent()
            {
                throw new System.NotImplementedException();
            }

            public JsonObject Serialize()
            {
                throw new System.NotImplementedException();
            }

            public void SetParent(IPropertyGroup parent)
            {
                throw new System.NotImplementedException();
            }

            public StepStatus StepTo(int stepNumber, ControlledParameterTemplate template)
            {
                this.stepNumber = stepNumber;

                if (template is IDoubleParameterTemplate doubleTemplate)
                {
                    this.stepValue = doubleTemplate.GetValue(stepNumber);
                    return StepStatus.Success;
                }

                return StepStatus.TypeError;
            }

            public double GetPartialStepValue(double stepValue, ControlledParameterTemplate template)
            {
                if (template is IDoubleParameterTemplate doubleTemplate)
                {
                    return doubleTemplate.GetPartialValue(stepValue);
                }

                Debug.LogError("Mismatched type.");
                return double.NaN;
            }

            public string GetValueString() => stepValue.ToString();
        }

        [Test]
        public void TestAdaptiveScan()
        {
            SimpleDoubleExponentialSteps exponential = new SimpleDoubleExponentialSteps()
            {
                BaseValue = 128,
                Minimum = 4,
                Maximum = 1024,
                ConvergenceValue = 0,
                DecreaseParameter = true,
                BaseMajorFactor = 2,
                StepsPerMajorFactor = 8,
            };
            ((ISimpleDoubleStepTemplate)exponential).Initialize();

            ControlledValue controlledValue = new ControlledValue();

            ControlledDoubleParameterTemplate controlledTemplate = new ControlledDoubleParameterTemplate(controlledValue)
            {
                ControllerParameter = 0,
                StepTemplate = exponential,
            };

            AdaptiveScanAlgorithm algorithm = new AdaptiveScanAlgorithm()
            {
                Steps = 9,
                InitialStepSize = 8,
                MaximumSlideDistance = 32,
                NarrowOnInvalidScan = false,
                OutOfBoundsBehavior = new ClampOutOfBoundsBehavior(),
                NarrowingBehavior = new ScalarNarrowBehavior() { Value = 0.5 },
                ScanTerminationRule = new NoScanTerminationRule(),
                StoppingRule = new NoAdditionalStoppingRule(),
                ThresholdScanCount = 3,
                NarrowingTermination = true,
            };

            algorithm.Initialize(0.5);
            algorithm.RegisterControlledParameters(controlledTemplate.ToSingleItemEnumberable());

            Debug.Assert(controlledValue.stepNumber == 0);
            Debug.Assert(controlledValue.stepValue == 128.0);
            Debug.Assert(controlledTemplate.Threshold == 0);

            // Threshold for always getting it correct
            double alwaysCorrectValue = 64.0;

            // Consistent 50% chance of being correct when under threshold
            bool nextChanceCorrect = true;

            List<int> stepNumbers = new List<int>();
            List<double> stepValues = new List<double>();
            List<bool> isCorrect = new List<bool>();
            while (!algorithm.IsDone())
            {
                stepNumbers.Add(controlledValue.stepNumber);
                stepValues.Add(controlledValue.stepValue);
                if (controlledValue.stepValue >= alwaysCorrectValue)
                {
                    algorithm.SubmitTrialResult(true);
                    isCorrect.Add(true);
                }
                else
                {
                    algorithm.SubmitTrialResult(nextChanceCorrect);
                    isCorrect.Add(nextChanceCorrect);
                    nextChanceCorrect = !nextChanceCorrect;
                }
            }

            GlobalRuntimeContext scriptContext = new GlobalRuntimeContext();
            algorithm.PopulateScriptContext(scriptContext);

            // The threshold should be close to alwaysCorrectValue
            Debug.Assert(Math.Abs((double)(controlledTemplate.Threshold - alwaysCorrectValue)) < alwaysCorrectValue / 4.0);

            // For inspection to debug the algorithm
            //Debug.Log($"stepNumbers: {string.Join(", ", stepNumbers)}");
            //Debug.Log($"stepValues: {string.Join(", ", stepValues)}");
            //Debug.Log($"isCorrect: {string.Join(", ", isCorrect)}");
            Debug.Log($"Threshold: {controlledTemplate.Threshold}");
        }
    }
}