using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using BGC.Mathematics;
using BGC.Audio.Extensions;

namespace BGC.Audio.Audiometry
{
    public class CalibrationProfile
    {
        private int CURRENT_VERSION = 3;
        public int Version { get; }

        public DateTime CalibrationDate { get; }

        public FilterBehavior FilterBehavior { get; }

        public TransducerProfile TransducerProfile { get; }

        public FrequencyCollection Oscillator { get; }
        public FrequencyCollection PureTone { get; }
        public FrequencyCollection Narrowband { get; }
        public LevelCollection Broadband { get; }

        public CalibrationProfile(TransducerProfile transducerProfile, FilterBehavior filterBehavior)
        {
            Version = CURRENT_VERSION;

            CalibrationDate = DateTime.Now;

            FilterBehavior = filterBehavior;

            TransducerProfile = transducerProfile;

            Oscillator = new FrequencyCollection();
            PureTone = new FrequencyCollection();
            Narrowband = new FrequencyCollection();
            Broadband = new LevelCollection();
        }

        public CalibrationProfile(JsonObject data)
        {
            Version = data.ContainsKey("Version") ? data["Version"].AsInteger : 1;

            CalibrationDate = data["CalibrationDate"].AsDateTime.Value;

            if (data.ContainsKey("FilterBehavior"))
            {
                FilterBehavior = DeserializeFilterBehavior(data["FilterBehavior"].AsString);
            }
            else
            {
                FilterBehavior = FilterBehavior.PureToneForHighFrequencies;
            }

            TransducerProfile = new TransducerProfile(data["Transducer"]);

            if (Version > 1)
            {
                Oscillator = new FrequencyCollection(data["Oscillator"]);
            }

            PureTone = new FrequencyCollection(data["PureTone"]);
            Narrowband = new FrequencyCollection(data["Narrowband"]);
            Broadband = new LevelCollection(data["Broadband"]);
        }

        public JsonObject Serialize() => new JsonObject()
        {
            ["Version"] = Version,

            ["CalibrationDate"] = CalibrationDate,

            ["FilterBehavior"] = SerializeFilterBehavior(FilterBehavior),

            ["Transducer"] = TransducerProfile.Serialize(),

            ["Oscillator"] = Oscillator.Serialize(),
            ["PureTone"] = PureTone.Serialize(),
            ["Narrowband"] = Narrowband.Serialize(),
            ["Broadband"] = Broadband.Serialize()
        };

        public bool IsLatestVersion => Version == CURRENT_VERSION;

        public bool IsComplete()
        {
            return Oscillator != null && Oscillator.IsComplete() &&
                PureTone != null && PureTone.IsComplete() && 
                Narrowband != null && Narrowband.IsComplete() && 
                Broadband != null && Broadband.IsComplete();
        }

        public double EstimateAttenuation(
            AudiometricCalibration.CalibrationSet calibrationSet,
            double frequency,
            double levelSPL,
            AudioChannel channel)
        {
            double attenuationEstimate = double.NaN;

            switch (calibrationSet)
            {
                case AudiometricCalibration.CalibrationSet.PureTone:
                    if (PureTone.Points.Count > 0)
                    {
                        attenuationEstimate = PureTone.GetAttenuation(frequency, levelSPL, channel);

                        if (double.IsNaN(attenuationEstimate))
                        {
                            attenuationEstimate = PureTone.GetAttenuation(frequency, levelSPL, channel.Flip());
                        }
                    }

                    if (!double.IsNaN(attenuationEstimate))
                    {
                        return attenuationEstimate;
                    }

                    //Gross estimate to start with
                    return 91.0 - levelSPL;

                case AudiometricCalibration.CalibrationSet.Narrowband:
                    if (Narrowband.Points.Count > 0)
                    {
                        attenuationEstimate = Narrowband.GetAttenuation(frequency, levelSPL, channel);

                        if (double.IsNaN(attenuationEstimate))
                        {
                            attenuationEstimate = Narrowband.GetAttenuation(frequency, levelSPL, channel.Flip());
                        }
                    }

                    if (!double.IsNaN(attenuationEstimate))
                    {
                        return attenuationEstimate;
                    }
                    goto case AudiometricCalibration.CalibrationSet.PureTone;

                case AudiometricCalibration.CalibrationSet.Broadband:
                    frequency = 2000.0;
                    if (Broadband.Points.Count > 0)
                    {
                        attenuationEstimate = Broadband.GetAttenuation(levelSPL, channel);

                        if (double.IsNaN(attenuationEstimate))
                        {
                            attenuationEstimate = Broadband.GetAttenuation(levelSPL, channel.Flip());
                        }
                    }

                    if (!double.IsNaN(attenuationEstimate))
                    {
                        return attenuationEstimate;
                    }
                    goto case AudiometricCalibration.CalibrationSet.Narrowband;

                default:
                    UnityEngine.Debug.LogError($"Unsupported CalibrationSet: {calibrationSet}");
                    goto case AudiometricCalibration.CalibrationSet.PureTone;
            }
        }

