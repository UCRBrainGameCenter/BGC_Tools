using System;
using UnityEngine;
using BGC.IO;
using BGC.Mathematics;
using System.IO;

namespace BGC.Audio
{
    /// <summary>
    /// A collection of common operations related to Spatialization of Audio
    /// </summary>
    public static class Spatial
    {
        public static int NearestValidOffset(double offset)
        {
            if (offset < -90.0 || offset > 90.0)
            {
                Debug.LogError($"Spatialization offset ({offset})is outside of bounds [-90.0,90.0].  Clamping.");
                offset = GeneralMath.Clamp(offset, -90.0, 90.0);
            }

            return (int)Math.Round(10.0 * offset);
        }

        public static IBGCStream GetFilter(double angle, string hrtfBasePath)
        {
            if (string.IsNullOrWhiteSpace(hrtfBasePath))
            {
                Debug.LogError($"GetFilter requires a non-null hrtfBasePath");
                throw new ArgumentNullException(nameof(hrtfBasePath));
            }

            int position = NearestValidOffset(angle);
            string path;
            string filePrefix = GenerateFilterFilePrefix(position);

            path = Path.Combine(hrtfBasePath, $"{filePrefix}_impulse.wav");

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

        private static string GenerateFilterFilePrefix(int position)
        {
            string filePrefix;

            if (position == 0)
            {
                filePrefix = "0";
            }
            else
            {
                string directionPrefix = (position > 0) ? "pos" : "neg";
                int absPosition = Math.Abs(position);
                int decimalPlace = absPosition % 10;
                int integralPlace = absPosition / 10;
                filePrefix = $"{directionPrefix}{integralPlace}p{decimalPlace}";
            }
            return filePrefix;
        }
    }
}
