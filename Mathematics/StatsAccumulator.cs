using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGC.Mathematics
{
    public class StatsAccumulator
    {
        public void Append(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v))
            {
                if (!isBadValueReported)
                {
                    UnityEngine.Debug.LogError($"Invalid Value submitted to StatsAccumulator.Append: {v}");
                    isBadValueReported = true;
                }
                return;
            }

            Count += 1;
            double delta = v - Mean;
            Mean += delta / Count;
            double delta2 = v - Mean;
            Mean2 += delta * delta2;
        }

        public double Mean { get; private set; } = 0.0;
        public double Mean2 { get; private set; } = 0.0;
        public int Count { get; private set; } = 0;
        public double Variance => Count >= 2 ? Mean2 / (Count - 1) : 0.0;
        public double StdDev => Math.Sqrt(Variance);

        // Error reporting
        private bool isBadValueReported = false;
    }

    public class WeightedStatsAccumulator
    {
        public void Append(double v, double w)
        {
            if (double.IsNaN(v) || double.IsInfinity(v))
            {
                if (!isBadValueReported)
                {
                    UnityEngine.Debug.LogError($"Invalid Value submitted to WeightedStatsAccumulator.Append: {v}");
                    isBadValueReported = true;
                }
                return;
            }
            if (w < 0.0 || double.IsNaN(w) || double.IsInfinity(w))
            {
                if (!isBadWeightReported)
                {
                    UnityEngine.Debug.LogError($"Invalid Weight submitted to WeightedStatsAccumulator.Append: {w}");
                    isBadWeightReported = true;
                }
                return;
            }

            if (w > 0.0)
            {
                Weight += w;
                double oldMean = Mean;
                Mean += (w / Weight) * (v - oldMean);
                Mean2 += w * (v - oldMean) * (v - Mean);
            }
        }

        public double Mean { get; private set; } = 0.0;
        public double Mean2 { get; private set; } = 0.0;
        public double Weight { get; private set; } = 0.0;
        public double Variance => Weight > 0.0 ? Mean2 / Weight : 0.0;
        public double StdDev => Math.Sqrt(Variance);

        // Error reporting
        private bool isBadValueReported = false;
        private bool isBadWeightReported = false;
    }
}
