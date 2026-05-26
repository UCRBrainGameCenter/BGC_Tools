using System.Collections.Generic;
using LightJson;

namespace BGC.Mathematics.QuestPlus
{
    /// <summary>
    /// A parametric psychometric function for QUEST+.
    ///
    /// For a given stimulus and parameter set, returns the probability of each
    /// possible outcome. The returned array must have length equal to the
    /// outcome count and sum to 1.
    /// </summary>
    public interface IPsychometricFunction
    {
        /// <summary>
        /// Canonical type identifier used for serialization (e.g. "weibull").
        /// </summary>
        string TypeId { get; }

        /// <summary>
        /// Number of stimulus dimensions the function consumes.
        /// </summary>
        int StimDimensionCount { get; }

        /// <summary>
        /// Number of parameter dimensions the function consumes.
        /// </summary>
        int ParamDimensionCount { get; }

        /// <summary>
        /// Number of outcomes the function returns probabilities for (typically 2).
        /// </summary>
        int OutcomeCount { get; }

        /// <summary>
        /// Returns the probabilities of each outcome at the given stimulus and
        /// parameter values. The output array must have length OutcomeCount and
        /// sum to 1. The caller will reuse the returned array; implementations
        /// should allocate fresh memory or use the supplied destination.
        /// </summary>
        /// <param name="stim">Stimulus value(s); length == StimDimensionCount.</param>
        /// <param name="paramVals">Parameter value(s); length == ParamDimensionCount.</param>
        /// <param name="destination">If non-null, results are written here; must be length OutcomeCount.</param>
        /// <returns>The destination array (or a freshly allocated one if destination is null).</returns>
        double[] Evaluate(double[] stim, double[] paramVals, double[] destination = null);

        /// <summary>
        /// Stable, canonical dimension ordering hint. The names are not used by
        /// the engine, but provided for diagnostics, plotting, and JSON output.
        /// </summary>
        IReadOnlyList<string> StimDimensionNames { get; }

        /// <summary>
        /// Stable, canonical parameter ordering hint. The names are not used by
        /// the engine, but provided for diagnostics, plotting, and JSON output.
        /// </summary>
        IReadOnlyList<string> ParamDimensionNames { get; }

        /// <summary>
        /// Serialize configuration parameters of this psychometric function.
        /// Domain values (sample points, prior, etc.) are NOT serialized here;
        /// those belong to the QUEST+ engine.
        /// </summary>
        JsonObject SerializeConfig();
    }
}
