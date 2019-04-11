using System;
using UnityEngine;
using BGC.IO;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// A collection of common operations related to Spatialization of Audio
    /// </summary>
    public static class Spatial
    {
        private const string HRTFDirectory = "HRTF";

        public static int NearestValidOffset(double offset)
        {
            if (offset < -90.0 || offset > 90.0)
            {
                Debug.LogError($"Spatialization offset ({offset})is outside of bounds [-90.0,90.0].  Clamping.");
                offset = GeneralMath.Clamp(offset, -90.0, 90.0);
            }

            return (int)Math.Round(10.0 * offset);
        }

        public static IBGCStream GetFilter(double angle)
        {
            int position = NearestValidOffset(angle);

            string path = GetFilterFilename(position);

            bool loadSuccess = WaveEncoding.LoadBGCStream(
                filepath: path,
                stream: out IBGCStream filter);

            if (!loadSuccess)
            {
                Debug.LogError($"Failed to load impulse response function from path {path}");
                return null;
            }

            return filter;
        }

        private static string GetFilterFilename(int position)
        {
            string filePrefix;

            if (position == 0)
            {
                filePrefix = "0";
            }
            else
            {
                string directionPrefix = (position > 0) ? "pos" : "neg";

                position = Math.Abs(position);

                int decimalPlace = position % 10;
                int integralPlace = position / 10;

                filePrefix = $"{directionPrefix}{integralPlace}p{decimalPlace}";
            }

            return DataManagement.PathForDataFile(HRTFDirectory, $"{filePrefix}_impulse.wav");
        }
    }
}
