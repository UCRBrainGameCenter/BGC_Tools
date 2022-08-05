namespace BGC.Scripting.Members
{
    //public class MathExtensions
    //{
    //    public static double Clamp(double value, double min, double max) => Mathematics.GeneralMath.Clamp(value, min, max);
    //    public static bool IsNaN(double value) => double.IsNaN(value);
    //}

    public class SystemAdapter
    {
        public static string Date => System.DateTime.Now.ToString("MM-dd-yyyy");
        public static string Time => System.DateTime.Now.ToString("HH:mm:ss");
        public static string DateTime => System.DateTime.Now.ToString("MM-dd-yy HH:mm:ss");
        public static string GetDate(string format) => System.DateTime.Now.ToString(format);
    }
}
