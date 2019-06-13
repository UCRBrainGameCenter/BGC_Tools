using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BGC.IO
{
    public static class FilePath
    {
        private static HashSet<char> invalidChars = null;
        private static HashSet<char> InvalidChars => invalidChars ??
            (invalidChars = new HashSet<char>(
                    Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct()));

        #region RepalceInvalidCharacters

        [Obsolete("Use SanitizeForFilename or SanitizeAndReplaceForFilename, instead")]
        public static void ReplaceInvalidCharacters(ref string path, string replacementValue)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();

            for (int i = 0; i < invalidCharacters.Length; ++i)
            {
                path = path.Replace(invalidCharacters[i].ToString(), replacementValue);
            }
        }

        /// <summary>
        /// Eliminates all invalid characters from string
        /// </summary>
        [Obsolete("Use SanitizeForFilename instead.")]
        public static void ReplaceInvalidCharacters(ref string path)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();

            for (int i = 0; i < invalidCharacters.Length; ++i)
            {
                path = path.Replace(invalidCharacters[i].ToString(), "");
            }
        }

        #endregion RepalceInvalidCharacters
        #region Sanitize

        /// <summary>
        /// Removes characters that are invalid for paths and filenames.
        /// Replaces them with nothing.
        /// Returns raw fallback value if name is null or empty after sanitizing.
        /// </summary>
        public static string SanitizeForFilename(
            string name,
            string fallback = "NewFile")
        {
            if (string.IsNullOrEmpty(name))
            {
                return fallback;
            }

            string sanitizedName = string.Concat(name.Where(c => !InvalidChars.Contains(c)));

            if (sanitizedName.Length == 0)
            {
                return fallback;
            }

            return sanitizedName;
        }

        /// <summary>
        /// Removes characters that are invalid for paths and filenames, also removes
        /// supplementary characters.
        /// Replaces them with nothing.
        /// Returns raw fallback value if name is null or empty after sanitizing.
        /// </summary>
        public static string SanitizeForFilename(
            string name,
            IEnumerable<char> additionalExclusions,
            string fallback = "NewFile")
        {
            if (string.IsNullOrEmpty(name))
            {
                return fallback;
            }

            string sanitizedName = string.Concat(
                name.Where(c => !InvalidChars.Contains(c) && !additionalExclusions.Contains(c)));

            if (sanitizedName.Length == 0)
            {
                return fallback;
            }

            return sanitizedName;
        }

        /// <summary>
        /// Removes characters that are invalid for paths and filenames, also removes a
        /// supplementary character.
        /// Replaces them with nothing.
        /// Returns raw fallback value if name is null or empty after sanitizing.
        /// </summary>
        public static string SanitizeForFilename(
            string name,
            char additionalExclusion,
            string fallback = "NewFile")
        {
            if (string.IsNullOrEmpty(name))
            {
                return fallback;
            }

            string sanitizedName = string.Concat(
                name.Where(c => !InvalidChars.Contains(c) && c != additionalExclusion));

            if (sanitizedName.Length == 0)
            {
                return fallback;
            }

            return sanitizedName;
        }

        #endregion Sanitize
        #region Sanitize And Replace

        /// <summary>
        /// Replaces characters that are invalid for paths and filenames with the specified
        /// replacementValue.
        /// Returns raw fallback value if name is null or empty after sanitizing.
        /// </summary>
        public static string SanitizeAndReplaceForFilename(
            string name,
            char replacementValue,
            string fallback = "NewFile")
        {
            if (string.IsNullOrEmpty(name))
            {
                return fallback;
            }

            string sanitizedName = string.Concat(
                name.Select(c => InvalidChars.Contains(c) ? replacementValue : c));

            if (sanitizedName.Length == 0)
            {
                return fallback;
            }

            return sanitizedName;
        }

        /// <summary>
        /// Replaces characters that are invalid for paths and filenames, and the additional
        /// supplied character, with the specified replacementValue.
        /// Returns raw fallback value if name is null or empty after sanitizing.
        /// </summary>
        public static string SanitizeAndReplaceForFilename(
            string name,
            char replacementValue,
            IEnumerable<char> additionalExclusions,
            string fallback = "NewFile")
        {
            if (string.IsNullOrEmpty(name))
            {
                return fallback;
            }

            string sanitizedName = string.Concat(
                name.Select(c => InvalidChars.Contains(c) || additionalExclusions.Contains(c) ?
                    replacementValue : c));

            if (sanitizedName.Length == 0)
            {
                return fallback;
            }

            return sanitizedName;
        }

        /// <summary>
        /// Replaces characters that are invalid for paths and filenames, and the additional
        /// supplied characters, with the specified replacementValue.
        /// Returns raw fallback value if name is null or empty after sanitizing.
        /// </summary>
        public static string SanitizeAndReplaceForFilename(
            string name,
            char replacementValue,
            char additionalExclusion,
            string fallback = "NewFile")
        {
            if (string.IsNullOrEmpty(name))
            {
                return fallback;
            }

            string sanitizedName = string.Concat(
                name.Select(c => InvalidChars.Contains(c) || c == additionalExclusion ?
                    replacementValue : c));

            if (sanitizedName.Length == 0)
            {
                return fallback;
            }

            return sanitizedName;
        }

        #endregion
        #region Next Available FilePath

        /// <summary>
        /// Returns an available filepath.
        /// Appends " (#)" to the filename, incrementing # until it is available, 
        /// starting with any modifier present in the filepath.
        /// </summary>
        /// <returns>An available filepath</returns>
        public static string NextAvailableFilePath(string filepath)
        {
            if (!File.Exists(filepath))
            {
                return filepath;
            }

            string directory = Path.GetDirectoryName(filepath);
            string fileExtension = Path.GetExtension(filepath);
            string fileName = GetCleanFileName(
                fileName: Path.GetFileNameWithoutExtension(filepath),
                modifierValue: out int initialModifierValue);

            for (int i = initialModifierValue; ; i++)
            {
                if (!File.Exists(filepath))
                {
                    return filepath;
                }

                filepath = Path.Combine(directory, $"{fileName} ({i}){fileExtension}");
            }
        }

        /// <summary>
        /// Identifies any filename matching the pattern "NameStuffHere (3)" and strips
        /// off and spits out the modifier
        /// </summary>
        private static string GetCleanFileName(string fileName, out int modifierValue)
        {
            //If FileName doesn't end with " (##)", it's not a modifier
            if (fileName.EndsWith(")") && fileName.Contains(" ("))
            {
                //FileName ends with (##)
                //It may have a modifier

                int indexOfOpen = fileName.LastIndexOf(" (");
                int indexOfClose = fileName.LastIndexOf(")");

                int indexInside = indexOfOpen + 2;
                int length = indexOfClose - indexInside;

                //Make sure there are contents
                //This would reject "TestFile ()"
                if (length > 0)
                {
                    string valueString = fileName.Substring(indexInside, length);

                    //Make sure value string can be parsed entirely, otherwise its not a modifier
                    //This would reject "TestFile (words)"
                    if (int.TryParse(valueString, out modifierValue))
                    {
                        //Reject negative numbers because that's now how we create modifiers
                        //This would reject "Test File (-1)"
                        if (modifierValue > 0)
                        {
                            modifierValue++;
                            return fileName.Substring(0, indexOfOpen);
                        }
                    }
                }
            }

            modifierValue = 2;
            return fileName;
        }

        #endregion Next Available FilePath
    }
}