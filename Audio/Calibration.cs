using System;
using UnityEngine;
using LightJson;
using BGC.IO;

namespace BGC.Audio
{
    /// <summary>
    /// Manages calibration values and processes
    /// </summary>
    public static class Calibration
    {
        private const int VERSION = 2;

        public const double TARGET_RMS = 1.0 / 32.0;
        public const double RMS_TO_PEAK = 2.8;
        public const double TARGET_PEAK = TARGET_RMS * RMS_TO_PEAK;

        public static bool RecalibrationNeeded = false;

        private const string dataDir = "System";
        private const string configFileName = "Calibration.json";

        private static readonly CalibrationValue defaultCalibrationIOS = new CalibrationValue(91.0, 91.0);
        private static readonly CalibrationValue defaultCalibrationWindows = new CalibrationValue(76.0, 76.0);
        private static CalibrationValue? customCalibration = null;
        private static CalibrationValue? resultsCalibration = null;

        private static bool initialized = false;

        private static class Keys
        {
            public const string Version = "Version";

            public const string Custom = "Custom";

            public const string LeftLevel = "LeftLevel";
            public const string RightLevel = "RightLevel";
        }

        public enum Tone
        {
            _250Hz = 0,
            _500Hz,
            _1000Hz,
            _1500Hz,
            _2000Hz,
            _3000Hz,
            _4000Hz,
            _6000Hz,
            _8000Hz,
            _9000Hz,
            _10_000Hz,
            _11_200Hz,
            _12_500Hz,
            _14_000Hz,
            _16_000Hz,
            MAX
        }

        public enum Source
        {
            Default = 0,
            Custom,
            Results,
            MAX
        }

