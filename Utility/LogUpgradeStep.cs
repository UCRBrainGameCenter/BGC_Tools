using System.IO;
using UnityEngine;
using BGC.IO.Logging;
using LightJson;

namespace BGC.Utility
{
    /// <summary>
    /// (Potentially) modifies the <paramref name="metaData"/> argument.
    /// </summary>
    /// <remarks>Intended to fix critical versioning issues.</remarks>
    public delegate void BGCMetaDataEditor(string logName, JsonObject metaData);

    /// <summary>
    /// Uses the column label and data to determine if it should override default BGC Parsing.
    /// </summary>
    /// <returns>Whether this method overrode the standard parsing.</returns>
    /// <remarks>Intended to fix critical versioning issues.</remarks>
    public delegate void BGCRemapper(string logName, string[] columnLabels, string[] data);

    /// <summary>
    /// A class meant to perform maintenance operations on logs
    /// </summary>
    public abstract class LogUpgradeStep
    {
        private readonly ApplicationVersion outputVersion;

        private readonly BGCMetaDataEditor metaDataEditor = null;
        private readonly BGCRemapper remapper = null;

        public LogUpgradeStep(
            ApplicationVersion outputVersion,
            BGCMetaDataEditor metaDataEditor,
            BGCRemapper remapper)
        {
            this.outputVersion = outputVersion;
            this.metaDataEditor = metaDataEditor;
            this.remapper = remapper;
        }

        public void TestAndApply(
            JsonObject metaData,
            BGCRemapHelper remapperHelper)
        {
            if (MatchesConstraints(metaData[LoggingKeys.Version].AsString))
            {
                metaDataEditor?.Invoke(
                    logName: remapperHelper.logName,
                    metaData: metaData);

                if (outputVersion.IsNull() == false)
                {
                    metaData[LoggingKeys.Version] = outputVersion.ToString();
                }

                if (remapper != null)
                {
                    remapperHelper.Add(remapper);
                }
            }
        }

        protected abstract bool MatchesConstraints(ApplicationVersion version);
    }

    /// <summary>
    /// Apply the associated remappers if the ApplicationVersion is in the described range
    /// </summary>
    public class LogUpgradeRangeStep : LogUpgradeStep
    {
        private readonly ApplicationVersion lowestVersion;
        private readonly ApplicationVersion highestVersion;

        public LogUpgradeRangeStep(
            string lowestVersion,
            string highestVersion,
            string outputVersion = "",
            BGCMetaDataEditor metaDataEditor = null,
            BGCRemapper remapper = null)
            : base(outputVersion, metaDataEditor, remapper)
        {
            this.lowestVersion = ApplicationVersion.BuildFromWild(lowestVersion, false);
            this.highestVersion = ApplicationVersion.BuildFromWild(highestVersion, true);
        }

        protected override bool MatchesConstraints(ApplicationVersion version) =>
            version.Between(lowestVersion, highestVersion);
    }

    /// <summary>
    /// Apply the associated remappers if the ApplicationVersion matches the described pattern
    /// </summary>
    public class LogUpgradePatternStep : LogUpgradeStep
    {
        private readonly string upgradePattern;

        public LogUpgradePatternStep(
            string upgradePattern,
            string outputVersion = "",
            BGCMetaDataEditor metaDataEditor = null,
            BGCRemapper remapper = null)
            : base(outputVersion, metaDataEditor, remapper)
        {
            this.upgradePattern = upgradePattern;
        }

        protected override bool MatchesConstraints(ApplicationVersion version) =>
            version.MatchesPattern(upgradePattern);
    }
}