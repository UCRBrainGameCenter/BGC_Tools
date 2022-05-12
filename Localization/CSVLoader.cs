using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Text.RegularExpressions;
using BGC.IO;

namespace BGC.Localization
{
    public class CSVLoader
    {
        private string csvFile;
        private char lineSeperator = '\n';
        private char surround = '"';
        //private readonly string[] fieldSeperator = { "," };

        public void LoadCSV(string csvName)
        {
            csvFile = File.ReadAllText($"{DataManagement.RootDirectory}/Assets/Resources/{csvName}.csv", Encoding.UTF8);
        }

        public void GetDictionaryValues(string attributeId, Dictionary<string, string> dict)
        {
            string[] lines = csvFile.Split(lineSeperator);

            int attributeIndex = -1;

            string[] headers = lines[0].Split(',');

            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].Contains(attributeId))
                {
                    attributeIndex = i;
                    break;
                }
            }

            Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                string[] fields = CSVParser.Split(line);

                for (int f = 0; f < fields.Length; f++)
                {
                    fields[f] = fields[f].TrimStart(' ', surround);
                    fields[f] = fields[f].TrimEnd(surround);
                    //Debug.Log(fields[f]);
                }

                if (fields.Length > attributeIndex)
                {
                    var key = fields[0];

                    if (dict.ContainsKey(key))
                    {
                        continue;
                    }

                    var value = fields[attributeIndex];

                    dict.Add(key, value);
                }
            }
        }
    }
}