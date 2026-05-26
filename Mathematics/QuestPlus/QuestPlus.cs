using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;

namespace BGC.Mathematics.QuestPlus
{
    /// <summary>
    /// A general multidimensional Bayesian adaptive psychometric procedure
    /// (QUEST+), following Watson (2017).
    ///
    /// Supports arbitrary numbers of stimulus dimensions, psychometric function
    /// parameters, and trial outcomes. The next stimulus is selected to minimize
    /// the expected entropy of the parameter posterior. After each trial, the
    /// posterior is updated by multiplying the prior likelihood by the observed
    /// outcome's likelihood and renormalizing.
    ///
    /// This implementation is engine-only — it does not know about Unity, tasks,
    /// or UI. Tasks wrap it and feed stimuli/responses through Update.
    /// </summary>
    public sealed class QuestPlus
    {
        // ---- Configuration ----

        /// <summary>Ordered stimulus domain dimensions.</summary>
        public IReadOnlyList<QuestPlusDimension> StimDomain => stimDomain;

        /// <summary>Ordered parameter domain dimensions.</summary>
        public IReadOnlyList<QuestPlusDimension> ParamDomain => paramDomain;

        /// <summary>Ordered outcome labels.</summary>
        public IReadOnlyList<string> OutcomeDomain => outcomeDomain;

        /// <summary>The psychometric function in use.</summary>
        public IPsychometricFunction PsychometricFunction { get; }

        /// <summary>How the next stimulus is selected.</summary>
        public StimSelectionMethod SelectionMethod { get; }

        /// <summary>Selection options (used when SelectionMethod is MinNEntropy).</summary>
        public StimSelectionOptions SelectionOptions { get; }

        /// <summary>How the current parameter estimate is computed.</summary>
        public ParamEstimationMethod EstimationMethod { get; set; }

        // ---- State ----

        /// <summary>
        /// Posterior probability density over parameter combinations,
        /// length ParamSize. Sums to 1.
        /// </summary>
        public IReadOnlyList<double> Posterior => posterior;

        /// <summary>
        /// Prior probability density over parameter combinations, length ParamSize.
        /// Sums to 1.
        /// </summary>
        public IReadOnlyList<double> Prior => prior;

        /// <summary>Trial history (oldest first).</summary>
        public IReadOnlyList<TrialRecord> History => history;

        /// <summary>Last computed expected entropy of the selected stimulus.</summary>
        public double LastEntropy { get; private set; } = double.NaN;

        // ---- Internals ----

        private readonly QuestPlusDimension[] stimDomain;
        private readonly QuestPlusDimension[] paramDomain;
        private readonly string[] outcomeDomain;

        private readonly int[] stimShape;
        private readonly int[] paramShape;
        private readonly int stimSize;
        private readonly int paramSize;
        private readonly int outcomeCount;

        // Flat row-major likelihood of shape (stimSize, paramSize, outcomeCount).
        // index = (stim * paramSize + param) * outcomeCount + outcome
        private readonly double[] likelihoods;

        private readonly double[] prior;
        private double[] posterior;

        private readonly List<TrialRecord> history = new List<TrialRecord>();
        private readonly Random rng;

        public int StimSize => stimSize;
        public int ParamSize => paramSize;
        public int OutcomeCount => outcomeCount;

