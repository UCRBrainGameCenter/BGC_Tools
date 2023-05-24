using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BGC.IO;
using BGC.Users;
using LightJson;
using UnityEngine;

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

        private const string ErrorThresholdKey = "AudiometricCalibrationErrorThreshold";

        private static string currentCalibrationName = null;
        private static CalibrationProfile customCalibration = null;
        private static CalibrationProfile calibrationResults = null;
        private static readonly Dictionary<string, CalibrationProfile> calibrationProfilesByName = new Dictionary<string, CalibrationProfile>();
        private static readonly List<CalibrationProfile> calibrationProfilesByDate = new List<CalibrationProfile>();

        private static CalibrationProfile incompleteCalibration = null;
        private static DateTime incompleteCalibrationDate = DateTime.MinValue;

        private static ValidationResults validationResults = null;

        private static double micCalibrationOffset = double.NaN;

        private static bool initialized = false;

        private static double calibrationErrorThreshold = 1.0;

        private static class Keys
        {
            public const string Version = "Version";

            public const string CurrentCalibration = "Current";
            public const string MicCalibrationOffset = "MicCalibrationOffset";
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

                LoadAllCalibrationFiles();

                FileReader.ReadJsonFile(
                    path: DataManagement.PathForDataFile(systemDirectory, configFileName),
                    successCallback: DeserializeCalibrationSettings,
                    failCallback: SerializeCalibrationSettings,
                    fileNotFoundCallback: SerializeCalibrationSettings);

                // If no calibration setting was found, use the latest one
                if (calibrationProfilesByDate.Count > 0 && customCalibration == null)
                {
                    customCalibration = calibrationProfilesByDate.Where(c => c.IsComplete()).FirstOrDefault();
                }

                calibrationErrorThreshold = PlayerData.GlobalData.GetDouble(ErrorThresholdKey, 1.0);
            }
        }

        public static double RMSToDB(double rms) => 20.0 * Math.Log10(rms);

        public static double GetCalibrationErrorThreshold() => calibrationErrorThreshold;

        public static void SetCalibrationErrorThreshold(double threshold)
        {
            if (threshold <= 0)
            {
                threshold = 0;
            }
            calibrationErrorThreshold = threshold;
            PlayerData.GlobalData.SetDouble(ErrorThresholdKey, calibrationErrorThreshold);
            PlayerData.Save();
        }

        // Load all calibration data in the data directory
        private static void LoadAllCalibrationFiles()
        {
            foreach (string fileName in Directory.EnumerateFiles(
                    DataManagement.PathForDataDirectory(dataDirectory),
                    "*.json",
                    new EnumerationOptions()
                    {
                        AttributesToSkip = FileAttributes.Directory,
                        MatchCasing = MatchCasing.CaseInsensitive,
                        RecurseSubdirectories = false,
                        IgnoreInaccessible = true,
                    }))
            {
                bool isProgressFile = fileName.EndsWith("_Progress.json");
                FileReader.ReadJsonFile(
                    path: fileName,
                    successCallback: data =>
                    {
                        try
                        {
                            if (data.ContainsKey("CalibrationDate"))
                            {
                                CalibrationProfile calibrationProfile = new CalibrationProfile(data);
                                if (isProgressFile)
                                {
                                    if (incompleteCalibration == null || incompleteCalibrationDate < calibrationProfile.CalibrationDate)
                                    {
                                        incompleteCalibration = calibrationProfile;
                                        incompleteCalibrationDate = calibrationProfile.CalibrationDate;
                                    }
                                }
                                else
                                {
                                    calibrationProfilesByName.Add(Path.GetFileName(fileName), calibrationProfile);
                                    calibrationProfilesByDate.Add(calibrationProfile);
                                }
                            }
                            else if (data.ContainsKey("ValidationDate"))
                            {
                                if (!isProgressFile)
                                {
                                    ValidationResults curValidationResults = new ValidationResults(data);
                                    if (validationResults == null || validationResults.ValidationDate < curValidationResults.ValidationDate)
                                    {
                                        validationResults = curValidationResults;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    });
            }

            SortCalibrationProfilesByDate();

            // Don't use validation from previous calibrations
            if (validationResults != null)
            {
                if (calibrationProfilesByDate.Count == 0 || validationResults.ValidationDate < calibrationProfilesByDate[0].CalibrationDate)
                {
                    validationResults = null;
                }
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

            if (parsedValue.ContainsKey(Keys.MicCalibrationOffset))
            {
                micCalibrationOffset = parsedValue[Keys.MicCalibrationOffset].AsNumber;
            }
            else
            {
                micCalibrationOffset = double.NaN;
            }

            if (!string.IsNullOrEmpty(currentCalibrationName))
            {
                if (calibrationProfilesByName.ContainsKey(currentCalibrationName) &&
                    calibrationProfilesByName[currentCalibrationName].IsComplete())
                {
                    customCalibration = calibrationProfilesByName[currentCalibrationName];
                }
                else
                {
                    FailedToLoadCalibrationSettings();
                }
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

#pragma warning disable IDE0060 // Remove unused parameter
        public static void SubmitMicCalibrationOffset(double measuredPower, double presentationLevel, double frequency = double.NaN)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            micCalibrationOffset = presentationLevel - measuredPower;

            SerializeCalibrationSettings();
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static double GetCorrectedMicLevel(double power, double frequency = double.NaN)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (!double.IsFinite(micCalibrationOffset))
            {
                return power;
            }

            return power + micCalibrationOffset;
        }

        public static void CorrectPSD(double[] psd)
        {
            if (!double.IsFinite(micCalibrationOffset))
            {
                return;
            }

            for (int i = 0; i < psd.Length; ++i)
            {
                psd[i] += micCalibrationOffset;
            }
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

                    if (!double.IsNaN(micCalibrationOffset))
                    {
                        data.Add(Keys.MicCalibrationOffset, micCalibrationOffset);
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
            incompleteCalibration = calibrationResults;
            incompleteCalibrationDate = calibrationResults.CalibrationDate;
        }

        public static void InitiateCalibration(CalibrationProfile existingProfile)
        {
            calibrationResults = existingProfile;
            incompleteCalibration = calibrationResults;
            incompleteCalibrationDate = calibrationResults.CalibrationDate;
        }

        public static CalibrationProfile GetIncompleteCalibration() => incompleteCalibration;

        public static void FinalizeCalibrationResults()
        {
            if (calibrationResults == null)
            {
                Debug.LogError("Null calibration results");
                return;
            }

            customCalibration = calibrationResults;
            calibrationResults = null;
            incompleteCalibration = null;
            incompleteCalibrationDate = DateTime.MinValue;

            currentCalibrationName = $"Calibration_{customCalibration.CalibrationDate:yy_MM_dd_HH_mm_ss}.json";

            FileWriter.WriteJson(
                path: DataManagement.PathForDataFile(dataDirectory, currentCalibrationName),
                createJson: customCalibration.Serialize,
                pretty: true);

            SerializeCalibrationSettings();

            DeleteCalibrationProgressFile();

            calibrationProfilesByName.Add(currentCalibrationName, customCalibration);
            calibrationProfilesByDate.Add(customCalibration);
            validationResults = null;
            SortCalibrationProfilesByDate();
        }

        private static void SortCalibrationProfilesByDate()
        {
            calibrationProfilesByDate.Sort((profile1, profile2) => DateTime.Compare(profile2.CalibrationDate, profile1.CalibrationDate));
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

        public static int NumHistoricCalibrationProfiles() => calibrationProfilesByDate.Count;

        public static CalibrationProfile GetHistoricCalibrationProfile(int index) => index >= 0 && index < calibrationProfilesByDate.Count ? calibrationProfilesByDate[index] : customCalibration;

        public static DateTime GetHistoricCalibrationDate(int index) => GetHistoricCalibrationProfile(index).CalibrationDate;

        public static ValidationResults GetValidationResults() => validationResults;

        public static bool IsHistoricCalibrationComplete(int index) => GetHistoricCalibrationProfile(index).IsComplete();

        public static void SetCustomCalibration(int index)
        {
            if (index >= 0 && index < calibrationProfilesByDate.Count)
            {
                CalibrationProfile selectedCalibration = GetHistoricCalibrationProfile(index);
                if (!selectedCalibration.IsComplete())
                {
                    throw new ArgumentException($"Cannot set a custom calibration file that is not complete.");
                }

                customCalibration = GetHistoricCalibrationProfile(index);
                currentCalibrationName = calibrationProfilesByName.FirstOrDefault(x => x.Value == customCalibration).Key;
                SerializeCalibrationSettings();
            }
        }

        public static bool IsSelectedHistoricCalibrationProfile(int index) => index >= 0 && index < calibrationProfilesByDate.Count && calibrationProfilesByDate[index] == customCalibration;

        /// <summary>
        /// Returns the scale factor per RMS.
        /// To use, multiple every sample by the output of this divided by the stream RMS.
        /// </summary>
        public static double GetLevelRMS(
            double levelHL,
            CalibrationSet calibrationSet,
            double calibrationFrequency,
            AudioChannel channel,
            Source source = Source.Custom,
            int historicCalibrationIndex = -1)
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
                    calibrationProfile = GetHistoricCalibrationProfile(historicCalibrationIndex);
                    if (calibrationProfile == null)
                    {
                        Debug.LogError($"Fallback behavior with Audiometric Calibration for Source: {source}");
                        goto case Source.Default;
                    }
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
            AudioChannel channel,
            int historicCalibrationIndex = -1)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    CalibrationProfile calibrationProfile = GetHistoricCalibrationProfile(historicCalibrationIndex);
                    if (calibrationProfile == null)
                    {
                        throw new Exception($"Unable to EstimateRMS without calibration profile");
                    }
                    return calibrationProfile.EstimateRMS(calibrationSet, frequency, levelHL, channel);

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
            Source source = Source.Custom,
            int historicCalibrationIndex = -1)
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
                    calibrationProfile = GetHistoricCalibrationProfile(historicCalibrationIndex);
                    if (calibrationProfile == null)
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
            AudioChannel channel,
            int historicCalibrationIndex = -1)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    CalibrationProfile calibrationProfile = GetHistoricCalibrationProfile(historicCalibrationIndex);
                    if (calibrationProfile == null)
                    {
                        throw new Exception($"Unable to EstimateRMS without calibration profile");
                    }
                    return calibrationProfile.EstimateOscillatorAttenuation(frequency, levelHL, channel);

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
            double levelHL,
            int historicCalibrationIndex = -1)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    CalibrationProfile calibrationProfile = GetHistoricCalibrationProfile(historicCalibrationIndex);
                    if (calibrationProfile == null)
                    {
                        throw new Exception($"Unable to use GetLevelSPL without calibration profile");
                    }
                    return calibrationProfile.TransducerProfile.GetSPL(frequency, levelHL);

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
            double levelSPL,
            int historicCalibrationIndex = -1)
        {
            switch (calibrationSource)
            {
                case Source.Custom:
                    CalibrationProfile calibrationProfile = GetHistoricCalibrationProfile(historicCalibrationIndex);
                    if (calibrationProfile == null)
                    {
                        throw new Exception($"Unable to use GetLevelHL without calibration profile");
                    }
                    return calibrationProfile.TransducerProfile.GetHL(frequency, levelSPL);

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
