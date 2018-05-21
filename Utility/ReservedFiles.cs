using System.Collections.Generic;
using System.IO;
using BGC.Web;
using BGC.IO;

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
            if(files.Contains(path))
            {
                files.Remove(path);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unreserve a file and push it to the server
        /// </summary>
        /// <param name="path"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static bool UnReserveFile(string path, string userName, string bucket, string serverPath)
        {
            bool removed = UnReserveFile(path);

            // @todo: #if !Unity-edadf
            AWSServer.PostFileToAWS(
                path, 
                bucket, 
                serverPath + "/" + Path.GetFileName(path),
                (bool error) => {
                    // move file from staging to permament on no error
                    if(error == false)
                    {
                        // @note: duplicate code from LogFilesToS3
                        string permanentPath = Path.Combine(LogDirectories.PermanentDirectory, userName);
                        if (Directory.Exists(permanentPath))
                        {
                            Directory.CreateDirectory(permanentPath);
                        }

                        permanentPath = Path.Combine(permanentPath, Path.GetFileName(path));

                        if (File.Exists(permanentPath))
                        {
                            File.Delete(permanentPath);
                        }

                        File.Move(path, permanentPath);
                    }
                });

            return removed;
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