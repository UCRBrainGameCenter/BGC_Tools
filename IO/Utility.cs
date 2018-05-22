using System.IO;

namespace BGC.IO
{
    public static class Utility
    {
        /// <summary>
        /// Move file from the source path to the destination path. 
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
    }
}