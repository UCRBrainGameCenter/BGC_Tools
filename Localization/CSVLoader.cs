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

        public void LoadCSV(string filepath)
        {
            csvFile = File.ReadAllText(filepath, Encoding.UTF8);
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

                    if (f == fields.Length - 1)
                    {
                        // if last element in row, we have to assume there's a return character at the end
                        // So we need to trim differently. Check if character before the return is a quotation
                        // if so, remove it. The quote is only there from the regex due to parsing commas in a CSV file.
                        
                        int stringLength = fields[f].Length;
                        if (stringLength >= 2 && fields[f][stringLength - 2] == '"')
                        {
                            fields[f] = fields[f].Remove(stringLength - 2, 1);
                        }
                    }
                    else
                    {
                        // if it's not the last element in the row, we can just trim the end without issue.
                        fields[f] = fields[f].TrimEnd(surround);
                    }
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