        public double GetAttenuation(
            AudiometricCalibration.CalibrationSet calibrationSet,
            double frequency,
            double levelSPL,
            AudioChannel channel)
        {
            switch (calibrationSet)
            {
                case AudiometricCalibration.CalibrationSet.PureTone:
                    return PureTone.GetAttenuation(frequency, levelSPL, channel);

                case AudiometricCalibration.CalibrationSet.Narrowband:
                    return Narrowband.GetAttenuation(frequency, levelSPL, channel);

                case AudiometricCalibration.CalibrationSet.Broadband:
                    return Broadband.GetAttenuation(levelSPL, channel);

                default:
                    UnityEngine.Debug.LogError($"Unsupported CalibrationSet: {calibrationSet}");
                    goto case AudiometricCalibration.CalibrationSet.PureTone;
            }
        }

        public double GetOscillatorAttenuation(
            double frequency,
            double levelSPL,
            AudioChannel channel) => Oscillator.GetOscillatorAttenuation(frequency, levelSPL, channel);

        public double EstimateOscillatorAttenuation(
            double frequency,
            double levelSPL,
            AudioChannel channel)
        {
            double oscillatorEstimate = double.NaN;

            if (Oscillator.Points.Count > 0)
            {
                oscillatorEstimate = Oscillator.GetOscillatorAttenuation(frequency, levelSPL, channel);

                if (double.IsNaN(oscillatorEstimate))
                {
                    oscillatorEstimate = Oscillator.GetOscillatorAttenuation(frequency, levelSPL, channel.Flip());
                }
            }

            if (!double.IsNaN(oscillatorEstimate))
            {
                return oscillatorEstimate;
            }

            //Gross estimate to start with
            return 130 - levelSPL;
        }

        private static string SerializeFilterBehavior(FilterBehavior filterBehavior) => filterBehavior switch
        {
            FilterBehavior.PureToneForHighFrequencies => "PureToneForHighFrequencies",
            _ => "AlwaysNarrowband",
        };

        private static FilterBehavior DeserializeFilterBehavior(string filterBehavior) => filterBehavior switch
        {
            "PureToneForHighFrequencies" => FilterBehavior.PureToneForHighFrequencies,
            _ => FilterBehavior.AlwaysNarrowband,
        };
    }


    public class FrequencyCollection
    {
        public List<FrequencyPoint> Points { get; }

        public FrequencyCollection()
        {
            Points = new List<FrequencyPoint>();
        }

        public FrequencyCollection(JsonArray data)
        {
            Points = new List<FrequencyPoint>();

            foreach (JsonObject frequencyPoint in data)
            {
                Points.Add(new FrequencyPoint(frequencyPoint));
            }
        }

        public bool IsComplete()
        {
            return Points.Count > 0 && Points.Any(p => p != null && p.IsComplete());
        }

        public void SetCalibrationPoint(
            double frequency,
            double levelSPL,
            AudioChannel channel,
            double attenuation) =>
            GetLevelCollection(frequency)
            .SetCalibrationValue(levelSPL, channel, attenuation);