        public QuestPlus(
            IReadOnlyList<QuestPlusDimension> stimDomain,
            IReadOnlyList<QuestPlusDimension> paramDomain,
            IReadOnlyList<string> outcomeDomain,
            IPsychometricFunction psychometricFunction,
            double[] prior = null,
            StimSelectionMethod selectionMethod = StimSelectionMethod.MinEntropy,
            StimSelectionOptions selectionOptions = null,
            ParamEstimationMethod estimationMethod = ParamEstimationMethod.Mean)
        {
            if (stimDomain == null || stimDomain.Count == 0)
            {
                throw new ArgumentException("stimDomain must contain at least one dimension.", nameof(stimDomain));
            }
            if (paramDomain == null || paramDomain.Count == 0)
            {
                throw new ArgumentException("paramDomain must contain at least one dimension.", nameof(paramDomain));
            }
            if (outcomeDomain == null || outcomeDomain.Count < 2)
            {
                throw new ArgumentException("outcomeDomain must contain at least two entries.", nameof(outcomeDomain));
            }
            if (psychometricFunction == null)
            {
                throw new ArgumentNullException(nameof(psychometricFunction));
            }

            if (stimDomain.Count != psychometricFunction.StimDimensionCount)
            {
                throw new ArgumentException(
                    $"Psychometric function expects {psychometricFunction.StimDimensionCount} stimulus dimension(s), but {stimDomain.Count} were provided.",
                    nameof(stimDomain));
            }
            if (paramDomain.Count != psychometricFunction.ParamDimensionCount)
            {
                throw new ArgumentException(
                    $"Psychometric function expects {psychometricFunction.ParamDimensionCount} parameter dimension(s), but {paramDomain.Count} were provided.",
                    nameof(paramDomain));
            }
            if (outcomeDomain.Count != psychometricFunction.OutcomeCount)
            {
                throw new ArgumentException(
                    $"Psychometric function expects {psychometricFunction.OutcomeCount} outcome(s), but {outcomeDomain.Count} were provided.",
                    nameof(outcomeDomain));
            }

            this.stimDomain = stimDomain.ToArray();
            this.paramDomain = paramDomain.ToArray();
            this.outcomeDomain = outcomeDomain.ToArray();
            PsychometricFunction = psychometricFunction;
            SelectionMethod = selectionMethod;
            SelectionOptions = selectionOptions ?? new StimSelectionOptions();
            EstimationMethod = estimationMethod;

            stimShape = this.stimDomain.Select(d => d.Length).ToArray();
            paramShape = this.paramDomain.Select(d => d.Length).ToArray();
            stimSize = ProductOf(stimShape);
            paramSize = ProductOf(paramShape);
            outcomeCount = this.outcomeDomain.Length;

            this.prior = NormalizePrior(prior, paramSize);
            this.posterior = (double[])this.prior.Clone();

            this.likelihoods = BuildLikelihoods();

            if (SelectionOptions.RandomSeed.HasValue)
            {
                rng = new Random(SelectionOptions.RandomSeed.Value);
            }
            else
            {
                rng = new Random();
            }
        }

        private static int ProductOf(int[] shape)
        {
            int p = 1;
            for (int i = 0; i < shape.Length; i++)
            {
                p *= shape[i];
            }
            return p;
        }

        private static double[] NormalizePrior(double[] prior, int expectedSize)
        {
            double[] result;
            if (prior == null)
            {
                result = new double[expectedSize];
                double v = 1.0 / expectedSize;
                for (int i = 0; i < expectedSize; i++)
                {
                    result[i] = v;
                }
                return result;
            }

            if (prior.Length != expectedSize)
            {
                throw new ArgumentException(
                    $"Prior length ({prior.Length}) must equal the product of parameter dimension sizes ({expectedSize}).",
                    nameof(prior));
            }

            double sum = 0;
            for (int i = 0; i < prior.Length; i++)
            {
                if (prior[i] < 0)
                {
                    throw new ArgumentException("Prior values must be non-negative.", nameof(prior));
                }
                sum += prior[i];
            }

            if (sum <= 0)
            {
                throw new ArgumentException("Prior values must sum to a positive number.", nameof(prior));
            }

            result = new double[expectedSize];
            for (int i = 0; i < expectedSize; i++)
            {
                result[i] = prior[i] / sum;
            }
            return result;
        }

        private double[] BuildLikelihoods()
        {
            double[] result = new double[stimSize * paramSize * outcomeCount];
            double[] stimValues = new double[stimShape.Length];
            double[] paramValues = new double[paramShape.Length];
            double[] outcomeProbs = new double[outcomeCount];

            int[] stimIdx = new int[stimShape.Length];
            int[] paramIdx = new int[paramShape.Length];

            for (int s = 0; s < stimSize; s++)
            {
                IndexToCoords(s, stimShape, stimIdx);
                for (int d = 0; d < stimShape.Length; d++)
                {
                    stimValues[d] = stimDomain[d].Values[stimIdx[d]];
                }

                for (int p = 0; p < paramSize; p++)
                {
                    IndexToCoords(p, paramShape, paramIdx);
                    for (int d = 0; d < paramShape.Length; d++)
                    {
                        paramValues[d] = paramDomain[d].Values[paramIdx[d]];
                    }

                    PsychometricFunction.Evaluate(stimValues, paramValues, outcomeProbs);

                    int baseIdx = (s * paramSize + p) * outcomeCount;
                    for (int o = 0; o < outcomeCount; o++)
                    {
                        result[baseIdx + o] = outcomeProbs[o];
                    }
                }
            }
            return result;
        }

