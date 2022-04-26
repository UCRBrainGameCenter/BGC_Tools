using BGC.Audio.AnalyticStreams;
using BGC.Audio.Audiometry;
using System.Collections.Generic;
using System.Linq;

namespace BGC.Audio
{
    public class PresentationConstraints
    {
        /// <summary>
        /// Indicates type of stimulus, whether broadband, narrowband, or puretone
        /// </summary>
        public readonly AudiometricCalibration.CalibrationSet calibrationSet;

        /// <summary>
        /// Frequency of puretone or narrowband
        /// </summary>
        public readonly double frequency;

        /// <summary>
        /// Indicates the set of PresentationConstraints is the result of a compromise between disagreeing components
        /// </summary>
        public readonly bool compromise;

        public PresentationConstraints(
            AudiometricCalibration.CalibrationSet calibrationSet,
            double frequency,
            bool compromise)
        {
            this.calibrationSet = calibrationSet;
            this.frequency = frequency;
            this.compromise = compromise;
        }

        public bool IsEquivalent(PresentationConstraints other) =>
            calibrationSet == other.calibrationSet && 
            (frequency == other.frequency || calibrationSet == AudiometricCalibration.CalibrationSet.Broadband);

        public static IEnumerable<PresentationConstraints> ExtractSetConstraintsChannelwise(IEnumerable<IBGCStream> streams)
        {
            int channels = streams.First().Channels;
            return streams
                .Select(x => x.GetPresentationConstraints())
                .Aggregate(
                    seed: Enumerable.Repeat<PresentationConstraints>(null, channels),
                    func: (result, next) => result.Zip(next, Compromise))
                .ToArray();
        }

        public static PresentationConstraints ExtractSetConstraints(IEnumerable<IAnalyticStream> streams)
        {
            IEnumerable<PresentationConstraints> constraints = streams
                    .Select(x => x.GetPresentationConstraints())
                    .Where(x => x != null);

            if (!constraints.Any())
            {
                return null;
            }

            PresentationConstraints result = constraints.First();

            List<PresentationConstraints> remainingConstraints = constraints
                .Skip(1)
                .Where(x => !result.IsEquivalent(x))
                .ToList();

            if (remainingConstraints.Count == 0)
            {
                //No conflicting constraints
                return result;
            }

            //Compromise for conflicting constraints
            foreach (PresentationConstraints constraint in remainingConstraints)
            {
                result = Compromise(result, constraint);
            }

            return result;
        }

        public static PresentationConstraints Compromise(PresentationConstraints first, PresentationConstraints second)
        {
            //Check if either is null
            if (first == null)
            {
                //Return second, whether or not it's null, since the first has no bearing
                return second;
            }

            if (second == null)
            {
                //First was not null, but return it since second is
                return first;
            }

            //
            //Both values non-null
            //

            //Check if they're equal
            if (first.IsEquivalent(second))
            {
                return first;
            }

            //
            //Check if either is already the loosest compromise
            //

            if (first.calibrationSet == AudiometricCalibration.CalibrationSet.Broadband && first.compromise)
            {
                //First is already the loosest constraint
                return first;
            }

            if (second.calibrationSet == AudiometricCalibration.CalibrationSet.Broadband && second.compromise)
            {
                //second is already the loosest constraint
                return first;
            }

            if (first.calibrationSet == AudiometricCalibration.CalibrationSet.Broadband ||
                second.calibrationSet == AudiometricCalibration.CalibrationSet.Broadband)
            {
                //Form a new broadband compromise
                return new PresentationConstraints(
                    calibrationSet: AudiometricCalibration.CalibrationSet.Broadband,
                    frequency: double.NaN,
                    compromise: true);
            }

            //
            //Neither is broadband
            //

            //If the frequencies match, that means one is narrowband and one is broadband
            if (first.frequency == second.frequency)
            {
                //Fallback to narrowband compromise
                return new PresentationConstraints(
                    calibrationSet: AudiometricCalibration.CalibrationSet.Narrowband,
                    frequency: first.frequency,
                    compromise: true);
            }

            //Either they are both puretones or narrowbands of different frequencies, or they are different stimulus
            //types of different frequencies.  Either way, broadband compromise
            return new PresentationConstraints(
                calibrationSet: AudiometricCalibration.CalibrationSet.Broadband,
                frequency: double.NaN,
                compromise: true);
        }
    }

}
