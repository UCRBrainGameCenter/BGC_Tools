using System.IO;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace BGC.Utility
{
    /// <summary>
    /// A Decorator-like helper container of BGCRemappers
    /// </summary>
    public class BGCRemapHelper
    {
        public string logName;
        private List<BGCRemapper> remappers = new List<BGCRemapper>();

        public BGCRemapHelper(string filePath)
        {
            logName = Path.GetFileNameWithoutExtension(filePath);
        }

        public void Add(BGCRemapper remapper)
        {
            Assert.IsNotNull(remapper);
            remappers.Add(remapper);
        }

        public bool IsEmpty() => remappers.Count == 0;

        public void Apply(string[] columnLabels, string[] data)
        {
            foreach (BGCRemapper remapper in remappers)
            {
                remapper.Invoke(logName, columnLabels, data);
            }
        }
    }
}