        private static void IndexToCoords(int flat, int[] shape, int[] coordsOut)
        {
            for (int d = shape.Length - 1; d >= 0; d--)
            {
                coordsOut[d] = flat % shape[d];
                flat /= shape[d];
            }
        }

        private static int CoordsToIndex(int[] coords, int[] shape)
        {
            int idx = 0;
            for (int d = 0; d < shape.Length; d++)
            {
                idx = idx * shape[d] + coords[d];
            }
            return idx;
        }

        // ---- Stimulus selection ----

        /// <summary>
        /// Compute and return the next stimulus to present. The returned array
        /// is a copy of the chosen stimulus values, in stim domain order.
        /// </summary>
        public double[] GetNextStim() => GetNextStimWithIndex(out _);

        /// <summary>
        /// Compute the next stimulus and also return its flat index in stimulus space.
        /// </summary>
        public double[] GetNextStimWithIndex(out int stimFlatIndex)
        {
            double[] expectedEntropies = ComputeExpectedEntropies();

            switch (SelectionMethod)
            {
                case StimSelectionMethod.MinEntropy:
                    stimFlatIndex = ArgMin(expectedEntropies);
                    LastEntropy = expectedEntropies[stimFlatIndex];
                    return StimValuesAtIndex(stimFlatIndex);

                case StimSelectionMethod.MinNEntropy:
                    stimFlatIndex = SelectMinNEntropy(expectedEntropies);
                    LastEntropy = expectedEntropies[stimFlatIndex];
                    return StimValuesAtIndex(stimFlatIndex);

                default:
                    throw new InvalidOperationException($"Unknown stimulus selection method: {SelectionMethod}");
            }
        }

        private double[] ComputeExpectedEntropies()
        {
            // For each stimulus s:
            //   For each outcome o:
            //     pk[s, o]    = sum_p posterior[p] * likelihoods[s, p, o]
            //     newPost[p]  = posterior[p] * likelihoods[s, p, o] / pk[s, o]
            //     H[s, o]     = -sum_p newPost[p] * log(newPost[p])
            //   EH[s]         = sum_o pk[s, o] * H[s, o]

            double[] eh = new double[stimSize];
            double[] joint = new double[paramSize];   // posterior * likelihood
            double[] pk = new double[outcomeCount];

            for (int s = 0; s < stimSize; s++)
            {
                double ehSum = 0.0;
                int stimBase = s * paramSize * outcomeCount;

                for (int o = 0; o < outcomeCount; o++)
                {
                    double total = 0.0;
                    for (int p = 0; p < paramSize; p++)
                    {
                        double v = posterior[p] * likelihoods[stimBase + p * outcomeCount + o];
                        joint[p] = v;
                        total += v;
                    }
                    pk[o] = total;

                    if (total <= 0.0)
                    {
                        // Outcome is essentially impossible under current posterior;
                        // contributes nothing to expected entropy.
                        continue;
                    }

                    // H = -sum_p (joint/total) * log(joint/total)
                    //   = -(1/total) * (sum_p joint*log(joint)) + log(total)
                    // But cleaner numerically to normalize first.
                    double h = 0.0;
                    for (int p = 0; p < paramSize; p++)
                    {
                        double prob = joint[p] / total;
                        if (prob > 0.0)
                        {
                            h -= prob * Math.Log(prob);
                        }
                    }
                    ehSum += total * h;
                }

                eh[s] = ehSum;
            }

            return eh;
        }

