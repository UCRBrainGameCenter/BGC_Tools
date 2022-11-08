﻿using LightJson;

namespace BGC.Mathematics
{
    public class TrialStats
    {
        public string Name { get; }
        public int Hits { get; private set; } = 0;
        public int Misses { get; private set; } = 0;
        public int NoResponses { get; private set; } = 0;
        public int Trials => Hits + Misses + NoResponses;
        public double HitAccuracy => GeneralMath.Accuracy(Hits, Trials);
        public double AverageReactionTime => reactionTimeAccum.Mean;
        public double HitAverageReactionTime => hitReactionTimeAccum.Mean;
        public double MissAverageReactionTime => missReactionTimeAccum.Mean;

        private StatsAccumulator reactionTimeAccum = new StatsAccumulator();
        private StatsAccumulator hitReactionTimeAccum = new StatsAccumulator();
        private StatsAccumulator missReactionTimeAccum = new StatsAccumulator();

        public TrialStats(string name)
        {
            Name = name;
        }

        public void Reset()
        {
            Hits = 0;
            Misses = 0;
            NoResponses = 0;
            reactionTimeAccum = new StatsAccumulator();
            hitReactionTimeAccum = new StatsAccumulator();
            missReactionTimeAccum = new StatsAccumulator();
        }

        public void Hit(double reactionTime)
        {
            Hits++;
            reactionTimeAccum.Append(reactionTime);
            hitReactionTimeAccum.Append(reactionTime);
        }

        public void Miss(double reactionTime)
        {
            Misses++;
            reactionTimeAccum.Append(reactionTime);
            missReactionTimeAccum.Append(reactionTime);
        }

        public void NoResponse() => NoResponses++;

        public void AddSummary(JsonObject obj)
        {
            obj.Add($"{Name}Hits", Hits);
            obj.Add($"{Name}Misses", Misses);
            obj.Add($"{Name}NoResponses", NoResponses);
            obj.Add($"{Name}Accuracy", HitAccuracy);
            obj.Add($"{Name}AverageReactionTime", AverageReactionTime);
            obj.Add($"{Name}HitAverageReactionTime", HitAverageReactionTime);
            obj.Add($"{Name}MissAverageReactionTime", MissAverageReactionTime);
        }
    }
}
