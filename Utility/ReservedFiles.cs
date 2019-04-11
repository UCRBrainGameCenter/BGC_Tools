using System;
using System.Collections.Generic;

namespace BGC.Utility
{
    public static class ReservedFiles
    {
        private static HashSet<string> files = new HashSet<string>();

        /// <summary>
        /// Reserve a file so other processes won't use them
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if the file was succesfully reserved</returns>
        public static bool ReserveFile(string path)
        {
            if (files.Contains(path) == false)
            {
                files.Add(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// UnReserve a file if found
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if hte file was found and removed</returns>
        public static bool UnReserveFile(string path)
        {
            if (files.Contains(path))
            {
                files.Remove(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns if the path is currently reserved
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if the file is reserved, false if not</returns>
        public static bool IsFileReserved(string path)
        {
            return files.Contains(path);
        }
    }
}