namespace BGC.Utility
{
    public static class IdManager
    {
        private static int count = -1;

        public static void Reset()
        {
            count = -1;
        }

        public static int GetId()
        {
            ++count;
            return count;
        }
    }
}