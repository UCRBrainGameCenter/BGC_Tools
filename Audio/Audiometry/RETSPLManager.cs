using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using BGC.IO;

namespace BGC.Audio.Audiometry
{
    public static class RETSPLManager
    {
        private const string RETSPL_DIR = "RETSPLs";

        private static readonly Dictionary<string, TransducerProfile> retspls =
            new Dictionary<string, TransducerProfile>();

        public static TransducerProfile GetRETSPL(string name) =>
            retspls.ContainsKey(name) ? retspls[name] : null;
        public static IEnumerable<string> GetRETSPLNames() =>
            retspls.Values.Select(x => x.Name);

        public static void DeserializeAll()
        {
            retspls.Clear();

            foreach (string filePath in DataManagement.GetDataFiles(RETSPL_DIR, "*.json"))
            {
                DeserializeFile(filePath);
            }

            if (!retspls.ContainsKey("DD450"))
            {
                CreateDD450();
            }
        }

        private static void DeserializeFile(string filePath)
        {
            FileReader.ReadJsonFile(
                path: filePath,
                successCallback: retsplData =>
                {
                    TransducerProfile retsplContainer = new TransducerProfile(retsplData);

                    if (retspls.ContainsKey(retsplContainer.Name))
                    {
                        string originalName = retsplContainer.Name;
                        int modifier = 1;

                        while (retspls.ContainsKey($"{originalName} ({modifier})"))
                        {
                            modifier++;
                        }

                        UnityEngine.Debug.LogError($"RETSPL named {retsplContainer.Name} already loaded.  Moving to {$"{originalName} ({modifier})"}.");
                        retsplContainer.Name = $"{originalName} ({modifier})";
                    }

                    //Correct Possible collisions or nonstandard naming
                    string correctFilePath = retsplContainer.GetDesiredFilePath();
                    if (filePath != correctFilePath && !System.IO.File.Exists(correctFilePath))
                    {
                        UnityEngine.Debug.LogWarning($"RETSPL Name/Path Mismatch, corrected: \"{filePath}\" to \"{correctFilePath}\"");
                        System.IO.File.Move(filePath, correctFilePath);
                        filePath = correctFilePath;
                    }

                    retsplContainer.filePath = filePath;

                    retspls.Add(retsplContainer.Name, retsplContainer);
                });
        }

        /// <summary>
        /// Returns the status of the attempted addition.
        /// </summary>
        public static SaveDataResult AddRETSPL(
            TransducerProfile retspl,
            bool replace = false)
        {
            if (retspls.ContainsKey(retspl.Name))
            {
                TransducerProfile oldRETSPL = retspls[retspl.Name];

                if (!replace)
                {
                    //Need replace permission
                    return SaveDataResult.OverwriteQuery;
                }

                //Replacing an existing RETSPL
                retspl.filePath = oldRETSPL.filePath;
                retspls[retspl.Name] = retspl;

                WriteRETSPL(retspl);

                return SaveDataResult.Overwritten;
            }

            retspl.filePath = FilePath.NextAvailableFilePath(retspl.GetDesiredFilePath());
            retspls.Add(retspl.Name, retspl);

            WriteRETSPL(retspl);

            return SaveDataResult.SavedNew;
        }

        private static void WriteRETSPL(TransducerProfile retspl)
        {
            FileWriter.WriteJson(
                path: retspl.filePath,
                createJson: retspl.Serialize,
                pretty: false);
        }

        private static string GetDesiredFilePath(this TransducerProfile retspl) =>
            DataManagement.PathForDataFile(
                dataDirectory: RETSPL_DIR,
                fileName: FilePath.SanitizeForFilename(
                    name: $"{retspl.Name}.json",
                    additionalExclusion: ' ',
                    fallback: "retspl.json"));
        
        /// <summary>
        /// Default RETSPLs for DD450 RadioEar Transducers
        /// </summary>
        private static void CreateDD450()
        {
            List<RETSPLEntry> dd450Values = new List<RETSPLEntry>()
            {
                new RETSPLEntry(125, 30.5),
                new RETSPLEntry(160, 26.0),
                new RETSPLEntry(200, 22.0),
                new RETSPLEntry(250, 18.0),
                new RETSPLEntry(315, 15.5),
                new RETSPLEntry(400, 13.5),
                new RETSPLEntry(500, 11.0),
                new RETSPLEntry(630, 8.0),
                new RETSPLEntry(750, 6.0),
                new RETSPLEntry(800, 6.0),
                new RETSPLEntry(1_000, 5.5),
                new RETSPLEntry(1_250, 6.0),
                new RETSPLEntry(1_500, 5.5),
                new RETSPLEntry(1_600, 5.5),
                new RETSPLEntry(2_000, 4.5),
                new RETSPLEntry(2_500, 3.0),
                new RETSPLEntry(3_000, 2.5),
                new RETSPLEntry(3_150, 4.0),
                new RETSPLEntry(4_000, 9.5),
                new RETSPLEntry(5_000, 14.0),
                new RETSPLEntry(6_000, 17.0),
                new RETSPLEntry(6_300, 17.5),
                new RETSPLEntry(8_000, 17.5),
                new RETSPLEntry(9_000, 19.0),
                new RETSPLEntry(10_000, 22.0),
                new RETSPLEntry(11_200, 23.0),
                new RETSPLEntry(12_500, 27.5),
                new RETSPLEntry(14_000, 35.0),
                new RETSPLEntry(16_000, 56.0)
            };

            AddRETSPL(new TransducerProfile("DD450", dd450Values), false);
        }
    }
}
