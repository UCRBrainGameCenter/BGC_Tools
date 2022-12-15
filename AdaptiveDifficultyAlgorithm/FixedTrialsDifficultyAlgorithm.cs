using System;
using System.Collections.Generic;
using System.Linq;
using BGC.Parameters;
using UnityEngine;

namespace BGC.AdaptiveDifficultyAlgorithm
{
    [PropertyChoiceTitle("Fixed Trials")]
    public class FixedTrialsDifficultyAlgorithm : AdaptiveDifficultyAlgorithmBase
    {
        // Parameters
        [DisplayInputField("Trials")]
        public string Trials { get; set; }

        // Internals
        private List<int> trials = new List<int>();
        private int trialIndex;
        private int threshold;
        private bool isDone;

        // IAdaptiveDifficultyAlgorithm implementation
        public override int Difficulty => trialIndex < trials.Count ? trials[trialIndex] : 0;

        public override int Threshold => threshold;

        public override bool IsDone => isDone;

        public override void Initialize()
        {
            try
            {
                trials = Trials.Split(',').Select(s => int.Parse(s.Trim())).ToList();
            }
            catch
            {
                throw new ArgumentException($"[FixedTrialsDifficultyAlgorithm] Invalid trials string: {Trials}");
            }
            if (trials.Count == 0)
            {
                throw new ArgumentException("[FixedTrialsDifficultyAlgorithm] Cannot run with empty trials.");
            }

            trialIndex = 0;
            threshold = 0;
            isDone = false;
        }

        public override bool SubmitTrialResult(bool correct)
        {
            if (correct && trials[trialIndex] > threshold)
            {
                threshold = trials[trialIndex];
            }

            trialIndex++;
            if (trialIndex >= trials.Count)
            {
                isDone = true;
                return false;
            }

            return true;
        }
    }
}
