namespace BGC.MonoUtility.Interpolation
{
    public interface ILerpAction<T>
    {
        void Initialize(T target);
        void CallAction(float t);
    }
}
