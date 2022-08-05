namespace BGC.Scripting.Members
{
    public class AudiometryAdapter
    {
        public static bool IsCalibrationReady() => Audio.Audiometry.AudiometricCalibration.IsCalibrationReady();
        public static double ConvertSPLToHL(double frequency, double levelSPL) => Audio.Audiometry.AudiometricCalibration.GetLevelHL(Audio.Audiometry.AudiometricCalibration.Source.Custom, frequency, levelSPL);
        public static double ConvertHLToSPL(double frequency, double levelHL) => Audio.Audiometry.AudiometricCalibration.GetLevelSPL(Audio.Audiometry.AudiometricCalibration.Source.Custom, frequency, levelHL);
    }
}
