public static class Conversion
{
    public static float SecondsToMS(float seconds)
    {
        return seconds * 1000f;
    }

    public static float MSToSeconds(float ms)
    {
        return ms * 0.001f;
    }

    public static double SecondsToMS(double seconds)
    {
        return seconds * 1000;
    }

    public static double MSToSeconds(double ms)
    {
        return ms * 0.001;
    }
}
