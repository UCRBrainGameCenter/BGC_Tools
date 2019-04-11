using System;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using BGC.IO;
using BGC.Mathematics;

namespace BGC.Audio
{
    /// <summary>
    /// Manages calibration values and processes
    /// </summary>
    public static class Calibration
    {
        private static class Keys
        {
            public const string CustomCalibration = "Custom";

            public const string Level = "Level";
            public const string LAdjustment = "LAdj";
            public const string RAdjustment = "RAdj";
        }

        public enum Tone
        {
            _250Hz = 0,
            _500Hz,
            _1000Hz,
            _2000Hz,
            _4000Hz,
            _8000Hz,
            Noise,
            MAX
        }

        public enum Source
        {
            Default = 0,
            Custom,
            Results,
            InProgress,
            MAX
        }

        private static List<CalibrationPoint> customCalibration = null;
        private static List<CalibrationPoint> resultsCalibration = null;
        private static List<CalibrationPoint> inProgressCalibration = null;

        private static readonly string dataDir = "System";
        private static readonly string configFileName = "Calibration.json";

        private static bool initialized = false;

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


        private static void DeserializeCalibration(JsonObject parsedValue)
        {
            JsonArray calibrationValues = parsedValue[Keys.CustomCalibration].AsJsonArray;

            if (calibrationValues.Count == 0)
            {
                return;
            }

            customCalibration = new List<CalibrationPoint>(calibrationValues.Count);
            foreach (JsonObject calibrationPoint in calibrationValues)
            {
                customCalibration.Add(new CalibrationPoint(
                    levelIn: calibrationPoint[Keys.Level],
                    levelOffsetL: calibrationPoint[Keys.LAdjustment].AsNumber,
                    levelOffsetR: calibrationPoint[Keys.RAdjustment].AsNumber));
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
            JsonArray jsonCalibrationArray = new JsonArray();
            if (customCalibration != null)
            {
                foreach (CalibrationPoint point in customCalibration)
                {
                    jsonCalibrationArray.Add(new JsonObject()
                    {
                        { Keys.Level, point.levelIn },
                        { Keys.LAdjustment, point.levelOffsetL },
                        { Keys.RAdjustment, point.levelOffsetR }
                    });
                }
            }

            return new JsonObject()
            {
                { Keys.CustomCalibration, jsonCalibrationArray }
            };
        }

        public static void FinishCalibration()
        {
            resultsCalibration = inProgressCalibration;
            inProgressCalibration = null;
        }

        public static void InitiateCalibration(int[] testLevels)
        {
            //Make a new inProgressList that's a copy of the appropriate values
            switch (GetSourceForVerificationPanel())
            {
                case Source.Custom:
                    inProgressCalibration = new List<CalibrationPoint>(customCalibration);
                    break;

                case Source.Results:
                    inProgressCalibration = new List<CalibrationPoint>(resultsCalibration);
                    break;

                case Source.Default:
                    inProgressCalibration = new List<CalibrationPoint>(testLevels.Length);

                    foreach (int level in testLevels)
                    {
                        inProgressCalibration.Add(new CalibrationPoint(
                            levelIn: level,
                            levelOffsetL: 0.0,
                            levelOffsetR: 0.0));
                    }
                    break;

                case Source.InProgress:
                default:
                    Debug.LogError($"Unexpected Source: {GetSourceForVerificationPanel()}");
                    break;
            }
        }

        public static void SubmitCalibrationValue(
            int step,
            AudioChannel channel,
            double offset)
        {
            double left = inProgressCalibration[step].levelOffsetL;
            double right = inProgressCalibration[step].levelOffsetR;

            switch (channel)
            {
                case AudioChannel.Left:
                    left += offset;
                    break;

                case AudioChannel.Right:
                    right += offset;
                    break;

                default:
                    Debug.LogError($"Unexpected Channel: {channel}");
                    break;
            }

            inProgressCalibration[step] = new CalibrationPoint(
                levelIn: inProgressCalibration[step].levelIn,
                levelOffsetL: left,
                levelOffsetR: right);

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

                case Source.InProgress:
                    inProgressCalibration = null;
                    break;

                case Source.Default:
                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    break;
            }
        }

        public static Source GetSourceForVerificationPanel()
        {
            if (resultsCalibration != null)
            {
                return Source.Results;
            }
            else if (customCalibration != null)
            {
                return Source.Custom;
            }

            return Source.Default;
        }

        public static void GetLevelOffset(
            double level,
            out double levelOffsetL,
            out double levelOffsetR,
            Source source = Source.Custom)
        {
            List<CalibrationPoint> points;

            switch (source)
            {
                case Source.InProgress:
                    if (inProgressCalibration == null || inProgressCalibration.Count == 0)
                    {
                        goto case Source.Results;
                    }
                    points = inProgressCalibration;
                    break;

                case Source.Results:
                    if (resultsCalibration == null || resultsCalibration.Count == 0)
                    {
                        goto case Source.Custom;
                    }
                    points = resultsCalibration;
                    break;

                case Source.Custom:
                    if (customCalibration == null || customCalibration.Count == 0)
                    {
                        goto case Source.Default;
                    }
                    points = customCalibration;
                    break;

                case Source.Default:
                    levelOffsetL = 0.0;
                    levelOffsetR = 0.0;
                    return;

                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    goto case Source.Default;
            }

            if (points.Count == 1)
            {
                levelOffsetL = points[0].levelOffsetL;
                levelOffsetR = points[0].levelOffsetR;
                return;
            }

            //If the requested level is before the first or after the last, use its assocaited offset
            if (level <= points[0].levelIn)
            {
                levelOffsetL = points[0].levelOffsetL;
                levelOffsetR = points[0].levelOffsetR;
                return;
            }

            //Else, we will LERP between the surrounding offsets
            for (int index = 0; index < points.Count - 1; index++)
            {
                if (level >= points[index].levelIn && level < points[index + 1].levelIn)
                {
                    
                    levelOffsetL = GeneralMath.Lerp(points[index].levelOffsetL,
                        points[index + 1].levelOffsetL,
                        (level - points[index].levelIn) / (points[index + 1].levelIn - points[index].levelIn));

                    levelOffsetR = GeneralMath.Lerp(points[index].levelOffsetR,
                        points[index + 1].levelOffsetR,
                        (level - points[index].levelIn) / (points[index + 1].levelIn - points[index].levelIn));

                    return;
                }
            }

            //Else, we return the last value
            levelOffsetL = points[points.Count - 1].levelOffsetL;
            levelOffsetR = points[points.Count - 1].levelOffsetR;
        }

        public static string GetToneName(this Tone tone)
        {
            switch (tone)
            {
                case Tone._250Hz: return "250 Hz";
                case Tone._500Hz: return "500 Hz";
                case Tone._1000Hz: return "1000 Hz";
                case Tone._2000Hz: return "2000 Hz";
                case Tone._4000Hz: return "4000 Hz";
                case Tone._8000Hz: return "8000 Hz";
                case Tone.Noise: return "Noise";
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

                default:
                    Debug.LogError($"Unexpected Tone: {tone}");
                    return 100;
            }
        }

        private readonly struct CalibrationPoint
        {
            public readonly int levelIn;
            public readonly double levelOffsetL;
            public readonly double levelOffsetR;

            public CalibrationPoint(int levelIn, double levelOffsetL, double levelOffsetR)
            {
                this.levelIn = levelIn;
                this.levelOffsetL = levelOffsetL;
                this.levelOffsetR = levelOffsetR;
            }
        }
    }
}
