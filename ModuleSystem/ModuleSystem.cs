using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LightJson;
using UnityEngine;

//Manifest File

//Each file has Tags
//Tag is a Category and a Value

//What Tags do we support?
//String
//Int
//List of Strings
//List of Ints
//Boolean
namespace BGC.ModuleSystem
{
    public abstract class Module : IEnumerable<ModuleFile>
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Utility.ApplicationVersion Version { get; set; } = "1.0.0";

        public List<ModuleTag> Tags { get; set; } = new List<ModuleTag>();
        public List<ModuleFile> Files { get; set; } = new List<ModuleFile>();

        public Module() { }

        public Module(
            string name,
            string description,
            string version,
            IEnumerable<ModuleTag> tags,
            IEnumerable<ModuleFile> files)
        {
            Name = name;
            Description = description;
            Version = version;

            Tags.AddRange(tags);
            Files.AddRange(files);
        }

        public Module(JsonObject data)
        {
            Name = data[Keys.Name];
            Description = data[Keys.Description];
            Version = data[Keys.Version].AsString;

            foreach (KeyValuePair<string, JsonValue> tag in data[Keys.Tags].AsJsonObject)
            {
                Tags.Add(new ModuleTag(tag.Key, tag.Value));
            }

            foreach (JsonValue file in data[Keys.Files].AsJsonArray)
            {
                Files.Add(new ModuleFile(file.AsJsonObject));
            }
        }

        public abstract string GetFullPath(ModuleFile file);
        public abstract byte[] GetFileData(ModuleFile file);
        public abstract Task<byte[]> GetFileDataAsync(ModuleFile file);

        public void Add(ModuleFile file) => Files.Add(file);

        public void Add(string tag, JsonValue value) => Tags.Add(new ModuleTag(tag, value));
        public void Add(string tag, JsonArray values) => Tags.Add(new ModuleTag(tag, values));

