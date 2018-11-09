
namespace BGC.Utility.Math
{
    public static class GeneralMath
    {
        /// <summary>
        /// x mod m
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static int Mod(int x, int m)
        {
            // https://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
            return ((x % m) + m) % m;
        }

        /// <summary>
        /// x mod m
        /// </summary>
        /// <param name="x"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static float Mod(float x, float m)
        {
            return ((x % m) + m) % m;
        }
}
}