namespace BGC.Parameters
{
    [PropertyGroupTitle("Standard Behavior")]
    public interface IStandardBehavior<T> : IPropertyGroup
    {
        T GetStandard(T targetValue);
    }

}
