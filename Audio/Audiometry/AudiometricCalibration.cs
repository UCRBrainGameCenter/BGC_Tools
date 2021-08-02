using System;
using System.Collections.Generic;
using UnityEngine;
using LightJson;
using BGC.IO;

namespace BGC.Audio.Audiometry
{
    /// <summary>
    /// Manages calibration values and processes
    /// </summary>
    public static class AudiometricCalibration
    {
        private const int VERSION = 1;

        private const string systemDirectory = "System";
        private const string configFileName = "AudiometricCalibration.json";

        private const string dataDirectory = "Calibration";

        private static string currentCalibrationName = null;
        private static CalibrationProfile customCalibration = null;
        private static CalibrationProfile resultsCalibration = null;

        private static bool initialized = false;

        private static class Keys
        {
            public const string Version = "Version";

            public const string CurrentCalibration = "Current";
        }

        public enum Source
        {
            Default = 0,
            Custom,
            Results,
            MAX
        }

        public enum CalibrationSet
        {
            PureTone,
            Narrowband,
            Broadband,
            MAX
        }

        public static void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                FileReader.ReadJsonFile(
                    path: DataManagement.PathForDataFile(systemDirectory, configFileName),
                    successCallback: DeserializeCalibration,
                    failCallback: Serialize,
                    fileNotFoundCallback: Serialize);
            }
        }

        private static void DeserializeCalibration(JsonObject parsedValue)
        {
            if (parsedValue.ContainsKey(Keys.CurrentCalibration))
            {
                currentCalibrationName = parsedValue[Keys.CurrentCalibration];
            }
            else
            {
                currentCalibrationName = null;
            }

            if (!string.IsNullOrEmpty(currentCalibrationName))
            {
                FileReader.ReadJsonFile(
                    path: DataManagement.PathForDataFile(dataDirectory, currentCalibrationName),
                    successCallback: data => customCalibration = new CalibrationProfile(data),
                    failCallback: FailedToLoadCalibration,
                    fileNotFoundCallback: FailedToLoadCalibration);
            }
            else
            {
                customCalibration = null;
            }
        }

        private static void FailedToLoadCalibration()
        {
            Debug.LogError(
                $"Unable to load calibration file: {DataManagement.PathForDataFile(dataDirectory, currentCalibrationName)}");
            currentCalibrationName = null;
            customCalibration = null;
        }

        public static void Serialize()
        {
            FileWriter.WriteJson(
                path: DataManagement.PathForDataFile(systemDirectory, configFileName),
                createJson: SerializeCalibration,
                pretty: true);
        }

        private static JsonObject SerializeCalibration()
        {
            JsonObject data = new JsonObject()
            {
                [Keys.Version] = VERSION
            };

            if (!string.IsNullOrEmpty(currentCalibrationName))
            {
                data.Add(Keys.CurrentCalibration, currentCalibrationName);
            }

            return data;
        }

        public static void PushCalibrationValue(
            double levelHL,
            CalibrationSet set,
            double frequency,
            AudioChannel channel,
            double rms)
        {
            if (resultsCalibration is null)
            {
                throw new Exception($"Calibration not initialized");
            }

            switch (set)
            {
                case CalibrationSet.PureTone:
                    resultsCalibration.PureTone.SetCalibrationPoint(frequency, levelHL, channel, rms);
                    break;

                case CalibrationSet.Narrowband:
                    resultsCalibration.Narrowband.SetCalibrationPoint(frequency, levelHL, channel, rms);
                    break;

                case CalibrationSet.Broadband:
                    resultsCalibration.Broadband.SetCalibrationValue(levelHL, channel, rms);
                    break;

                default:
                    Debug.LogError($"Unexpected CalibrationSet: {set}");
                    break;
            }
        }

        public static void InitiateCalibration(TransducerProfile transducerProfile)
        {
            resultsCalibration = new CalibrationProfile(transducerProfile);
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
            if (!(resultsCalibration is null))
            {
                return Source.Results;
            }
            else if (!(customCalibration is null))
            {
                return Source.Custom;
            }

            return Source.Default;
        }

        /// <summary>
        /// Returns the scale factor per RMS.
        /// To use, multiple every sample by the output of this divided by the stream RMS.
        /// </summary>
        public static (double leftRMS, double rightRMS) GetLevelRMS(
            double levelHL,
            CalibrationSet calibrationSet,
            double calibrationFrequency,
            Source source = Source.Custom)
        {
            CalibrationProfile calibrationProfile;
            switch (source)
            {
                case Source.Results:
                    if (resultsCalibration == null)
                    {
                        goto case Source.Custom;
                    }
                    calibrationProfile = resultsCalibration;
                    break;

                case Source.Custom:
                    if (customCalibration == null)
                    {
                        Debug.LogError($"Fallback behavior with Audiometric Calibration for Source: {source}");
                        goto case Source.Default;
                    }
                    calibrationProfile = customCalibration;
                    break;

                case Source.Default:
                    {
                        //Calculate
                        double value = (1.0 / 32.0) * Math.Pow(10.0, (levelHL - 110.0) / 20.0);
                        return (value, value);
                    }

                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    goto case Source.Default;
            }

            return calibrationProfile.GetRMS(calibrationSet, calibrationFrequency, levelHL);
        }

        public static double EstimateRMS(
            CalibrationSet calibrationSet,
            AudioChannel channel,
            double frequency,
            double levelHL)
        {
            if (resultsCalibration == null)
            {
                throw new Exception($"Unable to EstimateRMS with uninitialized calibration");
            }

            return resultsCalibration.EstimateRMS(calibrationSet, channel, frequency, levelHL);
        }

        public static double GetLevelSPL(double frequency, double levelHL)
        {
            if (resultsCalibration == null)
            {
                throw new Exception($"Unable to use GetLevelSPL with uninitialized calibration");
            }

            return resultsCalibration.TransducerProfile.GetSPL(frequency, levelHL);
        }
    }
}
