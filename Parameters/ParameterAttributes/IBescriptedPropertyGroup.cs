using BGC.Scripting;

namespace BGC.Parameters
{
    public interface IBescriptedPropertyGroup
    {
        int InitPriority { get; }
        void Initialize(GlobalRuntimeContext globalContext);

        void UpdateStateVarRectifier(InputRectificationContainer rectifier);
    }
}
