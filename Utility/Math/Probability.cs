namespace BGC.Utility.Math
{
    public static class Probability
    {
        /// <summary>
        /// Get a random index from an array where the previous has a 
        /// lower probability
        /// </summary>
        /// <param name="arrayLength"></param>
        /// <param name="previousIndex"></param>
        /// <returns></returns>
        public static int GetIndexWIthPreviousLowerProbability(float arrayLength, int previousIndex)
        {
            int index = -1;
            if (arrayLength <= 0)
            {   
                return index;
            }

            float regularProbability = 1f / arrayLength;
            float highProbability = 3f / (2f * arrayLength);
            float lowProbability = 1f / (2f * arrayLength);
            float lowSelectProbability = 1f / (arrayLength - 1f);

            float requriedProbForHighSelect = UnityEngine.Random.value;
            float requiredProbForLowSelect  = UnityEngine.Random.value;

            float cumulativeHighProbSelect = 0f;
            float cumulativeLowProbSelect  = 0f;

            bool lowProbabilitySelected = false;

            for (int i = 0; i < arrayLength; ++i)
            {
                if (i == previousIndex)
                {
                    cumulativeHighProbSelect += lowProbability;
                }
                else
                {
                    cumulativeLowProbSelect += lowSelectProbability;

                    if (lowProbabilitySelected && cumulativeLowProbSelect > requiredProbForLowSelect)
                    {
                        cumulativeHighProbSelect += highProbability;
                    }
                    else
                    {
                        cumulativeHighProbSelect += regularProbability;
                    }
                }

                if (cumulativeHighProbSelect > requriedProbForHighSelect)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                index = ((int) arrayLength) - 1;
            }

            return index;
        }
    }
}