        public static void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                FileReader.ReadJsonFile(
                    path: DataManagement.PathForDataFile(dataDir, configFileName),
                    successCallback: DeserializeCalibration,
                    failCallback: Serialize,
                    fileNotFoundCallback: Serialize);
            }
        }

        private static CalibrationValue PlatformDefaultCalibration()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                    return defaultCalibrationWindows;
                default:
                    return defaultCalibrationIOS;
            }
        }

        private static void DeserializeCalibration(JsonObject parsedValue)
        {
            if (!parsedValue.ContainsKey(Keys.Version))
            {
                //Version 1
                RecalibrationNeeded = true;
                Serialize();
                return;
            }

            if (parsedValue.ContainsKey(Keys.Custom))
            {
                customCalibration = new CalibrationValue(
                    levelLeft: parsedValue[Keys.Custom][Keys.LeftLevel],
                    levelRight: parsedValue[Keys.Custom][Keys.RightLevel]);
            }
            else
            {
                customCalibration = null;
            }
        }

        public static void Serialize()
        {
            FileWriter.WriteJson(
                path: DataManagement.PathForDataFile(dataDir, configFileName),
                createJson: SerializeCalibration,
                pretty: true);
        }

        private static JsonObject SerializeCalibration()
        {
            JsonObject data = new JsonObject()
            {
                { Keys.Version, VERSION }
            };

            if (customCalibration.HasValue)
            {
                data.Add(Keys.Custom,
                    new JsonObject()
                    {
                        { Keys.LeftLevel, customCalibration.Value.levelLeft },
                        { Keys.RightLevel, customCalibration.Value.levelRight }
                    });
            }

            return data;
        }

        public static void FinishCalibration(
            double levelLeft,
            double levelRight)
        {
            resultsCalibration = new CalibrationValue(levelLeft, levelRight);
        }

        public static (double initialLeft, double initialRight) InitiateCalibration()
        {
            switch (GetSourceForVerificationPanel())
            {
                case Source.Custom:
                    return (customCalibration.Value.levelLeft, customCalibration.Value.levelRight);

                case Source.Results:
                    return (resultsCalibration.Value.levelLeft, resultsCalibration.Value.levelRight);

                case Source.Default:
                    {
                        var defaultCalibration = PlatformDefaultCalibration();
                        return (defaultCalibration.levelLeft, defaultCalibration.levelRight);
                    }

                default:
                    Debug.LogError($"Unexpected Source: {GetSourceForVerificationPanel()}");
                    goto case Source.Default;
            }
        }

        public static void PushCalibrationResults()
        {
            if (resultsCalibration == null)
            {
                Debug.LogError("Null results calibration");
                return;
            }

            customCalibration = resultsCalibration;
            resultsCalibration = null;

            Serialize();
        }

        public static void DropCalibrationResults(Source source)
        {
            switch (source)
            {
                case Source.Custom:
                    customCalibration = null;
                    Serialize();
                    break;

                case Source.Results:
                    resultsCalibration = null;
                    break;

                case Source.Default:
                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    break;
            }
        }

        public static Source GetSourceForVerificationPanel()
        {
            if (resultsCalibration.HasValue)
            {
                return Source.Results;
            }
            else if (customCalibration.HasValue)
            {
                return Source.Custom;
            }

            return Source.Default;
        }

        /// <summary>
        /// Returns the scale factor per RMS.
        /// To use, multiple every sample by the output of this divided by the stream RMS.
        /// </summary>
        public static (double levelFactorL, double levelFactorR) GetLevelFactors(
            double levelL,
            double levelR,
            Source source = Source.Custom)
        {
            CalibrationValue calibrationValue = PlatformDefaultCalibration();
            switch (source)
            {
                case Source.Results:
                    if (resultsCalibration == null)
                    {
                        goto case Source.Custom;
                    }
                    calibrationValue = resultsCalibration.Value;
                    break;

                case Source.Custom:
                    if (customCalibration == null)
                    {
                        goto case Source.Default;
                    }
                    calibrationValue = customCalibration.Value;
                    break;

                case Source.Default:
                    //Already set to default
                    break;

                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    goto case Source.Default;
            }

            return (TARGET_RMS * Math.Pow(10.0, (levelL - calibrationValue.levelLeft) / 20.0),
                TARGET_RMS * Math.Pow(10.0, (levelR - calibrationValue.levelRight) / 20.0));
        }

        public static (double softLimit, double hardLimit) GetLimitRecommendations(Source source)
        {
            double minLevel;

            switch (source)
            {
                case Source.Default:
                    {
                        var defaultCalibration = PlatformDefaultCalibration();
                        minLevel = Math.Min(defaultCalibration.levelLeft, defaultCalibration.levelRight);
                    }
                    break;

                case Source.Custom:
                    minLevel = Math.Min(customCalibration.Value.levelLeft, customCalibration.Value.levelRight);
                    break;

                case Source.Results:
                    minLevel = Math.Min(resultsCalibration.Value.levelLeft, resultsCalibration.Value.levelRight);
                    break;

                default:
                    Debug.LogError($"Unexpected CalibrationSource: {source}");
                    goto case Source.Default;
            }

            double softLimit = minLevel + 20.0 * Math.Log10(0.1 / TARGET_RMS);
            double hardLimit = minLevel + 20.0 * Math.Log10(1.0 / (Math.Sqrt(2.0) * TARGET_RMS));

            return (softLimit, hardLimit);
        }

        public static string GetStimulusTypeName(this Audiometry.AudiometricCalibration.CalibrationSet calibrationSet)
        {
            switch (calibrationSet)
            {
                case Audiometry.AudiometricCalibration.CalibrationSet.PureTone: return "Pure Tone";
                case Audiometry.AudiometricCalibration.CalibrationSet.Narrowband: return "Narrowband";
                case Audiometry.AudiometricCalibration.CalibrationSet.Broadband: return "Broadband";
                default:
                    Debug.LogError($"Unexpected CalibrationSet: {calibrationSet}");
                    return "";
            }
        }

        public static string GetToneName(this Tone tone)
        {
            switch (tone)
            {
                case Tone._250Hz: return "250 Hz";
                case Tone._500Hz: return "500 Hz";
                case Tone._1000Hz: return "1000 Hz";
                case Tone._1500Hz: return "1500 Hz";
                case Tone._2000Hz: return "2000 Hz";
                case Tone._3000Hz: return "3000 Hz";
                case Tone._4000Hz: return "4000 Hz";
                case Tone._6000Hz: return "6000 Hz";
                case Tone._8000Hz: return "8000 Hz";
                case Tone._9000Hz: return "9000 Hz";
                case Tone._10_000Hz: return "10.0 kHz";
                case Tone._11_200Hz: return "11.2 kHz";
                case Tone._12_500Hz: return "12.5 kHz";
                case Tone._14_000Hz: return "14.0 kHz";
                case Tone._16_000Hz: return "16.0 kHz";
                case Tone.MAX: return "";

                default:
                    Debug.LogError($"Unexpected Tone: {tone}");
                    return "";
            }
        }

        public static double GetFrequency(this Tone tone)
        {
            switch (tone)
            {

                case Tone._250Hz: return 250;
                case Tone._500Hz: return 500;
                case Tone._1000Hz: return 1000;
                case Tone._2000Hz: return 2000;
                case Tone._4000Hz: return 4000;
                case Tone._8000Hz: return 8000;

                    //Expanded set
                case Tone._1500Hz: return 1500;
                case Tone._3000Hz: return 3000;
                case Tone._6000Hz: return 6000;
                case Tone._9000Hz: return 9000;
                case Tone._10_000Hz: return 10_000;
                case Tone._11_200Hz: return 11_200;
                case Tone._12_500Hz: return 12_500;
                case Tone._14_000Hz: return 14_000;
                case Tone._16_000Hz: return 16_000;

                default:
                    Debug.LogError($"Unexpected Tone: {tone}");
                    return 100;
            }
        }

        private readonly struct CalibrationValue
        {
            public readonly double levelLeft;
            public readonly double levelRight;

            public CalibrationValue(double levelLeft, double levelRight)
            {
                this.levelLeft = levelLeft;
                this.levelRight = levelRight;
            }
        }
    }
}