        private LevelCollection GetLevelCollection(double frequency)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].Frequency == frequency)
                {
                    //Found target frequency
                    return Points[i].Levels;
                }

                if (Points[i].Frequency > frequency)
                {
                    //Passed target frequency - create new
                    Points.Insert(i, new FrequencyPoint(frequency));
                    return Points[i].Levels;
                }
            }

            //Reached the end without finding it
            Points.Add(new FrequencyPoint(frequency));
            return Points[Points.Count - 1].Levels;
        }

        public bool TryGetLevelCollection(double frequency, out LevelCollection levelCollection)
        {
            levelCollection = null;

            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].Frequency == frequency)
                {
                    //Found target frequency
                    levelCollection = Points[i].Levels;
                    return true;
                }
            }

            return false;
        }

        public JsonArray Serialize()
        {
            JsonArray points = new JsonArray();
            foreach (FrequencyPoint point in Points)
            {
                points.Add(point.Serialize());
            }

            return points;
        }

        public double GetAttenuation(double frequency, double levelSPL, AudioChannel channel)
        {
            //Find Frequency

            if (frequency <= Points[0].Frequency)
            {
                return Points[0].Levels.GetAttenuation(levelSPL, channel);
            }

            if (frequency >= Points[Points.Count - 1].Frequency)
            {
                return Points[Points.Count - 1].Levels.GetAttenuation(levelSPL, channel);
            }

            //Frequency is between the upper and lower bounds of the FrequencyPoints

            int upperBound;

            for (upperBound = 1; upperBound < Points.Count - 1; upperBound++)
            {
                if (frequency < Points[upperBound].Frequency)
                {
                    //Found the first FrequencyPoint frequency larger than the target
                    break;
                }
            }

            //Could be equal to the lowerbound
            if (frequency == Points[upperBound - 1].Frequency)
            {
                return Points[upperBound - 1].Levels.GetAttenuation(levelSPL, channel);
            }
            else
            {
                // Interpolate between two adjacent FrequencyPoints
                double freqLower = Points[upperBound - 1].Frequency;
                double freqUpper = Points[upperBound].Frequency;

                // Get the attenuation at the lower and upper bounds
                double lowerBoundAttenuationDB = Points[upperBound - 1].Levels.GetAttenuation(levelSPL, channel);
                double upperBoundAttenuationDB = Points[upperBound].Levels.GetAttenuation(levelSPL, channel);

                // Convert to linear scale
                double lowerBoundAttenuationLinear = Math.Pow(10, lowerBoundAttenuationDB / 20.0);
                double upperBoundAttenuationLinear = Math.Pow(10, upperBoundAttenuationDB / 20.0);

                // Interpolate linearly between the two bounds, but use a logarithmic scale due to the frequency domain
                double t = Math.Log(frequency / freqLower) / Math.Log(freqUpper / freqLower);
                double attenuationLinear = GeneralMath.Lerp(lowerBoundAttenuationLinear, upperBoundAttenuationLinear, t);

                double attenuationDB = 20.0 * Math.Log10(attenuationLinear);
                return attenuationDB;
            }
        }

        public void SetOscillatorCalibrationPoint(
            double frequency,
            double levelSPL,
            AudioChannel channel,
            double attenuation) =>
            GetLevelCollection(frequency)
            .SetCalibrationValue(levelSPL, channel, attenuation);

        public double GetOscillatorAttenuation(double frequency, double levelSPL, AudioChannel channel) =>
            GetAttenuation(frequency, levelSPL, channel);

        public class FrequencyPoint
        {
            public double Frequency { get; }
            public LevelCollection Levels { get; }

            public FrequencyPoint(double frequency)
            {
                Frequency = frequency;
                Levels = new LevelCollection();
            }

            public FrequencyPoint(JsonObject data)
            {
                Frequency = data["Frequency"];
                Levels = new LevelCollection(data["Levels"].AsJsonArray);
            }

            public JsonObject Serialize() => new JsonObject()
            {
                ["Frequency"] = Frequency,
                ["Levels"] = Levels.Serialize()
            };

            public bool IsComplete()
            {
                return Levels.IsComplete();
            }
        }
    }

    public class LevelCollection
    {
        public List<CalibrationPoint> Points { get; }

        public LevelCollection()
        {
            Points = new List<CalibrationPoint>();
        }

        public LevelCollection(JsonArray data)
        {
            Points = new List<CalibrationPoint>();

            foreach (JsonObject calibrationPoint in data)
            {
                Points.Add(new CalibrationPoint(calibrationPoint));
            }
        }

        public JsonArray Serialize()
        {
            JsonArray points = new JsonArray();
            foreach (CalibrationPoint point in Points)
            {
                points.Add(point.Serialize());
            }

            return points;
        }

        public bool IsComplete()
        {
            return Points.Count > 0 && Points.Any(p => p != null && p.IsComplete());
        }

        public void SetCalibrationValue(
            double levelSPL,
            AudioChannel channel,
            double attenuation)
        {
            switch (channel)
            {
                case AudioChannel.Left:
                    GetCalibrationPoint(levelSPL).LeftAttenuation = attenuation;
                    break;

                case AudioChannel.Right:
                    GetCalibrationPoint(levelSPL).RightAttenuation = attenuation;
                    break;

                default:
                    throw new Exception($"Unexpected AudioChannel for Setting Calibration {channel}");
            }

        }

        private CalibrationPoint GetCalibrationPoint(double levelSPL)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].LevelSPL == levelSPL)
                {
                    //Found target level
                    return Points[i];
                }

                if (Points[i].LevelSPL > levelSPL)
                {
                    //Passed target level - create new
                    Points.Insert(i, new CalibrationPoint(levelSPL));
                    return Points[i];
                }
            }

            //Reached the end without finding it
            Points.Add(new CalibrationPoint(levelSPL));
            return Points[Points.Count - 1];
        }

        public bool TryGetCalibrationPoint(double levelSPL, out CalibrationPoint calibrationPoint)
        {
            calibrationPoint = null;
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].LevelSPL == levelSPL)
                {
                    //Found target level
                    calibrationPoint = Points[i];
                    return true;
                }
            }

            return false;
        }

        public double GetAttenuation(double levelSPL, AudioChannel channel)
        {
            List<CalibrationPoint> validPoints = Points.Where(x => !double.IsNaN(x.GetAttenuation(channel))).ToList();

            if (validPoints.Count == 0)
            {
                return double.NaN;
            }

            //Check below min
            if (levelSPL < validPoints[0].LevelSPL)
            {
                // Add additional attenuation to the first point
                double additionalAttenuationDB = validPoints[0].LevelSPL - levelSPL;
                return validPoints[0].GetAttenuation(channel) + additionalAttenuationDB;
            }
            else if (levelSPL == validPoints[0].LevelSPL)
            {
                return validPoints[0].GetAttenuation(channel);
            }

            //Check above max
            if (levelSPL > validPoints[validPoints.Count - 1].LevelSPL)
            {
                // Subtract additional attenuation from the last point
                double additionalAttenuationDB = levelSPL - validPoints[validPoints.Count - 1].LevelSPL;
                return validPoints[validPoints.Count - 1].GetAttenuation(channel) - additionalAttenuationDB;
            }
            else if (levelSPL == validPoints[validPoints.Count - 1].LevelSPL)
            {
                return validPoints[validPoints.Count - 1].GetAttenuation(channel);
            }

            //Level is between upper and lower bounds of the levels

            int upperBound;

            for (upperBound = 1; upperBound < validPoints.Count - 1; upperBound++)
            {
                if (levelSPL < validPoints[upperBound].LevelSPL)
                {
                    //Found the first LevelSPL larger than the target level
                    break;
                }
            }

            int lowerBound = upperBound - 1;

            double lowerBoundSPL = validPoints[lowerBound].LevelSPL;
            double upperBoundSPL = validPoints[upperBound].LevelSPL;

            if (levelSPL == lowerBoundSPL)
            {
                //Equal to the lowerbound
                return validPoints[lowerBound].GetAttenuation(channel);
            }
            else
            {
                //Interpolate linearly between two adjacent attenuation values
                double t = (levelSPL - lowerBoundSPL) / (upperBoundSPL - lowerBoundSPL);
                double lowerBoundAttenuation = validPoints[lowerBound].GetAttenuation(channel);
                double upperBoundAttenuation = validPoints[upperBound].GetAttenuation(channel);
                return GeneralMath.Lerp(lowerBoundAttenuation, upperBoundAttenuation, t);
            }
        }

        public class CalibrationPoint
        {
            public double LevelSPL { get; }

            public double LeftAttenuation { get; set; }
            public double RightAttenuation { get; set; }

            public CalibrationPoint(double levelSPL)
            {
                LevelSPL = levelSPL;

                LeftAttenuation = double.NaN;
                RightAttenuation = double.NaN;
            }

            public CalibrationPoint(JsonObject data)
            {
                LevelSPL = data["LevelSPL"];

                LeftAttenuation = data.ContainsKey("LeftAttenuation") ? data["LeftAttenuation"].AsNumber : double.NaN;
                RightAttenuation = data.ContainsKey("RightAttenuation") ? data["RightAttenuation"].AsNumber : double.NaN;
            }

            public bool IsComplete()
            {
                return double.IsFinite(LeftAttenuation) && double.IsFinite(RightAttenuation);
            }

            public double GetAttenuation(AudioChannel channel)
            {
                switch (channel)
                {
                    case AudioChannel.Left: return LeftAttenuation;
                    case AudioChannel.Right: return RightAttenuation;

                    case AudioChannel.Both:
                    default:
                        throw new Exception($"Unexpected AudioChannel for GetAttenuation: {channel}");
                }
            }

            public JsonObject Serialize()
            {
                JsonObject data = new JsonObject()
                {
                    ["LevelSPL"] = LevelSPL
                };

                if (!double.IsNaN(LeftAttenuation))
                {
                    data.Add("LeftAttenuation", LeftAttenuation);
                }

                if (!double.IsNaN(RightAttenuation))
                {
                    data.Add("RightAttenuation", RightAttenuation);
                }

                return data;
            }
        }
    }
}
