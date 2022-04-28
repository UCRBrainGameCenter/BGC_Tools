using System;
using System.IO;

namespace BGC.IO
{
    public static class Utility
    {
        /// <summary>
        /// Move file from the source path to the destination path if it exists. 
        /// 
        /// WARNING: if you set delete on duplicate to false it will not move 
        ///          src to dst. If you set it to true it will delete the current
        ///          file at dst and move src. After, it wil destroy the file at
        ///          src.
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        public static void SafeMove(string srcPath, string dstPath, bool deleteOnDuplicate=true)
        {
            if (!File.Exists(srcPath))
            {
                UnityEngine.Debug.LogError($"SafeMove: {srcPath} does not exist.");
                return;
            }

            bool canMoveFile = true;
            if (File.Exists(dstPath))
            {
                if (deleteOnDuplicate)
                {
                    File.Delete(dstPath);
                }
                else
                {
                    canMoveFile = false;
                }
            }

            if(canMoveFile)
            {
                File.Move(srcPath, dstPath);
            }
        }
        
        /// <summary>Checks if an exception is a disk full exception.</summary>
        /// <param name="ex">Exception to check against.</param>
        /// <returns>TRUE if the exception is a disk full exception</returns>
        /// <remarks>Taken from here: https://stackoverflow.com/a/9294382/17583023</remarks>
        public static bool IsDiskFullException(Exception ex)
        {
            const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
            const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

            return ex.HResult == HR_ERROR_HANDLE_DISK_FULL 
                   || ex.HResult == HR_ERROR_DISK_FULL;
        }
    }
}