namespace BGC.Utility
{
    public class IdManager
    {
        private int count = -1;

        /// <summary>
        /// Empty construct that starts ids at 0
        /// </summary>
        public IdManager() { }

        /// <summary>
        /// Construc id manager where the ids can either start at 0 or at the lowest
        /// possible integer value
        /// </summary>
        /// <param name="humanFriendlyIds"></param>
        public IdManager(bool humanFriendlyIds)
        {
            if (humanFriendlyIds == false)
            {
                count = int.MinValue;
            }
        }

        /// <summary>
        /// Reset starting id to 0
        /// </summary>
        public void Reset()
        {
            count = -1;
        }

        /// <summary>
        /// Get an unused id
        /// </summary>
        /// <returns></returns>
        public int GetId()
        {
            ++count;
            return count;
        }
    }
}