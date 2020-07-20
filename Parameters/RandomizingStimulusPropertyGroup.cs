using System;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Random")]
    public abstract class RandomizingStimulusPropertyGroup : StimulusPropertyGroup, IRandomizer
    {
        protected Random Randomizer => randomizerGetter();
        private Func<Random> randomizerGetter;

        #region IRandomizer

        void IRandomizer.AssignRandomizer(Func<Random> randomizerGetter)
        {
            this.randomizerGetter = randomizerGetter;
        }

        #endregion IRandomizer
    }
}