        private static int ArgMin(double[] arr)
        {
            int minIdx = 0;
            double minVal = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] < minVal)
                {
                    minVal = arr[i];
                    minIdx = i;
                }
            }
            return minIdx;
        }

        private int SelectMinNEntropy(double[] expectedEntropies)
        {
            int n = Math.Max(1, SelectionOptions.N);
            n = Math.Min(n, expectedEntropies.Length);

            // Indices of the n smallest entropies (ascending by entropy).
            int[] topIndices = Enumerable.Range(0, expectedEntropies.Length)
                .OrderBy(i => expectedEntropies[i])
                .Take(n)
                .ToArray();

            int maxReps = SelectionOptions.MaxConsecutiveReps;
            int attempts = 0;
            const int maxAttempts = 100;

            while (true)
            {
                int candidate = topIndices[rng.Next(n)];
                attempts++;

                if (maxReps <= 0 || history.Count < maxReps)
                {
                    return candidate;
                }

                // Check whether the last maxReps history entries are all the same stimulus
                // (by flat stim index) AND equal to this candidate.
                bool allSame = true;
                for (int k = 1; k <= maxReps && k <= history.Count; k++)
                {
                    if (history[history.Count - k].StimFlatIndex != candidate)
                    {
                        allSame = false;
                        break;
                    }
                }

                if (!allSame)
                {
                    return candidate;
                }

                if (attempts >= maxAttempts)
                {
                    // Give up — return any candidate. Avoids pathological infinite loops
                    // when all top-N stimuli happen to equal the recent history.
                    return candidate;
                }
            }
        }

        private double[] StimValuesAtIndex(int stimFlatIndex)
        {
            int[] coords = new int[stimShape.Length];
            IndexToCoords(stimFlatIndex, stimShape, coords);
            double[] values = new double[stimShape.Length];
            for (int d = 0; d < stimShape.Length; d++)
            {
                values[d] = stimDomain[d].Values[coords[d]];
            }
            return values;
        }

        // ---- Update ----

        /// <summary>
        /// Submit an observed outcome for the given stimulus values and update
        /// the posterior. The stimulus values must exactly match one of the
        /// sampled grid points in the stimulus domain (within a small tolerance).
        /// </summary>
        public void Update(double[] stim, int outcomeIndex)
        {
            if (stim == null)
            {
                throw new ArgumentNullException(nameof(stim));
            }
            if (stim.Length != stimShape.Length)
            {
                throw new ArgumentException(
                    $"Stimulus must have {stimShape.Length} value(s); got {stim.Length}.",
                    nameof(stim));
            }
            if (outcomeIndex < 0 || outcomeIndex >= outcomeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(outcomeIndex),
                    $"Outcome index {outcomeIndex} is outside [0, {outcomeCount - 1}].");
            }

            int stimFlatIndex = FindStimIndex(stim);
            UpdateByIndex(stimFlatIndex, outcomeIndex);
        }

        /// <summary>
        /// Submit an observed outcome by stimulus flat index. Lower-overhead
        /// variant when the caller already has the index from GetNextStimWithIndex.
        /// </summary>
        public void UpdateByIndex(int stimFlatIndex, int outcomeIndex)
        {
            if (stimFlatIndex < 0 || stimFlatIndex >= stimSize)
            {
                throw new ArgumentOutOfRangeException(nameof(stimFlatIndex));
            }
            if (outcomeIndex < 0 || outcomeIndex >= outcomeCount)
            {
                throw new ArgumentOutOfRangeException(nameof(outcomeIndex));
            }

            int baseIdx = (stimFlatIndex * paramSize) * outcomeCount;
            double total = 0.0;
            for (int p = 0; p < paramSize; p++)
            {
                double v = posterior[p] * likelihoods[baseIdx + p * outcomeCount + outcomeIndex];
                posterior[p] = v;
                total += v;
            }

            if (total <= 0.0)
            {
                // Pathological case: outcome is impossible under the current posterior.
                // Reset to uniform to avoid producing NaNs.
                double uniform = 1.0 / paramSize;
                for (int p = 0; p < paramSize; p++)
                {
                    posterior[p] = uniform;
                }
            }
            else
            {
                for (int p = 0; p < paramSize; p++)
                {
                    posterior[p] /= total;
                }
            }

            history.Add(new TrialRecord(history.Count, stimFlatIndex, StimValuesAtIndex(stimFlatIndex), outcomeIndex));
        }

        private int FindStimIndex(double[] stim)
        {
            int[] coords = new int[stimShape.Length];
            for (int d = 0; d < stimShape.Length; d++)
            {
                int found = -1;
                double[] vals = stimDomain[d].Values;
                for (int i = 0; i < vals.Length; i++)
                {
                    if (Math.Abs(vals[i] - stim[d]) <= 1e-9 * Math.Max(1.0, Math.Abs(vals[i])))
                    {
                        found = i;
                        break;
                    }
                }
                if (found < 0)
                {
                    throw new ArgumentException(
                        $"Stimulus value {stim[d]} for dimension '{stimDomain[d].Name}' is not in the sampled domain.",
                        nameof(stim));
                }
                coords[d] = found;
            }
            return CoordsToIndex(coords, stimShape);
        }

        // ---- Estimation ----

        /// <summary>
        /// Returns the current parameter estimate, one value per parameter dimension
        /// (in the same order as ParamDomain).
        /// </summary>
        public double[] GetParamEstimate()
        {
            switch (EstimationMethod)
            {
                case ParamEstimationMethod.Mean:
                    return GetParamEstimateMean();
                case ParamEstimationMethod.Mode:
                    return GetParamEstimateMode();
                default:
                    throw new InvalidOperationException($"Unknown estimation method: {EstimationMethod}");
            }
        }

        private double[] GetParamEstimateMean()
        {
            // For each param dim, compute marginal then weighted mean.
            double[] estimate = new double[paramShape.Length];
            for (int d = 0; d < paramShape.Length; d++)
            {
                double[] marginal = GetMarginalPosterior(d);
                double mean = 0.0;
                double[] vals = paramDomain[d].Values;
                for (int i = 0; i < marginal.Length; i++)
                {
                    mean += marginal[i] * vals[i];
                }
                estimate[d] = mean;
            }
            return estimate;
        }

        private double[] GetParamEstimateMode()
        {
            int flat = ArgMax(posterior);
            int[] coords = new int[paramShape.Length];
            IndexToCoords(flat, paramShape, coords);
            double[] estimate = new double[paramShape.Length];
            for (int d = 0; d < paramShape.Length; d++)
            {
                estimate[d] = paramDomain[d].Values[coords[d]];
            }
            return estimate;
        }

        private static int ArgMax(double[] arr)
        {
            int maxIdx = 0;
            double maxVal = arr[0];
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i] > maxVal)
                {
                    maxVal = arr[i];
                    maxIdx = i;
                }
            }
            return maxIdx;
        }

        /// <summary>
        /// Returns the marginal posterior PDF for the given parameter dimension.
        /// Length equals the number of samples in that dimension.
        /// </summary>
        public double[] GetMarginalPosterior(int paramDimIndex)
        {
            if (paramDimIndex < 0 || paramDimIndex >= paramShape.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(paramDimIndex));
            }

            int dimLen = paramShape[paramDimIndex];
            double[] marginal = new double[dimLen];
            int[] coords = new int[paramShape.Length];

            for (int p = 0; p < paramSize; p++)
            {
                IndexToCoords(p, paramShape, coords);
                marginal[coords[paramDimIndex]] += posterior[p];
            }

            // Re-normalize defensively (should already sum to 1).
            double sum = 0;
            for (int i = 0; i < dimLen; i++)
            {
                sum += marginal[i];
            }
            if (sum > 0)
            {
                for (int i = 0; i < dimLen; i++)
                {
                    marginal[i] /= sum;
                }
            }

            return marginal;
        }

        /// <summary>
        /// Returns the marginal posterior keyed by parameter name.
        /// </summary>
        public IReadOnlyDictionary<string, double[]> GetMarginalPosteriors()
        {
            Dictionary<string, double[]> result = new Dictionary<string, double[]>();
            for (int d = 0; d < paramShape.Length; d++)
            {
                result[paramDomain[d].Name] = GetMarginalPosterior(d);
            }
            return result;
        }

        // ---- Reset ----

        /// <summary>
        /// Reset the posterior to the prior and clear trial history.
        /// Stimulus/parameter domains and likelihoods are retained.
        /// </summary>
        public void Reset()
        {
            Array.Copy(prior, posterior, paramSize);
            history.Clear();
            LastEntropy = double.NaN;
        }

        // ---- Serialization ----

        /// <summary>
        /// Serialize the run state (history + posterior). Domain and psychometric
        /// function configuration are also captured so the state is self-contained.
        ///
        /// To restore, use DeserializeState on an existing QuestPlus instance configured
        /// with the same domain and psychometric function, or use the static
        /// Restore helper if your application persists the config externally.
        /// </summary>
        public JsonObject SerializeState()
        {
            JsonObject obj = new JsonObject
            {
                ["type"] = "QuestPlus",
                ["version"] = 1,
                ["selectionMethod"] = SelectionMethod.ToString(),
                ["estimationMethod"] = EstimationMethod.ToString(),
                ["psychometricFunctionType"] = PsychometricFunction.TypeId,
                ["psychometricFunctionConfig"] = PsychometricFunction.SerializeConfig(),
                ["stimDomain"] = SerializeDomain(stimDomain),
                ["paramDomain"] = SerializeDomain(paramDomain),
                ["outcomeDomain"] = SerializeStringArray(outcomeDomain),
                ["prior"] = SerializeDoubleArray(prior),
                ["posterior"] = SerializeDoubleArray(posterior),
                ["history"] = SerializeHistory(),
                ["lastEntropy"] = double.IsNaN(LastEntropy) ? JsonValue.Null : LastEntropy,
                ["selectionOptions"] = SerializeSelectionOptions(),
            };
            return obj;
        }

        private static JsonArray SerializeDomain(QuestPlusDimension[] domain)
        {
            JsonArray arr = new JsonArray();
            foreach (QuestPlusDimension d in domain)
            {
                JsonObject o = new JsonObject
                {
                    ["name"] = d.Name,
                    ["values"] = SerializeDoubleArray(d.Values),
                };
                arr.Add(o);
            }
            return arr;
        }

        private static JsonArray SerializeStringArray(string[] arr)
        {
            JsonArray a = new JsonArray();
            foreach (string s in arr)
            {
                a.Add(s);
            }
            return a;
        }

        private static JsonArray SerializeDoubleArray(double[] arr)
        {
            JsonArray a = new JsonArray();
            for (int i = 0; i < arr.Length; i++)
            {
                a.Add(arr[i]);
            }
            return a;
        }

        private JsonArray SerializeHistory()
        {
            JsonArray a = new JsonArray();
            foreach (TrialRecord r in history)
            {
                JsonObject o = new JsonObject
                {
                    ["trial"] = r.TrialIndex,
                    ["stimFlatIndex"] = r.StimFlatIndex,
                    ["stim"] = SerializeDoubleArray(r.Stim),
                    ["outcome"] = r.OutcomeIndex,
                };
                a.Add(o);
            }
            return a;
        }

        private JsonObject SerializeSelectionOptions()
        {
            JsonObject o = new JsonObject
            {
                ["n"] = SelectionOptions.N,
                ["maxConsecutiveReps"] = SelectionOptions.MaxConsecutiveReps,
            };
            if (SelectionOptions.RandomSeed.HasValue)
            {
                o["randomSeed"] = SelectionOptions.RandomSeed.Value;
            }
            return o;
        }

        /// <summary>
        /// Restore posterior and history from a previously serialized state.
        /// The domains, psychometric function, and outcomes must match the
        /// current instance (a mismatch will throw).
        /// </summary>
        public void DeserializeState(JsonObject json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }
            if (json["type"].AsString != "QuestPlus")
            {
                throw new ArgumentException("JSON is not a QuestPlus state object.");
            }
            if (json["version"].AsInteger != 1)
            {
                throw new ArgumentException($"Unsupported QuestPlus state version: {json["version"]}");
            }

            // Validate domain compatibility before mutating any state.
            ValidateDomain(json["stimDomain"].AsJsonArray, stimDomain, "stimDomain");
            ValidateDomain(json["paramDomain"].AsJsonArray, paramDomain, "paramDomain");
            JsonArray outcomes = json["outcomeDomain"].AsJsonArray;
            if (outcomes == null || outcomes.Count != outcomeDomain.Length)
            {
                throw new ArgumentException("outcomeDomain does not match.");
            }
            for (int i = 0; i < outcomeDomain.Length; i++)
            {
                if (outcomes[i].AsString != outcomeDomain[i])
                {
                    throw new ArgumentException($"outcomeDomain mismatch at index {i}.");
                }
            }
            if (json["psychometricFunctionType"].AsString != PsychometricFunction.TypeId)
            {
                throw new ArgumentException(
                    $"Psychometric function type mismatch: state has '{json["psychometricFunctionType"].AsString}', instance has '{PsychometricFunction.TypeId}'.");
            }

            JsonArray priorJson = json["prior"].AsJsonArray;
            if (priorJson == null || priorJson.Count != paramSize)
            {
                throw new ArgumentException("Serialized prior length does not match parameter size.");
            }
            for (int i = 0; i < paramSize; i++)
            {
                prior[i] = priorJson[i].AsNumber;
            }

            JsonArray postJson = json["posterior"].AsJsonArray;
            if (postJson == null || postJson.Count != paramSize)
            {
                throw new ArgumentException("Serialized posterior length does not match parameter size.");
            }
            posterior = new double[paramSize];
            for (int i = 0; i < paramSize; i++)
            {
                posterior[i] = postJson[i].AsNumber;
            }

            history.Clear();
            JsonArray hist = json["history"].AsJsonArray;
            if (hist != null)
            {
                foreach (JsonValue rv in hist)
                {
                    JsonObject ro = rv.AsJsonObject;
                    int trialIdx = ro["trial"].AsInteger;
                    int stimIdx = ro["stimFlatIndex"].AsInteger;
                    JsonArray stimArr = ro["stim"].AsJsonArray;
                    double[] stim = new double[stimArr.Count];
                    for (int i = 0; i < stimArr.Count; i++)
                    {
                        stim[i] = stimArr[i].AsNumber;
                    }
                    int outcome = ro["outcome"].AsInteger;
                    history.Add(new TrialRecord(trialIdx, stimIdx, stim, outcome));
                }
            }

            JsonValue lastEnt = json["lastEntropy"];
            LastEntropy = lastEnt.IsNumber ? lastEnt.AsNumber : double.NaN;
        }

        private static void ValidateDomain(JsonArray domain, QuestPlusDimension[] expected, string fieldName)
        {
            if (domain == null || domain.Count != expected.Length)
            {
                throw new ArgumentException($"{fieldName} does not match expected dimension count.");
            }
            for (int d = 0; d < expected.Length; d++)
            {
                JsonObject dim = domain[d].AsJsonObject;
                if (dim["name"].AsString != expected[d].Name)
                {
                    throw new ArgumentException($"{fieldName}[{d}] name mismatch.");
                }
                JsonArray vals = dim["values"].AsJsonArray;
                if (vals.Count != expected[d].Length)
                {
                    throw new ArgumentException($"{fieldName}[{d}] length mismatch.");
                }
                for (int i = 0; i < vals.Count; i++)
                {
                    double v = vals[i].AsNumber;
                    if (Math.Abs(v - expected[d].Values[i]) > 1e-9 * Math.Max(1.0, Math.Abs(v)))
                    {
                        throw new ArgumentException($"{fieldName}[{d}] value mismatch at index {i}.");
                    }
                }
            }
        }
    }

    /// <summary>Record of one completed QUEST+ trial.</summary>
    public sealed class TrialRecord
    {
        public int TrialIndex { get; }
        public int StimFlatIndex { get; }
        public double[] Stim { get; }
        public int OutcomeIndex { get; }

        public TrialRecord(int trialIndex, int stimFlatIndex, double[] stim, int outcomeIndex)
        {
            TrialIndex = trialIndex;
            StimFlatIndex = stimFlatIndex;
            Stim = stim;
            OutcomeIndex = outcomeIndex;
        }
    }
}
