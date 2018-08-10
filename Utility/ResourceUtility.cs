using BGC.Extensions;
using UnityEngine;

namespace BGC.Utility
{
    public class ResourceUtility
    {
        public const string ResourcesDirectory = "Resources";
        public const char FileDelimiter = '/';
        public const char Extension = '.';

        /// <summary>
        /// Checks path and returns true if a valid resource path was found
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool ConvertToValidResourcePath(ref string path)
        {
            string[] Dirs = path.Split(FileDelimiter);
            for (int i = 0; i < Dirs.Length; ++i)
            {
                if (Dirs[i].Equals(ResourcesDirectory))
                {
                    Dirs = Dirs.GetRange(i + 1, Dirs.Length);

                    string[] fileName = Dirs[Dirs.Length - 1].Split(Extension);
                    fileName = fileName.GetRange(0, fileName.Length - 1);

                    Dirs[Dirs.Length - 1] = fileName.Join(".");
                    path = Dirs.Join(FileDelimiter.ToString());

                    return true;
                }
            }

            return false;
        }
    }
}