        public void Add(string tag, IEnumerable<JsonValue> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values)));
        public void Add(string tag, IEnumerable<JsonObject> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<double> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<bool> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<int> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<string> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));


        public override string ToString() =>
            $"[{nameof(Module)} \"{Name}\" Version {Version} - {Tags.Count} Tags - {Files.Count} Files]";

        #region Serialization

        private static class Keys
        {
            public const string Name = "Name";
            public const string Description = "Description";
            public const string Version = "Version";
            public const string Tags = "Tags";
            public const string Files = "Files";
        }

        public JsonObject Serialize()
        {
            JsonObject jsonTagData = new JsonObject();

            foreach (ModuleTag tag in Tags)
            {
                tag.SerializeInto(jsonTagData);
            }

            JsonArray jsonFileData = new JsonArray();

            foreach (ModuleFile file in Files)
            {
                jsonFileData.Add(file.Serialize());
            }

            return new JsonObject()
            {
                { Keys.Name, Name },
                { Keys.Description, Description },
                { Keys.Version, Version.ToString() },
                { Keys.Tags, jsonTagData },
                { Keys.Files, jsonFileData }
            };
        }

        #endregion Serialization
        #region IEnumerator

        public IEnumerator<ModuleFile> GetEnumerator() => Files.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Files.GetEnumerator();

        #endregion IEnumerator
    }

    public class DirectoryModule : Module
    {
        public readonly string modulePath = "";

        public DirectoryModule(
            string name,
            string description,
            string version,
            IEnumerable<ModuleTag> tags,
            IEnumerable<ModuleFile> files,
            string modulePath)
            : base(name, description, version, tags, files)
        {
            this.modulePath = modulePath;
        }

        public DirectoryModule(
            string name,
            string description,
            string version)
            : base(name, description, version, Array.Empty<ModuleTag>(), Array.Empty<ModuleFile>())
        {
            modulePath = "";
        }

        public DirectoryModule(
            JsonObject data,
            string modulePath)
            : base(data)
        {
            this.modulePath = modulePath;
        }

        public override string GetFullPath(ModuleFile file) =>
            System.IO.Path.Combine(modulePath, file.Filepath);

        public override byte[] GetFileData(ModuleFile file) =>
            System.IO.File.ReadAllBytes(System.IO.Path.Combine(modulePath, file.Filepath));
        public override Task<byte[]> GetFileDataAsync(ModuleFile file) =>
            System.IO.File.ReadAllBytesAsync(System.IO.Path.Combine(modulePath, file.Filepath));
    }


    public class ModuleFile : IEnumerable<ModuleTag>
    {
        public string Filepath { get; set; } = "";
        public string Description { get; set; } = "";

        public List<ModuleTag> Tags { get; set; } = new List<ModuleTag>();

        public ModuleFile() { }

        public ModuleFile(
            string filepath,
            string description,
            IEnumerable<ModuleTag> tags = null)
        {
            Filepath = filepath ?? throw new Exception($"{nameof(ModuleFile)} must have a non-null filepath");
            Description = description ?? "";

            if (tags is not null)
            {
                Tags.AddRange(tags);
            }
        }

        public ModuleFile(JsonObject data)
        {
            Filepath = data[Keys.Filename];
            Description = data[Keys.Description];

            JsonObject tagData = data[Keys.Tags];

            Tags = new List<ModuleTag>();
            foreach (KeyValuePair<string, JsonValue> tag in tagData)
            {
                Tags.Add(new ModuleTag(tag.Key, tag.Value));
            }
        }

        public string GetFullPath(Module parentModule) => parentModule.GetFullPath(this);

        public byte[] GetData(Module parentModule) => parentModule.GetFileData(this);
        public Task<byte[]> GetDataAsync(Module parentModule) => parentModule.GetFileDataAsync(this);

        public void Add(string tag, JsonValue value) => Tags.Add(new ModuleTag(tag, value));
        public void Add(string tag, JsonArray values) => Tags.Add(new ModuleTag(tag, values));

        public void Add(string tag, IEnumerable<JsonValue> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values)));
        public void Add(string tag, IEnumerable<JsonObject> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<double> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<bool> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<int> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));
        public void Add(string tag, IEnumerable<string> values) => Tags.Add(new ModuleTag(tag, new JsonArray(values.Select(x => new JsonValue(x)))));



        public override string ToString() => $"[{nameof(ModuleFile)} \"{Filepath}\" Description \"{Description}\" - {Tags.Count} Tags]";


        #region Serialization

        private static class Keys
        {
            public const string Filename = "Filename";
            public const string Description = "Description";
            public const string Tags = "Tags";
        }

        public JsonObject Serialize()
        {
            JsonObject jsonTagData = new JsonObject();

            foreach (ModuleTag tag in Tags)
            {
                tag.SerializeInto(jsonTagData);
            }

            return new JsonObject()
            {
                { Keys.Filename, Filepath },
                { Keys.Description, Description },
                { Keys.Tags, jsonTagData }
            };
        }

        #endregion Serialization
        #region IEnumerator

        public IEnumerator<ModuleTag> GetEnumerator() => Tags.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Tags.GetEnumerator();

        #endregion IEnumerator
    }

    public class ModuleTag
    {
        public string Tag { get; set; } = "";
        public JsonValue Data { get; }

        public (string, JsonValue) AsTuple => (Tag, Data);

        public ModuleTag() { }

        public ModuleTag(string tag, JsonValue data)
        {
            Tag = tag ?? throw new Exception($"{nameof(ModuleTag)} cannot have null tag value");
            Data = data;
        }

        public override string ToString() => $"[{nameof(ModuleTag)} \"{Tag}\" - \"{Data}\"]";

        public static ModuleTag Deserialize(string tag, JsonValue value) => new ModuleTag(tag, value);

        #region Serialization

        public JsonObject SerializeInto(JsonObject tagData) => tagData.Add(Tag, Data);

        #endregion Serialization
    }
}
