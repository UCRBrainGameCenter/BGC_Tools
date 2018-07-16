using BGC.Extensions;

namespace BGC.IO
{
    public static class FilePath
    {
        private const string ResourcesDirectory = "Resources";
        private const char FileDelimiter = '/';
        /// <summary>
        /// Replace all invalid characters for a file with an empty value unless you 
        /// specificy otherwise
        /// </summary>
        /// <param name="path"></param>
        /// <param name="replacementValue"></param>
        public static void ReplaceInvalidCharacters(ref string path, string replacementValue="")
        {
            char[] invalidCharacters = System.IO.Path.GetInvalidFileNameChars();

            for (int i = 0; i < invalidCharacters.Length; ++i)
            {
                path = path.Replace(invalidCharacters[i].ToString(), replacementValue);
            }
        }

        public static bool GetValidResourcePath(ref string path)
        {
            string[] Dirs = path.Split(FileDelimiter);
            for(int i = 0; i < Dirs.Length; ++i)
            {
                if(Dirs[i].Equals(ResourcesDirectory))
                {
                    Dirs = Dirs.GetRange(i, Dirs.Length);
                    path = Dirs.Join(FileDelimiter.ToString());

                    return true;
                }
            }

            return false;
        }
    }
}