using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BGC.Localization
{
    public class LocalizationSystem
    {
        public enum Language
        {
            English,
            Espanol
        }

        public static Language language = Language.English;

        public static Dictionary<string, string> localizedEN = new Dictionary<string, string>();
        public static Dictionary<string, string> localizedSP = new Dictionary<string, string>();

        public static bool isInit;

        public static void Init(string csvName)
        {
            if (!isInit) { /*GetDefaultLanguage();*/ }
            CSVLoader csvLoader = new CSVLoader();
            csvLoader.LoadCSV(csvName);

            csvLoader.GetDictionaryValues("en", localizedEN);
            csvLoader.GetDictionaryValues("sp", localizedSP);

            isInit = true;
        }

        /// <summary>Initializes all files in the Resources/Localization folder</summary>
        public static void InitializeAllFiles(Language targetLanguage)
        {
            IEnumerable<string> filenames = BGC.IO.DataManagement.GetDataFiles("Assets/Resources/Localization", "*.csv")
                .Select(x => System.IO.Path.GetFileNameWithoutExtension(x));

            foreach (string filename in filenames)
            {
                Init($"Localization/{filename}");
            }

            language = targetLanguage;
            Debug.Log("Finished Initializing Localization");
        }

        /// <summary>Retrieves a localised value for static classes that have no member variables</summary>
        public static string GetLocalizedValue(string key)
        {
            string value = key;
            bool gotValue = false;

            switch (language)
            {
                case Language.English:
                    gotValue = localizedEN.TryGetValue(key, out value);
                    break;
                case Language.Espanol:
                    gotValue = localizedSP.TryGetValue(key, out value);
                    break;
            }

            if (!gotValue)
                return null;

            //Debug.Log("Given string: " + value);
            //Break up the string into two parts : terminals and variables
            var terminals = GetTerminals(value);

            /*
            foreach (var x in terminals)
            {
                Debug.Log("Terminal " + x);
            }
            foreach (var x in variables)
            {
                Debug.Log("Variable " + x);
            }
            */

            int tempCount = 0;
            string finalString = "";
            while (tempCount < terminals.Count)
            {
                if (tempCount < terminals.Count)
                    finalString += terminals[tempCount];

                tempCount++;
            }

            //for new lines in text
            finalString = finalString.Replace("\\n", "\n");
            //for times you wanna use "\"
            finalString = finalString.Replace("\\\"", "\"");
            //for sneaky Carriage Return <CR>
            finalString = finalString.Replace("\r", "");
            return finalString;
        }

        //Needs testing, not sure if returns properly
        public static bool TryGetLocalizedValue(string key, out string result)
        {
            string value = "";
            bool gotValue = false;

            switch (language)
            {
                case Language.English:
                    gotValue = localizedEN.TryGetValue(key, out value);
                    break;
                case Language.Espanol:
                    gotValue = localizedSP.TryGetValue(key, out value);
                    break;
            }

            //Debug.Log("Given string: " + value);
            //Break up the string into two parts : terminals and variables
            var terminals = GetTerminals(value);

            /*
            foreach (var x in terminals)
            {
                Debug.Log("Terminal " + x);
            }
            foreach (var x in variables)
            {
                Debug.Log("Variable " + x);
            }
            */

            int tempCount = 0;
            string finalString = "";
            while (tempCount < terminals.Count)
            {
                if (tempCount < terminals.Count)
                    finalString += terminals[tempCount];

                tempCount++;
            }

            //for new lines in text
            finalString = finalString.Replace("\\n", "\n");
            //for times you wanna use "\"
            finalString = finalString.Replace("\\\"", "\"");
            //for sneaky Carriage Return <CR>
            finalString = finalString.Replace("\r", "");
            result = finalString;
            return result != null;
        }

        public static string GetLocalizedValue(string key, object script)
        {
            //if (!isInit) { Init(); }

            string value = key;
            bool gotValue = false;

            switch (language)
            {
                case Language.English:
                    gotValue = localizedEN.TryGetValue(key, out value);
                    break;
                case Language.Espanol:
                    gotValue = localizedSP.TryGetValue(key, out value);
                    break;
            }

            if (!gotValue)
                return null;

            //Debug.Log("Given string: " + value);
            //Break up the string into two parts : terminals and variables
            var terminals = GetTerminals(value);
            var variables = GetVariables(value);

            /*
            foreach (var x in terminals)
            {
                Debug.Log("Terminal " + x);
            }
            foreach (var x in variables)
            {
                Debug.Log("Variable " + x);
            }
            */

            List<string> variableValues = new List<string>();
            if (variables.Count != 0)
            {
                variableValues = GetVariableValues(variables, script);
            }

            int tempCount = 0;
            string finalString = "";
            while (tempCount < terminals.Count || tempCount < variableValues.Count)
            {
                if (tempCount < terminals.Count)
                    finalString += terminals[tempCount];
                if (tempCount < variables.Count)
                    finalString += variableValues[tempCount];

                tempCount++;
            }

            //for new lines in text
            finalString = finalString.Replace("\\n", "\n");
            //for times you wanna use "\"
            finalString = finalString.Replace("\\\"", "\"");
            //for sneaky Carriage Return <CR>
            finalString = finalString.Replace("\r", "");
            return finalString;
        }

        public static List<string> GetTerminals(string text)
        {
            List<string> terminalStrings = new List<string>();

            int x = 0;
            StringBuilder sb = new StringBuilder();

            while (x < text.Length)
            {
                if (!text[x].Equals('{'))
                {
                    sb.Append(text[x]);
                }
                else
                {
                    terminalStrings.Add(sb.ToString());
                    sb.Clear();
                    x++;
                    while (x < text.Length && !text[x].Equals('}'))
                    {
                        x++;
                    }
                }
                x++;
            }

            if (sb.Length != 0)
            {
                terminalStrings.Add(sb.ToString());
            }
            return terminalStrings;
        }

        public static List<string> GetVariables(string text)
        {
            List<string> variablesString = new List<string>();

            int x = 0;
            StringBuilder sb = new StringBuilder();

            while (x < text.Length)
            {
                if (text[x].Equals('{'))
                {
                    x++;
                    while (x < text.Length && !text[x].Equals('}'))
                    {
                        sb.Append(text[x]);
                        x++;
                    }
                    x++;
                    variablesString.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    x++;
                }
            }

            return variablesString;
        }

        public static List<String> GetVariableValues(List<string> variables, object script)
        {
            List<string> variableValues = new List<string>();
            foreach (string t in variables)
            {
                variableValues.Add(GetValueFromFile(t, script));
            }
            return variableValues;
        }

        public static string GetValueFromFile(string key, object script)
        {
            //Get the type of the Script (IE. the class of it)
            var myType = script.GetType();

            // Get the FieldInfo object by passing the public variable name.
            //NOTE : GETFIELD OVER GETPROPERTY : SO answers only ever say property :(
            var field = myType.GetField(key);

            if (field == null)
            {
                Debug.Log("No variable/field found with name: " + key);
                return "";
            }

            //Get the value of the field inside the script
            var value = field.GetValue(script);

            //Debug.Log(field.GetValue(script));
            return value.ToString();
        }

        public static void GetDefaultLanguage()
        {
            var systemLanguage = Application.systemLanguage;
            if (Enum.IsDefined(typeof(Language), systemLanguage.ToString()))
            {
                language = (Language)Enum.Parse(typeof(Language), systemLanguage.ToString());
            }
            else
            {
                language = Language.English;
            }
        }
    }
}