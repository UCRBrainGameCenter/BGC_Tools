using BGC.Extensions;
using UnityEngine;

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

            float requriedProbForHighSelect = Random.value;
            float requiredProbForLowSelect  = Random.value;

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

        /// <summary>
        /// Given an array of weights for each index of the array, return
        /// a random index based on said weights.
        /// </summary>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static int GetRandomIndexBasedOnWeights(float[] weights, bool verbose=false)
        {
            if (weights == null || weights.Length <= 0)
            {
                if (verbose && weights == null)
                {
                    Debug.LogError("Array is null and this function returned -1.");
                }
                else if (verbose)
                {
                    Debug.LogError("Array is <= 0 and this function returned -1.");
                }

                return -1;
            }

            int index   = 0;
            float total = 0;

            for (int i = 0; i < weights.Length; ++i)
            {
                total += weights[i];
            }

            if (total <= 0)
            {
                index = weights.RandomIndex();
            }
            else
            {
                float x = 1f / total;

                float random = Random.value;
                float cumulative = weights[0] * x;

                while (cumulative < random)
                {
                    ++index;
                    cumulative += weights[index] * x;
                }
            }

            return index;
        }
    }
}