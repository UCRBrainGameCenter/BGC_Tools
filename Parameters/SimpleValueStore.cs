using System;
using System.Collections;
using System.Collections.Generic;

namespace BGC.Parameters
{
    [PropertyChoiceTitle("Fixed")]
    public abstract class SimpleValueStore<T> : StimulusPropertyGroup
    {
        [DisplayInputField("Value")]
        public T Value { get; set; }

        [DisplayInputFieldKey("Value")]
        public string ValueKey { get; set; }
    }
    
    public abstract class SimpleIntValueStore : SimpleValueStore<int>
    {
        [AppendSelection(
            typeof(MirroredStandardBehavior<int>),
            typeof(SplitStandardBehavior<int>))]
        public IStandardBehavior<int> StandardBehavior { get; set; }

        public int Standard => StandardBehavior.GetStandard(Value);
        public int GetValue(bool target) => target ? Value : Standard;
    }

    public abstract class SimpleDoubleValueStore : SimpleValueStore<double>
    {
        [AppendSelection(
            typeof(MirroredStandardBehavior<double>),
            typeof(SplitStandardBehavior<double>))]
        public IStandardBehavior<double> StandardBehavior { get; set; }

        public double Standard => StandardBehavior.GetStandard(Value);
        public double GetValue(bool target) => target ? Value : Standard;
    }
}
