using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

namespace BGC.Localization
{
    public class CSVLoader
    {
        private TextAsset csvFile;
        private char lineSeperator = '\n';
        private char surround = '"';
        //private readonly string[] fieldSeperator = { "," };

        public void LoadCSV(string csvName)
        {
            csvFile = Resources.Load<TextAsset>(csvName);
        }

        public void GetDictionaryValues(string attributeId, Dictionary<string, string> dict)
        {
            string[] lines = csvFile.text.Split(lineSeperator);

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