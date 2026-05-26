using System;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Mathematics.QuestPlus
{
    /// <summary>
    /// A named, discretely-sampled dimension used to define the QUEST+ stimulus
    /// domain or parameter domain.
    /// </summary>
    public sealed class QuestPlusDimension
    {
        public string Name { get; }
        public double[] Values { get; }
        public int Length => Values.Length;

        public QuestPlusDimension(string name, IEnumerable<double> values)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Dimension name cannot be null or empty.", nameof(name));
            }
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Name = name;
            Values = values.ToArray();

            if (Values.Length == 0)
            {
                throw new ArgumentException($"Dimension '{name}' must contain at least one value.", nameof(values));
            }
        }

        public QuestPlusDimension(string name, double singleValue)
            : this(name, new[] { singleValue })
        {
        }

        public static QuestPlusDimension Range(string name, double start, double stop, double step)
        {
            if (step == 0)
            {
                throw new ArgumentException("Step cannot be zero.", nameof(step));
            }

            List<double> values = new List<double>();
            if (step > 0)
            {
                for (double v = start; v <= stop + 1e-12; v += step)
                {
                    values.Add(v);
                }
            }
            else
            {
                for (double v = start; v >= stop - 1e-12; v += step)
                {
                    values.Add(v);
                }
            }
            return new QuestPlusDimension(name, values);
        }
    }
}
