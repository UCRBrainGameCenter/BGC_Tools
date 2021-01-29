using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using BGC.IO;

namespace BGC.Reports
{
    public class DataFile
    {
        private static string dataFileDirectory = null;

        private static string DataFileDirectory => dataFileDirectory ?? (dataFileDirectory = DataManagement.PathForDataDirectory("DataFiles"));
        private static string TimeStamp => DateTime.Now.ToString("yy_MM_dd_HH_mm_ss");

        private const string ANON_FIELD_NAME = "AnonField";

        public string Filename { get; private set; }
        public string FieldDelimiter { get; }
        public string RecordDelimiter { get; }

        public int TotalRecordCount { get; private set; } = 0;
        public int CurrentRecordNumber { get; private set; } = 0;

        public readonly List<string> fieldNames = new List<string>();
        public readonly Dictionary<string, List<string>> values = new Dictionary<string, List<string>>();

        private string filepath;

        public DataFile()
            : this(
                filename: TimeStamp,
                fieldNames: null,
                fieldDelimiter: ",",
                recordDelimiter: "\n",
                append: true)
        {
        }

        public DataFile(string filename)
            : this(
                filename: filename,
                fieldNames: null,
                fieldDelimiter: ",",
                recordDelimiter: "\n",
                append: true)
        {
        }

        public DataFile(
            string filename,
            IEnumerable<string> fieldNames,
            string fieldDelimiter,
            string recordDelimiter,
            bool append)
        {
            if (string.IsNullOrEmpty(fieldDelimiter))
            {
                throw new ArgumentException($"fieldDelimiter cannot be empty or null", nameof(fieldDelimiter));
            }

            if (string.IsNullOrEmpty(recordDelimiter))
            {
                throw new ArgumentException($"recordDelimiter cannot be empty or null", nameof(recordDelimiter));
            }

            if (fieldDelimiter == recordDelimiter)
            {
                throw new ArgumentException($"recordDelimiter cannot be equal to the fieldDelimiter", nameof(recordDelimiter));
            }

            FieldDelimiter = fieldDelimiter;
            RecordDelimiter = recordDelimiter;

            Filename = PrepareFileName(filename);
            filepath = Path.Combine(DataFileDirectory, Filename);

            if (File.Exists(filepath) && append)
            {
                //
                //Parse
                //

                string[] recordDelimSplitter = new string[] { recordDelimiter };
                string[] fieldDelimSplitter = new string[] { fieldDelimiter };

                string[] records = File.ReadAllText(filepath).Split(recordDelimSplitter, StringSplitOptions.RemoveEmptyEntries);

                int anonymousFieldCount = 0;

                //Parse Header
                if (records.Length > 0)
                {
                    foreach (string fieldName in records[0].Split(fieldDelimSplitter, StringSplitOptions.None))
                    {
                        this.fieldNames.Add(fieldName);
                        values.Add(fieldName, new List<string>());

                        //Handle tracking existing AnonFields
                        if (fieldName.StartsWith(ANON_FIELD_NAME) && fieldName.Length > ANON_FIELD_NAME.Length)
                        {
                            string remainderString = fieldName.Substring(ANON_FIELD_NAME.Length);

                            if (int.TryParse(remainderString, out int anonNumber))
                            {
                                anonymousFieldCount = Math.Max(anonNumber, anonymousFieldCount);
                            }
                        }
                    }
                }

                //Parse Records
                for (int recordNum = 0; recordNum < records.Length - 1; recordNum++)
                {
                    string[] fields = records[recordNum + 1].Split(fieldDelimSplitter, StringSplitOptions.None);

                    //Handle the creation of new AnonFields
                    while (fields.Length > this.fieldNames.Count)
                    {
                        string newFieldName = $"{ANON_FIELD_NAME}{++anonymousFieldCount}";
                        this.fieldNames.Add(newFieldName);
                        values.Add(newFieldName, new List<string>());
                    }

                    //Copy fields
                    for (int fieldNum = 0; fieldNum < fields.Length; fieldNum++)
                    {
                        List<string> valueList = values[this.fieldNames[fieldNum]];

                        while (valueList.Count < recordNum)
                        {
                            valueList.Add("");
                        }

                        valueList.Add(fields[fieldNum]);
                    }
                }

                TotalRecordCount = Math.Max(0, records.Length - 1);
                CurrentRecordNumber = TotalRecordCount;
            }

            if (fieldNames != null)
            {
                foreach (string fieldName in fieldNames)
                {
                    if (!this.fieldNames.Contains(fieldName))
                    {
                        this.fieldNames.Add(fieldName);
                        values.Add(fieldName, new List<string>());
                    }
                }
            }
        }

        public int FieldCount => fieldNames.Count;

        public void UpdateFileName(string filename)
        {
            Filename = PrepareFileName(filename);
            filepath = Path.Combine(DataFileDirectory, Filename);
        }

        private static string PrepareFileName(string filename)
        {
            filename = filename.Replace("%T", TimeStamp);
            filename = FilePath.SanitizeForFilename(filename, fallback: "NewDataFile");

            if (!filename.ToLower().EndsWith(".csv"))
            {
                filename = $"{filename}.csv";
            }

            return filename;
        }

        /// <summary>
        /// Gets fieldName by fieldNum
        /// </summary>
        public string GetFieldName(int fieldNum)
        {
            if (fieldNames.Count <= fieldNum)
            {
                //Cannot get a field that doesn't exist
                return null;
            }

            return fieldNames[fieldNum];
        }

