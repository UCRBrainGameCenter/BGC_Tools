namespace BGC.Utility
{
    public static class StaticIdManager
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