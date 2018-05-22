namespace BGC.Utility
{
    public static class StaticIdManager
    {
        private static IdManager idManager = new IdManager();

        public static void Reset()
        {
            idManager.Reset();
        }

        public static int GetId()
        {
            return idManager.GetId();
        }
    }
}