        /// <summary>
        /// Adds a field to the DataFile. Returns success.
        /// </summary>
        public bool AddField(string fieldName)
        {
            if (fieldName.Contains(FieldDelimiter) || fieldName.Contains(RecordDelimiter))
            {
                //Cannot add because it contains a delimiter
                return false;
            }

            if (fieldNames.Contains(fieldName))
            {
                //Cannot add because it already exists
                return false;
            }

            fieldNames.Add(fieldName);
            values.Add(fieldName, new List<string>());

            return true;
        }

        /// <summary>
        /// Adds all fields to the DataFile. Returns success.  All or nothing.
        /// </summary>
        public bool AddFields(IEnumerable<string> fieldNames)
        {
            if (fieldNames.Any(x => x.Contains(FieldDelimiter) || x.Contains(RecordDelimiter)))
            {
                //Cannot add because one contains a delimiter
                return false;
            }

            if (fieldNames.Any(this.fieldNames.Contains))
            {
                //Cannot add because one already exists
                return false;
            }

            foreach (string fieldName in fieldNames)
            {
                this.fieldNames.Add(fieldName);
                values.Add(fieldName, new List<string>());
            }

            return true;
        }

        /// <summary>
        /// Removes a field from the DataFile. Returns success.
        /// </summary>
        public bool RemoveField(string fieldName)
        {
            bool success = fieldNames.Remove(fieldName);
            values.Remove(fieldName);

            return success;
        }

        /// <summary>
        /// Removes fields from the DataFile. Returns success.
        /// </summary>
        public bool RemoveFields(IEnumerable<string> fieldNames)
        {
            if (!fieldNames.All(this.fieldNames.Contains))
            {
                return false;
            }

            foreach (string fieldName in fieldNames)
            {
                this.fieldNames.Remove(fieldName);
                values.Remove(fieldName);
            }

            return true;
        }

        /// <summary>
        /// Add a value to the current record, by fieldName. Returns success.
        /// </summary>
        public bool AddValue(string fieldName, string value)
        {
            if (!fieldNames.Contains(fieldName) && !AddField(fieldName))
            {
                //Cannot add data that doesn't exist
                return false;
            }

            List<string> entries = values[fieldName];

            //Add any missing entries
            for (int i = entries.Count; i < CurrentRecordNumber; i++)
            {
                entries.Add("");
            }

            if (entries.Count == CurrentRecordNumber)
            {
                entries.Add(value);
            }
            else
            {
                entries[CurrentRecordNumber] = value;
            }

            if (CurrentRecordNumber >= TotalRecordCount)
            {
                TotalRecordCount = CurrentRecordNumber + 1;
            }

            return true;
        }

        /// <summary>
        /// Add a value to the current record, by fieldNumber. Returns success.
        /// </summary>
        public bool AddValue(int fieldNum, string value)
        {
            if (fieldNames.Count <= fieldNum)
            {
                //Cannot add data that doesn't exist
                return false;
            }

            List<string> entries = values[fieldNames[fieldNum]];

            //Add any missing entries
            for (int i = entries.Count; i < CurrentRecordNumber; i++)
            {
                entries.Add("");
            }

            if (entries.Count == CurrentRecordNumber)
            {
                entries.Add(value);
            }
            else
            {
                entries[CurrentRecordNumber] = value;
            }

            if (CurrentRecordNumber >= TotalRecordCount)
            {
                TotalRecordCount = CurrentRecordNumber + 1;
            }

            return true;
        }

        //Get Value for current record number by fieldNum
        public string GetValue(string fieldName)
        {
            if (!fieldNames.Contains(fieldName))
            {
                //Cannot get a field that doesn't exist
                return "";
            }

            List<string> valueList = values[fieldName];

            if (CurrentRecordNumber >= valueList.Count)
            {
                //No data for current record number
                return "";
            }

            return valueList[CurrentRecordNumber];
        }

        //Get Value for current record number by fieldNum
        public string GetValue(int fieldNum)
        {
            if (fieldNames.Count <= fieldNum)
            {
                //Cannot get a field that doesn't exist
                return "";
            }

            List<string> valueList = values[fieldNames[fieldNum]];

            if (CurrentRecordNumber >= valueList.Count)
            {
                //No data for current record number
                return "";
            }

            return valueList[CurrentRecordNumber];
        }

        public void SetRecordNumber(int recordNumber) => CurrentRecordNumber = recordNumber;
        public void NextRecord() => CurrentRecordNumber++;

        /// <summary>
        /// Save the DataFile to file.
        /// </summary>
        public void Save()
        {
            using (StreamWriter fileWriter = new StreamWriter(new FileStream(filepath, FileMode.Create)))
            {
                fileWriter.NewLine = RecordDelimiter;

                //Write Header
                fileWriter.WriteLine(string.Join(FieldDelimiter, fieldNames));

                //Write Data
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < TotalRecordCount; i++)
                {
                    bool needsFieldDelim = false;

                    foreach (string field in fieldNames)
                    {
                        List<string> valueList = values[field];

                        //Handle field separator (after the first field)
                        if (needsFieldDelim)
                        {
                            stringBuilder.Append(FieldDelimiter);
                        }
                        else
                        {
                            needsFieldDelim = true;
                        }

                        if (valueList.Count > i)
                        {
                            //Add data if it exists
                            stringBuilder.Append(valueList[i]);
                        }
                    }

                    fileWriter.WriteLine(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
            }
        }
    }
}
