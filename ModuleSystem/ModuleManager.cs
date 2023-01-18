using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using UnityEngine;
using LightJson;
using LightJson.Serialization;
using BGC.IO;
using BGC.Utility;
using BGC.IO.Compression;

namespace BGC.ModuleSystem
{
    public static class ModuleManager
    {
        private static readonly Dictionary<string, Module> moduleLookup = new Dictionary<string, Module>();

        public static void DeserializeAll()
        {
            moduleLookup.Clear();

            string moduleSource = DataManagement.PathForDataDirectory("BGCModules");

            //Iterate over modules
            foreach (string moduleDirectory in Directory.GetDirectories(moduleSource))
            {
                foreach (string moduleVersionDirectory in Directory.GetDirectories(moduleDirectory))
                {
                    string manifestPath = Path.Combine(moduleVersionDirectory, "manifest.json");

                    if (ApplicationVersion.TryParse(
                        s: Path.GetFileName(moduleVersionDirectory),
                        out ApplicationVersion version) &&
                        File.Exists(manifestPath))
                    {

                        //Potential Target
                        DirectoryModule module = null;
                        try
                        {
                            JsonObject data = JsonReader.ParseFile(manifestPath).AsJsonObject;

                            module = new DirectoryModule(
                                data: data,
                                modulePath: moduleVersionDirectory);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Unable to parse Module Manifest file: {manifestPath}: {ex.Message}");
                            continue;
                        }

                        if (module.Version != version)
                        {
                            Debug.LogError($"Mismatched version for module {module.Name}. Expected {version}, found {module.Version}");
                            continue;
                        }

                        if (moduleLookup.ContainsKey(module.Name))
                        {
                            if (moduleLookup[module.Name].Version > version)
                            {
                                //Skipping older module
                                return;
                            }
                        }

                        moduleLookup[module.Name] = module;
                    }
                }
            }
        }

        public static Task DownloadModule(string moduleName) =>
            DownloadModule(moduleName, ApplicationVersion.NullVersion);

        public static async Task DownloadModule(string moduleName, ApplicationVersion version)
        {
            const string BASE_ADDRESS = "https://bgcgamefiles.s3.us-east-2.amazonaws.com/PART/BGCModules";
            string assetName;
            string outputDirectory;

            if (version == ApplicationVersion.NullVersion)
            {
                version = "1.0.0";
            }

            if (version != "1.0.0")
            {
                throw new NotSupportedException($"Proper versioning not supported yet. Needs support for reading manifest");
            }

            switch (moduleName)
            {
                case "crm_en":
                    assetName = "crm_en_v1.0.0";
                    outputDirectory = DataManagement.PathForDataSubDirectory("BGCModules", "crm_en", "1.0.0");
                    break;

                case "crm_sp":
                    assetName = "crm_sp_v1.0.0";
                    outputDirectory = DataManagement.PathForDataSubDirectory("BGCModules", "crm_sp", "1.0.0");
                    break;

                default:
                    throw new Exception($"Unexpected Modulename: {moduleName}");
            }

            string archiveName = Path.Combine(DataManagement.PathForDataDirectory("BGCModules"), $"{assetName}.zip");

            if (File.Exists(archiveName))
            {
                File.Delete(archiveName);
            }

            if (!Directory.Exists(outputDirectory))
            {
                //Create missing directory
                Directory.CreateDirectory(outputDirectory);
            }
            else if (Directory.GetFiles(outputDirectory).Length > 0)
            {
                //Clear out directory and recreate
                Directory.Delete(outputDirectory, true);
                Directory.CreateDirectory(outputDirectory);
            }

            Uri fileUri = new Uri($"{BASE_ADDRESS}/{assetName}.zip");

            try
            {
                //Download the tracks
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(fileUri))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        using (FileStream fileStream = new FileStream(archiveName, FileMode.CreateNew))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        throw new WebException($"Failed to retrieve resource: {response}");
                    }
                }

                if (!File.Exists(archiveName))
                {
                    throw new FileNotFoundException($"Downloaded Archive does not exist", archiveName);
                }

                bool success = Zip.DecompressFile(
                    inputFilePath: archiveName,
                    outputPath: outputDirectory);

                if (!success)
                {
                    throw new Exception($"Unable to unarchive asset: {archiveName}");
                }

                File.Delete(archiveName);
            }
            catch (Exception e)
            {
                //Try to clean up the mess

                if (File.Exists(archiveName))
                {
                    try
                    {
                        File.Delete(archiveName);
                    }
                    catch (Exception fileDeleteExcp)
                    {
                        Debug.LogException(fileDeleteExcp);
                    }
                }

                if (Directory.Exists(outputDirectory))
                {
                    try
                    {
                        Directory.Delete(outputDirectory, true);
                    }
                    catch (Exception dirDeleteExcp)
                    {
                        Debug.LogException(dirDeleteExcp);
                    }
                }

                throw new Exception("Failed to acquire SpatialRelease stimuli", e);
            }
        }

        public static IEnumerable<string> GetModules() => moduleLookup.Keys;
        public static Module GetModule(string moduleName) => moduleLookup.GetValueOrDefault(moduleName);
        public static IEnumerable<Module> GetModulesOfType(string type) =>
            moduleLookup.Values.Where(x => string.Compare(x.GetModuleType(), EncodeStrings(type), StringComparison.InvariantCulture) == 0);

        public static IEnumerable<Module> GetModulesWhere(Func<Module, bool> test) =>
            moduleLookup.Values.Where(test);

        public static string EncodeStrings(string stringValue) => new JsonValue(stringValue).ToString(false);
    }
}
