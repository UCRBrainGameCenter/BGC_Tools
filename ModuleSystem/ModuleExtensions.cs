using LightJson;
using System;
using System.Collections.Generic;
using System.Linq;

//Manifest File

//Each file has Tags
//Tag is a Category and a Value

namespace BGC.ModuleSystem
{
    public static class ModuleExtensions
    {
        public static string GetModuleType(this Module module) => module.GetModuleTag("Type").AsString;
        public static JsonValue GetModuleTag(this Module module, string tag) =>
            module.Tags.FirstOrDefault(x => string.Compare(x.Tag, tag) == 0)?.Data ?? JsonValue.Null;


        public static ModuleFile GetFirstFileWithTag(this Module manifest, string tag) =>
            manifest.Files.FirstOrDefault(x => x.ContainsTag(tag));
        public static ModuleFile GetFirstFileWithTagValue(this Module manifest, string tag, JsonValue value) =>
            manifest.Files.FirstOrDefault(x => x.ContainsTagValue(tag, value));

        public static ModuleFile GetFirstFileWithTags(
            this Module manifest,
            IEnumerable<string> tags)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach (string tag in tags)
            {
                files = files.Where(x => x.ContainsTag(tag));
            }

            return files.FirstOrDefault();
        }

        public static ModuleFile GetFirstFileWithTags(
            this Module manifest,
            params string[] tags)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach (string tag in tags)
            {
                files = files.Where(x => x.ContainsTag(tag));
            }

            return files.FirstOrDefault();
        }

        public static ModuleFile GetFirstFileWithTagValues(
            this Module manifest,
            IEnumerable<(string tag, JsonValue value)> tagValues)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach ((string tag, JsonValue value) in tagValues)
            {
                files = files.Where(x => x.ContainsTagValue(tag, value));
            }

            return files.FirstOrDefault();
        }

        public static ModuleFile GetFirstFileWithTagValues(
            this Module manifest,
            params (string tag, JsonValue value)[] tagValues)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach ((string tag, JsonValue value) in tagValues)
            {
                files = files.Where(x => x.ContainsTagValue(tag, value));
            }

            return files.FirstOrDefault();
        }


        public static IEnumerable<ModuleFile> GetAllFilesWithTag(this Module manifest, string tag) =>
            manifest.Files.Where(x => x.ContainsTag(tag)).ToList();
        public static IEnumerable<ModuleFile> GetAllFilesWithTagValue(this Module manifest, string tag, JsonValue value) =>
            manifest.Files.Where(x => x.ContainsTagValue(tag, value)).ToList();

        public static IEnumerable<ModuleFile> GetAllFilesWithTags(
            this Module manifest,
            IEnumerable<string> tags)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach (string tag in tags)
            {
                files = files.Where(x => x.ContainsTag(tag));
            }

            return files.ToList();
        }

        public static IEnumerable<ModuleFile> GetAllFilesWithTags(
            this Module manifest,
            params string[] tags)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach (string tag in tags)
            {
                files = files.Where(x => x.ContainsTag(tag));
            }

            return files.ToList();
        }

        public static IEnumerable<ModuleFile> GetAllFilesWithTagValues(
            this Module manifest,
            IEnumerable<(string tag, JsonValue value)> tagValues)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach ((string tag, JsonValue value) in tagValues)
            {
                files = files.Where(x => x.ContainsTagValue(tag, value));
            }

            return files.ToList();
        }

        public static IEnumerable<ModuleFile> GetAllFilesWithTagValues(
            this Module manifest,
            params (string tag, JsonValue value)[] tagValues)
        {
            IEnumerable<ModuleFile> files = manifest.Files;

            foreach ((string tag, JsonValue value) in tagValues)
            {
                files = files.Where(x => x.ContainsTagValue(tag, value));
            }

            return files.ToList();
        }


        public static ModuleFile GetRandomFileWithTag(
            this Module manifest,
            string tag,
            Random randomizer = null)
        {
            List<ModuleFile> allFilesWithTag = manifest.GetAllFilesWithTag(tag).ToList();

            if (allFilesWithTag.Count == 0)
            {
                return null;
            }

            int index;

            if (randomizer is not null)
            {
                index = randomizer.Next() % allFilesWithTag.Count;
            }
            else
            {
                index = Mathematics.CustomRandom.Next() % allFilesWithTag.Count;
            }

            return allFilesWithTag[index];
        }

        public static ModuleFile GetRandomFileWithTagValue(
            this Module manifest,
            string tag,
            JsonValue value,
            Random randomizer = null)
        {
            List<ModuleFile> allFilesWithTagValue = manifest.GetAllFilesWithTagValue(tag, value).ToList();

            if (allFilesWithTagValue.Count == 0)
            {
                return null;
            }

            int index;

            if (randomizer is not null)
            {
                index = randomizer.Next() % allFilesWithTagValue.Count;
            }
            else
            {
                index = Mathematics.CustomRandom.Next() % allFilesWithTagValue.Count;
            }

            return allFilesWithTagValue[index];
        }

        public static bool ContainsTag(this Module module, string tag) => module.Tags.Any(x => string.Compare(x.Tag, tag, StringComparison.InvariantCulture) == 0);
        public static bool ContainsTagValue(this Module module, string tag, JsonValue value) => module.Tags.Any(x =>
            (string.Compare(x.Tag, tag, StringComparison.InvariantCulture) == 0) &&
            (x.Data == value));

        public static bool ContainsTag(this ModuleFile moduleFile, string tag) => moduleFile.Tags.Any(x => string.Compare(x.Tag, tag, StringComparison.InvariantCulture) == 0);
        public static bool ContainsTagValue(this ModuleFile moduleFile, string tag, JsonValue value) => moduleFile.Tags.Any(x =>
            (string.Compare(x.Tag, tag, StringComparison.InvariantCulture) == 0) &&
            (x.Data == value));
    }
}
