namespace BGC.IO
{
    public static class FilePath
    {
        private const string ResourcesDirectory = "Resources";
        private const char FileDelimiter = '/';
        private const char Extension = '.';
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
                    Dirs = Dirs.GetRange(i + 1, Dirs.Length);
                    Dirs[Dirs.Length - 1] = Dirs[Dirs.Length - 1].Split(Extension)[0];
                    path = Dirs.Join(FileDelimiter.ToString());

                    return true;
                }
            }

            return false;
        }
    }
}