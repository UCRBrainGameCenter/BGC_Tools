using System;
using System.Threading;
using BGC.Mathematics;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Roving")]
    public abstract class RandomizingDoubleProperty : RandomizingStimulusPropertyGroup
    {
        [DisplayInputField("GenerationFrequency")]
        public GenerationPhase GenerationFrequency { get; set; }

        [AppendSelection(
            typeof(LinearRadialRove),
            typeof(LinearRangeRove),
            typeof(ExponentialRadialRove),
            typeof(ExponentialRangeRove))]
        public IRovingDoubleBehavior RovingBehavior { get; set; }

        protected virtual double GetRandomValue() => RovingBehavior.GetRandomValue(Randomizer);
        protected double UpperBound => RovingBehavior.UpperBound;
        protected double LowerBound => RovingBehavior.LowerBound;

        private double _doubleValue;
        private ThreadLocal<double> _threadLocalDoubleValue = new ThreadLocal<double>(() => 0.0);
        protected double DoubleValue
        {
            get
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance: return GetRandomValue();
                    case GenerationPhase.Interval: return _threadLocalDoubleValue.Value;
                    case GenerationPhase.Trial:
                    case GenerationPhase.Task: return _doubleValue;

                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        return _doubleValue;
                }
            }

            set
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance:
                        //Do Nothing
                        break;

                    case GenerationPhase.Interval:
                        _threadLocalDoubleValue.Value = value;
                        break;

                    case GenerationPhase.Trial:
                    case GenerationPhase.Task:
                        _doubleValue = value;
                        break;

                    case GenerationPhase.MAX:
                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        _doubleValue = value;
                        break;
                }
            }
        }

        public override void InitiatePhase(GenerationPhase phase)
        {
            switch (phase)
            {
                case GenerationPhase.Instance:
                    //Do Nothing
                    break;

                case GenerationPhase.Task:
                    DoubleValue = GetRandomValue();
                    break;

                case GenerationPhase.Interval:
                case GenerationPhase.Trial:
                    if (phase == GenerationFrequency)
                    {
                        DoubleValue = GetRandomValue();
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase value: {phase}");
                    break;
            }
        }
    }

    [PropertyChoiceTitle("Roving")]
    public abstract class RandomizingLinearDoubleProperty : RandomizingStimulusPropertyGroup
    {
        [DisplayInputField("GenerationFrequency")]
        public GenerationPhase GenerationFrequency { get; set; }

        [AppendSelection(
            typeof(LinearRadialRove),
            typeof(LinearRangeRove))]
        public IRovingDoubleBehavior RovingBehavior { get; set; }

        protected virtual double GetRandomValue() => RovingBehavior.GetRandomValue(Randomizer);
        protected double UpperBound => RovingBehavior.UpperBound;
        protected double LowerBound => RovingBehavior.LowerBound;

        private double _doubleValue;
        private ThreadLocal<double> _threadLocalDoubleValue = new ThreadLocal<double>(() => 0.0);
        protected double DoubleValue
        {
            get
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance: return GetRandomValue();
                    case GenerationPhase.Interval: return _threadLocalDoubleValue.Value;
                    case GenerationPhase.Trial:
                    case GenerationPhase.Task: return _doubleValue;

                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        return _doubleValue;
                }
            }

            set
            {
                switch (GenerationFrequency)
                {
                    case GenerationPhase.Instance:
                        //Do Nothing
                        break;

                    case GenerationPhase.Interval:
                        _threadLocalDoubleValue.Value = value;
                        break;

                    case GenerationPhase.Trial:
                    case GenerationPhase.Task:
                        _doubleValue = value;
                        break;

                    case GenerationPhase.MAX:
                    default:
                        UnityEngine.Debug.LogError($"Unexpected GenerationPhase: {GenerationFrequency}");
                        _doubleValue = value;
                        break;
                }
            }
        }

        public override void InitiatePhase(GenerationPhase phase)
        {
            switch (phase)
            {
                case GenerationPhase.Instance:
                    //Do Nothing
                    break;

                case GenerationPhase.Task:
                    DoubleValue = GetRandomValue();
                    break;

                case GenerationPhase.Interval:
                case GenerationPhase.Trial:
                    if (phase == GenerationFrequency)
                    {
                        DoubleValue = GetRandomValue();
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError($"Unexpected GenerationPhase value: {phase}");
                    break;
            }
        }
    }

    [PropertyChoiceTitle("Linear Radial Rove")]
    [FieldMirrorDisplay(fieldName: "CentralValue", mirroredFieldName: "CentralValue", displayTitle: "Central Value")]
    [FieldMirrorDisplay(fieldName: "RovingRadius", mirroredFieldName: "RovingRadius", displayTitle: "Roving Radius")]
    public class LinearRadialRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("CentralValue")]
        public double CentralValue { get; set; }
        [DisplayInputField("RovingRadius")]
        public double RovingRadius { get; set; }

        [DisplayInputFieldKey("CentralValue")]
        public string CentralValueKey { get; set; }
        [DisplayInputFieldKey("RovingRadius")]
        public string RovingRadiusKey { get; set; }

        [AppendSelection(
            typeof(UniformDistribution),
            typeof(GaussianDistribution),
            typeof(BetaDistribution),
            typeof(TruncatedExponentialDistribution),
            typeof(DiscreteUniformDistribution))]
        public IRovingDistribution Distribution { get; set; }

        public LinearRadialRove()
        {
            Distribution = new UniformDistribution();
        }

        void IRovingDoubleBehavior.SetCenterValue(double centerValue) =>
            CentralValue = centerValue;

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            CentralValue + (RovingRadius * (2.0 * Distribution.GetSample(Randomizer) - 1.0));

        double IRovingDoubleBehavior.LowerBound => CentralValue - RovingRadius;
        double IRovingDoubleBehavior.UpperBound => CentralValue + RovingRadius;
    }

    [PropertyChoiceTitle("Linear Range Rove")]
    [FieldMirrorDisplay(fieldName: "LowerBound", mirroredFieldName: "LowerBound", displayTitle: "Lower Bound")]
    [FieldMirrorDisplay(fieldName: "UpperBound", mirroredFieldName: "UpperBound", displayTitle: "Upper Bound")]
    public class LinearRangeRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("LowerBound")]
        public double LowerBound { get; set; }
        [DisplayInputField("UpperBound")]
        public double UpperBound { get; set; }

        [DisplayInputFieldKey("LowerBound")]
        public string LowerBoundKey { get; set; }
        [DisplayInputFieldKey("UpperBound")]
        public string UpperBoundKey { get; set; }

        [AppendSelection(
            typeof(UniformDistribution),
            typeof(GaussianDistribution),
            typeof(BetaDistribution),
            typeof(TruncatedExponentialDistribution),
            typeof(DiscreteUniformDistribution))]
        public IRovingDistribution Distribution { get; set; }

        public LinearRangeRove()
        {
            Distribution = new UniformDistribution();
        }

        void IRovingDoubleBehavior.SetCenterValue(double centerValue)
        {
            double halfRange = 0.5 * (UpperBound - LowerBound);
            LowerBound = centerValue - halfRange;
            UpperBound = centerValue + halfRange;
        }

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            LowerBound + ((UpperBound - LowerBound) * Distribution.GetSample(Randomizer));

        double IRovingDoubleBehavior.LowerBound => LowerBound;
        double IRovingDoubleBehavior.UpperBound => UpperBound;
    }

    [PropertyChoiceTitle("Exponential Radial Rove")]
    [FieldMirrorDisplay(fieldName: "CentralValue", mirroredFieldName: "CentralValue", displayTitle: "Central Value")]
    [FieldMirrorDisplay(fieldName: "RovingRadialFactor", mirroredFieldName: "RovingRadialFactor", displayTitle: "Roving Radial Factor")]
    public class ExponentialRadialRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("CentralValue")]
        public double CentralValue { get; set; }
        [DisplayInputField("RovingRadialFactor")]
        public double RovingRadialFactor { get; set; }

        [DisplayInputFieldKey("CentralValue")]
        public string CentralValueKey { get; set; }
        [DisplayInputFieldKey("RovingRadialFactor")]
        public string RovingRadialFactorKey { get; set; }

        [AppendSelection(
            typeof(UniformDistribution),
            typeof(GaussianDistribution),
            typeof(BetaDistribution),
            typeof(TruncatedExponentialDistribution),
            typeof(DiscreteUniformDistribution))]
        public IRovingDistribution Distribution { get; set; }

        public ExponentialRadialRove()
        {
            Distribution = new UniformDistribution();
        }

        void IRovingDoubleBehavior.SetCenterValue(double centerValue) =>
            CentralValue = centerValue;

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            LowerBound * Math.Pow((UpperBound / LowerBound), Distribution.GetSample(Randomizer));

        private double LowerBound => CentralValue / RovingRadialFactor;
        private double UpperBound => CentralValue * RovingRadialFactor;

        double IRovingDoubleBehavior.LowerBound => LowerBound;
        double IRovingDoubleBehavior.UpperBound => UpperBound;
    }

    [PropertyChoiceTitle("Exponential Range Rove")]
    [FieldMirrorDisplay(fieldName: "LowerBound", mirroredFieldName: "LowerBound", displayTitle: "Lower Bound")]
    [FieldMirrorDisplay(fieldName: "UpperBound", mirroredFieldName: "UpperBound", displayTitle: "Upper Bound")]
    public class ExponentialRangeRove : StimulusPropertyGroup, IRovingDoubleBehavior
    {
        [DisplayInputField("LowerBound")]
        public double LowerBound { get; set; }
        [DisplayInputField("UpperBound")]
        public double UpperBound { get; set; }

        [DisplayInputFieldKey("LowerBound")]
        public string LowerBoundKey { get; set; }
        [DisplayInputFieldKey("UpperBound")]
        public string UpperBoundKey { get; set; }

        [AppendSelection(
            typeof(UniformDistribution),
            typeof(GaussianDistribution),
            typeof(BetaDistribution),
            typeof(TruncatedExponentialDistribution),
            typeof(DiscreteUniformDistribution))]
        public IRovingDistribution Distribution { get; set; }

        public ExponentialRangeRove()
        {
            Distribution = new UniformDistribution();
        }

        void IRovingDoubleBehavior.SetCenterValue(double centerValue)
        {
            if (LowerBound > 0.0 && UpperBound > 0.0)
            {
                double factor = Math.Sqrt(UpperBound / LowerBound);
                if (!double.IsNaN(factor) && !double.IsInfinity(factor) && factor > 0.0)
                {
                    LowerBound = centerValue / factor;
                    UpperBound = centerValue * factor;
                    return;
                }
            }

            double halfRange = 0.5 * (UpperBound - LowerBound);
            LowerBound = Math.Max(double.Epsilon, centerValue - halfRange);
            UpperBound = Math.Max(LowerBound + double.Epsilon, centerValue + halfRange);
        }

        double IRovingDoubleBehavior.GetRandomValue(Random Randomizer) =>
            LowerBound * Math.Pow((UpperBound / LowerBound), Distribution.GetSample(Randomizer));

        double IRovingDoubleBehavior.LowerBound => LowerBound;
        double IRovingDoubleBehavior.UpperBound => UpperBound;
    }

    [PropertyGroupTitle("Distribution")]
    public interface IRovingDistribution : IPropertyGroup
    {
        double GetSample(Random randomizer);
    }

    [PropertyChoiceTitle("Uniform")]
    public class UniformDistribution : StimulusPropertyGroup, IRovingDistribution
    {
        double IRovingDistribution.GetSample(Random randomizer) => randomizer.NextDouble();
    }

    [PropertyChoiceTitle("Gaussian")]
    [DoubleFieldDisplay("StandardDeviations", displayTitle: "Standard Deviations", initial: 3, minimum: 0.5, maximum: 10)]
    public class GaussianDistribution : StimulusPropertyGroup, IRovingDistribution
    {
        [DisplayInputField("StandardDeviations")]
        public double StandardDeviations { get; set; }

        [DisplayInputFieldKey("StandardDeviations")]
        public string StandardDeviationsKey { get; set; }

        double IRovingDistribution.GetSample(Random randomizer)
        {
            double sigma = 0.5 / StandardDeviations;
            double value = CustomRandom.NormalDistribution(
                sigma, 0.5,
                Math.Max(double.Epsilon, randomizer.NextDouble()),
                randomizer.NextDouble());
            return Math.Max(0.0, Math.Min(1.0, value));
        }
    }

    [PropertyChoiceTitle("Beta")]
    [DoubleFieldDisplay("Alpha", displayTitle: "Alpha", initial: 2, minimum: 0.1, maximum: 100)]
    [DoubleFieldDisplay("Beta", displayTitle: "Beta", initial: 2, minimum: 0.1, maximum: 100)]
    public class BetaDistribution : StimulusPropertyGroup, IRovingDistribution
    {
        [DisplayInputField("Alpha")]
        public double Alpha { get; set; }

        [DisplayInputFieldKey("Alpha")]
        public string AlphaKey { get; set; }

        [DisplayInputField("Beta")]
        public double Beta { get; set; }

        [DisplayInputFieldKey("Beta")]
        public string BetaKey { get; set; }

        double IRovingDistribution.GetSample(Random randomizer)
        {
            double x = GammaSample(Alpha, randomizer);
            double y = GammaSample(Beta, randomizer);

            if (x + y == 0.0)
            {
                return 0.5;
            }

            return x / (x + y);
        }

        /// <summary>
        /// Generates a Gamma-distributed sample using Marsaglia and Tsang's method.
        /// </summary>
        private static double GammaSample(double shape, Random randomizer)
        {
            if (shape < 1.0)
            {
                double u = Math.Max(double.Epsilon, randomizer.NextDouble());
                return GammaSample(shape + 1.0, randomizer) * Math.Pow(u, 1.0 / shape);
            }

            double d = shape - 1.0 / 3.0;
            double c = 1.0 / Math.Sqrt(9.0 * d);

            while (true)
            {
                double x, v;
                do
                {
                    x = CustomRandom.NormalDistribution(
                        Math.Max(double.Epsilon, randomizer.NextDouble()),
                        randomizer.NextDouble());
                    v = 1.0 + c * x;
                } while (v <= 0.0);

                v = v * v * v;
                double u = randomizer.NextDouble();

                if (u < 1.0 - 0.0331 * (x * x) * (x * x))
                {
                    return d * v;
                }

                if (Math.Log(Math.Max(double.Epsilon, u)) < 0.5 * x * x + d * (1.0 - v + Math.Log(v)))
                {
                    return d * v;
                }
            }
        }
    }

    [PropertyChoiceTitle("Truncated Exponential")]
    [DoubleFieldDisplay("Lambda", displayTitle: "Lambda", initial: 3, minimum: 0.01, maximum: 50)]
    public class TruncatedExponentialDistribution : StimulusPropertyGroup, IRovingDistribution
    {
        [DisplayInputField("Lambda")]
        public double Lambda { get; set; }

        [DisplayInputFieldKey("Lambda")]
        public string LambdaKey { get; set; }

        double IRovingDistribution.GetSample(Random randomizer)
        {
            if (Lambda < 1e-10)
            {
                return randomizer.NextDouble();
            }

            double u = randomizer.NextDouble();
            double expNegLambda = Math.Exp(-Lambda);
            return -Math.Log(1.0 - u * (1.0 - expNegLambda)) / Lambda;
        }
    }

    [PropertyChoiceTitle("Discrete Uniform")]
    [IntFieldDisplay("Steps", displayTitle: "Steps", initial: 2, minimum: 2, maximum: 100)]
    public class DiscreteUniformDistribution : StimulusPropertyGroup, IRovingDistribution
    {
        [DisplayInputField("Steps")]
        public int Steps { get; set; }

        [DisplayInputFieldKey("Steps")]
        public string StepsKey { get; set; }

        double IRovingDistribution.GetSample(Random randomizer)
        {
            if (Steps <= 1)
            {
                return 0.5;
            }

            int k = randomizer.Next(0, Steps);
            return k / (double)(Steps - 1);
        }
    }

    [PropertyGroupTitle("Roving Behavior")]
    public interface IRovingDoubleBehavior : IPropertyGroup
    {
        void SetCenterValue(double centerValue);
        double GetRandomValue(System.Random Randomizer);
        double LowerBound { get; }
        double UpperBound { get; }
    }
}
