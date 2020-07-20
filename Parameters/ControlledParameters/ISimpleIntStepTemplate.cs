using BGC.Scripting;

namespace BGC.Parameters
{
    [PropertyGroupTitle("Step Template")]
    public interface ISimpleIntStepTemplate : IPropertyGroup
    {
        bool CouldStepTo(int stepNumber);
        int GetValue(int stepNumber);
        double GetPartialValue(double stepNumber);
        void Initialize();
    }
}
