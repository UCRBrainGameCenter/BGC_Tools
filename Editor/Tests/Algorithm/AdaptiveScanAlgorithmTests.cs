using BGC.Extensions.Linq;
using BGC.Parameters;
using BGC.Parameters.Algorithms.AdaptiveScan;
using BGC.Scripting;
using LightJson;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
                NonAdaptiveScansCount = 2,
                OutOfBoundsBehavior = new ClampOutOfBoundsBehavior(),
                NarrowingBehavior = new ScalarNarrowBehavior() { Value = 0.5 },
                ScanTerminationRule = new NoScanTerminationRule(),
                StoppingRule = new NoAdditionalStoppingRule(),
                ThresholdScanCount = 3,
                NarrowingTermination = true,
                NeverNarrowRatio = 0.5
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
            Debug.Log($"stepNumbers: {string.Join(", ", stepNumbers)}");
            Debug.Log($"stepValues: {string.Join(", ", stepValues)}");
            Debug.Log($"isCorrect: {string.Join(", ", isCorrect)}");
            Debug.Log($"Threshold: {controlledTemplate.Threshold}");
        }

        [Test]
        public void TestBoundaryHandling()
        {
            // Configure with a narrow range to ensure we hit boundaries
            SimpleDoubleLinearSteps linear = new SimpleDoubleLinearSteps()
            {
                BaseValue = 1,
                Minimum = 1,
                Maximum = 30,
                DecreaseParameter = false,
                BaseStepSize = 1,
            };
            ((ISimpleDoubleStepTemplate)linear).Initialize();

            ControlledValue controlledValue = new ControlledValue();

            ControlledDoubleParameterTemplate controlledTemplate = new ControlledDoubleParameterTemplate(controlledValue)
            {
                ControllerParameter = 0,
                StepTemplate = linear,
            };

            AdaptiveScanAlgorithm algorithm = new AdaptiveScanAlgorithm()
            {
                Steps = 7,
                InitialStepSize = 3,
                MaximumSlideDistance = 999,
                NarrowOnInvalidScan = false,
                NonAdaptiveScansCount = 0,
                OutOfBoundsBehavior = new ClampOutOfBoundsBehavior(),
                NarrowingBehavior = new DifferenceNarrowBehavior() { Value = 1 },
                ScanTerminationRule = new NoScanTerminationRule(),
                StoppingRule = new TotalScansStoppingRule() { Value = 7 },
                ThresholdScanCount = 3,
                NarrowingTermination = false,
                NeverNarrowRatio = 0.5
            };

            algorithm.Initialize(0.5);
            algorithm.RegisterControlledParameters(controlledTemplate.ToSingleItemEnumberable());

            List<int> stepNumbers = new List<int>();
            List<int> scanStarts = new List<int>();
            int scanCount = 0;
            int lastStepNumber = -1;

            while (!algorithm.IsDone())
            {
                // Detect the start of a new scan
                if ((scanCount % algorithm.Steps) == 0)
                {
                    scanCount++;
                    scanStarts.Add(controlledValue.stepNumber);
                }
                lastStepNumber = controlledValue.stepNumber;

                stepNumbers.Add(controlledValue.stepNumber);

                // Always correct so we race to the boundary
                algorithm.SubmitTrialResult(true);
            }

            GlobalRuntimeContext scriptContext = new GlobalRuntimeContext();
            algorithm.PopulateScriptContext(scriptContext);

            // Log everything for inspection
            Debug.Log($"Boundary Test - stepNumbers: {string.Join(", ", stepNumbers)}");
            Debug.Log($"Boundary Test - scanStarts: {string.Join(", ", scanStarts)}");
            Debug.Log($"Boundary Test - Threshold: {controlledTemplate.Threshold}");

            // Threshold should be very close to the max value
            Assert.IsTrue(Math.Abs(controlledTemplate.Threshold - linear.Maximum) < 0.1,
                "Threshold did not converge to the expected maximum value");

            // The start of each scan should always be between the min and max of the previous scan
            for (int i = 1; i < scanStarts.Count; i++)
            {
                int curStart = scanStarts[i];
                int prevStart = scanStarts[i - 1];
                int prevStepStart = stepNumbers[prevStart];
                int prevStepEnd = stepNumbers[curStart - 1];
                int curStepStart = stepNumbers[curStart];
                Assert.IsTrue(curStepStart >= prevStepStart && curStepStart <= prevStepEnd,
                    $"Scan {i} started outside the expected range: {curStepStart} not in [{prevStepStart}, {prevStepEnd}]");
            }
        }

        [Test]
        public void TestNeverNarrowRatio()
        {
            // Configure with a narrow range that will quickly hit boundaries
            SimpleDoubleLinearSteps linear = new SimpleDoubleLinearSteps()
            {
                BaseValue = 1, 
                Minimum = 1,
                Maximum = 17,
                DecreaseParameter = false,
                BaseStepSize = 1,
            };
            ((ISimpleDoubleStepTemplate)linear).Initialize();
    
            ControlledValue controlledValue = new ControlledValue();
            ControlledDoubleParameterTemplate controlledTemplate = new ControlledDoubleParameterTemplate(controlledValue)
            {
                ControllerParameter = 0,
                StepTemplate = linear,
            };
    
            AdaptiveScanAlgorithm algorithm = new AdaptiveScanAlgorithm()
            {
                Steps = 10,                         // We need enough steps to hit the boundary
                InitialStepSize = 2,                // Start with a moderate step size
                MaximumSlideDistance = 999,
                NarrowOnInvalidScan = false,
                NonAdaptiveScansCount = 0,          // Start adaptive immediately
                OutOfBoundsBehavior = new ClampOutOfBoundsBehavior(),
                NarrowingBehavior = new ScalarNarrowBehavior() { Value = 0.5 }, // Will halve the step size
                ScanTerminationRule = new NoScanTerminationRule(),
                StoppingRule = new TotalScansStoppingRule() { Value = 4 },
                ThresholdScanCount = 3,
                NarrowingTermination = false,
                NeverNarrowRatio = 0.5              // This is the feature we're testing
            };
    
            algorithm.Initialize(0.5);
            algorithm.RegisterControlledParameters(controlledTemplate.ToSingleItemEnumberable());
    
            List<int> stepSizes = new List<int>();
            List<int> stepNumbers = new List<int>();
            List<bool> isCorrect = new List<bool>();
            List<int> scanStarts = new List<int>();
            List<int> clampedCounts = new List<int>();
    
            int prevScanStart = 0;
            int prevStepSize = algorithm.CurStepSize;

            while (!algorithm.IsDone())
            {
                // Record current stepNumber
                stepNumbers.Add(controlledValue.stepNumber);
        
                // Track when a new scan starts
                if (algorithm.CurScanCount > scanStarts.Count)
                {
                    // Record info about the previous scan
                    scanStarts.Add(prevScanStart);
                    stepSizes.Add(prevStepSize);
            
                    // Record the number of steps clamped at the same value at the end of the scan
                    if (stepNumbers.Count >= algorithm.Steps)
                    {
                        // Get last scan's steps
                        var lastScanSteps = stepNumbers.Skip(stepNumbers.Count - algorithm.Steps).ToList();
                
                        // The maximum step number in this scan
                        int maxStep = lastScanSteps.Max();
                
                        // Count how many steps equal the maximum
                        int clamped = lastScanSteps.Count(s => s == maxStep);
                        clampedCounts.Add(clamped);
                    }
            
                    // Store current state for next comparison
                    prevScanStart = algorithm.CurScanStartStep;
                    prevStepSize = algorithm.CurStepSize;
                }
        
                // For the first scan, always correct to quickly reach the boundary
                if (algorithm.CurScanCount == 0)
                {
                    algorithm.SubmitTrialResult(true);
                    isCorrect.Add(true);
                }
                // For subsequent scans, answer correctly except at the highest step numbers
                // This should make the threshold fall in the middle, triggering narrowing
                else
                {
                    // Get exactly 7 correct and 3 incorrect
                    bool isCorrectResponse = algorithm.CurTrial < 7;
                    algorithm.SubmitTrialResult(isCorrectResponse);
                    isCorrect.Add(isCorrectResponse);
                }
            }
    
            // Add the final scan info
            scanStarts.Add(prevScanStart);
            stepSizes.Add(algorithm.CurStepSize);
    
            // Record final clamp count
            if (stepNumbers.Count >= algorithm.Steps)
            {
                var lastScanSteps = stepNumbers.Skip(stepNumbers.Count - algorithm.Steps).ToList();
                int maxStep = lastScanSteps.Max();
                int clamped = lastScanSteps.Count(s => s == maxStep);
                clampedCounts.Add(clamped);
            }
    
            // Log everything for inspection
            Debug.Log($"NeverNarrow Test - step numbers: {string.Join(", ", stepNumbers)}");
            Debug.Log($"NeverNarrow Test - responses: {string.Join(", ", isCorrect)}");
            Debug.Log($"NeverNarrow Test - scan starts: {string.Join(", ", scanStarts)}");
            Debug.Log($"NeverNarrow Test - step sizes: {string.Join(", ", stepSizes)}");
            Debug.Log($"NeverNarrow Test - clamped counts: {string.Join(", ", clampedCounts)}");
    
            // Find a scan where more than 50% of steps were clamped
            bool foundHighClampRatio = false;
            for (int i = 0; i < clampedCounts.Count - 1; i++)
            {
                double clampRatio = (double)clampedCounts[i] / algorithm.Steps;
        
                if (clampRatio > algorithm.NeverNarrowRatio)
                {
                    foundHighClampRatio = true;

                    Debug.Log($"Found scan {i} with {clampedCounts[i]} clamped steps out of {algorithm.Steps} " +
                        $"({clampRatio:P1} clamped) - step size: {stepSizes[i]}");

                    // Step size should NOT decrease (narrowing prevented)
                    Assert.IsTrue(stepSizes[i+1] >= stepSizes[i], 
                        $"Step size decreased from {stepSizes[i]} to {stepSizes[i+1]} " +
                        $"despite {clampRatio:P1} of steps being clamped (above the {algorithm.NeverNarrowRatio:P1} threshold)");
                }
            }
    
            // Make sure we actually tested the feature
            Assert.IsTrue(foundHighClampRatio, 
                "Test did not encounter a scan with over 50% clamped values - adjust test parameters");
        }
    }
}