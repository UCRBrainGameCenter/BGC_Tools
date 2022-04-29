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
        public TransducerProfile TransducerProfile { get; set; }

        public FrequencyCollection PureTone { get; set; }
        public FrequencyCollection Narrowband { get; set; }
        public LevelCollection Broadband { get; set; }

        public DateTime CalibrationDate { get; set; }


        public CalibrationProfile(TransducerProfile transducerProfile)
        {
            TransducerProfile = transducerProfile;

            CalibrationDate = DateTime.Now;

            PureTone = new FrequencyCollection();
            Narrowband = new FrequencyCollection();
            Broadband = new LevelCollection();
        }

        public CalibrationProfile(JsonObject data)
        {
            TransducerProfile = new TransducerProfile(data["Transducer"]);

            CalibrationDate = data["CalibrationDate"].AsDateTime.Value;

            PureTone = new FrequencyCollection(data["PureTone"]);
            Narrowband = new FrequencyCollection(data["Narrowband"]);
            Broadband = new LevelCollection(data["Broadband"]);
        }

        public JsonObject Serialize() => new JsonObject()
        {
            ["Transducer"] = TransducerProfile.Serialize(),

            ["CalibrationDate"] = CalibrationDate,

            ["PureTone"] = PureTone.Serialize(),
            ["Narrowband"] = Narrowband.Serialize(),
            ["Broadband"] = Broadband.Serialize()
        };

        public double EstimateRMS(
            AudiometricCalibration.CalibrationSet calibrationSet,
            double frequency,
            double levelHL,
            AudioChannel channel)
        {
            double rmsEstimate = double.NaN;

            switch (calibrationSet)
            {
                case AudiometricCalibration.CalibrationSet.PureTone:
                    if (PureTone.Points.Count > 0)
                    {
                        rmsEstimate = PureTone.GetRMS(frequency, levelHL, channel);

                        if (double.IsNaN(rmsEstimate))
                        {
                            rmsEstimate = PureTone.GetRMS(frequency, levelHL, channel.Flip());
                        }
                    }

                    if (!double.IsNaN(rmsEstimate))
                    {
                        return rmsEstimate;
                    }

                    //Gross estimate to start with
                    double levelSPL = TransducerProfile.GetSPL(frequency, levelHL);
                    return (1.0 / 32.0) * Math.Pow(10.0, (levelSPL - 91.0) / 20.0);

                case AudiometricCalibration.CalibrationSet.Narrowband:
                    if (Narrowband.Points.Count > 0)
                    {
                        rmsEstimate = Narrowband.GetRMS(frequency, levelHL, channel);

                        if (double.IsNaN(rmsEstimate))
                        {
                            rmsEstimate = Narrowband.GetRMS(frequency, levelHL, channel.Flip());
                        }
                    }

                    if (!double.IsNaN(rmsEstimate))
                    {
                        return rmsEstimate;
                    }
                    goto case AudiometricCalibration.CalibrationSet.PureTone;

                case AudiometricCalibration.CalibrationSet.Broadband:
                    frequency = 2000.0;
                    if (Broadband.Points.Count > 0)
                    {
                        rmsEstimate = Broadband.GetRMS(levelHL, channel);

                        if (double.IsNaN(rmsEstimate))
                        {
                            rmsEstimate = Broadband.GetRMS(levelHL, channel.Flip());
                        }
                    }

                    if (!double.IsNaN(rmsEstimate))
                    {
                        return rmsEstimate;
                    }
                    goto case AudiometricCalibration.CalibrationSet.Narrowband;

                default:
                    UnityEngine.Debug.LogError($"Unsupported CalibrationSet: {calibrationSet}");
                    goto case AudiometricCalibration.CalibrationSet.PureTone;
            }
        }

        public double GetRMS(
            AudiometricCalibration.CalibrationSet calibrationSet,
            double frequency,
            double levelHL,
            AudioChannel channel)
        {
            switch (calibrationSet)
            {
                case AudiometricCalibration.CalibrationSet.PureTone:
                    return PureTone.GetRMS(frequency, levelHL, channel);

                case AudiometricCalibration.CalibrationSet.Narrowband:
                    return Narrowband.GetRMS(frequency, levelHL, channel);

                case AudiometricCalibration.CalibrationSet.Broadband:
                    return Broadband.GetRMS(levelHL, channel);

                default:
                    UnityEngine.Debug.LogError($"Unsupported CalibrationSet: {calibrationSet}");
                    goto case AudiometricCalibration.CalibrationSet.PureTone;
            }
        }
    }


    public class FrequencyCollection
    {
        public List<FrequencyPoint> Points { get; set; }

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

        public void SetCalibrationPoint(
            double frequency,
            double levelHL,
            AudioChannel channel,
            double rms) =>
            GetLevelCollection(frequency)
            .SetCalibrationValue(levelHL, channel, rms);

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

        public JsonArray Serialize()
        {
            JsonArray points = new JsonArray();
            foreach (FrequencyPoint point in Points)
            {
                points.Add(point.Serialize());
            }

            return points;
        }

        public double GetRMS(double frequency, double levelHL, AudioChannel channel)
        {
            if (frequency <= Points[0].Frequency)
            {
                return Points[0].Levels.GetRMS(levelHL, channel);
            }

            if (frequency >= Points[Points.Count - 1].Frequency)
            {
                return Points[Points.Count - 1].Levels.GetRMS(levelHL, channel);
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
                return Points[upperBound - 1].Levels.GetRMS(levelHL, channel);
            }
            else
            {
                //Interpolation will be Exponential on the input (Octave-scale) and Linear on the output
                double t = Math.Log(frequency / Points[upperBound - 1].Frequency) /
                    Math.Log(Points[upperBound].Frequency / Points[upperBound - 1].Frequency);

                double lowerBoundRMS = Points[upperBound - 1].Levels.GetRMS(levelHL, channel);
                double upperBoundRMS = Points[upperBound].Levels.GetRMS(levelHL, channel);

                return GeneralMath.Lerp(lowerBoundRMS, upperBoundRMS, t);
            }
        }

        public class FrequencyPoint
        {
            public double Frequency { get; set; }
            public LevelCollection Levels { get; set; }

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
        }
    }

    public class LevelCollection
    {
        public List<CalibrationPoint> Points { get; set; }

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

        public void SetCalibrationValue(
            double levelHL,
            AudioChannel channel,
            double rms)
        {
            switch (channel)
            {
                case AudioChannel.Left:
                    GetCalibrationPoint(levelHL).LeftRMS = rms;
                    break;

                case AudioChannel.Right:
                    GetCalibrationPoint(levelHL).RightRMS = rms;
                    break;

                default:
                    throw new Exception($"Unexpected AudioChannel for Setting Calibration {channel}");
            }

        }

        private CalibrationPoint GetCalibrationPoint(double levelHL)
        {
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].LevelHL == levelHL)
                {
                    //Found target level
                    return Points[i];
                }

                if (Points[i].LevelHL > levelHL)
                {
                    //Passed target level - create new
                    Points.Insert(i, new CalibrationPoint(levelHL));
                    return Points[i];
                }
            }

            //Reached the end without finding it
            Points.Add(new CalibrationPoint(levelHL));
            return Points[Points.Count - 1];
        }

        public double GetRMS(double levelHL, AudioChannel channel)
        {
            List<CalibrationPoint> validPoints = Points.Where(x => !double.IsNaN(x.GetRMS(channel))).ToList();

            if (validPoints.Count == 0)
            {
                return double.NaN;
            }

            //Check below min
            if (levelHL < validPoints[0].LevelHL)
            {
                double additionalFactor = Math.Pow(10, (levelHL - validPoints[0].LevelHL) / 20.0);
                return additionalFactor * validPoints[0].GetRMS(channel);
            }
            else if (levelHL == validPoints[0].LevelHL)
            {
                return validPoints[0].GetRMS(channel);
            }

            //Check above max
            if (levelHL > validPoints[validPoints.Count - 1].LevelHL)
            {
                double additionalFactor = Math.Pow(10, (levelHL - validPoints[validPoints.Count - 1].LevelHL) / 20.0);
                return additionalFactor * validPoints[validPoints.Count - 1].GetRMS(channel);
            }
            else if (levelHL == validPoints[validPoints.Count - 1].LevelHL)
            {
                return validPoints[validPoints.Count - 1].GetRMS(channel);
            }

            //Level is between upper and lower bounds of the levels

            int upperBound;

            for (upperBound = 1; upperBound < validPoints.Count - 1; upperBound++)
            {
                if (levelHL < validPoints[upperBound].LevelHL)
                {
                    //Found the first LevelHL larger than the target level
                    break;
                }
            }

            if (levelHL == validPoints[upperBound - 1].LevelHL)
            {
                //Equal to the lowerbound
                return validPoints[upperBound - 1].GetRMS(channel);
            }
            else
            {
                //Interpolate exponentially between two adjacent RMS values

                //The progression parameter is determined linearly
                double t = (levelHL - validPoints[upperBound - 1].LevelHL) / (validPoints[upperBound].LevelHL - validPoints[upperBound - 1].LevelHL);

                return Math.Pow(validPoints[upperBound - 1].GetRMS(channel), 1 - t) * Math.Pow(validPoints[upperBound].LeftRMS, t);
            }
        }

        public class CalibrationPoint
        {
            public double LevelHL { get; set; }

            public double LeftRMS { get; set; }
            public double RightRMS { get; set; }

            public CalibrationPoint(double levelHL)
            {
                LevelHL = levelHL;

                LeftRMS = double.NaN;
                RightRMS = double.NaN;
            }

            public CalibrationPoint(JsonObject data)
            {
                LevelHL = data["LevelHL"];

                LeftRMS = data.ContainsKey("LeftRMS") ? data["LeftRMS"].AsNumber : double.NaN;
                RightRMS = data.ContainsKey("RightRMS") ? data["RightRMS"].AsNumber : double.NaN;
            }

            public double GetRMS(AudioChannel channel)
            {
                switch (channel)
                {
                    case AudioChannel.Left: return LeftRMS;
                    case AudioChannel.Right: return RightRMS;

                    case AudioChannel.Both:
                    default:
                        throw new Exception($"Unexpected AudioChannel for GetRMS: {channel}");
                }
            }

            public JsonObject Serialize()
            {
                JsonObject data = new JsonObject()
                {
                    ["LevelHL"] = LevelHL
                };

                if (!double.IsNaN(LeftRMS))
                {
                    data.Add("LeftRMS", LeftRMS);
                }
                
                if (!double.IsNaN(RightRMS))
                {
                    data.Add("RightRMS", RightRMS);
                }

                return data;
            }
        }
    }
}
