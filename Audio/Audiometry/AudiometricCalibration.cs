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
        private static CalibrationProfile calibrationResults = null;

        private static ValidationResults validationResults = null;

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
                    successCallback: DeserializeCalibrationSettings,
                    failCallback: SerializeCalibrationSettings,
                    fileNotFoundCallback: SerializeCalibrationSettings);
            }
        }

        private static void DeserializeCalibrationSettings(JsonObject parsedValue)
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
                    failCallback: FailedToLoadCalibrationSettings,
                    fileNotFoundCallback: FailedToLoadCalibrationSettings);
            }
            else
            {
                customCalibration = null;
            }
        }

        private static void FailedToLoadCalibrationSettings()
        {
            Debug.LogError(
                $"Unable to load calibration file: {DataManagement.PathForDataFile(dataDirectory, currentCalibrationName)}");
            currentCalibrationName = null;
            customCalibration = null;
        }

        public static void SerializeCalibrationSettings()
        {
            FileWriter.WriteJson(
                path: DataManagement.PathForDataFile(systemDirectory, configFileName),
                createJson: () =>
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
                },
                pretty: true);
        }

        public static void PushCalibrationValue(
            double levelHL,
            CalibrationSet set,
            double frequency,
            AudioChannel channel,
            double rms)
        {
            if (calibrationResults is null)
            {
                throw new Exception($"Calibration not initialized");
            }

            switch (set)
            {
                case CalibrationSet.PureTone:
                    calibrationResults.PureTone.SetCalibrationPoint(frequency, levelHL, channel, rms);
                    break;

                case CalibrationSet.Narrowband:
                    calibrationResults.Narrowband.SetCalibrationPoint(frequency, levelHL, channel, rms);
                    break;

                case CalibrationSet.Broadband:
                    calibrationResults.Broadband.SetCalibrationValue(levelHL, channel, rms);
                    break;

                default:
                    Debug.LogError($"Unexpected CalibrationSet: {set}");
                    break;
            }

            UpdateCalibrationProgressFile();
        }

        public static void PushOscillatorCalibrationValue(
            double levelHL,
            double frequency,
            AudioChannel channel,
            double attenuation)
        {
            if (calibrationResults is null)
            {
                throw new Exception($"Calibration not initialized");
            }

            calibrationResults.Oscillator.SetCalibrationPoint((int)frequency, levelHL, channel, ConvertOscillatorAttenuationToRMS(attenuation));

            UpdateCalibrationProgressFile();
        }

        private static string calibrationProgressFilePath = null;
        private static string CalibrationProgressFilePath => calibrationProgressFilePath ??= DataManagement.PathForDataFile(dataDirectory, $"Calibration_Progress.json");

        private static void UpdateCalibrationProgressFile()
        {
            DeleteCalibrationProgressFile();

            FileWriter.WriteJson(
                path: CalibrationProgressFilePath,
                createJson: calibrationResults.Serialize,
                pretty: true);
        }

        private static void DeleteCalibrationProgressFile()
        {
            if (System.IO.File.Exists(CalibrationProgressFilePath))
            {
                System.IO.File.Delete(CalibrationProgressFilePath);
            }
        }

        private static string validationProgressFilePath = null;
        private static string ValidationProgressFilePath => validationProgressFilePath ??= DataManagement.PathForDataFile(dataDirectory, $"Validation_Progress.json");

        private static void UpdateValidationProgressFile()
        {
            DeleteValidationProgressFile();

            FileWriter.WriteJson(
                path: ValidationProgressFilePath,
                createJson: validationResults.Serialize,
                pretty: true);
        }

        private static void DeleteValidationProgressFile()
        {
            if (System.IO.File.Exists(ValidationProgressFilePath))
            {
                System.IO.File.Delete(ValidationProgressFilePath);
            }
        }

        public static void InitiateCalibration(TransducerProfile transducerProfile)
        {
            calibrationResults = new CalibrationProfile(transducerProfile);
        }

        public static void FinalizeCalibrationResults()
        {
            if (calibrationResults == null)
            {
                Debug.LogError("Null calibration results");
                return;
            }

            customCalibration = calibrationResults;
            calibrationResults = null;

            currentCalibrationName = $"Calibration_{customCalibration.CalibrationDate:yy_MM_dd_HH_mm_ss}.json";

            FileWriter.WriteJson(
                path: DataManagement.PathForDataFile(dataDirectory, currentCalibrationName),
                createJson: customCalibration.Serialize,
                pretty: true);

            SerializeCalibrationSettings();

            DeleteCalibrationProgressFile();
        }

        public static void DropCalibrationResults(Source source)
        {
            switch (source)
            {
                case Source.Custom:
                    customCalibration = null;
                    SerializeCalibrationSettings();
                    break;

                case Source.Results:
                    calibrationResults = null;
                    break;

                case Source.Default:
                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    break;
            }
        }

        public static bool HasCalibrationProfile() => customCalibration != null;

        /// <summary>
        /// Returns the scale factor per RMS.
        /// To use, multiple every sample by the output of this divided by the stream RMS.
        /// </summary>
        public static double GetLevelRMS(
            double levelHL,
            CalibrationSet calibrationSet,
            double calibrationFrequency,
            AudioChannel channel,
            Source source = Source.Custom)
        {
            CalibrationProfile calibrationProfile;
            switch (source)
            {
                case Source.Results:
                    if (calibrationResults == null)
                    {
                        goto case Source.Custom;
                    }
                    calibrationProfile = calibrationResults;
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
                    //Calculate
                    return (1.0 / 32.0) * Math.Pow(10.0, (levelHL - 110.0) / 20.0);

                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    goto case Source.Default;
            }

            return calibrationProfile.GetRMS(calibrationSet, calibrationFrequency, levelHL, channel);
        }

        public static double EstimateRMS(
            Source calibrationSource,
            CalibrationSet calibrationSet,
            double frequency,
            double levelHL,
            AudioChannel channel)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    if (customCalibration == null)
                    {
                        throw new Exception($"Unable to EstimateRMS without calibration profile");
                    }
                    return customCalibration.EstimateRMS(calibrationSet, frequency, levelHL, channel);

                case Source.Results:
                    if (calibrationResults == null)
                    {
                        throw new Exception($"Unable to EstimateRMS with uninitialized calibration");
                    }
                    return calibrationResults.EstimateRMS(calibrationSet, frequency, levelHL, channel);

                case Source.Default:
                    throw new Exception($"Unable to EstimateRMS with uninitialized calibration");

                default:
                    throw new Exception($"Unexpected CalibrationSource for EstimateRMS: {calibrationSource}");
            }
        }

        /// <summary>
        /// Returns the attenuation for the oscillator
        /// </summary>
        public static double GetOscillatorAttenuation(
            double levelHL,
            double calibrationFrequency,
            AudioChannel channel,
            Source source = Source.Custom)
        {
            CalibrationProfile calibrationProfile;
            switch (source)
            {
                case Source.Results:
                    if (calibrationResults == null)
                    {
                        goto case Source.Custom;
                    }
                    calibrationProfile = calibrationResults;
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
                    //Calculate
                    return 130 - levelHL;

                default:
                    Debug.LogError($"Unexpected Source: {source}");
                    goto case Source.Default;
            }

            return calibrationProfile.GetOscillatorAttenuation(calibrationFrequency, levelHL, channel);
        }

        public static double EstimateOscillatorAttenuation(
            Source calibrationSource,
            double frequency,
            double levelHL,
            AudioChannel channel)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    if (customCalibration == null)
                    {
                        throw new Exception($"Unable to EstimateRMS without calibration profile");
                    }
                    return customCalibration.EstimateOscillatorAttenuation(frequency, levelHL, channel);

                case Source.Results:
                    if (calibrationResults == null)
                    {
                        throw new Exception($"Unable to EstimateRMS with uninitialized calibration");
                    }
                    return calibrationResults.EstimateOscillatorAttenuation(frequency, levelHL, channel);

                case Source.Default:
                    throw new Exception($"Unable to EstimateRMS with uninitialized calibration");

                default:
                    throw new Exception($"Unexpected CalibrationSource for EstimateRMS: {calibrationSource}");
            }
        }

        public static bool IsCalibrationReady() => customCalibration != null;

        public static double GetLevelSPL(
            Source calibrationSource,
            double frequency,
            double levelHL)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    if (customCalibration == null)
                    {
                        throw new Exception($"Unable to use GetLevelSPL without calibration profile");
                    }
                    return customCalibration.TransducerProfile.GetSPL(frequency, levelHL);

                case Source.Results:
                    if (calibrationResults == null)
                    {
                        throw new Exception($"Unable to use GetLevelSPL with uninitialized calibration");
                    }
                    return calibrationResults.TransducerProfile.GetSPL(frequency, levelHL);

                case Source.Default:
                case Source.MAX:
                default:
                    throw new Exception($"Unexpected CalibrationSource for GetLevelSPL: {calibrationSource}");
            }
        }

        public static double GetLevelHL(
            Source calibrationSource,
            double frequency,
            double levelSPL)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    if (customCalibration == null)
                    {
                        throw new Exception($"Unable to use GetLevelHL without calibration profile");
                    }
                    return customCalibration.TransducerProfile.GetHL(frequency, levelSPL);

                case Source.Results:
                    if (calibrationResults == null)
                    {
                        throw new Exception($"Unable to use GetLevelHL with uninitialized calibration");
                    }
                    return calibrationResults.TransducerProfile.GetHL(frequency, levelSPL);

                case Source.Default:
                case Source.MAX:
                default:
                    throw new Exception($"Unexpected CalibrationSource for GetLevelHL: {calibrationSource}");
            }
        }

        public static double ConvertOscillatorRMSToAttenuation(double rms) => -20 * Math.Log10(Math.Sqrt(2.0) * rms);
        public static double ConvertOscillatorAttenuationToRMS(double attenuation) => Math.Pow(10.0, -attenuation / 20.0) / Math.Sqrt(2.0);

        #region Validation

        public static void PushValidationValue(
            double levelHL,
            CalibrationSet set,
            double frequency,
            AudioChannel channel,
            double expectedRMS,
            double measuredLevelHL)
        {
            if (validationResults is null)
            {
                throw new Exception($"Validation not initialized");
            }

            switch (set)
            {
                case CalibrationSet.PureTone:
                    validationResults.PureTone.SetValidationPoint(frequency, levelHL, channel, expectedRMS, measuredLevelHL);
                    break;

                case CalibrationSet.Narrowband:
                    validationResults.Narrowband.SetValidationPoint(frequency, levelHL, channel, expectedRMS, measuredLevelHL);
                    break;

                case CalibrationSet.Broadband:
                    validationResults.Broadband.SetValidationValue(levelHL, channel, expectedRMS, measuredLevelHL);
                    break;

                default:
                    Debug.LogError($"Unexpected CalibrationSet: {set}");
                    break;
            }

            UpdateValidationProgressFile();
        }

        public static void PushOscillatorValidationValue(
            double levelHL,
            double frequency,
            AudioChannel channel,
            double expectedAttenuation,
            double measuredLevelHL)
        {
            if (validationResults is null)
            {
                throw new Exception($"Validation not initialized");
            }

            validationResults.Oscillator.SetValidationPoint(frequency, levelHL, channel, expectedAttenuation, measuredLevelHL);

            UpdateValidationProgressFile();
        }

        public static void InitiateValidation(TransducerProfile transducerProfile)
        {
            validationResults = new ValidationResults(transducerProfile);
        }

        public static ValidationResults FinalizeValidationResults()
        {
            if (validationResults == null)
            {
                Debug.LogError("Null validation results");
                return null;
            }

            string currentValidationName = $"Validation_{validationResults.ValidationDate:yy_MM_dd_HH_mm_ss}.json";

            FileWriter.WriteJson(
                path: DataManagement.PathForDataFile(dataDirectory, currentValidationName),
                createJson: validationResults.Serialize,
                pretty: true);

            DeleteValidationProgressFile();

            return validationResults;
        }

        public static void DropValidationResults()
        {
            validationResults = null;
        }

        public static CalibrationProfile GetCustomCalibration() => customCalibration;

        #endregion Validation
    }
}
