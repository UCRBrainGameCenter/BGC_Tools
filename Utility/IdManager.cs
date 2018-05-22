﻿namespace BGC.Utility
{
    public class IdManager
    {
        private int count = -1;

        public void Reset()
        {
            count = -1;
        }

        public int GetId()
        {
            ++count;
            return count;
        }
    }
}