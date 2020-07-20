using BGC.Scripting;

namespace BGC.Parameters
{
    [PropertyGroupTitle("Step Template")]
    public interface ISimpleDoubleStepTemplate : IPropertyGroup
    {
        bool CouldStepTo(int stepNumber);
        double GetValue(int stepNumber);
        double GetPartialValue(double stepNumber);
        void Initialize();
    }
}
